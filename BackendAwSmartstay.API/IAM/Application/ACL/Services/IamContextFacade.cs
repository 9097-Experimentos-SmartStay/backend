using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.Enums;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.IAM.Domain.Model.Queries;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.API.IAM.Interfaces.ACL;

namespace BackendAwSmartstay.API.IAM.Application.ACL.Services;

/// <summary>
/// Represents the Anti-Corruption Layer (ACL) facade for the Identity and Access Management (IAM) bounded context.
/// It orchestrates commands and queries to expose safe identity services to external bounded contexts.
/// </summary>
public class IamContextFacade(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    IUserRepository userRepository) : IIamContextFacade
{
    public async Task<UserContact?> FetchUserContactAsync(int userId)
    {
        var user = await userRepository.FindByIdAsync(userId);
        return user is null || user.Status == UserStatus.Inactive ? null : ToContact(user);
    }

    public async Task<IReadOnlyList<UserContact>> ListHotelStaffAsync(int hotelId, IReadOnlyCollection<string> roles)
    {
        var users = await userRepository.ListActiveByRolesAsync(roles.Select(role => new Role(role)).ToList());
        // Chain administrators operate every hotel; the others only the hotel they are assigned to.
        return users.Where(user => user.HotelId == hotelId || user.Role.Value == UserRoles.ChainAdmin)
            .Select(ToContact).ToList();
    }

    private static UserContact ToContact(User user) => new(user.Id, user.Email.Value,
        string.IsNullOrWhiteSpace(user.FirstName) ? null : $"{user.FirstName} {user.LastName}".Trim(), user.Role.Value);

    /// <summary>
    /// Retrieves the unique identifier of a user resource based on their email.
    /// </summary>
    /// <param name="email">The email of the resource to find.</param>
    /// <returns>The unique identifier (<c>Id</c>) of the user resource, or <c>0</c> if not found.</returns>
    public async Task<int> FetchUserIdByEmail(string email)
    {
        var getUserByEmailQuery = new GetUserByEmailQuery(email);
        var result = await userQueryService.Handle(getUserByEmailQuery);
        return result?.Id ?? 0;
    }

    /// <summary>
    /// Retrieves the email representation of a user resource based on their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user resource.</param>
    /// <returns>The email string belonging to the resource, or an empty string if the resource does not exist.</returns>
    public async Task<string> FetchEmailByUserId(int userId)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        var result = await userQueryService.Handle(getUserByIdQuery);
        return result?.Email.Value ?? string.Empty;
    }

    public async Task<ReissuedSession?> AssignHotelToAdministratorAsync(int userId, int hotelId, SessionContext currentSession)
    {
        var session = await userCommandService.Handle(
            new AssignHotelToAdministratorCommand(userId, hotelId, currentSession.RememberedSessionId));
        return session is { AccessToken: { } token, AccessTokenExpiresAt: { } expiresAt }
            ? new ReissuedSession(token, expiresAt, session.RefreshToken?.Value, session.RefreshToken?.ExpiresAt)
            : null;
    }
}