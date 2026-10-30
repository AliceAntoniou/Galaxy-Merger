using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static IPlanet;

public class StarSystem : ATeam
{
    [Header("Planets Prefabs")]
    [SerializeField] public AsteroidBelt asteroidBeltPrefab;
    [SerializeField] private int asteroidPrice;
    [SerializeField] public SllingshotPlanetSpawer SlingshotPlanetSpawerPrefab;
    [SerializeField] private int slingshotPrice;
    [Space]
    [SerializeField] public int starNbr;
    [SerializeField] public int maxPlanets = 1;
    private List<IPlanet> planets = new();


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
        return true;
    }

    void Start()
    {

    }

    void Update()
    {
        
    }
}
