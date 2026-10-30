using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : ATeam, IDamageable
{
    [Header("Resources")]
    [SerializeField] public int stardust;
    [SerializeField] public int protoStars;
    [Header("Planets Prefabs")]
    [SerializeField] private StarSystem starSystemPrefab;
    [Space]
    [SerializeField] private BlackHole enemyBlackHole;
    [SerializeField] private int size;
    private List<StarSystem> stars = new();

    public int GetSize() { return size; }
    public BlackHole GetEnemyBlackHole() { return enemyBlackHole; }

    public StarSystem SpawnStarSystem(int x, int y, int starNbr = 1)
    {
        if (protoStars < starNbr) return null;

        Vector3 pos = transform.position + new Vector3(x, y);
        if (team == Team.player) { pos.x = -x; }

        StarSystem star = Instantiate(starSystemPrefab);
        star.transform.position = pos;
        star.starNbr = starNbr;
        star.blackhole = this;
        star.SetTeam(team);

        stars.Add(star);
        return star;
    }

    private void Awake()
    {
        blackhole = this;
    }

    private void Start()
    {
        SpawnStarSystem(3, 0).AddAsteroidBelt();
        SpawnStarSystem(3, 2).AddAsteroidBelt();
        SpawnStarSystem(3, -2).AddSlingshotPlanetSpawer();
    }

    public void HealHp(int heal)
    {
        return;
    }

    public void TakeDamage(int dmg)
    {
        return;
    }
}
