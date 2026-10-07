using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;
using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using BackendAwSmartstay.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.API.IAM.Infrastructure.Seed;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace BackendAwSmartstay.API.Tests.IAM.Infrastructure;

[TestFixture]
public class IamSeederTests
{
    private const string AdminEmail = "hotelowner@chain.com";
    private const string StrongPassword = "Quinua-Andina-2026";

    private AppDbContext _dbContext = null!;
    private StubBreachedPasswordChecker _breachedPasswordChecker = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        _breachedPasswordChecker = new StubBreachedPasswordChecker();
    }

    [TearDown]
    public void TearDown() => _dbContext.Dispose();

    private ServiceProvider BuildServices(string? password) =>
        new ServiceCollection()
            .AddLogging()
            .AddSingleton(Options.Create(new InitialChainAdminSettings { Email = AdminEmail, Password = password }))
            .AddSingleton(_dbContext)
            .AddSingleton<IBreachedPasswordChecker>(_breachedPasswordChecker)
            .AddSingleton<IHashingService, HashingService>()
            .AddSingleton<IDomainEventDispatcher, NoOpDomainEventDispatcher>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<NewPasswordValidator>()
            .BuildServiceProvider();

    private async Task SeedAsync(string? password)
    {
        await using var services = BuildServices(password);
        using var scope = services.CreateScope();
        await IamSeeder.SeedAsync(scope.ServiceProvider);
    }

    private Task<User?> FindAdminAsync() =>
        new UserRepository(_dbContext).FindByEmailAsync(new Email(AdminEmail));

    [Test]
    public async Task SeedAsync_WithStrongPassword_CreatesVerifiedChainAdmin()
    {
        await SeedAsync(StrongPassword);

        var admin = await FindAdminAsync();
        admin.Should().NotBeNull();
        admin!.Role.Value.Should().Be(UserRoles.ChainAdmin);
        admin.EmailVerified.Should().BeTrue();
        admin.RequiresMfaEnrollment.Should().BeTrue();
        new HashingService().VerifyPassword(StrongPassword, admin.PasswordHash).Should().BeTrue();
    }

    [TestCase("Short1!", IamErrorCodes.PasswordTooShort)]
    [TestCase("kkkkkkkkkk", IamErrorCodes.PasswordRepetitive)]
    [TestCase("hotel.owner", IamErrorCodes.PasswordContainsEmail)]
    [TestCase("SmartStay2026!", IamErrorCodes.PasswordTooCommon)]
    public async Task SeedAsync_WithPasswordRejectedByPolicy_ThrowsAndCreatesNoUser(string password, string expectedCode)
    {
        var seeding = () => SeedAsync(password);

        (await seeding.Should().ThrowAsync<InvalidFieldException>()).Which.Violation.Code.Should().Be(expectedCode);
        (await FindAdminAsync()).Should().BeNull();
    }

    [Test]
    public async Task SeedAsync_WithBreachedPassword_ThrowsAndCreatesNoUser()
    {
        _breachedPasswordChecker.Status = BreachedPasswordStatus.Breached;

        var seeding = () => SeedAsync(StrongPassword);

        (await seeding.Should().ThrowAsync<InvalidFieldException>()).Which.Violation.Code.Should().Be(IamErrorCodes.PasswordBreached);
        (await FindAdminAsync()).Should().BeNull();
    }

    [Test]
    public async Task SeedAsync_WhenAdminAlreadyExists_SkipsPolicyAndKeepsExistingPassword()
    {
        await SeedAsync(StrongPassword);

        var seeding = () => SeedAsync("weak");

        await seeding.Should().NotThrowAsync();
        var admin = await FindAdminAsync();
        new HashingService().VerifyPassword(StrongPassword, admin!.PasswordHash).Should().BeTrue();
    }

    private sealed class StubBreachedPasswordChecker : IBreachedPasswordChecker
    {
        public BreachedPasswordStatus Status { get; set; } = BreachedPasswordStatus.NotFound;

        public Task<BreachedPasswordStatus> CheckAsync(string password, CancellationToken cancellationToken = default) =>
            Task.FromResult(Status);
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<IEvent> domainEvents, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
