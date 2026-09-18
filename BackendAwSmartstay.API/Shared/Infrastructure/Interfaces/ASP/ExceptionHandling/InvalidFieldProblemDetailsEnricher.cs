using System.Text.Json;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.ExceptionHandling;

/// <summary>
///     Reports an <see cref="InvalidFieldException"/> like a model validation error: same title and an
///     <c>errors</c> member keyed by the camelCase field name, so clients show it next to the field. <c>detail</c>
///     keeps the message too.
/// </summary>
public class InvalidFieldProblemDetailsEnricher : IProblemDetailsEnricher
{
    public void Enrich(ProblemDetailsContext context)
    {
        if (context.Exception is not InvalidFieldException invalidField) return;

        context.ProblemDetails.Title = "One or more validation errors occurred.";
        context.ProblemDetails.Extensions["errors"] = new Dictionary<string, string[]>
        {
            [JsonNamingPolicy.CamelCase.ConvertName(invalidField.Field)] = [invalidField.Message]
        };
    }
}
