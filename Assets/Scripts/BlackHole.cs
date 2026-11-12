using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlackHole : ATeam, IDamageable
{
    [Header("Resources")]
    [SerializeField] public int stardust;
    [SerializeField] public int protoStars;
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
    [SerializeField] private BlackHole enemyBlackHole;
    [SerializeField] private int size;
    public List<CelestialObject> celestialObjects = new();

    public int GetSize() { return size; }
    public BlackHole GetEnemyBlackHole() { return enemyBlackHole; }

    public CelestialObject SpawnCelestialObject(CelestialObject.Type type, Vector2 pos)
    {
        if (type == CelestialObject.Type.StarSystem)
        {
            if (protoStars < 1 || starSystemStardustPrice > stardust) return null;

            StarSystem star = Instantiate(starSystemPrefab);
            star.transform.position = pos;
            star.starNbr = 1;
            star.blackhole = this;
            star.SetTeam(team);
            star.SetType(CelestialObject.Type.StarSystem);

            celestialObjects.Add(star);
            protoStars -= 1;
            stardust -= starSystemStardustPrice;
            return star;
        }
        if (type == CelestialObject.Type.Nebula)
        {
            if (nebulaStardustPrice > stardust) return null;
            Nebula nebula = Instantiate(nebulaPrefab);
            nebula.transform.position = pos;
            nebula.blackhole = this;
            nebula.SetTeam(team);
            nebula.SetType(CelestialObject.Type.Nebula);

            celestialObjects.Add(nebula);
            stardust -= nebulaStardustPrice;
            return nebula;
        }
        if (type == CelestialObject.Type.StellarNurseries)
        {
            if (stellarNurseryStardustPrice > stardust) return null;
            StellarNursery stellarNursery = Instantiate(stellarNurseryPrefab);
            stellarNursery.transform.position = pos;
            stellarNursery.blackhole = this;
            stellarNursery.SetTeam(team);
            stellarNursery.SetType(CelestialObject.Type.Nebula);

            celestialObjects.Add(stellarNursery);
            stardust -= nebulaStardustPrice;
            return stellarNursery;
        }
        return null;
    }
    public void Upgrade()
    {
        if (stardust < size * 1000) return;
        stardust -= size * 1000;
        size++;
    }
    private void Awake()
    {
    }

    private void Start()
    {
    }
    private void Update()
    {
        transform.localScale = new Vector3((size - 1) * 2 + stardust / (size * 1000f), (size - 1) * 2 + stardust / (size * 1000f), 1);
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
