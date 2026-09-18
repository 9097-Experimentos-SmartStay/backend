using BackendAwSmartstay.API.Profiles.Application.Internal.Commands;
using BackendAwSmartstay.API.Profiles.Application.Internal.CommandServices;
using BackendAwSmartstay.API.Profiles.Application.Internal.Queries;
using BackendAwSmartstay.API.Profiles.Application.Internal.QueryServices;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Resources;
using BackendAwSmartstay.API.Profiles.Interfaces.REST.Transform;
using BackendAwSmartstay.Domain.Profiles.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Interfaces.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Profiles.Interfaces.REST;

/// <summary>
/// Inbound REST adapter for Staff Profiles and Assignments operations.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[SwaggerTag("Available Staff Profiles Endpoints.")]
public class StaffController(
    IStaffProfileCommandService staffCommandService,
    IStaffProfileQueryService staffQueryService)
    : ControllerBase
{
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Get staff profile by ID", OperationId = "GetStaffProfileById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Staff profile found.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetStaffProfileByIdQuery(new StaffProfileId(id));
        var staff = await staffQueryService.Handle(query);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpGet("code/{code}")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Get staff profile by employee code", OperationId = "GetStaffProfileByCode")]
    [SwaggerResponse(StatusCodes.Status200OK, "Staff profile found.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var query = new GetStaffProfileByEmployeeCodeQuery(new EmployeeCode(code));
        var staff = await staffQueryService.Handle(query);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpGet("user/{userId:int}")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Get staff profile by IAM User ID", OperationId = "GetStaffProfileByUserId")]
    [SwaggerResponse(StatusCodes.Status200OK, "Staff profile found.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var query = new GetStaffProfileByUserIdQuery(new UserId(userId));
        var staff = await staffQueryService.Handle(query);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpGet("hotel/{hotelId:int}")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Get staff profiles assigned to a hotel", OperationId = "GetStaffProfilesByHotel")]
    [SwaggerResponse(StatusCodes.Status200OK, "Staff profiles retrieved.", typeof(IEnumerable<StaffProfileResource>))]
    public async Task<IActionResult> GetByHotel(int hotelId)
    {
        var query = new GetStaffProfilesByHotelIdQuery(new TargetId(hotelId));
        var list = await staffQueryService.Handle(query);
        return Ok(list.Select(StaffResourceAssembler.ToResourceFromEntity));
    }

    [HttpGet]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Get all staff profiles", OperationId = "GetAllStaffProfiles")]
    [SwaggerResponse(StatusCodes.Status200OK, "Staff profiles retrieved.", typeof(IEnumerable<StaffProfileResource>))]
    public async Task<IActionResult> GetAll()
    {
        var list = await staffQueryService.Handle(new GetAllStaffProfilesQuery());
        return Ok(list.Select(StaffResourceAssembler.ToResourceFromEntity));
    }

    [HttpPost]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Create a new staff profile", OperationId = "CreateStaffProfile")]
    [SwaggerResponse(StatusCodes.Status201Created, "Staff profile created successfully.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input data.")]
    public async Task<IActionResult> Create([FromBody] CreateStaffProfileResource resource)
    {
        var command = StaffResourceAssembler.ToCommandFromResource(resource);
        var staff = await staffCommandService.Handle(command);
        if (staff is null) return BadRequest();
        var response = StaffResourceAssembler.ToResourceFromEntity(staff);
        return CreatedAtAction(nameof(GetById), new { id = staff.Id.Value }, response);
    }

    [HttpPost("{id:guid}/assignments")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Add an assignment to a staff profile", OperationId = "AddStaffAssignment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignment added successfully.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> AddAssignment(Guid id, [FromBody] AddStaffAssignmentResource resource)
    {
        var command = new AddStaffAssignmentCommand(
            new StaffProfileId(id),
            resource.Scope,
            new TargetId(resource.TargetId),
            resource.Role,
            new DateRange(resource.StartDate, resource.EndDate),
            DateOnly.FromDateTime(DateTime.UtcNow));

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPost("{id:guid}/assignments/{assignmentId:guid}/terminate")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Terminate a staff assignment", OperationId = "TerminateStaffAssignment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignment terminated successfully.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile or assignment not found.")]
    public async Task<IActionResult> TerminateAssignment(Guid id, Guid assignmentId, [FromBody] TerminateStaffAssignmentResource resource)
    {
        var command = new TerminateStaffAssignmentCommand(
            new StaffProfileId(id),
            new AssignmentId(assignmentId),
            resource.TerminationDate);

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPost("{id:guid}/assignments/{assignmentId:guid}/suspend")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Suspend a staff assignment", OperationId = "SuspendStaffAssignment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignment suspended successfully.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile or assignment not found.")]
    public async Task<IActionResult> SuspendAssignment(Guid id, Guid assignmentId)
    {
        var command = new SuspendStaffAssignmentCommand(
            new StaffProfileId(id),
            new AssignmentId(assignmentId));

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPost("{id:guid}/assignments/{assignmentId:guid}/reactivate")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Reactivate a staff assignment", OperationId = "ReactivateStaffAssignment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Assignment reactivated successfully.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile or assignment not found.")]
    public async Task<IActionResult> ReactivateAssignment(Guid id, Guid assignmentId)
    {
        var command = new ReactivateStaffAssignmentCommand(
            new StaffProfileId(id),
            new AssignmentId(assignmentId),
            DateOnly.FromDateTime(DateTime.UtcNow));

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPut("{id:guid}/legal-name")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Change staff legal name", OperationId = "ChangeStaffLegalName")]
    [SwaggerResponse(StatusCodes.Status200OK, "Legal name updated successfully.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> ChangeLegalName(Guid id, [FromBody] ChangeStaffLegalNameResource resource)
    {
        var command = new ChangeStaffLegalNameCommand(
            new StaffProfileId(id),
            new PersonName(resource.FirstName, resource.LastName));

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPut("{id:guid}/personal-contact")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Update staff personal contact information", OperationId = "UpdateStaffPersonalContact")]
    [SwaggerResponse(StatusCodes.Status200OK, "Personal contact information updated.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> UpdatePersonalContact(Guid id, [FromBody] UpdateStaffPersonalContactResource resource)
    {
        var address = resource.Street != null && resource.Number != null && resource.City != null && resource.PostalCode != null && resource.Country != null
            ? new StreetAddress(resource.Street, resource.Number, resource.City, resource.PostalCode, resource.Country)
            : null;

        var command = new UpdateStaffPersonalContactCommand(
            new StaffProfileId(id),
            resource.Phone != null ? new PhoneNumber(resource.Phone) : null,
            address);

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPut("{id:guid}/job-position")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Change staff job position", OperationId = "ChangeStaffJobPosition")]
    [SwaggerResponse(StatusCodes.Status200OK, "Job position updated.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> ChangeJobPosition(Guid id, [FromBody] ChangeStaffJobPositionResource resource)
    {
        var command = new ChangeStaffJobPositionCommand(
            new StaffProfileId(id),
            new JobPosition(resource.Position));

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPut("{id:guid}/habitual-shift")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Change staff habitual shift", OperationId = "ChangeStaffHabitualShift")]
    [SwaggerResponse(StatusCodes.Status200OK, "Habitual shift updated.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> ChangeHabitualShift(Guid id, [FromBody] ChangeStaffHabitualShiftResource resource)
    {
        var command = new ChangeStaffHabitualShiftCommand(
            new StaffProfileId(id),
            resource.Shift);

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPut("{id:guid}/identification")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Update staff identification document", OperationId = "UpdateStaffIdentification")]
    [SwaggerResponse(StatusCodes.Status200OK, "Identification document updated.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> UpdateIdentification(Guid id, [FromBody] UpdateStaffIdentificationResource resource)
    {
        var command = new UpdateStaffIdentificationCommand(
            new StaffProfileId(id),
            new IdentificationDocument(resource.DocumentType, resource.DocumentNumber));

        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Deactivate staff profile", OperationId = "DeactivateStaffProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "Staff profile deactivated.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var command = new DeactivateStaffProfileCommand(new StaffProfileId(id));
        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = Policies.ManageStaff)]
    [SwaggerOperation(Summary = "Activate staff profile", OperationId = "ActivateStaffProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "Staff profile activated.", typeof(StaffProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Staff profile not found.")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var command = new ActivateStaffProfileCommand(new StaffProfileId(id));
        var staff = await staffCommandService.Handle(command);
        if (staff is null) return NotFound();
        return Ok(StaffResourceAssembler.ToResourceFromEntity(staff));
    }
}
