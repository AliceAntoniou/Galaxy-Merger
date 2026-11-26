using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Multi_1
{
    [Serializable]
    public class EntityDefinition
    {
        public EntityType type;
        public NetworkObject prefab;
        public int stardustCost;
    }

    /// <summary>
    /// Entity manager (GDD section 3): every Star System / Planet / Unit is created by the SERVER.
    /// Clients only send requests; the server validates (ownership, tile exists, free tile, cost) then spawns.
    /// Place it in the Game scene on an object that also has a NetworkObject (in-scene placed).
    /// The Vector2Int "hex" is the cell (x, y) of the black hole's own Grid, the same cell as Tile.gridPosition.
    /// </summary>
    public class NetworkEntityManager : NetworkBehaviour
    {
        public static NetworkEntityManager Instance { get; private set; }

        [SerializeField] private List<EntityDefinition> definitions = new();
        [SerializeField] private GridManager2 gridManager;

        private readonly Dictionary<EntityType, EntityDefinition> _defs = new();
        private readonly Dictionary<(int slot, Vector2Int hex), NetworkObject> _occupied = new();

        private void Awake()
        {
            Instance = this;
            foreach (var d in definitions) _defs[d.type] = d;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
            _occupied.Clear();
        }

        // ---------------------------------------------------------------- CLIENT -> SERVER

        /// <summary>Place a Celestial Object (Stellar Nursery, Star System, Nebula, Tiny Black Hole) on a tile.</summary>
        [Rpc(SendTo.Server)]
        public void RequestPlaceRpc(EntityType type, Vector2Int hex, RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            var blackHole = BlackHole2.GetHuman(sender);
            Debug.Log("Je suis dans RequestPlaceRpc, blackHole = " + blackHole + ", type = " + type + ", hex = " + hex);
            if (blackHole == null) return;
            ServerPlace(blackHole, type, hex, sender);
        }

        /// <summary>Add a Planet to one of your Star Systems.</summary>
        [Rpc(SendTo.Server)]
        public void RequestAddPlanetRpc(NetworkObjectReference starSystem, EntityType planetType, RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            var blackHole = BlackHole2.GetHuman(sender);
            if (blackHole == null || !starSystem.TryGet(out var star)) return;

            var starEntity = star.GetComponent<NetworkEntity>();
            if (starEntity == null || starEntity.Type != EntityType.StarSystem) return;
            if (starEntity.OwnerSlot.Value != blackHole.Slot.Value) return;
            if (!_defs.TryGetValue(planetType, out var def)) return;

            // TODO: check the free planet slots of this Star System (+1 per upgrade, GDD 2.6.2).
            if (!blackHole.TrySpend(def.stardustCost)) return;

            var obj = Instantiate(def.prefab, star.transform.position, blackHole.transform.rotation);
            obj.GetComponent<NetworkEntity>().ServerInit(blackHole.Slot.Value, starEntity.Hex.Value);
            obj.SpawnWithOwnership(sender, true);
        }

        // ---------------------------------------------------------------- SERVER ONLY

        /// <summary>Also callable by the AI: it passes its own black hole and the host client id.</summary>
        public bool ServerPlace(BlackHole2 blackHole, EntityType type, Vector2Int hex, ulong ownerClientId)
        {
            if (!IsServer) return false;
            Debug.Log("IsServer");

            if (!_defs.TryGetValue(type, out var def))
            {
                Debug.LogWarning($"[Place] No definition for {type} in NetworkEntityManager.definitions");
                return false;
            }

            var cell = new Vector3Int(hex.x, hex.y, 0);
            int slot = blackHole.Slot.Value;

            if (!gridManager.HasTile(blackHole, cell))
            {
                Debug.LogWarning($"[Place] No tile at {hex} for slot {slot}");
                return false;
            }
            if (_occupied.ContainsKey((slot, hex)))
            {
                Debug.LogWarning($"[Place] Tile {hex} already occupied (slot {slot})");
                return false;
            }
            // TODO: Star System needs Protostars, adjacent to a Stellar Nursery...

            if (!blackHole.TrySpend(def.stardustCost))
            {
                Debug.LogWarning($"[Place] Not enough stardust ({blackHole.stardust}/{def.stardustCost})");
                return false;
            }

            Debug.Log("Placing " + type + " at " + hex + " cost " + def.stardustCost + " for player " + slot);

            // The server laid the grid out exactly like the clients, so its world position is the right one.
            Vector3 worldPos = blackHole.GetGrid().CellToWorld(cell);
            var obj = Instantiate(def.prefab, worldPos, blackHole.transform.rotation);
            obj.GetComponent<NetworkEntity>().ServerInit(slot, hex);
            obj.SpawnWithOwnership(ownerClientId, true);
            _occupied[(slot, hex)] = obj;
            return true;
        }

        /// <summary>Called by planets' spawners to create Units. Positions are then synced by NetworkTransform.</summary>
        public NetworkObject ServerSpawnUnit(EntityType unitType, Vector3 position, int ownerSlot)
        {
            if (!IsServer) return null;

            if (!unitType.IsUnit())
            {
                Debug.LogError($"[Unit] {unitType} is not a unit type (Comet, Asteroid, SlingshotPlanet). Check the planet's Unit Type.");
                return null;
            }
            if (!_defs.TryGetValue(unitType, out var def))
            {
                Debug.LogError($"[Unit] No definition for {unitType} in NetworkEntityManager.definitions");
                return null;
            }
            if (def.prefab.GetComponent<Planet2>() != null)
            {
                // A planet that spawns planets would loop forever and crash Unity.
                Debug.LogError($"[Unit] The prefab registered for {unitType} contains a Planet2: it must be the UNIT prefab.");
                return null;
            }

            var obj = Instantiate(def.prefab, position, Quaternion.identity);
            obj.GetComponent<NetworkEntity>().ServerInit(ownerSlot, Vector2Int.zero);
            obj.Spawn(true); // server-owned: the server runs unit AI/physics
            return obj;
        }

        public void ServerFreeTile(int slot, Vector2Int hex) => _occupied.Remove((slot, hex));
    }
}
