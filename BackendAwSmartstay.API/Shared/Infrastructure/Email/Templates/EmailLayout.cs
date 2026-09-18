using System.Net;
using System.Text;
using BackendAwSmartstay.API.Shared.Application.OutboundServices;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Email.Templates;

/// <summary>
///     Builds the simple SmartStay e-mail (HTML + plain text) from paragraphs and an optional call-to-action link.
///     Every text is HTML-encoded; the plain-text version repeats the link in full so it can be copied.
/// </summary>
public sealed class EmailLayout
{
    private readonly List<string> _paragraphs = [];
    private string? _greeting;
    private (string Label, string Url)? _action;
    private string? _footnote;

    public static EmailLayout Create() => new();

    public EmailLayout Greeting(string greeting) { _greeting = greeting; return this; }

    public EmailLayout Paragraph(string text) { _paragraphs.Add(text); return this; }

    public EmailLayout Action(string label, string url) { _action = (label, url); return this; }

    public EmailLayout Footnote(string text) { _footnote = text; return this; }

    public EmailMessage To(string recipient, string subject) => new(recipient, subject, Html(subject), Text());

    private string Html(string subject)
    {
        static string E(string value) => WebUtility.HtmlEncode(value);
        var html = new StringBuilder();
        html.Append("<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\"><title>").Append(E(subject)).Append("</title></head>");
        html.Append("<body style=\"margin:0;padding:24px;background:#f5f0e1;font-family:Arial,Helvetica,sans-serif;color:#1d2b3a\">");
        html.Append("<table role=\"presentation\" width=\"100%\" style=\"max-width:560px;margin:0 auto;background:#ffffff;border-radius:8px;padding:24px\"><tr><td>");
        html.Append("<h1 style=\"font-size:20px;color:#0d2a4f;margin:0 0 16px\">SmartStay</h1>");
        if (_greeting is not null) html.Append("<p style=\"margin:0 0 12px\">").Append(E(_greeting)).Append("</p>");
        foreach (var paragraph in _paragraphs)
            html.Append("<p style=\"margin:0 0 12px;line-height:1.5\">").Append(E(paragraph)).Append("</p>");
        if (_action is { } action)
        {
            html.Append("<p style=\"margin:24px 0\"><a href=\"").Append(E(action.Url))
                .Append("\" style=\"background:#e67e22;color:#ffffff;padding:12px 20px;border-radius:6px;text-decoration:none;font-weight:bold\">")
                .Append(E(action.Label)).Append("</a></p>");
            html.Append("<p style=\"margin:0 0 12px;font-size:12px;color:#5b6b7b\">Si el botón no funciona, copia este enlace en tu navegador: ")
                .Append(E(action.Url)).Append("</p>");
        }
        if (_footnote is not null)
            html.Append("<p style=\"margin:16px 0 0;font-size:12px;color:#5b6b7b\">").Append(E(_footnote)).Append("</p>");
        html.Append("</td></tr></table></body></html>");
        return html.ToString();
    }

    private string Text()
    {
        var text = new StringBuilder();
        if (_greeting is not null) text.AppendLine(_greeting).AppendLine();
        foreach (var paragraph in _paragraphs) text.AppendLine(paragraph).AppendLine();
        if (_action is { } action) text.AppendLine($"{action.Label}: {action.Url}").AppendLine();
        if (_footnote is not null) text.AppendLine(_footnote).AppendLine();
        text.Append("— El equipo de SmartStay");
        return text.ToString();
    }
}
