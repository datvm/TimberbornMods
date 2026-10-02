# ModDesc.txt

Do not write or update `ModDesc.txt` or `ChangeLog.txt` unless the user explicitly asks. The notes below apply only then.

When asked to write a `ModDesc.txt`, write a Steam Workshop description in Steam BBCode (not Markdown). Match the recent mods (Crane, Fourpillars, ConstructionSiteHauler, BuildingRenovations): player-facing, practical, and compact.

Voice: second person, what the player gets. Colleague tone: explain the mod to another player, not ad copy. No marketing fluff. Mix sentence length. A short sentence is fine when the next one carries the detail. Do not stack fragments. That includes "Not X. Not Y. Just Z." and any run of same-beat punches ("No queue. No repeat. Just science.").

Light humor only in `[i]` flavor lines. Bold names, UI labels, and caveats with `[b]`.

For a new building, keep the three parts separate. Do not fold them into one sentence:

- Lore name: the name on the building.
- Mechanical description: what the player can do with it.
- Flavor: one `[i]` line, and not inside the mechanical description.

BBCode only (Steam tags, not Markdown): `[b]`, `[i]`, `[h1]`/`[h2]`, `[img]url[/img]`, `[url=...]text[/url]`, `[list][*] item[/list]`, `[table]`/`[tr]`/`[th]`/`[td]`, `[spoiler]` only when needed. Place a screenshot soon after the intro and after major sections (Steam UGC URL when available).

End every description with the GitHub footer below. Do not paraphrase it.

Typical order (omit unused sections):

1. 1–2 sentences of what the mod does. Optional `[i]` flavor line.
2. Screenshot.
3. `[h1]`/`[h2]` for buildings, features, settings, how-to, tips, FAQs, extending.
4. If configurable: `Main Menu -> Mods -> Settings button next to the mod name`. Say when defaults leave vanilla unchanged.
5. Stats as dashes (`Unlock`, `Cost`, `Workers`, `Range`, …). Related mods as Steam `[url]`.
6. Footer exactly:

```
===
Mod source code and donation: https://github.com/datvm/TimberbornMods (feel free to use the code for anything).
```

# Versioning

Do not bump a mod's `manifest.json` version, `ChangeLog.txt` version heading, or `IModUpdateNotifier2` version / loc key unless the user explicitly asks to bump the version. Fold new notes into the current version entry. Do not write or update `ChangeLog.txt` itself unless the user explicitly asks.

# Code Style

Always use code block even for single conditional or loop statements. For example:
```cs
if (condition) { return; }

// NOT:
if (condition) return;
```

Always use latest C# features like pattern matching, switch expressions, and collection initializers. For example:
```cs
List<int> numbers = [];
MyType variable = new();

// NOT:
List<int> numbers = new();
MyType variable = new MyType();
```

Prefer no access modifier for lowest visibility. Do not use `_` to prefix fields. For example:
```cs
int field;
class MyClass {}

// NOT:
private int _field;
internal class MyClass {}
```

Info-only types (DTOs, checks, list entries, bags of data with no behavior) should be `readonly record struct`, or `record` if they need reference identity. For example:
```cs
readonly record struct Shot(Vector3Int Dest, GameObject Box, float Started);

// NOT:
sealed class Shot
{
    public Vector3Int Dest { get; }
    public GameObject Box { get; }
    public float Started { get; }
}
```

When a `BaseComponent` needs more than one or two injected services, put those dependencies on a `[BindSingleton]` service instead and inject only that service into the component. Put calculation / orchestration methods on the service. For simple shared deps like `ILoc`, expose a readonly field:

```cs
[BindSingleton]
public class DischargePipeService(IWaterService waterService, IThreadSafeWaterMap waterMap, ILoc t)
{
    public readonly ILoc t = t;
    // …
}

[AddTemplateModule2(typeof(DischargePipeSpec))]
public class DischargePipe(DischargePipeService service) : BaseComponent // …
```

// NOT: inject IWaterService + IThreadSafeWaterMap + ILoc directly into the component.

# Component lifecycle

On a `BaseComponent`, `Awake` only grabs references (`GetComponent`, injected services already on the constructor). Do computation in `InitializeEntity`: reading specs, asking services for rates, building status toggles, filling ports.

A placement preview runs `Awake` and does not run `InitializeEntity`. That `Awake` can run before other services' `Load`. Work done there can crash the preview. `BeltCarrier` is the pattern: `Awake` stores `BlockObject` and `MechanicalNode`; `InitializeEntity` reads `BeltCarrierSpec`, registers the stuck status, and asks `BeltCarrierService` for items per hour.

# Singleton lifecycle

`ILoadableSingleton.Load` and `IPostLoadableSingleton.PostLoad` run in **dependency order**. If B's constructor injects A, `A.Load` always runs before `B.Load` (same for `PostLoad`). Do not add `EnsureLoaded` / lazy-init sequences to force that order.

# Tests

Keep test method names short and descriptive. Prefer names like `InitializeWithCorrectCharacterType`, `IgnoreWrongCharacterType`, or `ComputeSegments` over long Given/When/Then-style names.

Prefer focused tests with clear arrange/act/assert spacing. Use helpers or factories for repeated setup when Unity or Timberborn types require reflection or publicized DLL setup.

# Tools

