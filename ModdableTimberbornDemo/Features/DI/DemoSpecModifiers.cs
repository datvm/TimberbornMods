namespace ModdableTimberbornDemo.Features.DI;

public class DemoSpecFrontRunner : ISpecServiceFrontRunner
{

    public void Load()
    {
        Debug.Log("DemoSpecFrontRunner.Load is called. This should be before SpecService.Load");
    }

}

public class DemoSpecTailRunner : ISpecServiceTailRunner
{

    public void Run(SpecService specService)
    {
        Debug.Log("DemoSpecTailRunner.Run is running!");
    }

}

public class DemoSpecModifier1 : BaseSpecModifier<GoodSpec>, ILoadableSingleton
{

    public void Load()
    {
        Debug.Log("DemoSpecModifier1.Load is called. This should be before SpecService.Load");
    }

    protected override IEnumerable<GoodSpec> Modify(IEnumerable<GoodSpec> specs)
    {
        foreach (var s in specs)
        {
            if (s.Id == "Log")
            {
                Debug.Log($"Increasing weight of {s.Id} from {s.Weight} to {s.Weight + 3}");
            }

            yield return s with { Weight = s.Weight + 3 };
        }
    }
}

public class DemoSpecModifier2 : BaseSpecModifier<GoodSpec>, ILoadableSingleton
{

        // Make sure it run after DemoSpecModifier1

    public void Load()
    {
        Debug.Log("DemoSpecModifier2.Load is called. This should be before SpecService.Load");
    }

    protected override IEnumerable<GoodSpec> Modify(IEnumerable<GoodSpec> specs)
    {
        foreach (var s in specs)
        {
            if (s.Id == "Log")
            {
                Debug.Log($"Decreasing weight of {s.Id} from {s.Weight} to {s.Weight / 2}");
            }

            yield return s with { Weight = s.Weight / 2 };
        }
    }
}
