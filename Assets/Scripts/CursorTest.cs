using UnityEngine;
using UnityEngine.InputSystem;

public class CursorTest : MonoBehaviour
{
    private Vector2 mousePos;

    void Update()
    {
        //Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 pos = Camera.main.ScreenToWorldPoint(mousePos);

        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    public void OnMoveMouse(InputAction.CallbackContext context)
    {
        mousePos = context.ReadValue<Vector2>();
    }
}
