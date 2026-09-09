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

public class StaffControllerTests
{
    private class FakeStaffCommandService : IStaffProfileCommandService
    {
        public Func<CreateStaffProfileCommand, Task<StaffProfile?>>? CreateHandler { get; set; }
        public Func<AddStaffAssignmentCommand, Task<StaffProfile?>>? AddAssignmentHandler { get; set; }
        public Func<TerminateStaffAssignmentCommand, Task<StaffProfile?>>? TerminateAssignmentHandler { get; set; }
        public Func<SuspendStaffAssignmentCommand, Task<StaffProfile?>>? SuspendAssignmentHandler { get; set; }
        public Func<ReactivateStaffAssignmentCommand, Task<StaffProfile?>>? ReactivateAssignmentHandler { get; set; }
        public Func<ChangeStaffLegalNameCommand, Task<StaffProfile?>>? ChangeNameHandler { get; set; }
        public Func<UpdateStaffPersonalContactCommand, Task<StaffProfile?>>? UpdateContactHandler { get; set; }
        public Func<ChangeStaffJobPositionCommand, Task<StaffProfile?>>? ChangePositionHandler { get; set; }
        public Func<ChangeStaffHabitualShiftCommand, Task<StaffProfile?>>? ChangeShiftHandler { get; set; }
        public Func<UpdateStaffIdentificationCommand, Task<StaffProfile?>>? UpdateIdentHandler { get; set; }
        public Func<DeactivateStaffProfileCommand, Task<StaffProfile?>>? DeactivateHandler { get; set; }
        public Func<ActivateStaffProfileCommand, Task<StaffProfile?>>? ActivateHandler { get; set; }

        public Task<StaffProfile?> Handle(CreateStaffProfileCommand command) =>
            CreateHandler != null ? CreateHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(AddStaffAssignmentCommand command) =>
            AddAssignmentHandler != null ? AddAssignmentHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(TerminateStaffAssignmentCommand command) =>
            TerminateAssignmentHandler != null ? TerminateAssignmentHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(SuspendStaffAssignmentCommand command) =>
            SuspendAssignmentHandler != null ? SuspendAssignmentHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(ReactivateStaffAssignmentCommand command) =>
            ReactivateAssignmentHandler != null ? ReactivateAssignmentHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(ChangeStaffLegalNameCommand command) =>
            ChangeNameHandler != null ? ChangeNameHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(UpdateStaffPersonalContactCommand command) =>
            UpdateContactHandler != null ? UpdateContactHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(ChangeStaffJobPositionCommand command) =>
            ChangePositionHandler != null ? ChangePositionHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(ChangeStaffHabitualShiftCommand command) =>
            ChangeShiftHandler != null ? ChangeShiftHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(UpdateStaffIdentificationCommand command) =>
            UpdateIdentHandler != null ? UpdateIdentHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(DeactivateStaffProfileCommand command) =>
            DeactivateHandler != null ? DeactivateHandler(command) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(ActivateStaffProfileCommand command) =>
            ActivateHandler != null ? ActivateHandler(command) : Task.FromResult<StaffProfile?>(null);
    }

    private class FakeStaffQueryService : IStaffProfileQueryService
    {
        public Func<GetStaffProfileByIdQuery, Task<StaffProfile?>>? GetByIdHandler { get; set; }
        public Func<GetStaffProfileByUserIdQuery, Task<StaffProfile?>>? GetByUserIdHandler { get; set; }
        public Func<GetStaffProfileByEmployeeCodeQuery, Task<StaffProfile?>>? GetByCodeHandler { get; set; }
        public Func<GetStaffProfilesByHotelIdQuery, Task<IEnumerable<StaffProfile>>>? GetByHotelHandler { get; set; }
        public Func<GetAllStaffProfilesQuery, Task<IEnumerable<StaffProfile>>>? GetAllHandler { get; set; }

        public Task<StaffProfile?> Handle(GetStaffProfileByIdQuery query) =>
            GetByIdHandler != null ? GetByIdHandler(query) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(GetStaffProfileByUserIdQuery query) =>
            GetByUserIdHandler != null ? GetByUserIdHandler(query) : Task.FromResult<StaffProfile?>(null);

        public Task<StaffProfile?> Handle(GetStaffProfileByEmployeeCodeQuery query) =>
            GetByCodeHandler != null ? GetByCodeHandler(query) : Task.FromResult<StaffProfile?>(null);