Timberborn game source should be read from `ModdingAssetUpdater/out`, which contains decompiled game DLLs. When investigating game behavior, search and read that folder instead of decompiling assemblies on demand.

More exported Timberborn game data is stored at `D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources`. For example, the `Buildings` folder there contains game building blueprints that may be useful when comparing mod blueprints to vanilla behavior.

If `ModdingAssetUpdater/out` does not exist or is empty, ask the user to decompile the game source there first.

Leave `BackwardCompatibleTemplateNames` as `[]`. Do not add old template names unless the user explicitly asks. We are developing; save migration is not needed unless requested.

Game assemblies are publicized for mod projects. It is okay to reference publicized Timberborn types and members directly in code and Harmony patches instead of avoiding `internal` or private members with reflection-only approaches.

Timberborn grid coordinates use **X/Y for the horizontal plane and Z for vertical**. Unity is the opposite: **Y is up**, with X/Z as the horizontal plane. When converting grid coordinates or directions into Unity/world vectors, check `CoordinateSystem` and its namespace code first (`GridToWorld`, `GridToWorldCentered`, and the reverse). It likely already has the conversion needed, so avoid adding local helpers like `ToUnityDirection` unless the built-in conversion does not fit.

## Timbermesh models (read / edit)

When you need to **read or edit** a `.timbermesh` model (node hierarchy, materials, meshes, doors/props, etc.), use the `tmesh2glb` tool from `Timbermesh2Blender` — do **not** try to parse the binary by hand.

Working directory for dumps/packs: **`D:\Temp\TimbermeshModel`**.

```powershell
# From repo root. -c Release is required: the Debug build replaces args with a hardcoded Forester path.
dotnet run -c Release --project Timbermesh2Blender -- dump -i <model-or-folder> -o D:\Temp\TimbermeshModel -f
# Edit the produced *.timbermesh.json
dotnet run -c Release --project Timbermesh2Blender -- pack -i D:\Temp\TimbermeshModel\<name>.timbermesh.json -o D:\Temp\TimbermeshModel -f
```

The installed `tmesh2glb` tool is fine too. It does not have that Debug override.

Notes:

- Default dump keeps vertex attributes as **base64** (lossless). Use `--decode-vertices` only when you need numeric position/normal/uv arrays.
- Prefer structural edits in JSON (delete/rename/reparent nodes, swap material names). Use **glb** export + Blender for real mesh sculpting.
- After packing, you can preview with `tmesh2glb glb -i ... -r <Resources> -o D:\Temp\TimbermeshModel -f`.
- Game Resources path: `D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources`.
- **Folktails first.** Unless the user explicitly asks for Iron Teeth (or both factions), only dump / edit / pack **Folktails** models. Do the Iron Teeth pass later, after the Folktails model is approved.

### UV mapping

A generated building that looks clipped has a bad `uv0` rectangle. The face shows a chopped mix: part of a board, a blank pale patch, a hole, a metal trim cut in half. A pale or white face is the same bug. It is the empty corner of the picture, or another material left on a made-up rectangle. Fix every material on the model, not only the one that looked worst.

The material's picture is a collage of many separate pictures in one square. `uv0` picks a spot in that square. `(0,0)` is one corner of the whole square. `(1,1)` is the opposite corner. A face set to `(0,0)`–`(1,1)` stretches the entire collage across that one face.

Two fixes that still look clipped:

- Any rectangle you invent, even one that sits inside the square. On the belt, a wide patch of the wood picture removed the hole and still cut the grain off at the edge of the face. The metal bar was left on a homemade patch and stayed a pale clipped plate.
- Shrinking the coordinates yourself for the faction atlas. The game already does `uv * UVScale + UVOffset` in `AutoAtlasingPrefabOptimizer`. Write coordinates against the original picture.

What works is one vanilla face's rectangle, copied unchanged, per face:

- Dump a vanilla mesh that uses the same material, with `--decode-vertices`. Read one face's `uv0` and use that exact box.
- Wood (`BaseWood_*`, both factions): `Platform.Full.Folktails` on `BaseWood_Brown.Folktails`. One board is `u 0.7504–0.8128`, `v 0.5001–0.8753`. The next boards on that platform are the same height, stepped across in `u` (`0.8129–0.8753`, `0.8754–0.9378`, `0.9379–0.998`).
- Metal (`BaseMetal.Folktails`): `MetalPlatform3x3.Full.Folktails`. The repeated small plate is `u 0.595–0.656`, `v 0.008–0.242`.
- One face, one rectangle. A wider surface is several faces, each with the next vanilla rectangle. The platform top is a row of boards, not one face covering many boards.
- Stay on the copied numbers. `0.5001` instead of `0.5000` is already in them so the face does not pick up the neighbor.

Before packing, dump with `--decode-vertices` and check every material. A triangle whose `uv0` runs `0–1`, or whose box is larger than the vanilla face you copied, is still clipped.

## Buildings (Folktails first)

Unless the user explicitly asks for Iron Teeth (or both factions), only create and edit **Folktails** buildings. Do not create `<Building>.IronTeeth.blueprint.json` or an Iron Teeth collection until they say they are ready for the IT pass.

When adding a building, copy a placeholder icon **into that building's own folder** (for example `Buildings/Water/Pipes/NewPipe/NewPipeIcon.png`) and point `LabeledEntitySpec.Icon` at that local file. Do not reuse another building's icon path.
