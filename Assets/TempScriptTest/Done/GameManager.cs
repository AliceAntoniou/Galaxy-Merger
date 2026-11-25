using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Network hub of the Game scene. Put it on an object that also has a NetworkObject
/// (placed in the scene, NO DontDestroyOnLoad: NGO scene management handles its lifetime).
/// Once every black hole (player) has spawned, EVERY peer lays out the galaxies locally and identically;
/// only the server starts the stardust production.
/// </summary>
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public List<BlackHole2> blackHoles = new List<BlackHole2>();

    [SerializeField] private GridManager2 gridManager;
    [Tooltip("Number of black holes that must exist before the game starts (2 = two galaxies).")]
    [SerializeField] private int expectedBlackHoles = 2;

    private readonly NetworkVariable<int> expected = new(2);
    private bool gameStarted;

    public bool GameStarted => gameStarted;
    public GridManager2 Grid => gridManager;

    private void Awake()
    {
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        instance = this;
        if (IsServer) expected.Value = expectedBlackHoles;
        expected.OnValueChanged += (_, __) => TryStartGame();

        foreach (var b in BlackHole2.All) RegisterBlackHole(b); // black holes that spawned before us
        TryStartGame();
    }

    public override void OnNetworkDespawn()
    {
        if (instance == this) instance = null;
        blackHoles.Clear();
    }

    public void RegisterBlackHole(BlackHole2 blackHole)
    {
        if (!blackHoles.Contains(blackHole))
        {
            blackHoles.Add(blackHole);
        }
        TryStartGame();
    }

    public void UnregisterBlackHole(BlackHole2 blackHole)
    {
        if (blackHoles.Remove(blackHole))
        {
            gridManager.ForgetBlackHole(blackHole);
        }
    }

    /// <summary>Kept so an old "Start Game" button does not break: the game now starts by itself.</summary>
    public void StartGame()
    {
        TryStartGame();
    }

    private void TryStartGame()
    {
        if (gameStarted || !IsSpawned) return;
        if (blackHoles.Count < expected.Value) return;

        gameStarted = true;

        // Same order on every peer (slot is synced) -> same angle, same position, same tiles.
        blackHoles.Sort((a, b) => a.Slot.Value.CompareTo(b.Slot.Value));
        gridManager.InitializeBlackHolePosition(blackHoles.Count);

        if (IsServer)
        {
            foreach (var blackHole in blackHoles)
            {
                blackHole.StartProduction();
            }
        }
    }

    // ------------------------------------------------------------ tiles over the network
    /// <summary>Test button: adds two tiles to YOUR black hole, on every machine.</summary>
    public void TestAddTileToBlackHole()
    {
        var mine = BlackHole2.GetLocalPlayer();
        if (mine == null) return;

        RequestAddTileRpc(mine.Slot.Value, new Vector3Int(-3, -1, 0));
        RequestAddTileRpc(mine.Slot.Value, new Vector3Int(-3, 0, 0));
    }

    [Rpc(SendTo.Server)]
    private void RequestAddTileRpc(int slot, Vector3Int cell, RpcParams rpcParams = default)
    {
        var blackHole = BlackHole2.GetBySlot(slot);
        if (blackHole == null) return;
        if (!blackHole.IsAI.Value && rpcParams.Receive.SenderClientId != blackHole.OwnerClientId) return;

        // TODO: real rules (stardust cost, cell adjacent to an existing tile, within the grid size...).
        AddTileRpc(slot, cell);
    }

    [Rpc(SendTo.Everyone)]
    private void AddTileRpc(int slot, Vector3Int cell)
    {
        var blackHole = BlackHole2.GetBySlot(slot);
        if (blackHole != null) gridManager.AddTileBehindBlackHole(blackHole, cell);
    }

    public List<BlackHole2> GetAllBlackHoles()
    {
        return blackHoles;
    }
}
