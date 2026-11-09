using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlacementTest : MonoBehaviour
{
    public  GameObject TestBuildingPrefab;
    private Vector3    mousePos;
    private HashSet<Vector3> unavailableGridPositions = new();

    private Vector3 GetMousePosOnGrid() 
    {
        return GridManager.instance.GetCellPos(Camera.main.ScreenToWorldPoint(mousePos));
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.started) SpawnBuilding(GetMousePosOnGrid(), TestBuildingPrefab);
    }

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
