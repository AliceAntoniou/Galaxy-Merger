using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class IA1 : ATeam
{
    [SerializeField] private float actionRate;
    [SerializeField] private GridManager grid;
    private float timer;
    private List<Vector3> tilesPos = new();

    private void Awake()
    {
        if (blackhole == null)
        {
            Debug.LogWarning("IA 1 doesn't have a blackhole assigned");
        }
        if (grid == null)
        {
            Debug.LogWarning("IA 1 doesn't have a grid");
        }
        actions.Add(SpawnCelestialObject);
        actions.Add(UpgradeBlackhole);
        actions.Add(SpawnPlanet);
    }
    void Start()
    {
        tilesPos = grid.GetTilesPositions();
    }

    private List<Action> actions = new();

    private void SpawnCelestialObject()
    {
        var pos = tilesPos[Random.Range(0, tilesPos.Count)];
        var type = (CelestialObject.Type) Random.Range(0, 3);

        grid.SpawnCelestialObject(type, pos);
    }
    private void UpgradeBlackhole()
    {
        if (grid.UpgradeBlackhole())
            tilesPos = grid.GetTilesPositions();
    }
    private void SpawnPlanet()
    {
        if (blackhole.celestialObjects.Count == 0) return;
        var obj = blackhole.celestialObjects[Random.Range(0, blackhole.celestialObjects.Count)];
        if (obj.GetCelestialType() == CelestialObject.Type.StarSystem)
        {
            StarSystem star = (StarSystem) obj;
            star.AddPlanet((Planet.Type)Random.Range(0, 3));
        }
    }
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= actionRate)
        {
            actions[Random.Range(0, actions.Count)].Invoke();
            timer = 0;
        }
    }
}
