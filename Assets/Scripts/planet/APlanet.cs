using UnityEngine;

public interface IPlanet
{
    //[Header("Planets Prefabs")]
    //[SerializeField] public const AsteroidBelt asteroidBeltPrefab = Resources.Load("Assets/Prefabs/Asteroid belt.prefab") as AsteroidBelt;

    public enum planetTypes
    {
        asteroidBelt
    }

    //static public IPlanet GetPlanet(planetTypes type)
    //{
    //    if (type == planetTypes.asteroidBelt) { return asteroidBeltPrefab; }
    //    return null;
    //}
}
