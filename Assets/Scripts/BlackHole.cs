using System.Collections.Generic;
using UnityEngine;

public class BlackHole : ATeam, IDamageable
{
    [Header("Resources")]
    [SerializeField] public int stardust;
    [SerializeField] public int protoStars;
    [Header("Buildings Prefabs")]
    [SerializeField] public StarSystem starSystemPrefab;
    [Space]
    [SerializeField] private BlackHole enemyBlackHole;
    [SerializeField] private int size;
    private List<StarSystem> stars = new();

    public int GetSize() { return size; }
    public BlackHole GetEnemyBlackHole() { return enemyBlackHole; }

    public StarSystem SpawnStarSystem(Vector2 pos, int starNbr = 1)
    {
        if (protoStars < starNbr) return null;

        StarSystem star = Instantiate(starSystemPrefab);
        star.transform.position = pos;
        star.starNbr = starNbr;
        star.blackhole = this;
        star.SetTeam(team);

        stars.Add(star);
        protoStars -= starNbr;
        return star;
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
