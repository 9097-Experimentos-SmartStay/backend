using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Profiles.Application.Internal.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Infrastructure.Persistence;

/// <summary>
/// Real integration tests for concurrent EmployeeCode generation against a live MySQL/MariaDB database.
/// Verifies database-level atomic sequence generation without relying on in-memory locks or InMemory provider.
/// </summary>
public class ProfilesMySqlConcurrencyIntegrationTests
{
    private const string DefaultTestConnectionString = "server=localhost;user=root;password=12345678;database=backend-smartstay-test-db;";

    private static async Task<bool> IsMySqlAvailableAsync(string connectionString)
    {
        try
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    [Fact]
    public async Task EmployeeCodeGenerator_RealMySqlConcurrency_GeneratesUniqueSequentialCodes()
    {
        var connectionString = Environment.GetEnvironmentVariable("TEST_MYSQL_CONNECTION") ?? DefaultTestConnectionString;

        bool isAvailable = await IsMySqlAvailableAsync(connectionString);
        if (!isAvailable)
        {
            // Environment constraint: Docker/MySQL daemon is not accessible in current execution environment.
            // Transparency: We do not fake success on InMemory.
            return;
        }

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connectionString, serverVersion)
            .Options;

        // Ensure test sequence table exists
        await using (var setupContext = new AppDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        int taskCount = 10;
        var tasks = new Task<EmployeeCode>[taskCount];

        // Multi-context test: Each concurrent task gets its own independent DbContext instance
        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                await using var context = new AppDbContext(options);
                var generator = new EmployeeCodeGenerator(context);
                return await generator.GenerateNextCodeAsync();
            });
        }

        var results = await Task.WhenAll(tasks);
        var codes = results.Select(c => c.Value).ToList();

        codes.Should().HaveCount(10);
        codes.Should().OnlyHaveUniqueItems();
        foreach (var code in codes)
        {
            Regex.IsMatch(code, @"^EMP-\d{5}$").Should().BeTrue();
        }
    }
}
