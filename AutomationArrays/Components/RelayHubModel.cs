namespace AutomationArrays.Components;

[AddTemplateModule2(typeof(RelayHubSpec))]
public class RelayHubModel : BaseComponent, IAwakableComponent
{
    const int SizeX = 3;
    const int SizeY = 3;

#nullable disable
    GameObject template;
#nullable enable

    readonly GameObject[] entries = new GameObject[SizeX * SizeY];

    public void Awake()
    {
        InitializeModels();
    }

    void InitializeModels()
    {
        var go = GameObject;

        template = go.FindChild("#Template");
        var container = go.FindChild("#Actual");

        var i = 0;
        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                var entry = GameObject.Instantiate(template, container.transform);
                entries[i] = entry;
                entry.transform.Translate(new(x, 0, y));
                    
                i++;
            }
        }

        template.SetActive(false);

        GetComponent<SegmentedIlluminator>().InitializeSegments(entries);
    }

}
