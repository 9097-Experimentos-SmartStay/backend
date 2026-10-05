using System;
using BackendAwSmartstay.API.BddTests.Support;
using BackendAwSmartstay.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Reqnroll;

namespace BackendAwSmartstay.API.BddTests.Hooks;

[Binding]
public class TestHooks
{
    private readonly ScenarioContext _scenarioContext;

    public TestHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void SetupScenario()
    {
        var db = TestDbContextFactory.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        _scenarioContext.Set(db);
    }

    [AfterScenario]
    public void CleanupScenario()
    {
        if (_scenarioContext.TryGetValue<AppDbContext>(out var db))
        {
            db.Database.EnsureDeleted();
            db.Dispose();
        }
    }
}
