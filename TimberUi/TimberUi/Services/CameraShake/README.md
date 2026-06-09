# CameraShake

## Purpose
A small two-class subsystem that lets mods shake the game camera (e.g. for impacts, explosions, feedback) and lets the player turn the effect off. `CameraShakeService` drives the per-frame jitter of the camera target position and angles; `CameraShakeSettingService` owns a persisted boolean preference and injects a toggle into the game's settings UI.

## Key types
- **`CameraShakeService`** (`CameraShakeService.cs`) — `IUpdatableSingleton`. Public API: `MoveTo(SelectableObject)` (centers the camera on a target via `CameraTargeter`, returns `this` for chaining) and `Shake(float duration, float strength)` (amplitude = `0.05f * strength`). Each frame, `UpdateSingleton` decrements `duration` by `Time.unscaledDeltaTime`, applies a random `insideUnitSphere * amplitude` offset to `cameraService.Target` plus random vertical/horizontal angle tilt, and restores the original transform when time runs out (`StopShaking`). Both `MoveTo` and `Shake` early-return when shake is disabled.
- **`CameraShakeSettingService`** (`CameraShakeSettingService.cs`) — `ILoadableSingleton`. Persists `IsDisabled` under settings key `TimberUi.CameraShakeDisabled`. Exposes `IsDisabled` (get) and `SetDisabledCamera(bool)`, plus an `OnCameraShakeDisabledChanged` event. On `Load` it reads the saved bool and calls `AddSettingOption`, which adds a toggle ("LV.TimberUi.DisableCameraShake") into the settings box and inserts it after the existing "UIScaleFactor" row.

## How it fits together
`CameraShakeService` depends on `CameraShakeSettingService` and gates all of its behavior on `IsDisabled` — so disabling the setting suppresses both shaking and `MoveTo`. The setting service stands alone otherwise (it wires the player-facing toggle), and the shake service is the runtime consumer. State (`originalPos`/`originalRot`) is captured the first time a shake starts and reused if `Shake` is called again mid-shake.

## Dependencies & patterns
- `CameraShakeService` ← `CameraService` (reads/writes `Target`, `VerticalAngle`, `HorizontalAngle`), `CameraShakeSettingService`, `CameraTargeter`.
- `CameraShakeSettingService` ← `ISettings` (`GetSafeBool`/`SetBool`), `ISettingsController`, `ILoc`.
- Uses `Time.unscaledDeltaTime` so shake runs even while the game is paused/slowed.
- `StopShaking` is also reachable publicly; it restores the original camera transform and clears the active-shake state.

## Usage notes
- Calling `Shake` again before the previous shake finishes extends the duration without resetting the baseline origin — the camera will restore to the position captured at the start of the first shake. This is the expected behavior when chaining rapid shakes.
