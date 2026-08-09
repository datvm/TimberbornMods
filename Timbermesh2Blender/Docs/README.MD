# TMesh2Glb (`tmesh2glb`)

Convert Timberborn `.timbermesh` models to:

- **`.glb`** for Blender (preview / visual edit)
- **`.timbermesh.json`** for machine-friendly AI / text editing (dump → edit → pack)

## Install / update

Requires the **.NET 10** SDK/runtime.

```powershell
# Install
dotnet tool install -g TMesh2Glb

# Update
dotnet tool update -g TMesh2Glb
```

### Uninstall

```powershell
dotnet tool uninstall -g TMesh2Glb
```

## Commands

```powershell
tmesh2glb glb  -i <input> -r <resources> -o <output> [-f]
tmesh2glb dump -i <input> -o <output> [-f] [--decode-vertices]
tmesh2glb pack -i <input> -o <output> [-f]
```

Legacy (no subcommand) still runs **glb** export when `-i/-r/-o` are passed.

| Command | Description |
|---------|-------------|
| `glb` | Convert to `.glb` for Blender |
| `dump` | Export `.timbermesh` → `.timbermesh.json` (AI/text intermediate) |
| `pack` | Pack `.timbermesh.json` → `.timbermesh` |

### Shared options

| Argument | Short | Description |
|----------|-------|-------------|
| `--input` | `-i` | Input **file** or **folder** |
| `--output` | `-o` | Output folder |
| `--flatten` | `-f` | Write files directly into the output folder (no nested folders) |
| `--help` | `-h` | Show help |

### GLB-only options

| Argument | Short | Description |
|----------|-------|-------------|
| `--resources` | `-r` | Game **Resources** folder (materials, textures, `.meta`, blueprints) |

### Dump-only options

| Argument | Description |
|----------|-------------|
| `--decode-vertices` | Expand float attributes (`position`, `normal`, …) to number arrays instead of base64 (larger; better for inspecting geometry) |

If an argument is omitted, the tool prompts for it in the console.

---

## GLB export (Blender)

```powershell
tmesh2glb glb `
  -i "D:\...\Resources\Buildings\Wood\Forester\Forester.Folktails.Model.timbermesh" `
  -r "D:\...\Resources" `
  -o "D:\Temp\TimbermeshBlender"
```

Import into Blender by dropping the file into the 3D Viewport or using `File > Import > glTF 2.0 (.glb/.gltf)`.

---

## JSON dump / pack (AI editing)

Working folder recommended for AI sessions: `D:\Temp\TimbermeshModel`.

### Dump

```powershell
tmesh2glb dump `
  -i "D:\...\Forester.Folktails.Model.timbermesh" `
  -o "D:\Temp\TimbermeshModel" `
  -f
```

Produces `Forester.Folktails.Model.timbermesh.json` with:

- Node hierarchy (`index`, `parent`, `name`, transforms)
- Materials + mesh index lists
- Vertex attributes as **base64** by default (lossless, compact)
- Summary fields: `materials`, `bounds`, `triangleCount`

### Edit

Edit the JSON (remove a door node, swap a material name, reparent, rename, etc.).

When deleting nodes:

- Remove the node entry from `nodes`
- Leave remaining `index` values as-is **or** renumber 0..n-1 — **pack remaps `parent` indices** from declared `index` values to packed array order
- Do not leave a `parent` pointing at a deleted index

### Pack

```powershell
tmesh2glb pack `
  -i "D:\Temp\TimbermeshModel\Forester.Folktails.Model.timbermesh.json" `
  -o "D:\Temp\TimbermeshModel" `
  -f
```

### Round-trip tip

After packing, re-export to GLB to visually verify:

```powershell
tmesh2glb glb -i "D:\Temp\TimbermeshModel\....timbermesh" -r "D:\...\Resources" -o "D:\Temp\TimbermeshModel" -f
```

---

## JSON format notes

- `format`: always `timbermesh-json`
- Coordinates stay in **Timbermesh / Unity** space (not glTF-mirrored)
- Material strings are Unity material **names** (must match game resources)
- Attribute encodings:
  - `base64` (default): raw little-endian buffer in `data`
  - `float-array` (`--decode-vertices`): flat floats in `values`
