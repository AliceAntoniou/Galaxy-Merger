using Unity.Netcode;
using UnityEngine;

namespace Multi_1
{
    /// <summary>Add to every spawnable prefab (also add a NetworkObject, and a NetworkTransform on units).</summary>
    public class NetworkEntity : NetworkBehaviour
    {
        public EntityType Type;
        [SerializeField] private int stardustPerSecond; // production added to the owner while alive

        public readonly NetworkVariable<int> OwnerSlot = new();
        public readonly NetworkVariable<Vector2Int> Hex = new();

        // Only Celestial Objects occupy a tile (planets sit on their Star System's tile, units are free).
        private bool OccupiesTile =>
            Type == EntityType.StellarNursery || Type == EntityType.StarSystem ||
            Type == EntityType.Nebula || Type == EntityType.TinyBlackHole;

        public void ServerInit(int ownerSlot, Vector2Int hex)
        {
            OwnerSlot.Value = ownerSlot;
            Hex.Value = hex;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer && stardustPerSecond > 0)
                BlackHole2.GetBySlot(OwnerSlot.Value)?.UpProductionRate(stardustPerSecond);

            // Every peer: mark the tile as occupied so Tile.GetEntityType() is correct everywhere.
            if (OccupiesTile) FindTile()?.SetEntityType(Type);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && stardustPerSecond > 0)
                BlackHole2.GetBySlot(OwnerSlot.Value)?.DownProductionRate(stardustPerSecond);

            if (OccupiesTile) FindTile()?.ClearEntityType();
        }

        private Tile FindTile()
        {
            var blackHole = BlackHole2.GetBySlot(OwnerSlot.Value);
            var gm = GameManager.instance;
            if (blackHole == null || gm == null) return null;
            return gm.Grid.GetTile(blackHole, new Vector3Int(Hex.Value.x, Hex.Value.y, 0));
        }
    }
}
