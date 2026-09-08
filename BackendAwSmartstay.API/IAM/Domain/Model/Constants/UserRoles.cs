namespace BackendAwSmartstay.API.IAM.Domain.Model.Constants;

/// <summary>
///     Defines valid user roles in the system.
/// </summary>
public static class UserRoles
{
    /// <summary>
    ///     Guest role - default role for new users/travelers.
    /// </summary>
    public const string Guest = "guest";

    /// <summary>
    ///     Admin role - administrators with full system access to a specific hotel.
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    ///     Chain Admin role - administrators managing multiple hotel properties.
    /// </summary>
    public const string ChainAdmin = "chain_admin";

    /// <summary>
    ///     General staff role - generic staff member.
    /// </summary>
    public const string Staff = "staff";

    /// <summary>
    ///     Reception staff role - handles assisted check-in/check-out and reception desks.
    /// </summary>
    public const string Reception = "reception";

    /// <summary>
    ///     Housekeeping staff role - manages room cleaning and room readiness.
    /// </summary>
    public const string Housekeeping = "housekeeping";

    /// <summary>
    ///     Maintenance staff role - tracks technical incidences and IoT predictive maintenance.
    /// </summary>
    public const string Maintenance = "maintenance";
}