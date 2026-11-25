using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 100% LOCAL view/logic of the hexagonal grids: no network code here.
/// It runs identically on every peer because it only depends on synced data (slot, size).
/// Tiles are plain GameObjects (NOT NetworkObjects), parented to their black hole.
/// </summary>
[RequireComponent(typeof(Grid))]
public class GridManager2 : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int centerDistance;
    [SerializeField] private int startingTileNumber = 6;

    private int size;
    private List<Tile> tiles = new();
    private (Vector3Int upPos, Vector3Int downPos, Vector3Int backPos) directionsForNextTile = (new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0));
    private Dictionary<BlackHole2, List<Vector3Int>> tileMapPerPlayer = new();
    private Dictionary<Vector3Int, Dictionary<BlackHole2, Tile>> allTiles = new();

    /// <summary>Call once all black holes are registered and sorted by slot (GameManager does it).</summary>
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

            Vector3 directionToCentre = Vector3.zero - blackhole.transform.position;

            float angleCenter = Mathf.Atan2(directionToCentre.y, directionToCentre.x) * Mathf.Rad2Deg;

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

        if (!tileMapPerPlayer.ContainsKey(blackHole))
            tileMapPerPlayer.Add(blackHole, new List<Vector3Int>());

        Grid grid = blackHole.GetGrid();

        // Parent first, then place in LOCAL space: follows the black hole's position and rotation.
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

    /// <summary>Note: "empty" here means "no tile exists at this cell for this black hole".</summary>
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

    /// <summary>Does this black hole have a tile at this cell? (used by the server to validate placements)</summary>
    public bool HasTile(BlackHole2 blackHole, Vector3Int cell)
    {
        return !IsCellEmpty(cell, blackHole);
    }

    /// <summary>The Tile of this black hole at this cell, or null.</summary>
    public Tile GetTile(BlackHole2 blackHole, Vector3Int cell)
    {
        if (allTiles.TryGetValue(cell, out var perBlackHole) && perBlackHole.TryGetValue(blackHole, out var tile))
            return tile;
        return null;
    }

    /// <summary>Called when a black hole despawns (its tiles are destroyed with it).</summary>
    public void ForgetBlackHole(BlackHole2 blackHole)
    {
        tileMapPerPlayer.Remove(blackHole);

        var emptyKeys = new List<Vector3Int>();
        foreach (var kvp in allTiles)
        {
            kvp.Value.Remove(blackHole);
            if (kvp.Value.Count == 0) emptyKeys.Add(kvp.Key);
        }
        foreach (var key in emptyKeys) allTiles.Remove(key);

        tiles.RemoveAll(t => t == null);
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
}
