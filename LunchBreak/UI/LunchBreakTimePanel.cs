global using Timberborn.AssetSystem;

namespace LunchBreak.UI;

public class LunchBreakTimePanel(LunchBreakManager man, UILayout layout, IAssetLoader assets, VisualElementInitializer veInitializer) : ILoadableSingleton
{

    NineSliceVisualElement panel = null!;
    Label lblStart = null!;
    Label lblEnd = null!;

    public void Load()
    {
        var wrapper = new VisualElement();
        wrapper.classList.Add("top-right-item__wrapper");
        
        panel = new();
        panel.classList.AddRange(["top-right-item--first-column", "square-large--green"]);
        wrapper.Add(panel);

        AddIcon(panel);

        AddButtonPair(panel, ChangeStartTime);
        lblStart = AddLabel(panel);

        AddButtonPair(panel, ChangeEndTime);
        lblEnd = AddLabel(panel);

        veInitializer.InitializeVisualElement(wrapper);
        layout.AddTopRight(wrapper, 3);

        UpdateTexts();
    }

    void AddIcon(VisualElement parent)
    {
        var lunchBoxIcon = assets.Load<Texture2D>("Sprites/lunchbreak");
        var img = new Image()
        {
            image = lunchBoxIcon,
        };
        img.style.width = img.style.height = 15;

        parent.Add(img);
    }

    void AddButtonPair(VisualElement parent, Action<int> onClick)
    {
        for (int i = 0; i < 2; i++)
        {
            var btn = new Button();
            btn.classList.Add("button-square");
            btn.classList.Add("button-square--small");

            if (i == 0)
            {
                btn.classList.Add("button-minus");
            }
            else
            {
                btn.classList.Add("button-plus");
                btn.classList.Add("button-plus--margin");
            }

            var z = i;
            btn.clicked += () => onClick(z == 0 ? -1 : 1);

            parent.Add(btn);
        }
    }

    Label AddLabel(VisualElement parent)
    {
        Label lbl = new();

        lbl.classList.Add("game-text-normal");
        lbl.classList.Add("text--yellow");
        parent.Add(lbl);

        return lbl;
    }

    void ChangeStartTime(int delta)
    {
        var curr = man.LunchBreakTime;
        var newValue = Math.Clamp(curr.x + delta, 0, curr.y);

        man.LunchBreakTime = curr with { x = newValue, };
        UpdateTexts();
    }

    void ChangeEndTime(int delta)
    {
        var curr = man.LunchBreakTime;
        var newValue = Math.Clamp(curr.y + delta, curr.x, 24);

        man.LunchBreakTime = curr with { y = newValue, };
        UpdateTexts();
    }

    void UpdateTexts()
    {
        var time = man.LunchBreakTime;
        lblStart.text = $"{time.x:00}h";
        lblEnd.text = $"{time.y:00}h";
    }

}
