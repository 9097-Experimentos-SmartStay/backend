using BackendAwSmartstay.API.IAM.Application.OutboundServices;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BCryptNet = BCrypt.Net.BCrypt;

namespace BackendAwSmartstay.API.IAM.Infrastructure.Hashing.BCrypt.Services;

/**
 * <summary>
 *     This class is responsible for hashing and validating passwords.
 * </summary>
 */
public class HashingService : IHashingService
{
    /**
     * <summary>
     *     This method hashes a password.
     * </summary>
     * <param name="password">The password to passwordHash.</param>
     * <returns>The hashed password.</returns>
     */
    public string HashPassword(string password)
    {
        // NIST SP 800-63B-4: passwords are normalized (NFKC) before hashing, so the same text typed on different
        // keyboards or systems verifies.
        return BCryptNet.HashPassword(PasswordPolicy.Normalize(password));
    }

    /**
     * <summary>
     *     This method validates a password against a passwordHash.
     * </summary>
     * <param name="password">The password to validate.</param>
     * <param name="passwordHash">The passwordHash to validate against.</param>
     * <returns>True if the password is valid, false otherwise.</returns>
     */
    public bool VerifyPassword(string password, string passwordHash)
    {
        var normalized = PasswordPolicy.Normalize(password);
        if (BCryptNet.Verify(normalized, passwordHash)) return true;
        // Hashes made before normalization was introduced hold the text as typed.
        return normalized != password && BCryptNet.Verify(password, passwordHash);
    }
}