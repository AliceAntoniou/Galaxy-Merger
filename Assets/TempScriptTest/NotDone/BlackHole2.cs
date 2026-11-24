using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class BlackHole2 : MonoBehaviour, IDamageable
{
    [SerializeField] private int size = 1;
    
    private Grid grid;
    
    [Header("Resources")] 
    
    public int stardust;
    public int protoStars;
    
    public CelestialObject.Type interfaceType;
    public Planet.Type interfacePlanetType;

    [SerializeField] private int stardustProductionRatePerSecond = 1;
    [SerializeField] private int maxStardust = 500;
    [SerializeField] private float downgradeRefund = 0.8f;

    private bool isProducing = true;
    
    [Header("Upgrade Settings")]
    
    public int[] blackholeUpgradePrice = { 100, 200, 400, 800, 1600 };

    //TODO: modifier
    //[Space]
    //[SerializeField] private int[] blackholeUpgradePrice = { };
    //[Space]
    //[SerializeField] private float stardustProductionRate;
    //[SerializeField] private float priceAugmentation;
    //[SerializeField] private float downgradeRefund;
    //[SerializeField] private float celestialObjectAutodestructionTime;
    //TODO : add to GameManager
    //[Space]
    //[Header("Celestial Objects Prefab")]
    //public StarSystem starSystemPrefab;
    //public StellarNursery stellarNurseryPrefab;
    //public Nebula nebulaPrefab;

    // public int GetMaxStardust()
    // {
    //     return (int)(GetUpgradePrice() + GetUpgradePrice() * (size / 10f));
    // }
    // private int GetUpgradePrice()
    // {
    //     return blackholeUpgradePrice[size - 1];
    // }
    
    //public float GetCelestialObjectAutodestructionTime() { return celestialObjectAutodestructionTime; }
    // private void UpdateWordlSize()
    // {
    //     float scale = (size - 1 + stardust * size / (float)(size * GetUpgradePrice())) * 2;
    //     transform.localScale = new Vector3(scale, scale, 1);
    // }
    //public BlackHole2 GetEnemyBlackHole() { return enemyBlackHole; }
    // public bool RemoveCelestialObject(CelestialObject obj)
    // {
    //     return blackhole.celestialObjects.Remove(obj);
    // }

    // public StarSystem SpawnStarSystem(Vector2 pos)
    // {
    //     if (protoStars < 1 || starSystemStardustPrice >= stardust) return null;
    //
    //     StarSystem star = Instantiate(starSystemPrefab);
    //     star.transform.position = pos;
    //     star.starNbr = 1;
    //     star.blackhole = this;
    //     star.SetTeam(team);
    //     star.SetCelestialType(CelestialObject.Type.StarSystem);
    //
    //     celestialObjects.Add(star);
    //     protoStars -= 1;
    //     stardust -= starSystemStardustPrice;
    //     starSystemStardustPrice += (int)(starSystemStardustPrice * priceAugmentation);
    //     return star;
    // }
    // public Nebula SpawnNebula(Vector2 pos)
    // {
    //     if (nebulaStardustPrice >= stardust) return null;
    //     Nebula nebula = Instantiate(nebulaPrefab);
    //     nebula.transform.position = pos;
    //     nebula.blackhole = this;
    //     nebula.SetTeam(team);
    //     nebula.SetCelestialType(CelestialObject.Type.Nebula);
    //
    //     celestialObjects.Add(nebula);
    //     stardust -= nebulaStardustPrice;
    //     nebulaStardustPrice += (int)(nebulaStardustPrice * priceAugmentation);
    //     return nebula;
    // }
    // public StellarNursery SpawnStellarNursery(Vector2 pos)
    // {
    //     if (stellarNurseryStardustPrice >= stardust) return null;
    //     StellarNursery stellarNursery = Instantiate(stellarNurseryPrefab);
    //     stellarNursery.transform.position = pos;
    //     stellarNursery.blackhole = this;
    //     stellarNursery.SetTeam(team);
    //     stellarNursery.SetCelestialType(CelestialObject.Type.Nebula);
    //
    //     celestialObjects.Add(stellarNursery);
    //     stardust -= stellarNurseryStardustPrice;
    //     stellarNurseryStardustPrice += (int)(stellarNurseryStardustPrice * priceAugmentation);
    //     return stellarNursery;
    // }

    // public CelestialObject SpawnCelestialObject(CelestialObject.Type type, Vector2 pos)
    // {
    //     switch (type)
    //     {
    //         case CelestialObject.Type.StarSystem:
    //             return SpawnStarSystem(pos);
    //         case CelestialObject.Type.Nebula:
    //             return SpawnNebula(pos);
    //         case CelestialObject.Type.StellarNurseries:
    //             return SpawnStellarNursery(pos);
    //     }
    //     return null;
    // }
    // public bool Upgrade()
    // {
    //     if (stardust < GetUpgradePrice()) return false;
    //     stardust -= GetUpgradePrice();
    //     size++;
    //     return true;
    // }
    // public void Downgrade()
    // {
    //     size--;
    //     stardust = (int)(GetUpgradePrice() * downgradeRefund);
    // }

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Start()
    {
        GameManager.instance.RegisterBlackHole(this);
    }
    
    public void StartProduction()
    {
        isProducing = true;
        StartCoroutine(AutoProduceStardust());
    }
    
    public void StopProduction()
    {
        isProducing = false;
    }

    private void StardustProduction()
    {
        if (stardust >= maxStardust) return;
        stardust += stardustProductionRatePerSecond;
    }

    public void UpProductionRate(int amount)
    {
        stardustProductionRatePerSecond += amount;
    }
    
    public void DownProductionRate(int amount)
    {
        if (stardustProductionRatePerSecond - amount <= 0)
        {
            stardustProductionRatePerSecond = 1;
            return;
        }
        
        stardustProductionRatePerSecond -= amount;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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
    
    public bool Upgrade()
    {
        if (stardust < GetUpgradePrice()) return false;
        
        stardust -= GetUpgradePrice();
        size++;
        
        return true;
    }

    private void Downgrade()
    {
        stardust = (int)(GetUpgradePrice() * downgradeRefund);
        size--;
    }

    #region Getter

        public int GetUpgradePrice()
        {
            return blackholeUpgradePrice[size - 1];
        }

        public int GetHp()
        {
            return stardust;
        }
        
        public int GetSize()
        {
            return size;
        }

        public Grid GetGrid()
        {
            return grid;
        }
        
    #endregion

    private IEnumerator<WaitForSeconds> AutoProduceStardust()
    {
        while (isProducing)
        {
            yield return new WaitForSeconds(1f);
            StardustProduction();
        }
    }
}
