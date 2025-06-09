
namespace ConfigurableShantySpeaker.Components;

public class ShantySpeakerConfigComponent : BaseComponent, IPersistentEntity, IFinishedStateListener
{
    static readonly ComponentKey SaveKey = new("ShantySpeakerConfig");
    static readonly PropertyKey<int> VolumeKey = new("Volume");
    static readonly PropertyKey<bool> StoppedKey = new("Stopped");
    static readonly PropertyKey<string?> SoundNameKey = new("SoundName");

    public static int DefaultVolume = AudioSourceFactory.SoundCutOffDistance;

    public int Volume { get; private set; } = DefaultVolume;
    public bool Stopped { get; private set; }

    public bool IsPlaying { get; private set; }
    public bool IsFinished { get; private set; }

    public string? SoundName { get; private set; }
    public string EffectiveSoundName => SoundName ?? DefaultSoundName;

    string DefaultSoundName => spec.SoundName;
    string? playingSoundName;

#nullable disable
    FinishableBuildingSoundPlayerSpec spec;
    SoundSystem soundSystem;
    ShantySoundService shantySoundService;
#nullable enable

    [Inject]
    public void Inject(ISoundSystem soundSystem, ShantySoundService shantySoundService)
    {
        this.soundSystem = (SoundSystem)soundSystem;
        this.shantySoundService = shantySoundService;
    }

    public void Awake()
    {
        spec = GetComponentFast<FinishableBuildingSoundPlayerSpec>();
    }

    public void Start()
    {
        if (Volume != DefaultVolume)
        {
            SetVolume(Volume);
        }
    }

    public void SetVolume(int volume)
    {
        Volume = volume;
        SetMusicVolume();
    }

    public void SetSoundName(string? name)
    {
        var wasPlaying = IsPlaying;
        StopPlaying();
        SoundName = name;

        if (wasPlaying)
        {
            StartPlaying();
        }
    }

    public void Play()
    {
        Stopped = false;

        if (IsFinished && !IsPlaying)
        {
            StartPlaying();
        }
    }

    public void Stop()
    {
        Stopped = true;
        StopPlaying();
    }

    void StartPlaying()
    {
        IsPlaying = true;

        var obj = GameObjectFast;
        var sound = EffectiveSoundName;
        playingSoundName = sound;
        soundSystem.LoopSingle3DSound(obj, sound, 128);
        SetMusicVolume();
        soundSystem.SetCustomMixer(obj, sound, MixerNames.BuildingMixerNameKey);
    }

    void StopPlaying()
    {
        IsPlaying = false;

        if (playingSoundName is not null)
        {
            soundSystem.StopSound(GameObjectFast, playingSoundName);
            playingSoundName = null;
        }
    }

    void SetMusicVolume()
    {
        if (!IsPlaying) { return; }

        var emitter = soundSystem._soundEmitterRetriever.GetSoundEmitter(GameObjectFast);

        var audioSrc = emitter._loopingSoundPlayer._currentlyPlaying;
        audioSrc.rolloffMode = AudioRolloffMode.Linear;
        audioSrc.minDistance = 0;
        audioSrc.maxDistance = Volume;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(VolumeKey))
        {
            Volume = s.Get(VolumeKey);
        }

        if (s.Has(StoppedKey))
        {
            Stopped = s.Get(StoppedKey);
        }

        if (s.Has(SoundNameKey))
        {
            SoundName = s.Get(SoundNameKey);

            Debug.Log(SoundName);

            if (SoundName.Length == 0
                || !shantySoundService.HasSound(SoundName))
            {
                SoundName = null;
            }
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(VolumeKey, Volume);
        s.Set(StoppedKey, Stopped);
        s.Set(SoundNameKey, SoundName ?? "");
    }

    public void OnEnterFinishedState()
    {
        IsFinished = true;

        if (!Stopped)
        {
            Play();
        }
    }

    public void OnExitFinishedState()
    {
        IsFinished = false;
        StopPlaying();
    }

    public void ResetSpeaker()
    {
        StopPlaying();
        SoundName = null;
        Volume = DefaultVolume;

        Play();
    }
}
