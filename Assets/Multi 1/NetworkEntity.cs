using Unity.Netcode;
using UnityEngine;

namespace Multi_1
{
    /// <summary>Add to every spawnable prefab (also add a NetworkObject, and a NetworkTransform on units).</summary>
    public class NetworkEntity : NetworkBehaviour
    {
        public EntityType Type;
        [SerializeField] private float stardustPerSecond; // production added to the owner while alive (TBD)

        public readonly NetworkVariable<int> OwnerSlot = new();
        public readonly NetworkVariable<Vector2Int> Hex = new();

        public void ServerInit(int ownerSlot, Vector2Int hex)
        {
            OwnerSlot.Value = ownerSlot;
            Hex.Value = hex;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer && stardustPerSecond != 0f)
                PlayerNetwork.GetBySlot(OwnerSlot.Value)?.AddProduction(stardustPerSecond);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && stardustPerSecond != 0f)
                PlayerNetwork.GetBySlot(OwnerSlot.Value)?.AddProduction(-stardustPerSecond);
        }
    }
}
