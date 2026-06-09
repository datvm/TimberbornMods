# SystemFileDialog

## Purpose

A thin cross-platform abstraction over the native OS "open file" and "save file" dialogs, so mod code can prompt the user for a path without bundling a UI. The interface exposes two methods (`ShowOpenFileDialog`, `ShowSaveFileDialog`), each accepting an optional filter string and returning the chosen path or `null` on cancel. There are three concrete implementations — Windows, Linux, and macOS — selected automatically at runtime based on the current OS.

The Windows implementation uses P/Invoke against `comdlg32.dll`; Linux shells out to `zenity` or `kdialog`; macOS shells out to `osascript` (AppleScript).

## Key types

- **`ISystemFileDialogService`**: the contract. Two nullable-returning methods plus a **static** `TryBinding(Configurator)` that selects the correct implementation by runtime OS and binds it as a singleton. Logs an error for unsupported platforms.
- **`WindowsSystemFileDialogService`** (`namespace TimberUi.Services.SystemFileDialog`): delegates to the static `WindowsFileDialogs`. `ParseFilter` converts a `;`-delimited extension string into the Win32 double-null-terminated filter format.
- **`WindowsFileDialogs`** (static helper, same file): the actual P/Invoke layer. Declares `GetOpenFileName`/`GetSaveFileName` from `comdlg32.dll`, the `OpenFileName` (OPENFILENAME) struct, and the `ShowOpen/SaveFileDialog` methods that populate the struct and invoke the native function.
- **`LinuxSystemFileDialogService`**: tries `zenity` first, falls back to `kdialog`, each detected by `File.Exists` on the tool's path. Builds tool-specific filter arguments and shells out via `Process.Start`, reading one line of stdout.
- **`MacOSSystemFileDialogService`**: builds an AppleScript snippet (`choose file` / `choose file name`) and runs it through `osascript -e`, returning the POSIX path of the selection.

## How it fits together

- **Selection**: `ISystemFileDialogService.TryBinding` is the single entry point for wiring. It checks `RuntimeInformation.IsOSPlatform(...)` for Windows → Linux → OSX and calls `configurator.TryBind<ISystemFileDialogService>()?.To<...>().AsSingleton()`. `TryBind` returns `null` if already bound, so a consumer can pre-bind their own implementation without being overridden. A consumer's `Configurator` calls `ISystemFileDialogService.TryBinding(this)` during setup.
- **Filter convention**: callers pass a `;`-delimited list such as `".json;.txt"`. Each platform reshapes it:
  - Windows: `ParseFilter` produces `"{input}\0*ext;*ext...\0\0"` — the raw input as the description and a `*`-prefixed pattern list as the pattern, both null-terminated (Win32 OPENFILENAME format). `null` → `DefaultFilter` = `"All Files\0*.*\0\0"`.
  - Linux/zenity: `--file-filter="Supported files | *.ext *.ext2"`.
  - Linux/kdialog: `"*.ext *.ext2|Supported files"`.
  - macOS: an AppleScript list `{"ext","ext2"}` passed as the `of type` clause.
- **Invocation per platform**:
  - **Windows**: populates an `OpenFileName` managed class (marshaled as the OPENFILENAME struct), pre-allocates an `lpstrFile` buffer, sets `nFilterIndex = 1`, and flags. Open uses `OFN_NOCHANGEDIR | OFN_EXPLORER`; Save uses `OFN_OVERWRITEPROMPT | OFN_EXPLORER`. Returns `lpstrFile` on success, `null` on cancel.
  - **Linux**: `RunProcess` starts the tool with `RedirectStandardOutput`, `UseShellExecute = false`, `CreateNoWindow = true`, reads a single line of stdout, waits for exit, and returns the line only if exit code == 0 (zenity returns 1 on cancel).
  - **macOS**: builds a multi-line AppleScript wrapped in try/error, runs `osascript -e '<script>'`, reads all stdout, trims, and returns on exit 0 with non-empty output.

## Dependencies & patterns

- **`System.Runtime.InteropServices`**: `RuntimeInformation`/`OSPlatform` for platform selection; `DllImport`, `StructLayout`, `Marshal`, `CharSet.Auto` for the Windows P/Invoke.
- **`System.Diagnostics.Process`**: Linux and macOS shell-out.
- **DI**: the Bartholomew/IoC `Configurator` with `TryBind(...).To(...).AsSingleton()`.
- **Try/fallback pattern** on Linux (zenity → kdialog). **AppleScript-as-string** on macOS. **Raw Win32 interop** on Windows.
