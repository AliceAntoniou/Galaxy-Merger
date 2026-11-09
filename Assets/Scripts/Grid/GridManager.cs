using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    [SerializeField] private Grid grid;

    [SerializeField] private float gridSnapXOffset;
    [SerializeField] private float gridSnapYOffset;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public Vector3 GetCellPos(Vector3 pos) 
    {
        Vector3Int cellPosition = new Vector3Int(grid.WorldToCell(pos).x, grid.WorldToCell(pos).y, 0);
        return grid.GetCellCenterWorld(cellPosition);
    }
}
