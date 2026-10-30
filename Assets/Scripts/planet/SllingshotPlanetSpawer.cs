using UnityEngine;

public class SllingshotPlanetSpawer : ATeam, IPlanet
{
    [SerializeField] private float spawnRate;
    [SerializeField] private SlingshotPlanet slingshotPlanetPrefab;

    void Start()
    {
        InvokeRepeating("SpawnSlingshotPlanet", spawnRate, spawnRate);
    }

    private void SpawnSlingshotPlanet()
    {
        SlingshotPlanet ssp = Instantiate(slingshotPlanetPrefab);
        ssp.blackhole = blackhole;
        if (GetTeam() == Team.player)
        {
            ssp.transform.position = new Vector2(blackhole.transform.position.x + blackhole.GetSize(), blackhole.transform.position.y + Random.Range(-2, 2));
            ssp.SetTeam(Team.player);
        }
        else
        {
            ssp.transform.position = new Vector2(blackhole.transform.position.x - blackhole.GetSize(), blackhole.transform.position.y + Random.Range(-2, 2));
            ssp.SetTeam(Team.enemy);
        }
    }

    void Update()
    {
        
    }
}
