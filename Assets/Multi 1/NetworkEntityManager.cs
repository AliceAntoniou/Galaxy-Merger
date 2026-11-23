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
        public float stardustCost;
    }

    /// <summary>
    /// Entity manager (GDD section 3): every Star System / Planet / Unit is created by the SERVER.
    /// Clients only send requests; the server validates (ownership, cost, free tile) then spawns.
    /// Place it in the Game scene on an object that also has a NetworkObject (in-scene placed).
    /// </summary>
    public class NetworkEntityManager : NetworkBehaviour
    {
        public static NetworkEntityManager Instance { get; private set; }

        [SerializeField] private List<EntityDefinition> definitions = new();
        [Tooltip("Index = player slot. Center of each player's Galaxy Grid.")]
        [SerializeField] private Transform[] gridOrigins = new Transform[2];
        [SerializeField] private float hexSize = 1f; // TBD in the GDD

        private readonly Dictionary<EntityType, EntityDefinition> _defs = new();
        private readonly Dictionary<(int slot, Vector2Int hex), NetworkObject> _occupied = new();

        private void Awake()
        {
            Instance = this;
            foreach (var d in definitions) _defs[d.type] = d;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Axial hex coordinates -> world position (pointy-top). Adjust to match your grid.</summary>
        public Vector3 HexToWorld(int slot, Vector2Int hex)
        {
            float x = hexSize * Mathf.Sqrt(3f) * (hex.x + hex.y * 0.5f);
            float y = hexSize * 1.5f * hex.y;
            var origin = gridOrigins.Length > slot && gridOrigins[slot] != null ? gridOrigins[slot].position : Vector3.zero;
            return origin + new Vector3(x, y, 0f);
        }

        // ---------------------------------------------------------------- CLIENT -> SERVER

        /// <summary>Place a Celestial Object (Stellar Nursery, Star System, Nebula, Tiny Black Hole) on a grid tile.</summary>
        [Rpc(SendTo.Server)]
        public void RequestPlaceRpc(EntityType type, Vector2Int hex, RpcParams rpcParams = default)
        {
            var player = PlayerNetwork.GetHuman(rpcParams.Receive.SenderClientId);
            if (player == null) return;
            ServerPlace(player, type, hex, rpcParams.Receive.SenderClientId);
        }

        /// <summary>Add a Planet to one of your Star Systems.</summary>
        [Rpc(SendTo.Server)]
        public void RequestAddPlanetRpc(NetworkObjectReference starSystem, EntityType planetType, RpcParams rpcParams = default)
        {
            var player = PlayerNetwork.GetHuman(rpcParams.Receive.SenderClientId);
            if (player == null || !starSystem.TryGet(out var star)) return;

            var starEntity = star.GetComponent<NetworkEntity>();
            if (starEntity == null || starEntity.Type != EntityType.StarSystem) return;
            if (starEntity.OwnerSlot.Value != player.Slot.Value) return;
            if (!_defs.TryGetValue(planetType, out var def)) return;

            // TODO: check the free planet slots of this Star System (+1 per upgrade, GDD 2.6.2).
            if (!player.TrySpend(def.stardustCost)) return;

            var obj = Instantiate(def.prefab, star.transform.position, Quaternion.identity);
            obj.GetComponent<NetworkEntity>().ServerInit(player.Slot.Value, starEntity.Hex.Value);
            obj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        }

        // ---------------------------------------------------------------- SERVER ONLY

        /// <summary>Also callable by the AI: it passes its own player and the host client id.</summary>
        public bool ServerPlace(PlayerNetwork player, EntityType type, Vector2Int hex, ulong ownerClientId)
        {
            if (!IsServer) return false;
            if (!_defs.TryGetValue(type, out var def)) return false;

            int slot = player.Slot.Value;
            if (_occupied.ContainsKey((slot, hex))) return false;
            // TODO: hex inside player.GridDiameter, Star System needs Protostar rules, Star Systems adjacent to a Stellar Nursery...

            if (!player.TrySpend(def.stardustCost)) return false;

            var obj = Instantiate(def.prefab, HexToWorld(slot, hex), Quaternion.identity);
            obj.GetComponent<NetworkEntity>().ServerInit(slot, hex);
            obj.SpawnWithOwnership(ownerClientId, true);
            _occupied[(slot, hex)] = obj;
            return true;
        }

        /// <summary>Called by planets' spawners to create Units. Positions are then synced by NetworkTransform.</summary>
        public NetworkObject ServerSpawnUnit(EntityType unitType, Vector3 position, int ownerSlot)
        {
            if (!IsServer || !_defs.TryGetValue(unitType, out var def)) return null;

            var obj = Instantiate(def.prefab, position, Quaternion.identity);
            obj.GetComponent<NetworkEntity>().ServerInit(ownerSlot, Vector2Int.zero);
            obj.Spawn(true); // server-owned: the server runs unit AI/physics
            return obj;
        }

        public void ServerFreeTile(int slot, Vector2Int hex) => _occupied.Remove((slot, hex));
    }
}