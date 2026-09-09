using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.Queries;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Profiles.Interfaces.REST;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Resources;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Aggregates;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.Enums;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BackendAwSmartstay.API.Tests.Profiles.Interfaces.REST;

public class GuestsControllerTests
{
    private class FakeGuestCommandService : IGuestProfileCommandService
    {
        public Func<CreateGuestProfileCommand, Task<GuestProfile?>>? CreateHandler { get; set; }
        public Func<LinkGuestToUserCommand, Task<GuestProfile?>>? LinkUserHandler { get; set; }
        public Func<SetGuestIdentificationCommand, Task<GuestProfile?>>? SetIdentHandler { get; set; }
        public Func<CorrectGuestIdentificationCommand, Task<GuestProfile?>>? CorrectIdentHandler { get; set; }
        public Func<UpdateGuestContactInformationCommand, Task<GuestProfile?>>? UpdateContactHandler { get; set; }
        public Func<DeactivateGuestProfileCommand, Task<GuestProfile?>>? DeactivateHandler { get; set; }
        public Func<ActivateGuestProfileCommand, Task<GuestProfile?>>? ActivateHandler { get; set; }

        public Task<GuestProfile?> Handle(CreateGuestProfileCommand command) =>
            CreateHandler != null ? CreateHandler(command) : Task.FromResult<GuestProfile?>(null);

        public Task<GuestProfile?> Handle(LinkGuestToUserCommand command) =>
            LinkUserHandler != null ? LinkUserHandler(command) : Task.FromResult<GuestProfile?>(null);

        public Task<GuestProfile?> Handle(SetGuestIdentificationCommand command) =>
            SetIdentHandler != null ? SetIdentHandler(command) : Task.FromResult<GuestProfile?>(null);

        public Task<GuestProfile?> Handle(CorrectGuestIdentificationCommand command) =>
            CorrectIdentHandler != null ? CorrectIdentHandler(command) : Task.FromResult<GuestProfile?>(null);

        public Task<GuestProfile?> Handle(UpdateGuestContactInformationCommand command) =>
            UpdateContactHandler != null ? UpdateContactHandler(command) : Task.FromResult<GuestProfile?>(null);

        public Task<GuestProfile?> Handle(DeactivateGuestProfileCommand command) =>
            DeactivateHandler != null ? DeactivateHandler(command) : Task.FromResult<GuestProfile?>(null);

        public Task<GuestProfile?> Handle(ActivateGuestProfileCommand command) =>
            ActivateHandler != null ? ActivateHandler(command) : Task.FromResult<GuestProfile?>(null);
    }

    private class FakeGuestQueryService : IGuestProfileQueryService
    {
        public Func<GetGuestProfileByIdQuery, Task<GuestProfile?>>? GetByIdHandler { get; set; }
        public Func<GetGuestProfileByEmailQuery, Task<GuestProfile?>>? GetByEmailHandler { get; set; }
        public Func<GetGuestProfileByUserIdQuery, Task<GuestProfile?>>? GetByUserIdHandler { get; set; }
        public Func<GetAllGuestProfilesQuery, Task<IEnumerable<GuestProfile>>>? GetAllHandler { get; set; }

        public Task<GuestProfile?> Handle(GetGuestProfileByIdQuery query) =>
            GetByIdHandler != null ? GetByIdHandler(query) : Task.FromResult<GuestProfile?>(null);

        public Task<GuestProfile?> Handle(GetGuestProfileByEmailQuery query) =>
            GetByEmailHandler != null ? GetByEmailHandler(query) : Task.FromResult<GuestProfile?>(null);

        public Task<GuestProfile?> Handle(GetGuestProfileByUserIdQuery query) =>
            GetByUserIdHandler != null ? GetByUserIdHandler(query) : Task.FromResult<GuestProfile?>(null);

        public Task<IEnumerable<GuestProfile>> Handle(GetAllGuestProfilesQuery query) =>
            GetAllHandler != null ? GetAllHandler(query) : Task.FromResult<IEnumerable<GuestProfile>>(new List<GuestProfile>());
    }

    private static GuestProfile CreateSampleGuest(GuestProfileId? id = null)
    {
        return new GuestProfile(
            id ?? GuestProfileId.New(),
            new PersonName("John", "Doe"),
            new PhoneNumber("+1234567890"),
            new EmailAddress("john.doe@example.com"),
            new IdentificationDocument(DocumentType.Dni, "12345678"),
            new StreetAddress("Main St", "100", "City", "12345", "Country"));
    }

