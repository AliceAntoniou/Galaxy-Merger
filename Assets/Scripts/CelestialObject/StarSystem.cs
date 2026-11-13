using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StarSystem : CelestialObject
{
    [Header("Planets Prefabs")]
    [SerializeField] public AsteroidBelt asteroidBeltPrefab;
    [SerializeField] private int asteroidPrice;
    [SerializeField] public SllingshotPlanetSpawer SlingshotPlanetSpawerPrefab;
    [SerializeField] private int slingshotPrice;
    [Space]
    [SerializeField] public int starNbr;
    [SerializeField] public int maxPlanets = 1;
    private List<Planet> planets = new();


    //public bool AddPlanet(planetTypes type)
    //{
    //    IPlanet planet = Instantiate(IPlanet.GetPlanet(type));
    //    return true;
    //}
    public bool AddAsteroidBelt()
    {
        if (blackhole.stardust < asteroidPrice) return false;
        if (planets.Count >= maxPlanets) return false;

        Vector3 pos = transform.position + new Vector3(0, 0, planets.Count + 1);
        AsteroidBelt planet = Instantiate(asteroidBeltPrefab, pos, transform.rotation);
        planet.transform.localScale = new Vector3(1 + 0.2f * planets.Count, 1 + 0.2f * planets.Count, 1);
        planet.blackhole = blackhole;
        planet.SetTeam(team);

        planets.Add(planet);
        blackhole.stardust -= asteroidPrice;
        asteroidPrice += asteroidPrice / 10;
        return true;
    }
    public bool AddSlingshotPlanetSpawer()
    {
        if (blackhole.stardust < slingshotPrice) return false;
        if (planets.Count >= maxPlanets) return false;

        Vector3 pos = transform.position + new Vector3(0, 0, planets.Count + 1);
        SllingshotPlanetSpawer planet = Instantiate(SlingshotPlanetSpawerPrefab, pos, transform.rotation);
        planet.transform.localScale = new Vector3(1 + 0.2f * planets.Count, 1 + 0.2f * planets.Count, 1);
        planet.blackhole = blackhole;
        planet.SetTeam(team);

        planets.Add(planet);
        blackhole.stardust -= slingshotPrice;
        slingshotPrice += slingshotPrice / 10;
        return true;
    }
    public bool AddPlanet(Planet.Type type)
    {
        switch (type)
        {
            case Planet.Type.asteroidBelt:
                return AddAsteroidBelt();
            case Planet.Type.iceBelt:
                return false;
            case Planet.Type.slingshotPlanet:
                return AddSlingshotPlanetSpawer();
        }
        return false;
    }

    void Start()
    {

    }

    void Update()
    {
        
    }
    private void OnDestroy()
    {
        foreach (var planet in planets)
        {
            if (planet != null)
                Destroy(planet.gameObject);
        }
    }
}
