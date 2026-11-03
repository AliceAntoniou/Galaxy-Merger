using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.CullingGroup;
using static UnityEngine.EventSystems.EventTrigger;

public class BlackHole : ATeam, IDamageable, IEquatable<BlackHole>
{
    [Header("Resources")]
    [SerializeField] public int stardust;
    private NetworkVariable<int> stardustServ = new();
    [SerializeField] public int protoStars;
    private NetworkVariable<int> protoStarsServ = new();
    [Header("Planets Prefabs")]
    [SerializeField] private StarSystem starSystemPrefab;
    [Space]
    [SerializeField] public BlackHole enemyBlackHole;
    [SerializeField] private int size;
    private NetworkVariable<int> sizeServ = new();
    private List<StarSystem> stars = new();

    public bool Equals(BlackHole other)
    {
        if (other.enemyBlackHole == enemyBlackHole &&
            other.stardust == stardust &&
            other.protoStars == protoStars &&
            other.size == size) return true;
        return false;
    }
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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetTeam(Team.player);
            transform.position = new Vector2(0, 0);
        } else
        {
            SetTeam(Team.enemy);
            transform.position = new Vector2(20, 0);
        }
        //stardustServ.OnValueChanged = (int previous, int current) => { if (stardustServ.Value != ) };
        //protoStarsServ.OnValueChanged = protoStars;
        //sizeServ.OnValueChanged = size;

    }
    public override void OnNetworkDespawn()
    {
    }

    private void Awake()
    {
        blackhole = this;
    }

    private void Start()
    {
        //SpawnStarSystem(3, 0).AddAsteroidBelt();
        //SpawnStarSystem(3, 2).AddAsteroidBelt();
        //SpawnStarSystem(3, -2).AddSlingshotPlanetSpawer();
    }
    private void FixedUpdate()
    {
        if (enemyBlackHole == null) return;
        stardust--;
        if (stardust <= 0)
        {
            size--;
            stardust = size * 1000;
        }
        transform.localScale = new Vector3(size - 1 + stardust / (size * 1000f), size - 1 + stardust / (size * 1000f), 1);
        stardustServ.Value = stardust;
        protoStarsServ.Value = protoStars;
        sizeServ.Value = size;
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

    public void StartGame(BlackHole enemy)
    {
        enemyBlackHole = enemy;
        SpawnStarSystem(3, 0);
        //SubmitStartRpc();
    }

    [Rpc(SendTo.Server)]
    void SubmitStartRpc(RpcParams rpcParams = default)
    {
    }
    public void HealHp(int heal)
    {
    }

    public void TakeDamage(int dmg)
    {
        stardust -= dmg;
        stardustServ.Value = stardust;
    }
}
