using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput inputs;
    private new Camera camera;
    private Vector3 target = Vector3.zero;
    private float speed = 10;

    private void Awake()
    {
        inputs = GetComponent<PlayerInput>();
        camera = inputs.camera;
    }
    public void OnMoveMouse(InputAction.CallbackContext context)
    {
        Vector2 pos = context.ReadValue<Vector2>();
        target = camera.transform.position;
        if (pos == Vector2.zero) return;
        if (pos.x < 100)
            target.x = camera.transform.position.x - 100;
        if (pos.x > camera.pixelWidth - 100)
            target.x = camera.transform.position.x + 100;
    }
    private void FixedUpdate()
    {
        Vector3 direction = target - camera.transform.position;
        camera.transform.position += direction.normalized * speed * Time.deltaTime;
    }
}
