using System.Net.Mime;
using BackendAwSmartstay.API.Accommodations.Interfaces.REST.Resources;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendAwSmartstay.API.Accommodations.Interfaces.REST;

/// <summary>
///     RESTful API interface controller responsible for handling master data options, catalogs,
///     and configuration rules related to hotel categories and property amenities within the accommodation bounded context.
/// </summary>
/// <remarks>
///     This controller serves as a master data gateway. While reading catalog information is open to all validated profiles,
///     mutating the global catalog state changes constraints globally and is restricted strictly to multi-property corporate managers.
/// </remarks>
[Authorize]
[ApiController]
[Route("api/v1/accommodations/options")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Accommodation Options (Master Data)")]
public class AccommodationOptionsController(AppDbContext context) : ControllerBase
{
    /// <summary>
    ///     Retrieves all registered hotel category options present in the master catalog.
    /// </summary>
    /// <returns>An asynchronous action result containing an enumerable collection of verified category definitions.</returns>
    [HttpGet("categories")]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get hotel categories catalogue",
        Description = "Retrieves a read-only list of available hotel category state partitions. Open to all actors.",
        OperationId = "GetCategories")]
    [SwaggerResponse(StatusCodes.Status200OK, "The hotel category options list was successfully retrieved.", typeof(IEnumerable<string>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await context.Set<Domain.Model.Entities.HotelCategory>().ToListAsync();
        return Ok(categories.Select(category => category.Name));
    }

    /// <summary>
    ///     Retrieves all globally tracked accommodation amenity specifications.
    /// </summary>
    /// <returns>An asynchronous action result containing an enumerable view layout of available amenity fields.</returns>
    [HttpGet("amenities")]
    [Authorize(UserRoles.Guest, UserRoles.Admin, UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Get available room and hotel amenities",
        Description = "Retrieves a read-only collection of all features and standard amenities recognized by the system.",
        OperationId = "GetAmenities")]
    [SwaggerResponse(StatusCodes.Status200OK, "The master amenity options list was successfully retrieved.", typeof(IEnumerable<string>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "The authenticated identity has insufficient privilege levels.")]
    public async Task<IActionResult> GetAmenities()
    {
        var amenities = await context.Set<Domain.Model.Entities.Amenity>().ToListAsync();
        return Ok(amenities.Select(amenity => amenity.Name));
    }

    /// <summary>
    ///     Registers a new unique hotel category node inside the global structural catalog partition.
    /// </summary>
    /// <param name="resource">The incoming configuration resource representation containing parameters for target generation.</param>
    /// <returns>A confirmation outcome showing the tracking state of the newly appended category resource metadata.</returns>
    [HttpPost("categories")]
    [Authorize(UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Create a new hotel category definition entry",
        Description = "Appends a new structural entry to the shared hotel type directory. Restricted strictly to ChainAdmin operators.",
        OperationId = "CreateCategory")]
    [SwaggerResponse(StatusCodes.Status201Created, "The global category option was processed, initialized, and synchronized.", typeof(string))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Only corporate accounts with multi-property validation depth may append to global options.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "A category node with identical descriptive fields is already registered within the data store.")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryResource resource)
    {
        var exists = await context.Set<Domain.Model.Entities.HotelCategory>()
            .AnyAsync(category => category.Name == resource.Name);
            
        if (exists) return Conflict($"Category '{resource.Name}' already exists within the system constraints.");
        
        var category = new Domain.Model.Entities.HotelCategory { Name = resource.Name };
        
        context.Set<Domain.Model.Entities.HotelCategory>().Add(category);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { }, category.Name);
    }

    /// <summary>
    ///     Registers a new unique feature amenity variant inside the global structural inventory schema.
    /// </summary>
    /// <param name="resource">The incoming input layout mapping specifications required for amenity catalog expansion.</param>
    /// <returns>A confirmation representation containing the structural name marker of the compiled item.</returns>
    [HttpPost("amenities")]
    [Authorize(UserRoles.ChainAdmin)]
    [SwaggerOperation(
        Summary = "Create a new master amenity option node",
        Description = "Appends a new trackable amenity option to the global definition scheme. Restricted to full multi-property clearance operators.",
        OperationId = "CreateAmenity")]
    [SwaggerResponse(StatusCodes.Status201Created, "The master amenity category item was successfully created and cached.", typeof(string))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "The request lacks a valid identity identification token.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Access denied. Standard hotel admins lack authorization parameters to scale shared catalogs.")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "An identical amenity feature code is already tracking inside the domain persistence mapping tree.")]
    public async Task<IActionResult> CreateAmenity([FromBody] CreateAmenityResource resource)
    {
        var exists = await context.Set<Domain.Model.Entities.Amenity>()
            .AnyAsync(amenity => amenity.Name == resource.Name);
            
        if (exists) return Conflict($"Amenity '{resource.Name}' already exists within the master catalogue schemas.");

        var amenity = new Domain.Model.Entities.Amenity { Name = resource.Name };
        
        context.Set<Domain.Model.Entities.Amenity>().Add(amenity);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAmenities), new { }, amenity.Name);
    }
}