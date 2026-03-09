namespace TimberLive.Models;

public record Breadcrumb(string Name, string Url, Breadcrumb? Parent = null)
{

    public static readonly Breadcrumb Home = new("Home", "/");
    public static readonly Breadcrumb Beavers = Home.CreateChild("Beavers & Bots", "/beavers");
    public static readonly Breadcrumb Wiki = Home.CreateChild("Wiki", "/wiki");
    public static readonly Breadcrumb WikiTemplates = Wiki.CreateChild("Buildings & Plants", "/wiki/templates");
    public static readonly Breadcrumb WikiTemplateDetails = WikiTemplates.CreateChild("…", "");
    public static readonly Breadcrumb WikiGoods = Wiki.CreateChild("Goods", "/wiki/goods");

    public Breadcrumb CreateChild(string name, string url = "")
        => new(name, url, this);

    public IEnumerable<Breadcrumb> GetParents()
    {
        return GoToRoot().Reverse();

        IEnumerable<Breadcrumb> GoToRoot()
        {
            var curr = Parent;
            
            while (curr is not null)
            {
                yield return curr;
                curr = curr.Parent;
            }
        }
    }

}
