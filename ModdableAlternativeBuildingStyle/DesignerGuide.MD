# Adding a style

This part is JSON and a `.timbermesh`. You add a look to a building that already exists.

You do three things:

1. Put the finished `.timbermesh` next to that building.
2. Write a style blueprint that shows only the finished body.
3. Append a name and that blueprint's path on the building's partial blueprint.

A **style blueprint** is the JSON file the dropdown loads. It is a new file. It is the finished look, and it is separate from the building's own blueprint.

## A name and a blueprint path

A style is a name plus a blueprint path. You append it on the building's partial blueprint, in `AlternativeBuildingStyleSpec`, with `Styles#append`.

The Folktails levee is the worked example. Its partial blueprint is `Buildings/Landscaping/Levee/Levee.Folktails.blueprint.json`:

```json
{
    "AlternativeBuildingStyleSpec": {
        "Styles#append": [
            {
                "Name": "Metal",
                "BlueprintPath": "Buildings/Landscaping/Levee/MetalLeveeStyles.Folktails.blueprint"
            },
            {
                "Name": "Classic",
                "BlueprintPath": "Buildings/Landscaping/Levee/ClassicLeveeStyles.Folktails.blueprint"
            }
        ]
    }
}
```

`Name` is the text in the dropdown. `BlueprintPath` is the style blueprint, without the `.json` extension.

The partial blueprint uses the same path as the vanilla building, so the game merges your file onto it. `Styles#append` adds entries. The rest of the building stays the vanilla blueprint. Put one object in the array per style. Metal and Classic are two styles on the same levee.

The building panel shows a **Building Style** dropdown when that list has at least one entry. The first entry is **Original**. Its value is an empty string, and that empty string means the vanilla look. The other names are listed in alphabetical order after it.

## Where the files go

Make the mod folder yourself. Inside it, use the same relative paths the game loads. Drop that folder into the Timberborn Mods directory. That is the whole install. A JSON style has no compile step.

```
MyLeveeStyle/
  manifest.json
  Buildings/
    Landscaping/
      Levee/
        Levee.Folktails.blueprint.json
        MetalLeveeStyles.Folktails.blueprint.json
        MetalLevee.Folktails.Model.timbermesh
```

`Levee.Folktails.blueprint.json` is the partial blueprint from the previous section. `MetalLeveeStyles.Folktails.blueprint.json` is the style blueprint. The mesh file is the model that blueprint names. On disk the file ends in `.timbermesh`. In JSON, leave that extension off:

```json
"TimbermeshSpec": {
  "Model": "Buildings/Landscaping/Levee/MetalLevee.Folktails.Model"
}
```

`manifest.json` sits in the mod folder. A style pack needs `ModdableAlternativeBuildingStyle` in `RequiredMods` so the dropdown and the merge exist:

```json
"RequiredMods": [
  { "Id": "ModdableAlternativeBuildingStyle" }
]
```

A faction style is the same kind of folder: JSON, plus a path to the other faction's model. The Forester files look like this:

```
MyFactionStyles/
  manifest.json
  Buildings/
    Wood/
      Forester/
        Forester.Folktails.blueprint.json
        Forester.Folktails.ITStyle.blueprint.json
```

The style blueprint's `Model` is `Buildings/Wood/Forester/Forester.IronTeeth.Model`. That model already ships with the game, so the folder does not need a second copy of it.

## The finished body

The style blueprint describes the finished body and nothing else.

Use `TimbermeshSpec` and `CollidersSpec` at the root when the building is one finished mesh. The levee, the dam, and the floodgate are that shape. Metal's style blueprint is `Buildings/Landscaping/Levee/MetalLeveeStyles.Folktails.blueprint.json`:

```json
{
  "TimbermeshSpec": {
    "Model": "Buildings/Landscaping/Levee/MetalLevee.Folktails.Model"
  },
  "CollidersSpec": {
    "BoxColliders": [
      {
        "Center": {
          "X": 0.5,
          "Y": 0.510495067,
          "Z": 0.5
        },
        "Size": {
          "X": 0.9,
          "Y": 1.02099013,
          "Z": 0.9
        }
      }
    ],
    "SphereColliders": [],
    "CapsuleColliders": []
  }
}
```

Use a `Children` map when the finished model is several pieces and the game shows or hides those pieces by name. A hedge, a fence, a path, and a gate are that shape. Each key is a child name that already exists on the finished model.

## Connecting buildings

The game picks a child from the neighbor pattern: a lone piece, a corner, a straight run, a crossing, and so on. Your style blueprint needs a child for every one of those names. A missing child comes up empty. The game still turns the original piece off, and the style has no mesh to put there.

The wheat hedge style is `Buildings/Decoration/Hedge/WheatHedgeStyles.Folktails.blueprint.json`. The same six keys sit under `#Finished` in the vanilla `Hedge.Folktails.blueprint.json`:

