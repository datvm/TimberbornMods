namespace BathBombs.Services;

public class BathBombService(
    EntityService entities,
    IBlockService blocks,
    IWaterService waterService,
    ExplosionSoundPlayer explosionSoundPlayer,
    IInstantiator instantiator,
    IAssetLoader assets,
    ILoc t
) : ITickableSingleton
{
    const int SoundEffectTicks = 10;
    readonly LinkedList<SoundEffectObject> soundEffects = [];

    public string GetDescription(float amount) => t.T("LV.BBb.BurstAmount", amount);

    public void Detonate(BathBombComponent comp)
    {
        var coords = comp.Coordinates;
        var water = comp.Water;
        var prefabPath = comp.ExplosionPrefabPath;

        var effCoords = comp.GetComponent<BlockObjectCenter>().WorldCenter;

        RemoveBathBomb(comp);
        TriggerNeighbors(coords);
        PlayEffect(effCoords, prefabPath);
        AddWater(coords, water);
    }

    void PlayEffect(Vector3 coords, string prefabPath)
    {
        var prefab = assets.Load<GameObject>(prefabPath);
        var go = instantiator.Instantiate(prefab, null);
        go.transform.position = coords;

        var obj = new SoundEffectObject(go);
        explosionSoundPlayer.Play(go);
        soundEffects.AddLast(obj);
    }

    public void Tick()
    {
        if (soundEffects.Count == 0) { return; }

        var curr = soundEffects.First;
        while (curr is not null)
        {
            var next = curr.Next;

            if (curr.Value.TickPassed >= SoundEffectTicks)
            {
                curr.Value.Remove();
                soundEffects.Remove(curr);
            }

            curr = next;
        }
    }

    void AddWater(Vector3Int coords, BathBombWater water)
    {
        var (amount, contaminated) = water;

        Action<Vector3Int, float> addWater = contaminated ? waterService.AddContaminatedWater : waterService.AddCleanWater;
        addWater(coords, amount);
    }

    void RemoveBathBomb(BathBombComponent comp)
    {
        comp.GetComponent<Deconstructible>().DisableDeconstruction();
        entities.Delete(comp);
    }

    void TriggerNeighbors(Vector3Int src)
    {
        foreach (var n in Deltas.Neighbors4Vector3Int)
        {
            var coord = src + n;

            var bo = blocks.GetFirstObjectWithComponentAt<BathBombComponent>(coord);
            if (bo)
            {
                bo.Trigger();
            }
        }
    }

    class SoundEffectObject(GameObject go)
    {

        public GameObject GameObject { get; } = go;
        public int TickPassed { get; set; }

        public void Remove()
        {
            UnityEngine.Object.Destroy(GameObject);
        }

    }

}
