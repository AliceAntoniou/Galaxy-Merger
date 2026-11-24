using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class BlackHole : ATeam, IDamageable
{
    [SerializeField] private BlackHole enemyBlackHole;
    [SerializeField] private int size;
    [Header("Resources")]
    [SerializeField] public int stardust;
    [SerializeField] public int protoStars;
    [Space]
    [SerializeField] public CelestialObject.Type interfaceType;
    [SerializeField] public Planet.Type interfacePlanettype;
    [Space]
    [SerializeField] private int starSystemStardustPrice;
    [SerializeField] private int stellarNurseryStardustPrice;
    [SerializeField] private int nebulaStardustPrice;
    [Space]
    [SerializeField] private int[] blackholeUpgradePrice = { };
    [Space]
    [SerializeField] private float stardustProductionRate;
    [SerializeField] private float priceAugmentation;
    [SerializeField] private float downgradeRefund;
    [SerializeField] private float celestialObjectAutodestructionTime;
    [Space]
    [Header("Celestial Objects Prefab")]
    [SerializeField] public StarSystem starSystemPrefab;
    [SerializeField] public StellarNursery stellarNurseryPrefab;
    [SerializeField] public Nebula nebulaPrefab;


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
    public float GetCelestialObjectAutodestructionTime() { return celestialObjectAutodestructionTime; }
    private void UpdateWordlSize()
    {
        float scale = (size - 1 + stardust * size / (float)(size * GetUpgradePrice())) * 2;
        transform.localScale = new Vector3(scale, scale, 1);
    }
    public BlackHole GetEnemyBlackHole() { return enemyBlackHole; }
    public bool RemoveCelestialObject(CelestialObject obj)
    {
        return blackhole.celestialObjects.Remove(obj);
    }

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
        starSystemStardustPrice += (int)(starSystemStardustPrice * priceAugmentation);
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
        nebulaStardustPrice += (int)(nebulaStardustPrice * priceAugmentation);
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
        stellarNurseryStardustPrice += (int)(stellarNurseryStardustPrice * priceAugmentation);
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
    public void Downgrade()
    {
        size--;
        stardust = (int)(GetUpgradePrice() * downgradeRefund);
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

        TakeDamage(target.GetHp());
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
            Downgrade();
        }
    }
    public int GetHp()
    {
        return stardust;
    }
}
