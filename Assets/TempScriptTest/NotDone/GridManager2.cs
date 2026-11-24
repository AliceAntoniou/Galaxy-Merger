using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

//TODO: passer la grille en locale

[RequireComponent(typeof(Grid))]
public class GridManager2 : MonoBehaviour
{
    //[SerializeField] private Grid grid;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int centerDistance;
    //[SerializeField] private GameObject shopUI;
    [SerializeField] private int startingTileNumber = 6;

    private int size;
    private List<Tile> tiles = new();
    private (Vector3Int upPos, Vector3Int downPos, Vector3Int backPos) directionsForNextTile = (new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0));
    private Dictionary<BlackHole2, List<Vector3Int>> tileMapPerPlayer = new();
    private Dictionary<Vector3Int, Dictionary<BlackHole2, Tile>> allTiles = new();

    // private static Vector3Int GetGridCenter()
    // {
    //     // if (team == Team.player) return new Vector3Int(-size - centerDistance, 0, 0);
    //     // return new Vector3Int(size + centerDistance, 0, 0);
    //     return Vector3Int.zero;
    // }
    // private static Vector3Int GetBlackHoleCenter()
    // {
    //     return Vector3Int.zero;
    // }
    
    private void Awake()
    {
        //SetSize(blackhole.GetSize());
    }
    
    public void InitializeBlackHolePosition(int numberOfBlackHoles)
    {
        
        for (int i = 0; i < numberOfBlackHoles; i++)
        {
            BlackHole2 blackhole = GameManager.instance.blackHoles[i];

            if (!tileMapPerPlayer.ContainsKey(blackhole))
            {
                tileMapPerPlayer.Add(blackhole, new List<Vector3Int>());
            }

            size = blackhole.GetSize();
            
            float angle = 360f / numberOfBlackHoles * i;
            float radians = angle * Mathf.Deg2Rad;

            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            Vector3 worldPosition = new Vector3(
                cos * size * blackhole.GetGrid().cellSize.x,
                sin * size * blackhole.GetGrid().cellSize.y,
                0);
            
            blackhole.transform.position = worldPosition;
            
            
            Vector3 directionToCentre =
                Vector3Int.zero - blackhole.transform.position;

            float angleCenter = Mathf.Atan2(
                directionToCentre.y,
                directionToCentre.x
            ) * Mathf.Rad2Deg;

            blackhole.transform.rotation = Quaternion.Euler(0, 0, angleCenter);
            
            InitializeTilesForBlackHole(blackhole);
        }
        
        Debug.Log("Black holes initialized.");
    }

    private void InitializeTilesForBlackHole(BlackHole2 blackhole)
    {
        for (int i = 0; i < startingTileNumber; i++)
        {
            // Initialize tiles behind the black hole
            
            int nextPosModulo = i % 3;
            int nextPosIndex = i / 3 + 1;
            
            Vector3Int nextPos = Vector3Int.zero;

            switch (nextPosModulo)
            {
                case 0:
                    nextPos = directionsForNextTile.backPos * nextPosIndex;
                    break;
                case 1:
                    nextPos = directionsForNextTile.upPos + nextPosIndex * directionsForNextTile.backPos;
                    break;
                case 2:
                    nextPos = directionsForNextTile.downPos + nextPosIndex * directionsForNextTile.backPos;
                    break;
            }
            
            AddTileBehindBlackHole(blackhole, nextPos);
        }
    }

    public void AddTileBehindBlackHole(BlackHole2 blackHole, Vector3Int position)
    {
        if (!IsCellEmpty(position, blackHole)) return;

        Grid grid = blackHole.GetGrid();

        Tile newTile = Instantiate(tilePrefab, blackHole.transform).GetComponent<Tile>();

        newTile.transform.localPosition = grid.CellToLocal(position);
        newTile.transform.localRotation = Quaternion.identity;

        tiles.Add(newTile);

        if (!allTiles.ContainsKey(position))
            allTiles[position] = new Dictionary<BlackHole2, Tile>();

        allTiles[position].Add(blackHole, newTile);
        tileMapPerPlayer[blackHole].Add(position);
        newTile.Initialize(this, blackHole);
    }
    
    private bool IsCellEmpty(Vector3Int pos, BlackHole2 blackhole)
    {
        if (allTiles.TryGetValue(pos, out Dictionary<BlackHole2, Tile> tile))
        {
            if (tile.ContainsKey(blackhole))
            {
                return false;
            }
        }
        return true;
    }
    
    public Vector3Int GetGridPosition(Tile tile)
    {
        foreach (KeyValuePair<Vector3Int, Dictionary<BlackHole2, Tile>> kvp in allTiles)
        {
            if (kvp.Value.ContainsValue(tile))
            {
                return kvp.Key;
            }
        }
        throw new System.Exception("Tile not found in grid.");
    }
    
    // private void RemoveTile(SpriteRenderer tile)
    // {
    //     var pos = new Vector3(tile.transform.position.x, tile.transform.position.y, 0);
    //     if (!IsCellEmpty(pos))
    //     {
    //         if (IsBlackhole(tile.transform.position))
    //         {
    //             foreach (var obj in blackhole.celestialObjects)
    //             {
    //                 if (Vector3.Distance(obj.transform.position, pos) == 0)
    //                 {
    //                     blackhole.RemoveCelestialObject(obj);
    //                     Destroy(obj.gameObject);
    //                     break;
    //                 }
    //             }
    //         }
    //         else if (!IsOnGrid(pos))
    //         {
    //             foreach (var obj in blackhole.celestialObjects)
    //             {
    //                 if (Vector3.Distance(obj.transform.position, pos) == 0)
    //                 {
    //                     obj.StartAutodestruction();
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    //     Destroy(tile.gameObject);
    // }
    // private void SetSize(int newSize)
    // {
    //     size = newSize;
    //     Vector3Int center = GetGridCenter();
    //     tiles.ForEach(delegate (SpriteRenderer tile) { RemoveTile(tile); });
    //     tiles.Clear();
    //     for (int x = center.x - size; x <= center.x + size; x++)
    //     {
    //         for (int y = -size; y <= size; y++)
    //         {
    //             var pos = new Vector3Int(x, y, 0);
    //             if (GetTileDistance(center, pos) > size ||
    //                 GetTileDistance(GetBlackHoleCenter(), pos) < size) continue;
    //             tiles.Add(Instantiate(tilePrefab, grid.CellToWorld(new Vector3Int(x, y, 2)), this.transform.rotation));
    //             foreach (var obj in blackhole.celestialObjects)
    //             {
    //                 if (Vector3.Distance(obj.transform.position, grid.CellToWorld(pos)) == 0)
    //                 {
    //                     obj.StopAutodestruction();
    //                 }
    //             }
    //         }
    //     }
    // }
    // public List<Vector3> GetTilesPositions()
    // {
    //     List<Vector3> list = new();
    //     foreach (var tile in tiles)
    //     {
    //         var pos = tile.transform.position;
    //         pos.z = 0;
    //         list.Add(pos);
    //     }
    //     return list;
    // }
    // private void Update()
    // {
    //     if (size != blackhole.GetSize())
    //     {
    //         SetSize(blackhole.GetSize());
    //     }
    // }

    // public Vector3 GetCellPos(Vector3 pos)
    // {
    //     Vector3Int cellPosition = grid.WorldToCell(pos);
    //     cellPosition.z = 0;
    //     return grid.GetCellCenterWorld(cellPosition);
    // }
    // private Vector3 GetMousePosOnGrid()
    // {
    //     return GetCellPos(Camera.main.ScreenToWorldPoint(mousePos));
    // }

    // public bool UpgradeBlackhole()
    // {
    //     if (blackhole.Upgrade())
    //     {
    //         SetSize(blackhole.GetSize());
    //         return true;
    //     }
    //     return false;
    // }

    // public void SpawnCelestialObject(CelestialObject.Type type, Vector3 pos)
    // {
    //     if (!IsCellEmpty(pos)) return;
    //
    //     blackhole.SpawnCelestialObject(type, pos);
    // }
    // public void SpawnPlanet(StarSystem star, Planet.Type type)
    // {
    //     star.AddPlanet(type);
    // }
    // public void OnClick(InputAction.CallbackContext context)
    // {
    //     if (!context.started) return;
    //     Vector3 pos = GetMousePosOnGrid();
    //     if (IsBlackhole(pos))
    //     {
    //         UpgradeBlackhole();
    //         return;
    //     }
    //     if (!IsOnGrid(pos)) return;
    //     if (IsCellEmpty(pos))
    //     {
    //         //shopUI.SetActive(true);
    //         SpawnCelestialObject(blackhole.interfaceType, pos);
    //         return;
    //     }
    //     CelestialObject obj = GetCelestialObject(pos);
    //     if (obj != null)
    //     {
    //         if (obj.GetCelestialType() == CelestialObject.Type.StarSystem)
    //         {
    //             SpawnPlanet((StarSystem)obj, blackhole.interfacePlanettype);
    //             return;
    //         }
    //         return;
    //     }
    // }

    // public void OnMoveMouse(InputAction.CallbackContext context)
    // {
    //     mousePos = context.ReadValue<Vector2>();
    // }

    // public bool IsCellEmpty(Vector3 gridPos)
    // {
    //     foreach (var obj in blackhole.celestialObjects)
    //     {
    //         if (Vector3.Distance(obj.transform.position, gridPos) == 0)
    //         {
    //             return false;
    //         }
    //     }
    //     return true; ;
    // }

    // static private int GetTileDistance(Vector3Int a, Vector3Int b)
    // {
    //     int dx = b.x - a.x;
    //     int dy = b.y - a.y;
    //     int x = Mathf.Abs(dx);
    //     int y = Mathf.Abs(dy);
    //     if ((dx < 0) ^ ((a.y & 1) == 1))
    //         x = Mathf.Max(0, x - (y + 1) / 2);
    //     else
    //         x = Mathf.Max(0, x - (y) / 2);
    //     return x + y;
    // }
    //
    // public bool IsOnGrid(Vector3 pos)
    // {
    //     Vector3Int gridPos = grid.WorldToCell(pos);
    //     int size = blackhole.GetSize();
    //
    //     return (GetTileDistance(GetGridCenter(), gridPos) <= size &&
    //         GetTileDistance(GetBlackHoleCenter(), gridPos) >= size);
    // }
    // public bool IsBlackhole(Vector3 pos)
    // {
    //     Vector3Int gridPos = grid.WorldToCell(pos);
    //     int size = blackhole.GetSize();
    //
    //     return GetTileDistance(GetBlackHoleCenter(), gridPos) < size;
    // }
    // public CelestialObject GetCelestialObject(Vector3 pos)
    // {
    //     foreach (var celestialObject in blackhole.celestialObjects)
    //     {
    //         if (celestialObject.transform.position == GetCellPos(pos)) return celestialObject;
    //     }
    //     return null;
    // }
    //
    
}
