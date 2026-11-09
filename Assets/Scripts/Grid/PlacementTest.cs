using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlacementTest : MonoBehaviour
{
    public  GameObject TestBuildingPrefab;
    private Vector3    mousePos;
    private HashSet<Vector3> unavailableGridPositions = new();

    public void OnMoveMouse(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }

    private void SpawnBuilding(Vector3 gridPos, GameObject go)
    {
        if (unavailableGridPositions.Contains(gridPos))
        {
            Debug.Log("Can't place here!");
            return;
        }

        go = Instantiate(go);
        go.transform.position = gridPos;
        unavailableGridPositions.Add(gridPos);
    }
}
