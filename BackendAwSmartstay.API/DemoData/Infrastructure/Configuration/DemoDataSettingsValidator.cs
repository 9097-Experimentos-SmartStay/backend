using BackendAwSmartstay.API.Accommodations.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Model.Constants;
using BackendAwSmartstay.API.IAM.Domain.Model.ValueObjects;
using BackendAwSmartstay.API.IAM.Domain.Services;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.DemoData.Infrastructure.Configuration;

/// <summary>
///     Validates <see cref="DemoDataSettings"/> when the host starts (<c>ValidateOnStart</c>), only when the demo data
///     is enabled:
///     <list type="bullet">
///         <item><c>DefaultPassword</c> is required and must pass the domain password policy for a guest (at least 15
///         characters, not common nor repetitive), which also satisfies the staff minimum;</item>
///         <item><c>EmailBase</c> must be a valid e-mail without its own <c>+</c> tag (the aliases add one);</item>
///         <item><c>Hotel1Payment</c>, when given, must be valid hotel payment settings (same value object as the
///         app's form), so a typo stops the startup instead of seeding unusable payment data.</item>
///     </list>
/// </summary>
public class DemoDataSettingsValidator : IValidateOptions<DemoDataSettings>
{
    public ValidateOptionsResult Validate(string? name, DemoDataSettings settings)
    {
        if (!settings.Enabled) return ValidateOptionsResult.Success;

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.DefaultPassword))
        {
            failures.Add("DemoData:DefaultPassword is required when DemoData:Enabled is true (set 'DemoData__DefaultPassword' as a secret).");
        }
        else
        {
            var check = PasswordPolicy.Check(settings.DefaultPassword, new Role(UserRoles.Guest), email: null);
            if (!check.IsAcceptable)
                failures.Add($"DemoData:DefaultPassword does not satisfy the password policy of the seeded accounts " +
                             $"(guests need at least {PasswordPolicy.GuestMinimumLength} characters): {check.Problem}");
        }

        if (!Email.IsValid(settings.EmailBase) || settings.EmailBase.Contains('+'))
            failures.Add("DemoData:EmailBase must be a valid e-mail address without a '+' tag (e.g. 'name@gmail.com'): " +
                         "the demo accounts are its plus-addressing aliases (name+admin1@gmail.com).");

        var payment = settings.Hotel1Payment;
        if (payment.IsProvided)
        {
            try
            {
                HotelPaymentSettings.Create(payment.AccountHolder, payment.Yape, payment.Plin, payment.BankName,
                    payment.BankAccountNumber, payment.BankAccountCci);
            }
            catch (InvalidFieldsException exception)
            {
                failures.AddRange(exception.Violations.Select(violation =>
                    $"DemoData:Hotel1Payment ({violation.Field}): {violation.Message}"));
            }
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
