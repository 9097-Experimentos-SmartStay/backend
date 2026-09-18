using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.IAM.Domain.Model.Exceptions;

public class UserNotFoundException : EntityNotFoundException
{
    public UserNotFoundException(int userId)
        : base("User", userId) { }
}
