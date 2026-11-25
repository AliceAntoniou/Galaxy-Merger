using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The black hole IS the player (human or AI). The SERVER owns the state (stardust, size, production);
/// every peer reads it through NetworkVariables. Clients act only through RPCs (RequestUpgradeRpc...).
/// Prefab: NetworkObject + BlackHole2 + Grid (+ collider). Do NOT add a NetworkTransform:
/// the position/rotation is computed locally and identically by GridManager2 from the slot.
/// </summary>
[RequireComponent(typeof(Grid))]
public class BlackHole2 : NetworkBehaviour, IDamageable
{
    // ------------------------------------------------------------ registry (all peers)
    public static readonly List<BlackHole2> All = new();

    public static BlackHole2 GetBySlot(int slot)
    {
        foreach (var b in All) if (b.Slot.Value == slot) return b;
        return null;
    }

    /// <summary>The human black hole controlled by this client id (the AI is owned by the host too, so it is skipped).</summary>
    public static BlackHole2 GetHuman(ulong clientId)
    {
        foreach (var b in All) if (!b.IsAI.Value && b.OwnerClientId == clientId) return b;
        return null;
    }

    /// <summary>The black hole this machine's human plays.</summary>
    public static BlackHole2 GetLocalPlayer()
    {
        foreach (var b in All) if (!b.IsAI.Value && b.IsOwner) return b;
        return null;
    }

    // ------------------------------------------------------------ synced state
    public readonly NetworkVariable<int> Slot = new();
    public readonly NetworkVariable<bool> IsAI = new();
    private readonly NetworkVariable<int> sizeNet = new(1);
    private readonly NetworkVariable<int> stardustNet = new(0);
    private readonly NetworkVariable<int> protoStarsNet = new(0);

    // Read-only views so the rest of your code (UI, etc.) can keep reading blackHole.stardust.
    // Writing must go through the server methods below.
    public int stardust => stardustNet.Value;
    public int protoStars => protoStarsNet.Value;

    public bool IsLocalHuman => !IsAI.Value && IsOwner;

    // ------------------------------------------------------------ local-only data
    private Grid grid;

    [Header("Resources")]
    [SerializeField] private int startingSize = 1;
    [SerializeField] private int startingStardust = 0;

    // Local UI selection (not synced)
    public CelestialObject.Type interfaceType;
    public Planet.Type interfacePlanetType;

    [Header("Server tuning")]
    [SerializeField] private int stardustProductionRatePerSecond = 1;
    [SerializeField] private int maxStardust = 500;
    [SerializeField] private float downgradeRefund = 0.8f;

    [Header("Upgrade Settings")]
    public int[] blackholeUpgradePrice = { 100, 200, 400, 800, 1600 };

    private bool isProducing;
    private Coroutine productionCoroutine;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    // ------------------------------------------------------------ network lifecycle
    public override void OnNetworkSpawn()
    {
        All.Add(this);
        if (GameManager.instance != null) GameManager.instance.RegisterBlackHole(this);
    }

    public override void OnNetworkDespawn()
    {
        StopProduction();
        All.Remove(this);
        if (GameManager.instance != null) GameManager.instance.UnregisterBlackHole(this);
    }

    /// <summary>Server only, called BEFORE Spawn (see NetworkSessionManager.SpawnPlayer).</summary>
    public void ServerInit(int slot, bool isAI)
    {
        Slot.Value = slot;
        IsAI.Value = isAI;
        sizeNet.Value = startingSize;
        stardustNet.Value = startingStardust;
    }

    // ------------------------------------------------------------ production (server)
    public void StartProduction()
    {
        if (!IsServer || productionCoroutine != null) return;
        isProducing = true;
        productionCoroutine = StartCoroutine(AutoProduceStardust());
    }

    public void StopProduction()
    {
        isProducing = false;
        if (productionCoroutine != null)
        {
            StopCoroutine(productionCoroutine);
            productionCoroutine = null;
        }
    }

    private IEnumerator AutoProduceStardust()
    {
        var wait = new WaitForSeconds(1f);
        while (isProducing)
        {
            yield return wait;
            StardustProduction();
        }
    }

    private void StardustProduction()
    {
        if (stardustNet.Value >= maxStardust) return;
        stardustNet.Value = Mathf.Min(maxStardust, stardustNet.Value + stardustProductionRatePerSecond);
    }

    public void UpProductionRate(int amount)
    {
        if (!IsServer) return;
        stardustProductionRatePerSecond += amount;
    }

    public void DownProductionRate(int amount)
    {
        if (!IsServer) return;
        if (stardustProductionRatePerSecond - amount <= 0)
        {
            stardustProductionRatePerSecond = 1;
            return;
        }
        stardustProductionRatePerSecond -= amount;
    }

    // ------------------------------------------------------------ stardust / protostars (server)
    public bool TrySpend(int amount)
    {
        if (!IsServer || stardustNet.Value < amount) return false;
        stardustNet.Value -= amount;
        return true;
    }

    public void AddStardust(int amount)
    {
        if (!IsServer) return;
        stardustNet.Value = Mathf.Clamp(stardustNet.Value + amount, 0, maxStardust);
    }

    public bool TrySpendProtoStars(int amount)
    {
        if (!IsServer || protoStarsNet.Value < amount) return false;
        protoStarsNet.Value -= amount;
        return true;
    }

    public void AddProtoStars(int amount)
    {
        if (!IsServer) return;
        protoStarsNet.Value += amount;
    }

    // ------------------------------------------------------------ combat (server)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return; // physics/damage are resolved on the server only

        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target == null) return;

        TakeDamage(target.GetHp());
        target.TakeDamage(sizeNet.Value);
    }

    public void HealHp(int heal)
    {
    }

    public void TakeDamage(int dmg)
    {
        if (!IsServer) return;

        stardustNet.Value -= dmg;

        if (stardustNet.Value <= 0)
        {
            Downgrade();
        }
    }

    // ------------------------------------------------------------ upgrade (server + RPC)
    public bool Upgrade()
    {
        if (!IsServer) return false;
        if (sizeNet.Value >= blackholeUpgradePrice.Length) return false; // max size reached

        int price = GetUpgradePrice();
        if (stardustNet.Value < price) return false;

        stardustNet.Value -= price;
        sizeNet.Value++;
        return true;
    }

    /// <summary>Called by the owner's UI. The server re-checks who is asking.</summary>
    [Rpc(SendTo.Server)]
    public void RequestUpgradeRpc(RpcParams rpcParams = default)
    {
        if (IsAI.Value || rpcParams.Receive.SenderClientId != OwnerClientId) return;
        Upgrade();
    }

    private void Downgrade()
    {
        if (sizeNet.Value <= 1)
        {
            stardustNet.Value = 0;
            // TODO: lowest size and no stardust -> defeat condition
            return;
        }

        stardustNet.Value = (int)(GetUpgradePrice() * downgradeRefund);
        sizeNet.Value--;
        // TODO: grid loses one size, edge tiles/objects fade out
    }

    #region Getter

        public int GetUpgradePrice()
        {
            return blackholeUpgradePrice[Mathf.Clamp(sizeNet.Value - 1, 0, blackholeUpgradePrice.Length - 1)];
        }

        public int GetHp()
        {
            return stardustNet.Value;
        }

        public int GetSize()
        {
            return sizeNet.Value;
        }

        public Grid GetGrid()
        {
            return grid;
        }

    #endregion
}
