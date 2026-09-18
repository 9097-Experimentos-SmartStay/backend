using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;
using BackendAwSmartstay.API.Shared.Infrastructure.Email.Configuration;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Transport;

/// <summary>
///     Delivers e-mails through the Brevo transactional e-mail API (<c>POST /v3/smtp/email</c>, header
///     <c>api-key</c>) over HTTPS (<c>Email:Transport=BrevoApi</c>). Used in production: port 443 always leaves the
///     hosting provider, unlike the SMTP ports (connection timeouts to <c>smtp-relay.brevo.com:587</c>).
/// </summary>
/// <remarks>
///     The typed <see cref="HttpClient"/> comes from <c>IHttpClientFactory</c> with a resilience pipeline (see
///     <c>EmailServiceCollectionExtensions</c>). Outcome of a request: 2xx = accepted; 4xx other than 408/429 =
///     permanent (<see cref="EmailDeliveryException.IsPermanent"/>: invalid sender or recipient, bad API key...);
///     408, 429, 5xx, timeouts and connection errors = transient (the outbox retries later). Neither the API key nor
///     the body of the e-mail is ever logged.
/// </remarks>
public class BrevoApiEmailTransport(HttpClient httpClient, IOptions<EmailSettings> options) : IEmailTransport
{
    /// <summary>Header that carries the API key.</summary>
    public const string ApiKeyHeader = "api-key";

    /// <summary>Endpoint of the transactional e-mails, relative to <c>Email:Brevo:BaseUrl</c>.</summary>
    public const string SendEmailPath = "v3/smtp/email";

    private const int MaxErrorLength = 300;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var sender = options.Value.From;
        var request = new SendEmailRequest(
            new Contact(sender.Address!, string.IsNullOrWhiteSpace(sender.Name) ? null : sender.Name),
            [new Contact(message.To, null)],
            message.Subject,
            message.HtmlBody,
            message.TextBody);

        // Serialized up front (not streamed): the request carries a Content-Length instead of a chunked body.
        using var content = new StringContent(JsonSerializer.Serialize(request, Json), Encoding.UTF8, "application/json");
        using var response = await httpClient.PostAsync(SendEmailPath, content, cancellationToken);
        if (response.IsSuccessStatusCode) return;

        var status = (int)response.StatusCode;
        var permanent = status is >= 400 and < 500
                        && response.StatusCode is not (HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests);
        throw new EmailDeliveryException($"Brevo API answered {status}{await DescribeErrorAsync(response, cancellationToken)}.", permanent);
    }

    /// <summary>The <c>code</c> and <c>message</c> of a Brevo error body (never echoes the request), truncated.</summary>
    private static async Task<string> DescribeErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(Json, cancellationToken);
            if (error is null) return string.Empty;
            var text = $": {error.Code} {error.Message}".TrimEnd();
            return text.Length > MaxErrorLength ? text[..MaxErrorLength] : text;
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException or InvalidOperationException)
        {
            return string.Empty; // Not a JSON error body (e.g. an HTML page of a proxy).
        }
    }

    // Request and error shapes of https://developers.brevo.com/reference/sendtransacemail
    private sealed record SendEmailRequest(Contact Sender, IReadOnlyList<Contact> To, string Subject, string HtmlContent, string TextContent);

    private sealed record Contact(string Email, string? Name);

    private sealed record ErrorResponse(string? Code, string? Message);
}
