using Multi_1;
using UnityEngine;

/// <summary>Local object (not networked). Which entity sits on it comes from the server, see NetworkEntity.</summary>
public class Tile : MonoBehaviour
{
    [SerializeField] private EntityType entityType = EntityType.None;

    private GridManager2 gridManager;
    private BlackHole2 blackhole;

    public Vector3Int gridPosition;

    public BlackHole2 Owner => blackhole;

    public void Initialize(GridManager2 gridManager, BlackHole2 blackhole)
    {
        this.gridManager = gridManager;
        this.blackhole = blackhole;
        this.gridPosition = gridManager.GetGridPosition(this);
    }

    public void SetEntityType(EntityType entityType)
    {
        this.entityType = entityType;
    }

    public void ClearEntityType()
    {
        entityType = EntityType.None;
    }

    public EntityType GetEntityType()
    {
        return entityType;
    }

    private void OnMouseDown()
    {
        Debug.Log("Tile clicked: " + gridPosition + ", EntityType: " + entityType);

        // Only the human who owns this galaxy can build on it. The server re-validates anyway.
        if (blackhole == null || !blackhole.IsLocalHuman) return;

        // TODO: open the shop, then e.g.
        // NetworkEntityManager.Instance.RequestPlaceRpc(type, new Vector2Int(gridPosition.x, gridPosition.y));
    }
}
