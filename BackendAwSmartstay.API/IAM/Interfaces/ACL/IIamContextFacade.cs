namespace BackendAwSmartstay.API.IAM.Interfaces.ACL;

public interface IIamContextFacade
{
    Task<int> CreateUser(string email, string password);
    Task<int> FetchUserIdByEmail(string email);
    Task<string> FetchEmailByUserId(int userId);
}