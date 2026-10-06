using BackendAwSmartstay.API.IAM.Domain.Model.Aggregates;
using BackendAwSmartstay.API.IAM.Domain.Repositories;
using BackendAwSmartstay.API.Shared.Domain.Repositories;

namespace BackendAwSmartstay.API.IAM.Application.Internal.CommandServices;

/// <summary>The signed-in user changes or removes their own profile picture.</summary>
public class UserAvatarCommandService(
    IUserAvatarRepository avatarRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    /// <summary>The user's picture, or null when they have none.</summary>
    public Task<UserAvatar?> FindAsync(int userId) => avatarRepository.FindByIdAsync(userId);

    /// <summary>Sets (or replaces) the user's picture.</summary>
    /// <exception cref="BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions.InvalidFieldException">
    ///     Empty, too large, or not a JPEG, PNG or WebP image (400 on <c>file</c>).
    /// </exception>
    public async Task<UserAvatar> ChangeAsync(int userId, byte[] content)
    {
        var now = timeProvider.GetUtcNow();
        var avatar = await avatarRepository.FindByIdAsync(userId);
        if (avatar is null)
        {
            avatar = UserAvatar.Upload(userId, content, now);
            await avatarRepository.AddAsync(avatar);
        }
        else
        {
            avatar.Replace(content, now);
        }

        await unitOfWork.CompleteAsync();
        return avatar;
    }

    /// <summary>Removes the user's picture (nothing happens when they have none).</summary>
    public async Task RemoveAsync(int userId)
    {
        var avatar = await avatarRepository.FindByIdAsync(userId);
        if (avatar is null) return;
        avatarRepository.Remove(avatar);
        await unitOfWork.CompleteAsync();
    }
}
