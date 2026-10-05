sealed class BlueprintExporter
{
    readonly string inputDir;
    readonly string outputRoot;
    readonly JsonObject input;
    readonly JsonObject variants;
    readonly JsonSerializerOptions json = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public BlueprintExporter(string inputPath)
    {
        var fullPath = Path.GetFullPath(inputPath);
        inputDir = Path.GetDirectoryName(fullPath) ?? ".";
        input = JsonNode.Parse(File.ReadAllText(fullPath))!.AsObject();
        outputRoot = Path.GetFullPath(Path.Combine(inputDir, Require(input, "OutputRoot").GetValue<string>()));
        variants = Require(input, "Variants").AsObject();
    }

    public int Run()
    {
        var written = 0;
        foreach (var exportNode in Require(input, "Exports").AsArray())
        {
            var export = exportNode!.AsObject();
            var template = Load(inputDir, Require(export, "Template").GetValue<string>());
            foreach (var faction in Facets(export))
            {
                var names = export["Variants"]?.AsArray();
                if (names is null)
                {
                    written += Write(template, export, Context(null, faction));
                    continue;
                }

                foreach (var nameNode in names)
                {
                    written += Write(template, export, Context(Variant(nameNode!.GetValue<string>()), faction));
                }
            }
        }

        WriteIcons();
        Console.WriteLine($"Wrote {written} files under {outputRoot}");
        return written;
    }

    void WriteIcons()
    {
        using var belt = Icon("BeltIcon.png");
        using var corner = Icon("CornerBeltIcon.png");
        using var up = Icon("BeltUpIcon.png");
        using var down = Icon("BeltDownIcon.png");
        using var impermeable = Icon("ImpermeableIcon.png");
        using var border = Icon("BeltBorder.png");
        using var typeface = SKTypeface.FromFile(InstalledFont("Roboto Black"))
            ?? throw new InvalidOperationException("Could not read the installed Roboto Black font.");
        var tier = new Dictionary<string, int>();
        var index = 0;
        foreach (var pair in variants)
        {
            index++;
            tier[pair.Key] = index;
            Stamp(border, typeface, Path.Combine(outputRoot, "Sprites", "BottomBar", $"Conveyor{pair.Key}GroupIcon.png"), index);
        }

        foreach (var exportNode in Require(input, "Exports").AsArray())
        {
            var export = exportNode!.AsObject();
            var names = export["Variants"]?.AsArray();
            if (names is null || Require(export, "Set")["LabeledEntitySpec.Icon"] is not JsonValue iconValue || !iconValue.TryGetValue<string>(out var iconPattern))
            {
                continue;
            }

            foreach (var nameNode in names)
            {
                var name = nameNode!.GetValue<string>();
                var relative = Interpolate(iconPattern, Variant(name));
                var plain = relative.Contains("Impermeable");
                var template = belt;
                if (plain)
                {
                    template = impermeable;
                }
                else if (relative.Contains("Corner"))
                {
                    template = corner;
                }
                else if (relative.Contains("RiserUp"))
                {
                    template = up;
                }
                else if (relative.Contains("RiserDown"))
                {
                    template = down;
                }

                Stamp(template, typeface, Path.Combine(outputRoot, relative + ".png"), plain ? 0 : tier[name]);
            }
        }
    }

    SKBitmap Icon(string name)
    {
        var path = Path.Combine(inputDir, "Templates", "Icons", name);
        return SKBitmap.Decode(path) ?? throw new InvalidOperationException($"Could not read {path}.");
    }

    static string InstalledFont(string family)
    {
        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            using var key = hive.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts");
            if (key?.GetValue($"{family} (TrueType)") is not string file)
            {
                continue;
            }

            if (!Path.IsPathRooted(file))
            {
                file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts", file);
            }

            if (File.Exists(file))
            {
                return file;
            }
        }

        throw new InvalidOperationException($"{family} is not installed.");
    }

    static void Stamp(SKBitmap template, SKTypeface typeface, string path, int marks)
    {
        using var bitmap = template.Copy();
        if (marks > 0)
        {
            using var canvas = new SKCanvas(bitmap);
            using var font = new SKFont(typeface, 90);
            using var paint = new SKPaint { Color = new SKColor(0xBC, 0xA2, 0x6C), IsAntialias = true };
            var text = new string('>', marks);
            font.MeasureText(text, out var bounds);
            var limit = InkRight(template) + 4f;
            var room = bitmap.Width - 6f - limit;
            while (font.Size > 28f && bounds.Width > room && Hits(template, bitmap.Width - 6f - bounds.Right, bitmap.Height - 6f - bounds.Bottom, bounds))
            {
                font.Size -= 2f;
                font.MeasureText(text, out bounds);
            }

            var x = bitmap.Width - 6f - bounds.Right;
            var y = bitmap.Height - 6f - bounds.Bottom;
            canvas.DrawText(text, x, y, font, paint);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100) ?? throw new InvalidOperationException($"Could not encode {path}.");
        File.WriteAllBytes(path, data.ToArray());
    }

    static int InkRight(SKBitmap bitmap)
    {
        var right = 0;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = bitmap.Width - 1; x > right; x--)
            {
                if (Ink(bitmap.GetPixel(x, y)))
                {
                    right = x;
                    break;
                }
            }
        }

        return right;
    }

    static bool Hits(SKBitmap bitmap, float x, float y, SKRect bounds)
    {
        var left = Math.Max(0, (int)MathF.Floor(x + bounds.Left));
        var top = Math.Max(0, (int)MathF.Floor(y + bounds.Top));
        var right = Math.Min(bitmap.Width - 1, (int)MathF.Ceiling(x + bounds.Right));
        var bottom = Math.Min(bitmap.Height - 1, (int)MathF.Ceiling(y + bounds.Bottom));
        for (var py = top; py <= bottom; py++)
        {
            for (var px = left; px <= right; px++)
            {
                if (Ink(bitmap.GetPixel(px, py)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static bool Ink(SKColor color) => color.Alpha > 20 && color.Red + color.Green + color.Blue > 40;

    IEnumerable<string?> Facets(JsonObject export)
    {
        if (export["Once"] is JsonValue once && once.TryGetValue<bool>(out var single) && single)
        {
            yield return null;
            yield break;
        }

        if (input["Factions"] is not JsonArray factions || factions.Count == 0)
        {
            yield return null;
            yield break;
        }

        foreach (var faction in factions)
        {
            yield return faction!.GetValue<string>();
        }
    }

    static JsonObject? Context(JsonObject? variant, string? faction)
    {
        if (faction is null)
        {
            return variant?.DeepClone().AsObject();
        }

        var copy = variant?.DeepClone().AsObject() ?? new JsonObject();
        copy["Faction"] = faction;
        copy["Metal"] = faction == "IronTeeth" ? "MetalPart" : "MetalBlock";
        if (faction == "IronTeeth"
            && copy["Costs"] is JsonObject costs
            && costs["IronTeeth"] is JsonObject picked)
        {
            copy["Cost"] = picked.DeepClone();
        }

        return copy;
    }

    int Write(JsonObject template, JsonObject export, JsonObject? variant)
    {
        var copy = template.DeepClone().AsObject();
        FillTokens(copy, variant);
        foreach (var pair in Require(export, "Set").AsObject())
        {
            SetPath(copy, pair.Key, Resolve(pair.Value, variant));
        }

        var relative = Interpolate(Require(export, "Output").GetValue<string>(), variant);
        var path = Path.GetFullPath(Path.Combine(outputRoot, relative));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, copy.ToJsonString(json) + Environment.NewLine);
        Console.WriteLine(relative);
        return 1;
    }

    JsonObject Variant(string name)
    {
        if (variants[name] is JsonObject variant)
        {
            return variant;
        }

        throw new InvalidOperationException($"Variant '{name}' is missing from Variants.");
    }

    JsonNode? Resolve(JsonNode? node, JsonObject? variant)
    {
        if (node is null)
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            return ResolveText(text, variant);
        }

        if (node is JsonObject obj)
        {
            if (obj.ContainsKey("$each"))
            {
                return ExpandEach(obj, variant);
            }

            if (obj.ContainsKey("$concat"))
            {
                return ExpandConcat(obj, variant);
            }

            var copy = new JsonObject();
            foreach (var pair in obj)
            {
                copy[pair.Key] = Resolve(pair.Value, variant);
            }

            return copy;
        }

        if (node is JsonArray array)
        {
            var copy = new JsonArray();
            foreach (var item in array)
            {
                copy.Add(Resolve(item, variant));
            }

            return copy;
        }

        return node.DeepClone();
    }

    JsonNode ResolveText(string text, JsonObject? variant)
    {
        if (text.Length >= 2 && text[0] == '{' && text[^1] == '}' && text.IndexOf('{', 1) < 0)
        {
            return Lookup(variant, text[1..^1]).DeepClone();
        }

        if (!text.Contains('{'))
        {
            return JsonValue.Create(text);
        }

        var replaced = Regex.Replace(text, @"\{([A-Za-z0-9_.]+)\}", match => Scalar(variant, match.Groups[1].Value));
        return JsonValue.Create(replaced);
    }

    void FillTokens(JsonNode? node, JsonObject? variant)
    {
        if (node is JsonObject obj)
        {
            foreach (var key in obj.Select(pair => pair.Key).ToList())
            {
                if (obj[key] is JsonValue value && value.TryGetValue<string>(out var text) && text.Contains('{'))
                {
                    obj[key] = ResolveText(text, variant);
                    continue;
                }

                FillTokens(obj[key], variant);
            }

            return;
        }

        if (node is not JsonArray array)
        {
            return;
        }

        for (var i = 0; i < array.Count; i++)
        {
            if (array[i] is JsonValue value && value.TryGetValue<string>(out var text) && text.Contains('{'))
            {
                array[i] = ResolveText(text, variant);
                continue;
            }

            FillTokens(array[i], variant);
        }
    }

    JsonArray ExpandEach(JsonObject each, JsonObject? variant)
    {
        IEnumerable<string> names = each["$each"] is JsonArray listed
            ? listed.Select(node => node!.GetValue<string>())
            : variants.Select(pair => pair.Key);
        if (each["Item"] is not JsonNode item)
        {
            throw new InvalidOperationException("$each needs an Item.");
        }

        var result = new JsonArray();
        foreach (var name in names)
        {
            var resolved = Resolve(item, WithFaction(Variant(name), variant));
            if (resolved is JsonArray many)
            {
                foreach (var one in many)
                {
                    result.Add(one?.DeepClone());
                }

                continue;
            }

            result.Add(resolved);
        }

        return result;
    }

    static JsonObject WithFaction(JsonObject speed, JsonObject? outer)
    {
        var copy = speed.DeepClone().AsObject();
        if (outer is null)
        {
            return copy;
        }

        foreach (var pair in outer)
        {
            if (!copy.ContainsKey(pair.Key))
            {
                copy[pair.Key] = pair.Value?.DeepClone();
            }
        }

        return copy;
    }

    JsonArray ExpandConcat(JsonObject concat, JsonObject? variant)
    {
        if (concat["$concat"] is not JsonArray parts)
        {
            throw new InvalidOperationException("$concat needs an array.");
        }

        var result = new JsonArray();
        foreach (var part in parts)
        {
            var resolved = Resolve(part, variant);
            if (resolved is JsonArray many)
            {
                foreach (var one in many)
                {
                    result.Add(one?.DeepClone());
                }

                continue;
            }

            result.Add(resolved);
        }

        return result;
    }

    static void SetPath(JsonObject root, string path, JsonNode? value)
    {
        var parts = path.Split('.');
        var cursor = root;
        for (var i = 0; i < parts.Length - 1; i++)
        {
            if (cursor[parts[i]] is not JsonObject child)
            {
                throw new InvalidOperationException($"'{path}' does not pass through an object named '{parts[i]}'.");
            }

            cursor = child;
        }

        cursor[parts[^1]] = value;
    }

    JsonNode Lookup(JsonObject? variant, string path)
    {
        JsonNode? cursor = variant ?? throw new InvalidOperationException($"'{{{path}}}' needs a variant.");
        foreach (var part in path.Split('.'))
        {
            if (cursor is not JsonObject obj || obj[part] is not JsonNode next)
            {
                throw new InvalidOperationException($"Variant has no '{path}'.");
            }

            cursor = next;
        }

        return cursor;
    }

    string Scalar(JsonObject? variant, string path)
    {
        var node = Lookup(variant, path);
        if (node is JsonValue value)
        {
            if (value.TryGetValue<string>(out var text))
            {
                return text;
            }

            if (value.TryGetValue<int>(out var number))
            {
                return number.ToString();
            }
        }

        throw new InvalidOperationException($"'{{{path}}}' is not a string or number, so it cannot be written inside text.");
    }

    string Interpolate(string text, JsonObject? variant)
    {
        if (!text.Contains('{'))
        {
            return text;
        }

        return Regex.Replace(text, @"\{([A-Za-z0-9_.]+)\}", match => Scalar(variant, match.Groups[1].Value));
    }

    static JsonObject Load(string dir, string relative)
    {
        var path = Path.GetFullPath(Path.Combine(dir, relative));
        if (JsonNode.Parse(File.ReadAllText(path)) is not JsonObject obj)
        {
            throw new InvalidOperationException($"{relative} is not a JSON object.");
        }

        return obj;
    }

    static JsonNode Require(JsonObject obj, string name)
    {
        if (obj[name] is not JsonNode node)
        {
            throw new InvalidOperationException($"Missing '{name}'.");
        }

        return node;
    }
}
