using BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BackendAwSmartstay.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

/// <summary>
/// Convention for naming routes in kebab-case.
/// </summary>
public class KebabCaseRouteNamingConvention : IControllerModelConvention
{

    /// <summary>
    /// Applies the kebab-case naming convention to the controller and its actions.
    /// </summary>
    /// <param name="controller">The controller model to apply the convention to.</param>
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);

        foreach (var selector in controller.Actions.SelectMany(a => a.Selectors))
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
    }

    /// <summary>
    /// Replaces the [controller] placeholder in the route template with the kebab-case controller name.
    /// </summary>
    /// <param name="selector">The selector model.</param>
    /// <param name="name">The controller name.</param>
    /// <returns>The updated attribute route model.</returns>
    private static AttributeRouteModel? ReplaceControllerTemplate(SelectorModel selector, string name)
    {
        return selector.AttributeRouteModel != null
            ? new AttributeRouteModel
            {
                Template = selector.AttributeRouteModel.Template?.Replace("[controller]", name.ToKebabCase())
            }
            : null;
    }
}
