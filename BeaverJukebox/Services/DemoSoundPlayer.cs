
namespace BeaverJukebox.Services;

public class DemoSoundPlayer(SoundEmitterRetriever soundEmitterRetriever) : IUnloadableSingleton
{

    GameObject? obj;

    GameObject FreshObj
    {
        get
        {
            if (obj)
            {
                UnityEngine.Object.Destroy(obj);
            }

            obj = new();
            return obj;
        }
    }

    public void Play(string soundName)
    {
        var emitter = soundEmitterRetriever.GetSoundEmitter(FreshObj);
        emitter.Start2D(soundName, 0);
    }

    public void StopAll()
    {
        _ = FreshObj;
        //var emitter = soundEmitterRetriever.GetSoundEmitter(obj);
        //var sounds = emitter._sounds;

        //foreach (var sound in sounds._sounds.Keys)
        //{
        //    emitter.Stop(sound);
        //}  
    }

    public void PlayOriginal(string soundName) => Play(soundName + ModdableAudioClipService.OriginalSoundPostfix);

    public void Unload()
    {
        UnityEngine.Object.Destroy(obj);
    }
}
