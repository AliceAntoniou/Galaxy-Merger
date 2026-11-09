using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private BlackHole blackHole;

    [SerializeField] private float gridSnapXOffset;
    [SerializeField] private float gridSnapYOffset;
    private Vector3 mousePos;
    private HashSet<Vector3> unavailableGridPositions = new();


    private void Awake()
    {
    }

    public Vector3 GetCellPos(Vector3 pos) 
    {
        Vector3Int cellPosition = new Vector3Int(grid.WorldToCell(pos).x, grid.WorldToCell(pos).y, 0);
        return grid.GetCellCenterWorld(cellPosition);
    }
    private Vector3 GetMousePosOnGrid()
    {
        return GetCellPos(Camera.main.ScreenToWorldPoint(mousePos));
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Vector3 pos = GetMousePosOnGrid();
        if (!IsOnGrid(pos)) return;
        if (!IsCellEmpty(pos)) {
            if (blackHole.SpawnStarSystem(pos)) unavailableGridPositions.Add(pos);
        }
    }

    public void OnMoveMouse(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }

    private bool IsCellEmpty(Vector3 gridPos)
    {
        return unavailableGridPositions.Contains(gridPos);
    }

    int GetTileDistance(Vector3Int a, Vector3Int b)
    {
        int dx = b.x - a.x;     // signed deltas
        int dy = b.y - a.y;
        int x = Mathf.Abs(dx);  // absolute deltas
        int y = Mathf.Abs(dy);
        // special case if we start on an odd row or if we move into negative x direction
        if ((dx < 0) ^ ((a.y & 1) == 1))
            x = Mathf.Max(0, x - (y + 1) / 2);
        else
            x = Mathf.Max(0, x - (y) / 2);
        return x + y;
    }

    private bool IsOnGrid(Vector3 gridPos)
    {
        Vector3Int pos = grid.WorldToCell(gridPos);
        Debug.Log(pos);
        int size = blackHole.GetSize();
        Vector3Int gridCenter = new Vector3Int(-size, 0, 0);
        Debug.Log(GetTileDistance(gridCenter, pos));
        if (GetTileDistance(gridCenter, pos) > size)
        {
            return false;
        }
        return true;
    }
}
