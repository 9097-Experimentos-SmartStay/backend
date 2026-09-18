using BackendAwSmartstay.API.IAM.Domain.Model.Commands;
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
    IUserQueryService userQueryService) : IIamContextFacade
{
    /// <summary>
    /// Creates a new user resource within the system using the provided credentials.
    /// </summary>
    /// <param name="email">The unique identifier name for the new user resource.</param>
    /// <param name="password">The plain text password to be securely processed for the new user.</param>
    /// <returns>The unique identifier of the newly created user resource, or <c>0</c> if the creation failed.</returns>
    public async Task<int> CreateUser(string email, string password)
    {
        var signUpCommand = new SignUpCommand(email, password);
        await userCommandService.Handle(signUpCommand);
        var getUserByEmailQuery = new GetUserByEmailQuery(email);
        var result = await userQueryService.Handle(getUserByEmailQuery);
        return result?.Id ?? 0;
    }

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
}