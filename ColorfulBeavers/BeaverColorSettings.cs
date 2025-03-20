global using Timberborn.PlatformUtilities;
using System.Collections.Frozen;

namespace ColorfulBeavers;

public class BeaverColorSettings
{

    public static readonly BeaverColorSettings Instance = new();

    public static readonly string FolderPath = Path.Combine(UserDataFolder.Folder, "ColorfulBeavers");
    public static readonly string ColorNamesPath = Path.Combine(FolderPath, "ColorNames.txt");
    public static readonly string RandomColorsPath = Path.Combine(FolderPath, "RandomColors.txt");

    private BeaverColorSettings() { }

    FrozenDictionary<string, Vector3Int> colorDeterminedByName = null!;
    ImmutableArray<Vector3Int> randomColors;

    public void Load()
    {
        Directory.CreateDirectory(FolderPath);

        LoadColorNames();
        LoadRandomColors();
    }

    void LoadColorNames()
    {
        if (!File.Exists(ColorNamesPath))
        {
            File.WriteAllText(ColorNamesPath, DefaultColorNames);
        }

        var lines = File.ReadAllLines(ColorNamesPath);
        var dict = new Dictionary<string, Vector3Int>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            try
            {
                var parts = line.Split(';');
                dict[parts[0]] = Parse(parts.AsSpan(1));
            }
            catch (Exception)
            {
                throw new InvalidDataException($"Invalid color for named beaver (ColorNames.txt) at line {i + 1}");
            }
        }

        colorDeterminedByName = dict.ToFrozenDictionary();
    }

    void LoadRandomColors()
    {
        if (!File.Exists(RandomColorsPath))
        {
            File.WriteAllText(RandomColorsPath, DefaultRandomColors);
        }

        var lines = File.ReadAllLines(RandomColorsPath);
        randomColors = [.. lines
            .Select((l, i) =>
            {
                try
                {
                    return Parse(l.Split(';'));
                }
                catch
                {
                    throw new InvalidDataException($"Invalid color for random colors (RandomColors.txt) at line {i + 1}");
                }
            })];
    }

    public Vector3Int? AssignColor(string? name, bool random)
    {
        if (name is not null && colorDeterminedByName.TryGetValue(name, out var color))
        {
            return color;
        }
        else if (random)
        {
            return randomColors[UnityEngine.Random.Range(0, randomColors.Length)];
        }

        return null;
    }

    static Vector3Int Parse(in Span<string> parts) => new(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));

    #region Default Value

    const string DefaultColorNames = """
Rotsack;90;50;30
Gonog;27;94;32
Anji;0;100;0
Vulzal;120;40;80
Zufem;180;220;100
Zebajin;50;150;200
Ayargajin;200;90;30
Sligo;80;140;60
Napokue;130;100;180
Equinus;90;60;30
Zevrij;220;80;150
Tazingo;255;200;50
Kurzol;70;50;90
Sollix;240;160;80
Rujabu;170;40;110
Prerrahar;100;20;20
Nuenvan;150;180;200
Titamor;255;150;200
Jubukraa;210;50;90
Mezilkree;60;200;140
Xukundi;130;30;70
Halasuwa;190;230;90
Zulgeteb;40;80;60
Ngonel;250;120;60
Trolgar;80;100;40
Bidibog;110;140;70
Vuajuag;160;80;220
Azajoth;20;40;80
Qodvux;200;150;50
Donudor;230;180;120
Zojavuck;100;200;80
Pyribag;255;90;40
Stakvizz;240;210;70
Qagatsock;150;110;90
Dorlegax;60;90;120
Jumuziz;220;100;180
Krodrag;50;70;30
Zodock;140;100;60
Subumroz;80;120;160
Ubuzur;200;70;130
Zernogath;30;50;90
Qevolvax;180;140;220
Lepud;140;90;70
Qakugoth;90;60;110
Shozlith;120;200;90
Chikyil;255;230;80
Kuguthe;110;80;50
Naengeax;130;170;210
Chudhagas;170;60;40
Quscuy;200;180;150
Zircoy;160;220;200
Dujunua;140;120;180
Jozlozuy;230;110;160
Gnonu;240;140;80
Szozsil;100;180;130
Sbudd;190;160;90
Vulzal;120;40;80
Zufem;180;220;100
Zebajin;50;150;200
Ayargajin;200;90;30
Sligo;80;140;60
Napokue;130;100;180
Equinus;90;60;30
Zevrij;220;80;150
Tazingo;255;200;50
Kurzol;70;50;90
Sollix;240;160;80
Rujabu;170;40;110
Prerrahar;100;20;20
Nuenvan;150;180;200
Titamor;255;150;200
Jubukraa;210;50;90
Mezilkree;60;200;140
Xukundi;130;30;70
Halasuwa;190;230;90
Zulgeteb;40;80;60
Ngonel;250;120;60
Trolgar;80;100;40
Bidibog;110;140;70
Vuajuag;160;80;220
Azajoth;20;40;80
Qodvux;200;150;50
Donudor;230;180;120
Zojavuck;100;200;80
Pyribag;255;90;40
Stakvizz;240;210;70
Qagatsock;150;110;90
Dorlegax;60;90;120
Jumuziz;220;100;180
Krodrag;50;70;30
Zodock;140;100;60
Subumroz;80;120;160
Ubuzur;200;70;130
Zernogath;30;50;90
Qevolvax;180;140;220
Lepud;140;90;70
Qakugoth;90;60;110
Shozlith;120;200;90
Chikyil;255;230;80
Kuguthe;110;80;50
Naengeax;130;170;210
Chudhagas;170;60;40
Quscuy;200;180;150
Zircoy;160;220;200
Dujunua;140;120;180
Jozlozuy;230;110;160
Gnonu;240;140;80
Szozsil;100;180;130
Sbudd;190;160;90
""";

    const string DefaultRandomColors = """
131;51;234
255;127;175
255;198;191
248;247;242
248;68;79
120;189;196
1;44;61
120;189;196
62;79;60
""";
    #endregion

}