```json
{
  "Children": {
    "Hedge0000": {
      "TimbermeshSpec": {
        "Model": "Buildings/Decoration/Hedge/WheatHedge0000.Folktails.Model"
      }
    },
    "Hedge0001": {
      "TimbermeshSpec": {
        "Model": "Buildings/Decoration/Hedge/WheatHedge0001.Folktails.Model"
      }
    }
  }
}
```

The file continues with `Hedge0011`, `Hedge0111`, `Hedge1010`, and `Hedge1111`. Copy the names from the building. The bits in the name are the connection, and a hedge's set is not the same set as a path's.

A faction style does the same for the wood fence. `Buildings/Decoration/WoodFence/WoodFence.Folktails.ITStyle.blueprint.json` has `WoodFence0000`, `WoodFence0001`, `WoodFence0011`, `WoodFence0111`, `WoodFence1010`, and `WoodFence1111`, each with the Iron Teeth model.

The path's finished children are `DirtPath0000`, `DirtPath0010`, `DirtPath0011`, `DirtPath0111`, `DirtPath1010`, and `DirtPath1111`. The gate style lists those path pieces and the two door pieces. From `Buildings/Paths/Gate/Gate.Folktails.ITStyle.blueprint.json`:

```json
"Children": {
    "#Open": {
        "TimbermeshSpec": {
            "Model": "Buildings/Paths/Gate/GateOpen.IronTeeth.Model"
        }
    },
    "#Closed": {
        "TimbermeshSpec": {
            "Model": "Buildings/Paths/Gate/GateClosed.IronTeeth.Model"
        }
    },
    "DirtPath0000": {
        "TimbermeshSpec": {
            "Model": "Buildings/Paths/Path/DirtPath0000.Model"
        }
    }
}
```

`#Open` and `#Closed` are the names in the gate's `OpenModelName` and `ClosedModelName`. The rest of that file repeats the other `DirtPath` children. Read the finished model, then give the style one child per name you found.

## Colliders

Copy the colliders from the building you are restyling. The footprint and the entrance stay where the game already expects them.

For a single body, copy `CollidersSpec` from the finished object. The Metal levee box above is the box on `#Finished` in the vanilla `Levee.Folktails.blueprint.json`: center `(0.5, 0.510495067, 0.5)`, size `(0.9, 1.02099013, 0.9)`. The Metal floodgate style copies that building's box and its capsule the same way.

A connecting child keeps the colliders already on that child. The wheat hedge style supplies a `TimbermeshSpec` per child and no new collider. The gate is both at once: `Gate.Folktails.ITStyle.blueprint.json` has a root `CollidersSpec` for the gate body, then the `Children` map for the door and the path.

## Node names

A node whose name starts with `#` is a part the game already moves or shows. Give your model the matching name and that part follows the original motion.

An exact name matches as itself. The lever's on pose is `#On` on both factions. `OnModelName` is `#On` in `Lever.Folktails.blueprint.json` and in `Lever.IronTeeth.blueprint.json`.

A faction token matches the other faction. `Folktails` swaps with `IronTeeth` anywhere in the name. `FT` swaps with `IT` when it is its own token. The chronometer hand is `#HandFT` on Folktails and `#HandIT` on Iron Teeth (`HandName` on each blueprint). A floodgate frame follows the same way when the two names differ only by that swap. The game keeps moving the original node, and the other faction's mesh is parented under it.

A Blender suffix is a different name. `#Turret` and `#Turret.005` do not match. `#Barrel` and `#Barrel.003` do not match either. Those need the firework launcher spec in the next part.

## What a JSON style leaves alone

A style blueprint changes the finished body. The building's workplace, cost, and recipes stay as they are. These also stay with the original building, so plan on them staying vanilla:

- A wonder's staged models.
- The worker slot count. The building keeps its own slots. A mesh with a different number of spots leaves that count as it is.
- A growable's life stages.
- The construction scaffold. `#Unfinished` stays up for the whole build. The style has no scaffold of its own.
- Piles of stored goods.
- Pipes, shafts, and particles.

When the look depends on one of those, or on a node name the swap cannot find, use a listener.

# Custom behavior

A JSON style needs no build. Read this part when a JSON style cannot finish the job.

A listener is code you compile in your own mod. The classes below are examples of the behavior.

## A listener

A **listener** is a component on the same building. It runs after a style is applied, and again after the style is restored to Original. Implement `IAlternativeBuildingStyleListener` on that building:

```csharp
public interface IAlternativeBuildingStyleListener
{
    void OnAlternativeStyleChanged(AlternativeBuildingStyleComponent building, string? style);
}
```

`style` is the selected name. It is null when the player picks Original.

The building raises `AlternativeStyleChanged` after the listeners:

