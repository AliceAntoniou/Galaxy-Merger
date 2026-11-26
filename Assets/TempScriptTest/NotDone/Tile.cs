using Multi_1;
using Unity.Netcode;
using UnityEngine;

/// <summary>Local object (not networked). Which entity sits on it comes from the server, see NetworkEntity.</summary>
public class Tile : MonoBehaviour
{
    [SerializeField] private EntityType entityType = EntityType.None;
    [Tooltip("Tick only if you do NOT use TileClickInput (legacy OnMouseDown).")]
    [SerializeField] private bool useOnMouseDown = true;

    private GridManager2 gridManager;
    private BlackHole2 blackhole;

    public Vector3Int gridPosition;

    public BlackHole2 Owner => blackhole;
    /// <summary>The Celestial Object standing on this tile (null if free). Set on every machine by NetworkEntity.</summary>
    public NetworkEntity Entity { get; private set; }

    public void Initialize(GridManager2 gridManager, BlackHole2 blackhole)
    {
        this.gridManager = gridManager;
        this.blackhole = blackhole;
        this.gridPosition = gridManager.GetGridPosition(this);
    }

    public void SetEntity(NetworkEntity entity)
    {
        Entity = entity;
        entityType = entity.Type;
    }

    public void SetEntityType(EntityType entityType)
    {
        this.entityType = entityType;
    }

    public void ClearEntityType()
    {
        Entity = null;
        entityType = EntityType.None;
    }

    public EntityType GetEntityType()
    {
        return entityType;
    }

    private void OnMouseDown()
    {
        Debug.Log("Tile clicked: " + gridPosition + ", EntityType: " + entityType);

        // Only the owner of this galaxy can build on it. The server re-validates anyway.
        if (blackhole == null || !blackhole.IsLocalHuman) return;

        var selected = BuildMenuUI.Selected;
        var manager = NetworkEntityManager.Instance;
        if (selected == EntityType.None || manager == null) return;

        if (selected.IsCelestial())
        {
            if (entityType != EntityType.None) return; // already occupied
            manager.RequestPlaceRpc(selected, new Vector2Int(gridPosition.x, gridPosition.y));
        }
        else if (selected.IsPlanet())
        {
            if (entityType != EntityType.StarSystem) return; // planets go on a Star System
            manager.RequestAddPlanetRpc(new NetworkObjectReference(Entity.NetworkObject), selected);
        }
    }
}
