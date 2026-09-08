namespace BackendAwSmartstay.API.Shared.Domain.Repositories;

/// <summary>
/// Represents a unit of work for managing transactions across repositories.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Completes the unit of work asynchronously, committing all changes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CompleteAsync();
}
