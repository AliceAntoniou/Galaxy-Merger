using System.Collections;
using UnityEngine;

public class AsteroidBelt : ATeam, IPlanet
{
    [SerializeField] private float spawnRate;
    [SerializeField] private Asteroid asteroidPrefab;

    private void Start()
    {
        InvokeRepeating("SpawnAsteroid", spawnRate, spawnRate);
    }
    private void SpawnAsteroid()
    {
        Asteroid aster = Instantiate(asteroidPrefab);
        aster.blackhole = blackhole;
        if (GetTeam() == Team.player)
        {
            aster.transform.position = new Vector2(blackhole.transform.position.x + blackhole.GetSize(), blackhole.transform.position.y + Random.Range(-2, 2));
            aster.SetTeam(Team.player);
        }   
        else
        {
            aster.transform.position = new Vector2(blackhole.transform.position.x - blackhole.GetSize(), blackhole.transform.position.y + Random.Range(-2, 2));
            aster.SetTeam(Team.enemy);
        }
    }
    private void FixedUpdate()
    {
    }
}
