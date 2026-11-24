using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public List<BlackHole2> blackHoles = new List<BlackHole2>();

    [SerializeField] private GridManager2 gridManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void RegisterBlackHole(BlackHole2 blackHole)
    {
        if (!blackHoles.Contains(blackHole))
        {
            blackHoles.Add(blackHole);
        }
    }
    
    public void StartGame()
    {
        gridManager.InitializeBlackHolePosition(blackHoles.Count);

        foreach (var blackHole in blackHoles)
        {
            blackHole.StartProduction();
        }
    }
    
    public void TestAddTileToBlackHole()
    {
        BlackHole2 blackHole = blackHoles[1];
        gridManager.AddTileBehindBlackHole(blackHole, new Vector3Int(-3, -1, 0)); // sous (-1,-1)
        gridManager.AddTileBehindBlackHole(blackHole, new Vector3Int(-3, 0, 0));  // derrière (-2,0)
    }
    
    public void UnregisterBlackHole(BlackHole2 blackHole)
    {
        if (blackHoles.Contains(blackHole))
        {
            blackHoles.Remove(blackHole);
        }
    }
    
    public List<BlackHole2> GetAllBlackHoles()
    {
        return blackHoles;
    }
}
