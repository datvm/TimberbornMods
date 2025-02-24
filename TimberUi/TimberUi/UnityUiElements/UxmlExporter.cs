namespace UnityEditor.UIElements.Debugger;

// From: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/UIElementsEditor/Debugger/UxmlExporter.cs

#nullable disable

public class UxmlExporter
{
    private const string UIElementsNamespace = "UnityEngine.UIElements";

    [Flags]
    public enum ExportOptions
    {
        None = 0,
        NewLineOnAttributes = 1,
        StyleFields = 2,
        AutoNameElements = 4,
        PrintTemplate = 8,
    }

    public static string Dump(VisualElement selectedElement, string templateId, ExportOptions options)
    {
        Dictionary<XNamespace, string> nsToPrefix = new Dictionary<XNamespace, string>()
        {
            { UIElementsNamespace, "ui" }
        };

        HashSet<string> usings = new HashSet<string>();

        var doc = new XDocument();
        XElement template = new XElement("UXML");
        doc.Add(template);

        Recurse(template, nsToPrefix, usings, selectedElement, options);

        foreach (var it in nsToPrefix)
        {
            template.Add(new XAttribute(XNamespace.Xmlns + it.Value, it.Key));
        }

        foreach (var it in usings.OrderByDescending(x => x))
        {
            template.AddFirst(new XElement("Using", new XAttribute("alias", it), new XAttribute("path", it)));
        }

        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            OmitXmlDeclaration = true,
            NewLineOnAttributes = (options & ExportOptions.NewLineOnAttributes) == ExportOptions.NewLineOnAttributes,
            NewLineHandling = NewLineHandling.Replace
        };

        StringBuilder sb = new StringBuilder();
        using (XmlWriter writer = XmlWriter.Create(sb, settings))
            doc.Save(writer);

        return sb.ToString();
    }

    private static void Recurse(XElement parent, Dictionary<XNamespace, string> nsToPrefix, HashSet<string> usings, VisualElement ve, ExportOptions options)
    {
        //todo: handle namespace
        XElement elt;

        string ns = ve.GetType().Namespace ?? "";
        string typeName = ve.typeName;
        Dictionary<string, string> attrs = new Dictionary<string, string>();

        string nsp;
        if (ve is TemplateContainer)
        {
            var templateId = ((TemplateContainer)ve).templateId;
            elt = new XElement(templateId);
            usings.Add(templateId);
        }
        else if (nsToPrefix.TryGetValue(ns, out nsp))
        {
            elt = new XElement((XNamespace)ns + typeName);
        }
        else
            elt = new XElement(typeName);

        parent.Add(elt);

        foreach (var attr in attrs)
            elt.SetAttributeValue(attr.Key, attr.Value);

        var elementText = ve is TextElement ? (ve as TextElement).text : "";

        if (!String.IsNullOrEmpty(ve.name) && ve.name[0] != '_')
            elt.SetAttributeValue("name", ve.name);
        else if ((options & ExportOptions.AutoNameElements) == ExportOptions.AutoNameElements)
        {
            var genName = ve.GetType().Name + elementText.Replace(" ", "");
            elt.SetAttributeValue("name", genName);
        }

        if (!String.IsNullOrEmpty(elementText))
            elt.SetAttributeValue("text", elementText);

        var classes = ve.GetClassesForIteration();
        if (classes.Any())
            elt.SetAttributeValue("class", string.Join(" ", classes.ToArray()));

        if (ve is TemplateContainer && ((options & ExportOptions.PrintTemplate) != ExportOptions.PrintTemplate))
        {
            return;
        }

        foreach (var childElement in ve.Children())
        {
            Recurse(elt, nsToPrefix, usings, childElement, options);
        }
    }
}