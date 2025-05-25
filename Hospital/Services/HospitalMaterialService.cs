namespace Hospital.Services;

public class HospitalMaterialService : ILoadableSingleton, IUnloadableSingleton
{

    public Material HospitalIconMaterial { get; private set; } = null!;

    public void Load()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");

        HospitalIconMaterial = new(shader);
        HospitalIconMaterial.SetColor("_BaseColor", new Color(0.8f, 0.2f, 0.2f, 1f));
    }

    public GameObject CreateIconPart(bool rotate, Transform parent, int height)
    {
        var iconPart = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        iconPart.transform.parent = parent;
        iconPart.transform.localPosition = new(.5f, height - .5f, .5f);
        iconPart.transform.localScale = new(.5f, .5f, .5f);

        if (rotate)
        {
            iconPart.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }

        iconPart.GetComponent<Renderer>().material = HospitalIconMaterial;

        return iconPart;
    }

    public void Unload()
    {
        UnityEngine.Object.Destroy(HospitalIconMaterial);
    }
}
