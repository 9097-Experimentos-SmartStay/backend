using BackendAwSmartstay.API.Shared.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Implementation of the unit of work pattern using Entity Framework Core.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    /// <inheritdoc/>
    public async Task CompleteAsync()
    {
        await context.SaveChangesAsync();
    }
}
