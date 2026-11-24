using Multi_1;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private EntityType entityType = EntityType.None;
    
    private GridManager2 gridManager;
    private BlackHole2 blackhole;
    
    public Vector3Int gridPosition;
    
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
    }
}