```csharp
public event EventHandler<string?>? AlternativeStyleChanged;

void NotifyStyleChanged()
{
    var style = AlternativeStyle;
    foreach (var listener in listeners)
    {
        listener.OnAlternativeStyleChanged(this, style);
    }

    AlternativeStyleChanged?.Invoke(this, style);
}
```

Use a listener when the style has to retarget a part, or play a motion the model does not already contain. Use the event when something off the building needs the same moment.

`Awake` only grabs references. A placement preview runs `Awake` and skips `InitializeEntity`. Read the spec, register for events, and capture transforms in `InitializeEntity`, or in `OnAlternativeStyleChanged` once a style is on the building.

## Inventor weather station

The component sits on the Folktails Inventor, and it acts only when the selected style name is `Weather Station`.

The partial blueprint is `Buildings/Science/Inventor/Inventor.Folktails.blueprint.json`, in the mod folder with your other blueprints. The empty spec is what attaches the component. The class under it is an example of that component:

```json
{
  "AlternativeBuildingStyleSpec": {
    "Styles#append": [
      {
        "Name": "Weather Station",
        "BlueprintPath": "Buildings/Science/Inventor/WeatherStationInventorStyles.Folktails.blueprint"
      }
    ]
  },
  "InventorWeatherVaneSpec": {}
}
```

```csharp
[AddTemplateModule2(typeof(InventorWeatherVaneSpec))]
public class InventorWeatherVane(WindService wind, EventBus events)
    : BaseComponent, IInitializableEntity, IDeletableEntity, IAlternativeBuildingStyleListener
{
    const string StyleName = "Weather Station";
    const string ArrowName = "WeatherVaneWindArrow";

    public void InitializeEntity()
    {
        events.Register(this);
        if (SelectedStyle() == StyleName)
        {
            CaptureArrow(Transform);
        }
    }

    public void OnAlternativeStyleChanged(AlternativeBuildingStyleComponent building, string? style)
    {
        if (style != StyleName)
        {
            arrow = null;
            return;
        }

        CaptureArrow(building.Transform);
    }
}
```

The model has two arrows. `WeatherVaneWorkArrow` carries a node animation, and the inventor's existing work animation plays it, so that arrow spins while the inventor works. The component never looks that arrow up.

`WeatherVaneWindArrow` is the upper arrow. It has no node animation. While Weather Station is selected, `Align` aims it from `WindService.WindDirection`, including when a `WindChangedEvent` arrives:

```csharp
var direction = wind.WindDirection;
// WindRotationAnimator yaws by SignedAngle(Vector2.down, wind), which aims local +Z along this vector.
// The arrow head sits on local -Z, the same way #WindLogMisc points.
var forward = CoordinateSystem.GridToWorld(new Vector3(direction.x, -direction.y, 0f));
arrow.rotation = Quaternion.LookRotation(forward, Vector3.up);
```

Any other style, including Original, drops the reference and leaves the arrow alone.

## Floodgate

The door the game moves is named on the building. The Folktails floodgate keeps `FloodgateAnimationControllerSpec.GateName` at `#Floodgate1x1FolktailsMisc`.

The Iron Teeth style passes the other door on the style blueprint. From `Buildings/Landscaping/Floodgate/Floodgate.Folktails.ITStyle.blueprint.json`:

```json
"FloodgateAnimationStyleSpec": {
    "GateName": "#Floodgate1x1IronTeethMisc"
}
```

`FloodgateStyleListener` is on the floodgate. While that style is active it finds the named door and makes it the controller's `Gate`, then moves it to the current height:

```csharp
var gate = VisibleGate(building, style) ?? BuildingGate(animation);
if (!gate)
{
    return;
}

animation.Gate = gate;
var height = floodgate.IsInputOn ? floodgate.AutomationHeight : floodgate.Height;
animation.MoveGateInstantly(height);
```

Later height changes write to `animation.Gate`, so they move the visible door.

Original and Metal have no `FloodgateAnimationStyleSpec`. `VisibleGate` finds nothing, and `BuildingGate` keeps the building's own gate name.

## Firework launcher

This is the same idea as the floodgate door. Folktails aims `#Turret` and `#Barrel` (`FireworkLauncherSpec` on `FireworkLauncher.Folktails.blueprint.json`). The Iron Teeth model uses the Blender names `#Turret.005` and `#Barrel.003`.

The Folktails building's Iron Teeth style names those suffixes in `FireworkLauncher.Folktails.ITStyle.blueprint.json`:

```json
"FireworkLauncherAnimationStyleSpec": {
    "Turret": "#Turret.005",
    "Barrel": "#Barrel.003"
}
```

The other direction, `FireworkLauncher.IronTeeth.FTStyle.blueprint.json`, passes `#Turret` and `#Barrel`. `FireworkLauncherStyleListener` writes the named transforms onto the launcher while that style is active. Original has no spec, so the building's own turret and barrel stay.
