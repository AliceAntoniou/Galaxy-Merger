using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Grid))]
public class GridManager : ATeam
{
    [SerializeField] private Grid grid;
    [SerializeField] private SpriteRenderer tilePrefab;
    [SerializeField] private int centerDistance;
    [SerializeField] private GameObject shopUI;

    private Vector3 mousePos;
    private HashSet<Vector3> unavailableGridPositions = new();
    private int size;
    private List<SpriteRenderer> tiles = new();

    private Vector3Int GetGridCenter()
    {
        if (team == Team.player) return new Vector3Int(-size - centerDistance, 0, 0);
        return new Vector3Int(size + centerDistance, 0, 0);
    }
    private static Vector3Int GetBlackHoleCenter()
    {
        return Vector3Int.zero;
    }
    private void Awake()
    {
        SetSize(blackhole.GetSize());
    }
    private void Start()
    {
    }
    private void RemoveTile(SpriteRenderer tile)
    {
        var pos = new Vector3(tile.transform.position.x, tile.transform.position.y, 0);
        if (IsBlackhole(tile.transform.position) && !IsCellEmpty(pos))
            {
            foreach (var obj in blackhole.celestialObjects)
            {
                Debug.Log($"{obj.transform.position}");
                if (Vector3.Distance(obj.transform.position, pos) == 0)
                {
                    blackhole.celestialObjects.Remove(obj);
                    Destroy(obj.gameObject);
                    break;
                }
            }
            unavailableGridPositions.Remove(pos);
        }
        Destroy(tile.gameObject);
    }
    private void SetSize(int newSize)
    {
        size = newSize;
        Vector3Int center = GetGridCenter();
        tiles.ForEach(delegate (SpriteRenderer tile) { RemoveTile(tile); });
        tiles.Clear();
        for (int x = center.x - size; x <= center.x + size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                if (GetTileDistance(center, new Vector3Int(x, y, 0)) > size ||
                    GetTileDistance(GetBlackHoleCenter(), new Vector3Int(x, y, 0)) < size) continue;
                tiles.Add(Instantiate(tilePrefab, grid.CellToWorld(new Vector3Int(x, y, 2)), this.transform.rotation));
            }
        }
    }
    public List<Vector3> GetTilesPositions()
    {
        List<Vector3> list = new();
        foreach (var tile in tiles)
        {
            var pos = tile.transform.position;
            pos.z = 0;
            list.Add(pos);
        }
        return list;
    }
    private void Update()
    {
        if (size != blackhole.GetSize())
        {
            SetSize(blackhole.GetSize());
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

    public bool UpgradeBlackhole()
    {
        if (blackhole.Upgrade())
        {
            SetSize(blackhole.GetSize());
            return true;
        }
        return false;
    }

    public void SpawnCelestialObject(CelestialObject.Type type, Vector3 pos)
    {
        if (!IsCellEmpty(pos)) return;

        if (blackhole.SpawnCelestialObject(type, pos))
        {
            unavailableGridPositions.Add(pos);
        }
    }
    public void SpawnPlanet(StarSystem star, Planet.Type type)
    {
        star.AddPlanet(type);
    }
    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        Vector3 pos = GetMousePosOnGrid();
        if (IsBlackhole(pos))
        {
            UpgradeBlackhole();
            return;
        }
        if (!IsOnGrid(pos)) return;
        if (IsCellEmpty(pos))
        {
            //shopUI.SetActive(true);
            SpawnCelestialObject(blackhole.interfaceType, pos);
            return;
        }
        CelestialObject obj = GetCelestialObject(pos);
        if (obj != null)
        {
            if (obj.GetCelestialType() == CelestialObject.Type.StarSystem)
            {
                SpawnPlanet((StarSystem)obj, blackhole.interfacePlanettype);
                return;
            }
            return;
        }
    }

    public void OnMoveMouse(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }

    public bool IsCellEmpty(Vector3 gridPos)
    {
        return !unavailableGridPositions.Contains(gridPos);
    }

    static private int GetTileDistance(Vector3Int a, Vector3Int b)
    {
        int dx = b.x - a.x;
        int dy = b.y - a.y;
        int x = Mathf.Abs(dx);
        int y = Mathf.Abs(dy);
        if ((dx < 0) ^ ((a.y & 1) == 1))
            x = Mathf.Max(0, x - (y + 1) / 2);
        else
            x = Mathf.Max(0, x - (y) / 2);
        return x + y;
    }

    public bool IsOnGrid(Vector3 pos)
    {
        Vector3Int gridPos = grid.WorldToCell(pos);
        int size = blackhole.GetSize();

        Debug.Log(GetTileDistance(GetBlackHoleCenter(), gridPos));
        return (GetTileDistance(GetGridCenter(), gridPos) <= size &&
            GetTileDistance(GetBlackHoleCenter(), gridPos) >= size);
    }
    public bool IsBlackhole(Vector3 pos)
    {
        Vector3Int gridPos = grid.WorldToCell(pos);
        int size = blackhole.GetSize();

        return GetTileDistance(GetBlackHoleCenter(), gridPos) < size;
    }
    public CelestialObject GetCelestialObject(Vector3 pos)
    {
        foreach (var celestialObject in blackhole.celestialObjects)
        {
            if (celestialObject.transform.position == GetCellPos(pos)) return celestialObject;
        }
        return null;
    }
}
