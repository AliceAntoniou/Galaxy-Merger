using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Grid))]
public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private BlackHole blackHole;
    [SerializeField] private SpriteRenderer tilePrefab;
    [SerializeField] private int centerDistance;
    [SerializeField] private GameObject shopUI;

    private Vector3 mousePos;
    private HashSet<Vector3> unavailableGridPositions = new();
    private int size;
    private List<SpriteRenderer> tiles = new();

    [SerializeField] private CelestialObject.Type type;

    private Vector3Int GetGridCenter()
    {
        return new Vector3Int(-size - centerDistance, 0, 0);
    }
    private static Vector3Int GetBlackHoleCenter()
    {
        return Vector3Int.zero;
    }
    private void Awake()
    {
    }
    private void Start()
    {
        setSize(blackHole.GetSize());
    }
    private void RemoveTile(SpriteRenderer tile)
    {
        if (!IsCellEmpty(tile.transform.position))
        {
            foreach (var obj in blackHole.celestialObjects)
            {
                if (Vector3.Distance(obj.transform.position, tile.transform.position) == 0)
                {
                    blackHole.celestialObjects.Remove(obj);
                    Destroy(obj.gameObject);
                    break;
                }
            }
        }
        unavailableGridPositions.Remove(tile.transform.position);
        Destroy(tile.gameObject);
    }
    private void setSize(int newSize)
    {
        size = newSize;
        Vector3Int center = GetGridCenter();
        tiles.ForEach(delegate (SpriteRenderer tile) { if (IsBlackhole(tile.transform.position)) RemoveTile(tile); });
        tiles.Clear();
        for (int x = center.x - size; x <= center.x + size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (GetTileDistance(center, new Vector3Int(x, y, 0)) > size ||
                    GetTileDistance(GetBlackHoleCenter(), new Vector3Int(x, y, 0)) < size) continue;
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
        Vector3Int cellPosition = grid.WorldToCell(pos);
        cellPosition.z = 0;
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
        if (IsBlackhole(pos))
        {
            blackHole.Upgrade();
            return;
        }
        if (!IsOnGrid(pos)) return;
        if (IsCellEmpty(pos))
        {
            //shopUI.SetActive(true);
            if (type == CelestialObject.Type.StarSystem)
            {
                if (blackHole.SpawnCelestialObject(CelestialObject.Type.StarSystem, pos))
                {
                    unavailableGridPositions.Add(pos);
                    return;
                }
            }
            if (type == CelestialObject.Type.Nebula)
            {
                if (blackHole.SpawnCelestialObject(CelestialObject.Type.Nebula, pos))
                {
                    unavailableGridPositions.Add(pos);
                    return;
                }
            }
            if (type == CelestialObject.Type.StellarNurseries)
            {
                if (blackHole.SpawnCelestialObject(CelestialObject.Type.StellarNurseries, pos))
                {
                    unavailableGridPositions.Add(pos);
                    return;
                }
            }
            return;
        }
        CelestialObject obj = GetCelestialObject(pos);
        if (obj != null)
        {
            //open obj info
            Debug.Log("open obj info");
            return;
        }
    }

    public void OnMoveMouse(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }

    private bool IsCellEmpty(Vector3 gridPos)
    {
        return !unavailableGridPositions.Contains(gridPos);
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

    public bool IsOnGrid(Vector3 pos)
    {
        Vector3Int gridPos = grid.WorldToCell(pos);
        int size = blackHole.GetSize();

        return (GetTileDistance(GetGridCenter(), gridPos) <= size &&
            GetTileDistance(GetBlackHoleCenter(), gridPos) >= size);
    }
    public bool IsBlackhole(Vector3 pos)
    {
        Vector3Int gridPos = grid.WorldToCell(pos);
        int size = blackHole.GetSize();

        return GetTileDistance(GetBlackHoleCenter(), gridPos) < size;
    }
    public CelestialObject GetCelestialObject(Vector3 pos)
    {
        foreach (var celestialObject in blackHole.celestialObjects)
        {
            if (celestialObject.transform.position == GetCellPos(pos)) return celestialObject;
        }
        return null;
    }
}
