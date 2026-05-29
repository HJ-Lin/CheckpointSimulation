using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 2f;
    public float zoomSpeed = 5f;
    public float minZoom = 10f;
    public float maxZoom = 50f;
    public Vector2 panBoundsX = new Vector2(-20f, 20f);
    public Vector2 panBoundsZ = new Vector2(-20f, 20f);

    private Vector3 dragOrigin;
    private Camera cam;
    private Mouse mouse;

    void Start()
    {
        cam = GetComponent<Camera>();
        mouse = Mouse.current;
    }

    void Update()
    {
        if (mouse == null) return;

        // Pan with right mouse button or middle mouse
        if (mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
        {
            dragOrigin = mouse.position.ReadValue();
        }

        if (mouse.rightButton.isPressed || mouse.middleButton.isPressed)
        {
            Vector3 currentMousePos = mouse.position.ReadValue();
            Vector3 diff = (currentMousePos - dragOrigin) * panSpeed * Time.deltaTime;

            // Convert screen space movement to isometric world space
            // For isometric: North is top-left, East is top-right
            // This is a 45-degree rotation of the input
            float isometricX = (diff.x + diff.y) * 0.5f;
            float isometricZ = (diff.y - diff.x) * 0.5f;

            Vector3 newPos = transform.position - new Vector3(isometricX, 0, isometricZ);
            newPos.x = Mathf.Clamp(newPos.x, panBoundsX.x, panBoundsX.y);
            newPos.z = Mathf.Clamp(newPos.z, panBoundsZ.x, panBoundsZ.y);
            transform.position = newPos;
            dragOrigin = currentMousePos;
        }

        // Zoom with scroll wheel
        if (mouse.scroll.ReadValue() != Vector2.zero)
        {
            float scroll = mouse.scroll.ReadValue().y;
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}