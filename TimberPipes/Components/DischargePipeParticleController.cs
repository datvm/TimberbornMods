namespace TimberPipes.Components;

[AddTemplateModule2(typeof(DischargePipe))]
public class DischargePipeParticleController : TickableComponent, IAwakableComponent, IInitializableEntity, IPostLoadableEntity
{
    const string AttachmentId = "Water";

#nullable disable
    DischargePipe discharge;
    ParticlesRunner particlesRunner;
#nullable enable

    public void Awake()
    {
        discharge = GetComponent<DischargePipe>();
    }

    public void InitializeEntity()
    {
        particlesRunner = GetComponent<ParticlesCache>().GetParticlesRunner(AttachmentId);
    }

    public void PostLoadEntity()
    {
        UpdateParticles();
    }

    public override void Tick()
    {
        UpdateParticles();
    }

    void UpdateParticles()
    {
        if (discharge.IsEjecting)
        {
            particlesRunner.Play();
        }
        else
        {
            particlesRunner.Stop();
        }
    }
}
