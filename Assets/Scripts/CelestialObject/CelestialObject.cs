using UnityEngine;

public class CelestialObject : ATeam
{
    private Type type;
    private bool autodestruction = false;
    private float autodestructionTimer;
    private SpriteRenderer sprite;
    public enum Type
    {
        StellarNurseries,
        StarSystem,
        Nebula,
        TinyBlackhole
    }
    private void Awake()
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
    }
    public Type GetCelestialType()
    {
        return type;
    }
    public void SetCelestialType(Type newType)
    {
        type = newType;
    }
    public void StartAutodestruction()
    {
        autodestructionTimer = blackhole.GetCelestialObjectAutodestructionTime();
        autodestruction = true;
    }
    public void StopAutodestruction()
    {
        autodestruction = false;
        autodestructionTimer = blackhole.GetCelestialObjectAutodestructionTime();
        UpdateSpriteColor();
    }
    private void UpdateSpriteColor()
    {
        var color = sprite.color;
        color.a = autodestructionTimer / blackhole.GetCelestialObjectAutodestructionTime();
        sprite.color = color;
    }
    protected void Update()
    {
        if (autodestruction)
        {
            autodestructionTimer -= Time.deltaTime;
            if (autodestructionTimer <= 0)
            {
                blackhole.RemoveCelestialObject(this);
                Destroy(gameObject);
            }
            UpdateSpriteColor();
        }
    }
}
