# Blueprint generation

Speed-variant conveyor blueprints are generated. Edit `ConveyorBeltBlueprintScript`, then run it. Do not hand-edit the files it writes.

From the repo root:

```powershell
dotnet run --project ConveyorBeltBlueprintScript/ConveyorBeltBlueprintScript.csproj -- ConveyorBeltBlueprintScript/input.json
```

`Templates\` holds the JSON. `input.json` lists the speeds and the properties each export replaces. Output goes to `ConveyorBelt/Root`.

The script writes every conveyor blueprint: belts, corners, risers, the impermeable belt, lift in, lift out, merger, splitter, smart splitter, the settings spec, and the Folktails collection. Icon art is in `Templates/Icons`. It stamps `BeltBorder.png` with Roboto Black `>` marks, one more per speed, onto the Log, Plank, Late, and End group buttons. It stamps `BeltIcon.png` onto belts, `CornerBeltIcon.png` onto corners, `BeltUpIcon.png` onto risers up, and `BeltDownIcon.png` onto risers down, the same way. Risers sit in those speed groups. `ImpermeableIcon.png` is copied as-is. The root conveyor group icon is left alone.