        public Task<IEnumerable<StaffProfile>> Handle(GetStaffProfilesByHotelIdQuery query) =>
            GetByHotelHandler != null ? GetByHotelHandler(query) : Task.FromResult<IEnumerable<StaffProfile>>(new List<StaffProfile>());

        public Task<IEnumerable<StaffProfile>> Handle(GetAllStaffProfilesQuery query) =>
            GetAllHandler != null ? GetAllHandler(query) : Task.FromResult<IEnumerable<StaffProfile>>(new List<StaffProfile>());
    }

    private static StaffProfile CreateSampleStaff(StaffProfileId? id = null, EmployeeCode? code = null)
    {
        return new StaffProfile(
            id ?? StaffProfileId.New(),
            new UserId(1),
            code ?? new EmployeeCode("EMP-00001"),
            new PersonName("Jane", "Smith"),
            new EmailAddress("jane.smith@smartstay.com"),
            new JobPosition("Manager"),
            HabitualShift.Morning,
            new PhoneNumber("+1987654321"));
    }

    [Fact]
    public async Task Create_ValidRequest_ShouldReturnCreatedAtAction_WithResource()
    {
        var staff = CreateSampleStaff();
        var commandService = new FakeStaffCommandService
        {
            CreateHandler = _ => Task.FromResult<StaffProfile?>(staff)
        };
        var queryService = new FakeStaffQueryService();
        var controller = new StaffController(commandService, queryService);

        var request = new CreateStaffProfileResource(
            1, "Jane", "Smith", "jane.smith@smartstay.com",
            "Manager", HabitualShift.Morning, "+1987654321", null, null, null, null, null, null, null);

        var result = await controller.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(StaffController.GetById));
        var resource = createdResult.Value.Should().BeOfType<StaffProfileResource>().Subject;
        resource.Code.Should().Be("EMP-00001");
    }

    [Fact]
    public async Task Create_WhenCommandFails_ShouldReturnBadRequest()
    {
        var commandService = new FakeStaffCommandService
        {
            CreateHandler = _ => Task.FromResult<StaffProfile?>(null)
        };
        var controller = new StaffController(commandService, new FakeStaffQueryService());

        var request = new CreateStaffProfileResource(
            1, "Jane", "Smith", "jane.smith@smartstay.com",
            "Manager", HabitualShift.Morning, null, null, null, null, null, null, null, null);

        var result = await controller.Create(request);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task GetById_ExistingId_ShouldReturnOk_WithResource()
    {
        var staffId = StaffProfileId.New();
        var staff = CreateSampleStaff(staffId);
        var queryService = new FakeStaffQueryService
        {
            GetByIdHandler = q => Task.FromResult<StaffProfile?>(q.ProfileId == staffId ? staff : null)
        };
        var controller = new StaffController(new FakeStaffCommandService(), queryService);

        var result = await controller.GetById(staffId.Value);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resource = okResult.Value.Should().BeOfType<StaffProfileResource>().Subject;
        resource.Id.Should().Be(staffId.Value);
        resource.Position.Should().Be("Manager");
    }

    [Fact]
    public async Task GetById_NonExistingId_ShouldReturnNotFound()
    {
        var queryService = new FakeStaffQueryService
        {
            GetByIdHandler = _ => Task.FromResult<StaffProfile?>(null)
        };
        var controller = new StaffController(new FakeStaffCommandService(), queryService);

        var result = await controller.GetById(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByCode_ExistingCode_ShouldReturnOk()
    {
        var staff = CreateSampleStaff(code: new EmployeeCode("EMP-00042"));
        var queryService = new FakeStaffQueryService
        {
            GetByCodeHandler = _ => Task.FromResult<StaffProfile?>(staff)
        };
        var controller = new StaffController(new FakeStaffCommandService(), queryService);

        var result = await controller.GetByCode("EMP-00042");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resource = okResult.Value.Should().BeOfType<StaffProfileResource>().Subject;
        resource.Code.Should().Be("EMP-00042");
    }

    [Fact]
    public async Task GetByUserId_ExistingUserId_ShouldReturnOk()
    {
        var staff = CreateSampleStaff();
        var queryService = new FakeStaffQueryService
        {
            GetByUserIdHandler = _ => Task.FromResult<StaffProfile?>(staff)
        };
        var controller = new StaffController(new FakeStaffCommandService(), queryService);

        var result = await controller.GetByUserId(1);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<StaffProfileResource>();
    }

    [Fact]
    public async Task GetByHotel_ShouldReturnOk_WithList()
    {
        var staff1 = CreateSampleStaff();
        var queryService = new FakeStaffQueryService
        {
            GetByHotelHandler = _ => Task.FromResult<IEnumerable<StaffProfile>>(new[] { staff1 })
        };
        var controller = new StaffController(new FakeStaffCommandService(), queryService);

        var result = await controller.GetByHotel(101);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = okResult.Value.Should().BeAssignableTo<IEnumerable<StaffProfileResource>>().Subject;
        list.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithList()
    {
        var staff1 = CreateSampleStaff();
        var staff2 = CreateSampleStaff();
        var queryService = new FakeStaffQueryService
        {
            GetAllHandler = _ => Task.FromResult<IEnumerable<StaffProfile>>(new[] { staff1, staff2 })
        };
        var controller = new StaffController(new FakeStaffCommandService(), queryService);

        var result = await controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var list = okResult.Value.Should().BeAssignableTo<IEnumerable<StaffProfileResource>>().Subject;
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddAssignment_ValidRequest_ShouldReturnOk()
    {
        var staff = CreateSampleStaff();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        staff.AddAssignment(ScopeLevel.Hotel, new TargetId(101), StaffRole.Admin, new DateRange(today, today.AddMonths(3)), today);

        var commandService = new FakeStaffCommandService
        {
            AddAssignmentHandler = _ => Task.FromResult<StaffProfile?>(staff)
        };
        var controller = new StaffController(commandService, new FakeStaffQueryService());

        var request = new AddStaffAssignmentResource(ScopeLevel.Hotel, 101, StaffRole.Admin, today, today.AddMonths(3));
        var result = await controller.AddAssignment(staff.Id.Value, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var resource = okResult.Value.Should().BeOfType<StaffProfileResource>().Subject;
        resource.Assignments.Should().HaveCount(1);
    }

    [Fact]
    public async Task TerminateAssignment_ValidRequest_ShouldReturnOk()
    {
        var staff = CreateSampleStaff();
        var commandService = new FakeStaffCommandService
        {
            TerminateAssignmentHandler = _ => Task.FromResult<StaffProfile?>(staff)
        };
        var controller = new StaffController(commandService, new FakeStaffQueryService());

        var request = new TerminateStaffAssignmentResource(DateOnly.FromDateTime(DateTime.UtcNow));
        var result = await controller.TerminateAssignment(staff.Id.Value, Guid.NewGuid(), request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<StaffProfileResource>();
    }

    [Fact]
    public async Task ChangeLegalName_ValidRequest_ShouldReturnOk()
    {
        var staff = CreateSampleStaff();
        var commandService = new FakeStaffCommandService
        {
            ChangeNameHandler = _ => Task.FromResult<StaffProfile?>(staff)
        };
        var controller = new StaffController(commandService, new FakeStaffQueryService());

        var request = new ChangeStaffLegalNameResource("NewFirst", "NewLast");
        var result = await controller.ChangeLegalName(staff.Id.Value, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<StaffProfileResource>();
    }

    [Fact]
    public async Task ChangeJobPosition_ValidRequest_ShouldReturnOk()
    {
        var staff = CreateSampleStaff();
        var commandService = new FakeStaffCommandService
        {
            ChangePositionHandler = _ => Task.FromResult<StaffProfile?>(staff)
        };
        var controller = new StaffController(commandService, new FakeStaffQueryService());

        var request = new ChangeStaffJobPositionResource("General Manager");
        var result = await controller.ChangeJobPosition(staff.Id.Value, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<StaffProfileResource>();
    }

    [Fact]
    public async Task Deactivate_ValidRequest_ShouldReturnOk()
    {
        var staff = CreateSampleStaff();
        var commandService = new FakeStaffCommandService
        {
            DeactivateHandler = _ => Task.FromResult<StaffProfile?>(staff)
        };
        var controller = new StaffController(commandService, new FakeStaffQueryService());

        var result = await controller.Deactivate(staff.Id.Value);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<StaffProfileResource>();
    }
}
