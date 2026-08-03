# TMesh2Glb (`tmesh2glb`)

Convert Timberborn `.timbermesh` models to `.glb` for Blender.

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

## Usage

```powershell
tmesh2glb -i <input> -r <resources> -o <output> [-f]
```

| Argument | Short | Description |
|----------|-------|-------------|
| `--input` | `-i` | Timbermesh **file** or **folder** (searches recursively for `.timbermesh` / model `.prefab`) |
| `--resources` | `-r` | Game **Resources** folder (materials, textures, `.meta`, blueprints) |
| `--output` | `-o` | Folder to write `.glb` files into |
| `--flatten` | `-f` | Write all `.glb` files directly into the output folder (no nested folders) |
| `--help` | `-h` | Show help |
If an argument is omitted, the tool prompts for it in the console.

### Examples

Single model:

```powershell
tmesh2glb `
  -i "D:\...\Resources\Buildings\Wood\Forester\Forester.Folktails.Model.timbermesh" `
  -r "D:\...\Resources" `
  -o "D:\Temp\TimbermeshBlender"
```

Whole folder:

```powershell
tmesh2glb `
  -i "D:\...\Resources\Buildings\Wood" `
  -r "D:\...\Resources" `
  -o "D:\Temp\TimbermeshBlender"
```

By default, output keeps the relative folder layout under `-o` (e.g. `Forester\....glb`).  
With `-f` / `--flatten`, every file is written as `output\<name>.glb` only.

Flattened folder example:

```powershell
tmesh2glb `
  -i "D:\...\Resources\Buildings\Wood" `
  -r "D:\...\Resources" `
  -o "D:\Temp\TimbermeshBlender" `
  -f
```

Import into Blender by dropping the file into the 3D Viewport or using `File > Import > glTF 2.0 (.glb/.gltf)`.