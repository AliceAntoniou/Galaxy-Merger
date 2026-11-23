using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Multi_1
{
    /// <summary>
    /// One instance per galaxy (human or AI). The server is the only one that writes the state
    /// (Stardust, Black hole level); clients ask through RPCs and read the NetworkVariables.
    /// Values marked TBD come from the GDD and are meant to be tuned.
    /// </summary>
    public class PlayerNetwork : NetworkBehaviour
    {
        public static readonly List<PlayerNetwork> All = new();

        public static PlayerNetwork GetBySlot(int slot)
        {
            foreach (var p in All) if (p.Slot.Value == slot) return p;
            return null;
        }

        /// <summary>The human player controlled by this client id (the AI is owned by the host too, so it is skipped).</summary>
        public static PlayerNetwork GetHuman(ulong clientId)
        {
            foreach (var p in All) if (!p.IsAI.Value && p.OwnerClientId == clientId) return p;
            return null;
        }

        public const int MaxBlackHoleLevel = 5;

        public readonly NetworkVariable<int> Slot = new();
        public readonly NetworkVariable<bool> IsAI = new();
        public readonly NetworkVariable<float> Stardust = new(100f); // TBD starting amount
        public readonly NetworkVariable<int> BlackHoleLevel = new(1);

        /// <summary>Galaxy Grid diameter: level 1..5 gives 3, 5, 7, 9, 11 hexagons (GDD 2.4.1).</summary>
        public int GridDiameter => 1 + 2 * BlackHoleLevel.Value;

        [Header("Server tuning (TBD in the GDD)")]
        [SerializeField] private float baseProductionPerSecond = 1f;
        [SerializeField] private float baseUpgradePrice = 100f;
        [SerializeField, Range(0f, 1f)] private float levelLossRefundRatio = 0.5f;

        private float _extraProduction;
        private float _tick;
        private const float TickInterval = 0.25f; // stardust is synced 4x/second, not every frame

        public override void OnNetworkSpawn()
        {
            All.Add(this);
        }

        public override void OnNetworkDespawn()
        {
            All.Remove(this);
        }

        // ---------------------------------------------------------------- SERVER

        public void ServerInit(int slot, bool isAI)
        {
            Slot.Value = slot;
            IsAI.Value = isAI;
        }

        private void Update()
        {
            if (!IsServer) return;
            _tick += Time.deltaTime;
            if (_tick < TickInterval) return;

            Stardust.Value += (baseProductionPerSecond + _extraProduction) * _tick;
            _tick = 0f;
        }

        public void AddProduction(float perSecond) => _extraProduction += perSecond;

        public bool TrySpend(float amount)
        {
            if (!IsServer || Stardust.Value < amount) return false;
            Stardust.Value -= amount;
            return true;
        }

        public float NextUpgradePrice() => baseUpgradePrice * BlackHoleLevel.Value; // TBD formula

        public bool ServerTryUpgradeBlackHole()
        {
            if (!IsServer || BlackHoleLevel.Value >= MaxBlackHoleLevel) return false;
            if (!TrySpend(NextUpgradePrice())) return false;
            BlackHoleLevel.Value++;
            // TODO: refund Celestial Objects that are now under the black hole (GDD: <TBD 0% or more than 100%>)
            return true;
        }

        /// <summary>An enemy unit hit this black hole (GDD 2.4.2).</summary>
        public void ServerTakeHit(float damage)
        {
            if (!IsServer) return;

            if (Stardust.Value >= damage)
            {
                Stardust.Value -= damage;
                return;
            }

            if (BlackHoleLevel.Value > 1)
            {
                float refund = baseUpgradePrice * (BlackHoleLevel.Value - 1) * levelLossRefundRatio;
                BlackHoleLevel.Value--;
                Stardust.Value += refund;
                // TODO: grid loses one size, edge Celestial Objects fade out over <TBD 60s> while still producing.
            }
            else
            {
                // TODO: lowest level and no stardust: decide the defeat condition (not defined in the GDD yet).
            }
        }

        // ---------------------------------------------------------------- CLIENT -> SERVER

        [Rpc(SendTo.Server)]
        public void RequestUpgradeBlackHoleRpc(RpcParams rpcParams = default)
        {
            // Only the human who owns this galaxy may upgrade it (the host also owns the AI object).
            if (IsAI.Value || rpcParams.Receive.SenderClientId != OwnerClientId) return;
            ServerTryUpgradeBlackHole();
        }
    }
}
