using System.ComponentModel.DataAnnotations;
using BackendAwSmartstay.Domain.Shared.Domain.Model.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Validation;

/// <summary>
///     Runs after the built-in validator providers and wraps their validators so every model validation error also
///     records the stable code of its rule (<see cref="FieldViolationCodes"/>). The messages and the validation itself
///     are unchanged.
/// </summary>
public sealed class CodeRecordingModelValidatorProvider : IModelValidatorProvider
{
    public void CreateValidators(ModelValidatorProviderContext context)
    {
        foreach (var item in context.Results)
        {
            if (item.Validator is null or CodeRecordingValidator or CodedValidatableObjectValidator) continue;

            if (item.ValidatorMetadata is ValidationAttribute attribute)
                item.Validator = new CodeRecordingValidator(item.Validator, FieldViolationCodes.For(attribute));
            else if (item.ValidatorMetadata is null
                     && typeof(IValidatableObject).IsAssignableFrom(context.ModelMetadata.ModelType))
                item.Validator = new CodedValidatableObjectValidator();
        }
    }

    /// <summary>Delegates to the framework validator of an attribute and records the attribute's code.</summary>
    private sealed class CodeRecordingValidator(IModelValidator inner, FieldViolationCodes.CodedRule rule) : IModelValidator
    {
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            var results = inner.Validate(context).ToList();
            foreach (var result in results)
                FieldViolationCodes.Record(context.ActionContext.HttpContext, result.Message, rule);
            return results;
        }
    }

    /// <summary>
    ///     Validates an <see cref="IValidatableObject"/> resource like the framework adapter does, recording the code of
    ///     each <see cref="CodedValidationResult"/>.
    /// </summary>
    private sealed class CodedValidatableObjectValidator : IModelValidator
    {
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            if (context.Model is not IValidatableObject validatable) return [];

            var validationContext = new ValidationContext(validatable, context.ActionContext.HttpContext.RequestServices, items: null)
            {
                DisplayName = context.ModelMetadata.GetDisplayName(),
                MemberName = context.ModelMetadata.Name
            };

            var results = new List<ModelValidationResult>();
            foreach (var result in validatable.Validate(validationContext))
            {
                if (result == ValidationResult.Success || result.ErrorMessage is null) continue;
                var rule = result is CodedValidationResult coded
                    ? new FieldViolationCodes.CodedRule(coded.Code, coded.Parameters)
                    : new FieldViolationCodes.CodedRule(ErrorCodes.FieldInvalid, null);
                FieldViolationCodes.Record(context.ActionContext.HttpContext, result.ErrorMessage, rule);

                var members = result.MemberNames.ToList();
                if (members.Count == 0)
                    results.Add(new ModelValidationResult(memberName: null, result.ErrorMessage));
                else
                    results.AddRange(members.Select(member => new ModelValidationResult(member, result.ErrorMessage)));
            }
            return results;
        }
    }
}
