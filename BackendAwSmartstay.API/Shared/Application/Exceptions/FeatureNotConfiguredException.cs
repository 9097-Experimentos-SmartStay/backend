using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Shared.Application.Exceptions;

/// <summary>
///     An optional feature cannot run because this deployment lacks its configuration (e.g. the credentials of a
///     third-party service). The API answers 503 with the feature's own code: the request is valid and may work
///     once the operator configures it.
/// </summary>
public class FeatureNotConfiguredException(string code, string message) : DomainException(code, message);
