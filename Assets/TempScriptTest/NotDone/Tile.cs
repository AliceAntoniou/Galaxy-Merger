using Multi_1;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private EntityType entityType = EntityType.None;
    
    private GridManager2 gridManager;
    
    public void Initialize(GridManager2 gridManager)
    {
        this.gridManager = gridManager;
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
}
