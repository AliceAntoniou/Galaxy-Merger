using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(Grid))]
public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private BlackHole blackHole;
    [SerializeField] private Tile tilePrefab;

    private Vector3 mousePos;
    private HashSet<Vector3> unavailableGridPositions = new();
    private int size;
    private List<Tile> tiles = new();


    private void Awake()
    {
    }
    private void Start()
    {
        setSize(blackHole.GetSize());
    }

    private void setSize(int newSize)
    {
        size = newSize;
        Vector3Int center = new Vector3Int(-size, 0, 0);
        Vector3Int blackholeCenter = Vector3Int.zero;
        tiles.ForEach(delegate (Tile tile) { Destroy(tile.gameObject); });
        tiles.Clear();
        for (int x = center.x - size; x <= center.x + size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (GetTileDistance(center, new Vector3Int(x, y, 0)) > size ||
                    GetTileDistance(blackholeCenter, new Vector3Int(x, y, 0)) < size) continue;
                tiles.Add(Instantiate(tilePrefab, grid.CellToWorld(new Vector3Int(x, y, 0)), this.transform.rotation));
            }
        }
    }
    private void Update()
    {
        if (size != blackHole.GetSize())
        {
            setSize(blackHole.GetSize());
        }
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
        if (!IsCellEmpty(pos))
        {
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

    static private int GetTileDistance(Vector3Int a, Vector3Int b)
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
        int size = blackHole.GetSize();
        Vector3Int gridCenter = new Vector3Int(-size, 0, 0);

        return (GetTileDistance(gridCenter, pos) <= size &&
            GetTileDistance(Vector3Int.zero, pos) >= size);
    }
}
