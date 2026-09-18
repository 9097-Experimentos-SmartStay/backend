using BackendAwSmartstay.API.IAM.Domain.Model.Commands;

namespace BackendAwSmartstay.API.IAM.Domain.Services;

/// <summary>The data an authenticator app needs: the Base32 secret and the <c>otpauth://</c> URI of the QR code.</summary>
public sealed record MfaEnrollment(string Secret, string OtpAuthUri, string Issuer, string AccountName, int Digits, int PeriodSeconds, string Algorithm);

/// <summary>Two-factor authentication use cases (US-52) and session termination.</summary>
public interface IMfaCommandService
{
    Task<MfaEnrollment> Handle(StartMfaEnrollmentCommand command);

    Task<AuthenticationResult> Handle(ConfirmMfaEnrollmentCommand command);

    Task<AuthenticationResult> Handle(VerifyMfaCommand command);

    Task Handle(ResetMfaCommand command);

    Task Handle(SignOutEverywhereCommand command);
}
