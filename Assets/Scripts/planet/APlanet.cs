using UnityEngine;

public class Planet : ATeam
{
    private Type type;
    public enum Type
    {
        asteroidBelt,
        iceBelt,
        slingshotPlanet
    }
    public Type GetPlanetType()
    {
        return type;
    }
    public void SetPlanetType(Type newType)
    {
        type = newType;
    }
}
