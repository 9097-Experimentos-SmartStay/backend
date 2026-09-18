using BackendAwSmartstay.API.DemoData.Infrastructure.Configuration;
using BackendAwSmartstay.API.DemoData.Infrastructure.Seeding;
using Microsoft.Extensions.Options;

namespace BackendAwSmartstay.API.DemoData.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public static class WebApplicationBuilderExtensions
{
    /// <summary>
    ///     Demo dataset (opt-in, <c>DemoData__Enabled=true</c>): its settings are validated at startup only when it is
    ///     enabled, and the seeder runs after the migrations (<see cref="SeedDemoDataAsync"/>).
    /// </summary>
    public static void AddDemoDataServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<DemoDataSettings>()
            .Bind(builder.Configuration.GetSection(DemoDataSettings.SectionName))
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<DemoDataSettings>, DemoDataSettingsValidator>();
        builder.Services.AddScoped<DemoDataSeeder>();
    }

    /// <summary>Creates the demo dataset when enabled and not present yet. Errors propagate: startup fails fast.</summary>
    public static async Task SeedDemoDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync();
    }
}