    [Fact]
    public async Task Create_ValidRequest_ShouldReturnCreatedAtAction_WithResource()
    {
        var guest = CreateSampleGuest();
        var commandService = new FakeGuestCommandService
        {
            CreateHandler = _ => Task.FromResult<GuestProfile?>(guest)
        };
        var queryService = new FakeGuestQueryService();
        var controller = new GuestsController(commandService, queryService);

        var request = new CreateGuestProfileResource(
            "John", "Doe", "+1234567890", "john.doe@example.com",
            DocumentType.Dni, "12345678", "Main St", "100", "City", "12345", "Country", null);

        var result = await controller.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(GuestsController.GetById));
        var resource = createdResult.Value.Should().BeOfType<GuestProfileResource>().Subject;
        resource.FirstName.Should().Be("John");
        resource.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task Create_WhenCommandFails_ShouldReturnBadRequest()
    {
        var commandService = new FakeGuestCommandService
        {
            CreateHandler = _ => Task.FromResult<GuestProfile?>(null)
        };
        var queryService = new FakeGuestQueryService();
        var controller = new GuestsController(commandService, queryService);

        var request = new CreateGuestProfileResource(
            "John", "Doe", "+1234567890", null, null, null, null, null, null, null, null, null);

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task GetById_ExistingId_ShouldReturnOk_WithResource()
    {
        var guestId = GuestProfileId.New();
        var guest = CreateSampleGuest(guestId);
        var queryService = new FakeGuestQueryService
        {
            GetByIdHandler = q => Task.FromResult<GuestProfile?>(q.ProfileId == guestId ? guest : null)
        };
        var controller = new GuestsController(new FakeGuestCommandService(), queryService);

        var result = await controller.GetById(guestId.Value);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resource = okResult.Value.Should().BeOfType<GuestProfileResource>().Subject;
        resource.Id.Should().Be(guestId.Value);
        resource.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetById_NonExistingId_ShouldReturnNotFound()
    {
        var queryService = new FakeGuestQueryService
        {
            GetByIdHandler = _ => Task.FromResult<GuestProfile?>(null)
        };
        var controller = new GuestsController(new FakeGuestCommandService(), queryService);

        var result = await controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByEmail_ExistingEmail_ShouldReturnOk()
    {
        var guest = CreateSampleGuest();
        var queryService = new FakeGuestQueryService
        {
            GetByEmailHandler = _ => Task.FromResult<GuestProfile?>(guest)
        };
        var controller = new GuestsController(new FakeGuestCommandService(), queryService);

        var result = await controller.GetByEmail("john.doe@example.com");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resource = okResult.Value.Should().BeOfType<GuestProfileResource>().Subject;
        resource.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task GetByUserId_ExistingUserId_ShouldReturnOk()
    {
        var guest = CreateSampleGuest();
        var queryService = new FakeGuestQueryService
        {
            GetByUserIdHandler = _ => Task.FromResult<GuestProfile?>(guest)
        };
        var controller = new GuestsController(new FakeGuestCommandService(), queryService);

        var result = await controller.GetByUserId(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<GuestProfileResource>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithList()
    {
        var guest1 = CreateSampleGuest();
        var guest2 = CreateSampleGuest();
        var queryService = new FakeGuestQueryService
        {
            GetAllHandler = _ => Task.FromResult<IEnumerable<GuestProfile>>(new[] { guest1, guest2 })
        };
        var controller = new GuestsController(new FakeGuestCommandService(), queryService);

        var result = await controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = okResult.Value.Should().BeAssignableTo<IEnumerable<GuestProfileResource>>().Subject;
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task LinkToUser_ExistingGuest_ShouldReturnOk()
    {
        var guest = CreateSampleGuest();
        var commandService = new FakeGuestCommandService
        {
            LinkUserHandler = _ => Task.FromResult<GuestProfile?>(guest)
        };
        var controller = new GuestsController(commandService, new FakeGuestQueryService());

        var result = await controller.LinkToUser(guest.Id.Value, new LinkGuestToUserResource(1, "verified@example.com"));

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<GuestProfileResource>();
    }

    [Fact]
    public async Task UpdateContactInfo_ExistingGuest_ShouldReturnOk()
    {
        var guest = CreateSampleGuest();
        var commandService = new FakeGuestCommandService
        {
            UpdateContactHandler = _ => Task.FromResult<GuestProfile?>(guest)
        };
        var controller = new GuestsController(commandService, new FakeGuestQueryService());

        var result = await controller.UpdateContactInfo(guest.Id.Value, new UpdateGuestContactInformationResource("+1999888777", "New St", "1", "City", "1000", "Country"));

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<GuestProfileResource>();
    }

    [Fact]
    public async Task Deactivate_ExistingGuest_ShouldReturnOk()
    {
        var guest = CreateSampleGuest();
        var commandService = new FakeGuestCommandService
        {
            DeactivateHandler = _ => Task.FromResult<GuestProfile?>(guest)
        };
        var controller = new GuestsController(commandService, new FakeGuestQueryService());

        var result = await controller.Deactivate(guest.Id.Value);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<GuestProfileResource>();
    }

    [Fact]
    public async Task Activate_ExistingGuest_ShouldReturnOk()
    {
        var guest = CreateSampleGuest();
        var commandService = new FakeGuestCommandService
        {
            ActivateHandler = _ => Task.FromResult<GuestProfile?>(guest)
        };
        var controller = new GuestsController(commandService, new FakeGuestQueryService());

        var result = await controller.Activate(guest.Id.Value);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<GuestProfileResource>();
    }
}
