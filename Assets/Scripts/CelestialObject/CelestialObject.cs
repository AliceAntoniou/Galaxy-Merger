using UnityEngine;

public class CelestialObject : ATeam
{
    private Type type;
    public enum Type
    {
        StellarNurseries,
        StarSystem,
        Nebula,
        TinyBlackhole
    }
    public Type GetCelestialType()
    {
        return type;
    }
    public void SetCelestialType(Type newType)
    {
        type = newType;
    }
}
