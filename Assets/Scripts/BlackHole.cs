using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class BlackHole : ATeam, IDamageable
{
    [Header("Resources")]
    [SerializeField] public int stardust;
    [SerializeField] public int protoStars;
    [Space]
    [SerializeField] public CelestialObject.Type interfaceType;
    [SerializeField] public Planet.Type interfacePlanettype;
    [Space]
    [Header("Celestial Objects")]
    [SerializeField] public StarSystem starSystemPrefab;
    [SerializeField] private int starSystemStardustPrice;
    [Space]
    [SerializeField] public StellarNursery stellarNurseryPrefab;
    [SerializeField] private int stellarNurseryStardustPrice;
    [Space]
    [SerializeField] public Nebula nebulaPrefab;
    [SerializeField] private int nebulaStardustPrice;
    [Space]
    [SerializeField] private int[] blackholeUpgradePrice = { };
    [Space]
    [SerializeField] private BlackHole enemyBlackHole;
    [SerializeField] private float stardustProductionRate;
    [SerializeField] private int size;
    public List<CelestialObject> celestialObjects = new();

    public int GetMaxStardust()
    {
        return (int)(GetUpgradePrice() + GetUpgradePrice() * (size / 10f));
    }
    private int GetUpgradePrice()
    {
        return blackholeUpgradePrice[size - 1];
    }
    public int GetSize() { return size; }
    private void UpdateWordlSize()
    {
        float scale = (size - 1 + stardust * size / (float)(size * GetUpgradePrice())) * 2;
        transform.localScale = new Vector3(scale, scale, 1);
    }
    public BlackHole GetEnemyBlackHole() { return enemyBlackHole; }

    public StarSystem SpawnStarSystem(Vector2 pos)
    {
        if (protoStars < 1 || starSystemStardustPrice >= stardust) return null;

        StarSystem star = Instantiate(starSystemPrefab);
        star.transform.position = pos;
        star.starNbr = 1;
        star.blackhole = this;
        star.SetTeam(team);
        star.SetCelestialType(CelestialObject.Type.StarSystem);

        celestialObjects.Add(star);
        protoStars -= 1;
        stardust -= starSystemStardustPrice;
        starSystemStardustPrice += starSystemStardustPrice / 10;
        return star;
    }
    public Nebula SpawnNebula(Vector2 pos)
    {
        if (nebulaStardustPrice >= stardust) return null;
        Nebula nebula = Instantiate(nebulaPrefab);
        nebula.transform.position = pos;
        nebula.blackhole = this;
        nebula.SetTeam(team);
        nebula.SetCelestialType(CelestialObject.Type.Nebula);

        celestialObjects.Add(nebula);
        stardust -= nebulaStardustPrice;
        nebulaStardustPrice += nebulaStardustPrice / 10;
        return nebula;
    }
    public StellarNursery SpawnStellarNursery(Vector2 pos)
    {
        if (stellarNurseryStardustPrice >= stardust) return null;
        StellarNursery stellarNursery = Instantiate(stellarNurseryPrefab);
        stellarNursery.transform.position = pos;
        stellarNursery.blackhole = this;
        stellarNursery.SetTeam(team);
        stellarNursery.SetCelestialType(CelestialObject.Type.Nebula);

        celestialObjects.Add(stellarNursery);
        stardust -= stellarNurseryStardustPrice;
        stellarNurseryStardustPrice += stellarNurseryStardustPrice / 10;
        return stellarNursery;
    }

    public CelestialObject SpawnCelestialObject(CelestialObject.Type type, Vector2 pos)
    {
        switch (type)
        {
            case CelestialObject.Type.StarSystem:
                return SpawnStarSystem(pos);
            case CelestialObject.Type.Nebula:
                return SpawnNebula(pos);
            case CelestialObject.Type.StellarNurseries:
                return SpawnStellarNursery(pos);
        }
        return null;
    }
    public bool Upgrade()
    {
        if (stardust < GetUpgradePrice()) return false;
        stardust -= GetUpgradePrice();
        size++;
        return true;
    }
    private void Awake()
    {
    }
    private void StardustProduction()
    {
        if (stardust >= GetMaxStardust()) return;
        stardust += size;
    }
    private void Start()
    {
        InvokeRepeating("StardustProduction", stardustProductionRate, stardustProductionRate);
    }
    private void Update()
    {
        UpdateWordlSize();
        if (size <= 0)
        {
            //end
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((1 << collision.gameObject.layer) == GetMask())
        {
            return;
        }

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target == null) return;

        target.TakeDamage(size);
    }

    public void HealHp(int heal)
    {
    }

    public void TakeDamage(int dmg)
    {
        stardust -= dmg;
        if (stardust <= 0)
        {
            size--;
            stardust = size * 1000;
        }
    }
}
