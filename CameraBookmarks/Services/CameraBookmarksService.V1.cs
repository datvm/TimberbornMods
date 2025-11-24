namespace CameraBookmarks.Services;

public class CameraBookmarksService(
    InputService inputService,
    CameraService cameraService,
    ISingletonLoader loader,
    ILoc t,
    QuickNotificationService quickNotificationService
) : ILoadableSingleton, ISaveableSingleton, IInputProcessor
{
    public const int MaxBookmarks = 10;
    const string SaveHotkeyId = "CameraBookmarkSave{0}";
    const string JumpHotkeyId = "CameraBookmarkJump{0}";

    static readonly SingletonKey SaveKey = new(nameof(CameraBookmarks));
    static readonly PropertyKey<string> BookmarksKey = new("Bookmarks");

    readonly CameraState?[] states = new CameraState?[MaxBookmarks];

    public void Load()
    {
        LoadSavedData();
        inputService.AddInputProcessor(this);
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(BookmarksKey))
        {
            var stored = JsonConvert.DeserializeObject<SerializableCameraState?[]>(s.Get(BookmarksKey)) ?? [];

            var count = Math.Min(stored.Length, MaxBookmarks);
            for (int i = 0; i < count; i++)
            {
                states[i] = stored[i]?.ToCameraState();
            }
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(BookmarksKey, JsonConvert.SerializeObject(states.Select(SerializableCameraState.FromCameraState)));
    }

    public bool ProcessInput()
    {
        for (int i = 0; i < MaxBookmarks; i++)
        {
            var saveHotkey = string.Format(SaveHotkeyId, i);
            if (inputService.IsKeyDown(saveHotkey))
            {
                SaveBookmark(i);
                return true;
            }

            var jumpHotkey = string.Format(JumpHotkeyId, i);
            if (inputService.IsKeyDown(jumpHotkey))
            {
                JumpTo(i);
                return true;
            }
        }

        return false;
    }

    public void SaveBookmark(int index)
    {
        var state = cameraService.GetCurrentState();
        states[index] = state;

        quickNotificationService.SendNotification(t.T("LV.CBk.CameraSaved", index));
    }

    public void JumpTo(int index)
    {
        var state = states[index];
        if (state is null)
        {
            quickNotificationService.SendNotification(t.T("LV.CBk.NoBookmark", index));
        }
        else
        {
            cameraService.RestoreState(state.Value);
        }
    }

}

record SerializableCameraState(SerializableVector3 Target, float ZoomLevel, float HorizontalAngle, float VerticalAngle)
{

    public static SerializableCameraState? FromCameraState(CameraState? state)
        => state.HasValue ? FromCameraState(state.Value) : null;

    public static SerializableCameraState FromCameraState(CameraState state)
        => new(state.Target, state.ZoomLevel, state.HorizontalAngle, state.VerticalAngle);

    public CameraState ToCameraState() => new(Target, ZoomLevel, HorizontalAngle, VerticalAngle);

}

readonly record struct SerializableVector3(float X, float Y, float Z)
{
    public static implicit operator SerializableVector3(Vector3 v) => new(v.x, v.y, v.z);
    public static implicit operator Vector3(SerializableVector3 v) => new(v.X, v.Y, v.Z);
}