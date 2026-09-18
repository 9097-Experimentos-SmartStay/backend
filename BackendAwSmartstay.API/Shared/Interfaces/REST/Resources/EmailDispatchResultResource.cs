namespace BackendAwSmartstay.API.Shared.Interfaces.REST.Resources;

/// <summary>Outcome of a run of the e-mail dispatch job.</summary>
/// <param name="Sent">E-mails accepted by the mail provider in this run.</param>
/// <param name="Retrying">E-mails that failed transiently and were scheduled for another attempt.</param>
/// <param name="Failed">E-mails given up in this run (permanent rejection or last attempt).</param>
public record EmailDispatchResultResource(int Sent, int Retrying, int Failed);
