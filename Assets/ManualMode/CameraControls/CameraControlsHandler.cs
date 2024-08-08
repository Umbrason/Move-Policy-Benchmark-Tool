
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControlsHandler : MonoBehaviour, CameraControlInputs.ICameraControlsActions
{
    public float minZoom;
    public float maxZoom;
    private CameraControlInputs input;
    Cached<Camera> cached_camera = new(Cached<Camera>.GetOption.Children);
    Camera Camera => cached_camera[this];

    void Start()
    {
        input = new CameraControlInputs();
        input.CameraControls.SetCallbacks(this);
        input.CameraControls.Enable();
        zoom = Camera.orthographicSize;
    }

    public void OnPan(InputAction.CallbackContext context)
    {
        pan = context.ReadValueAsButton();
    }

    private float zoom;
    private Vector2? mousePos;
    private bool pan;

    public void OnZoom(InputAction.CallbackContext context)
    {
        var delta = context.ReadValue<float>();
        zoom -= delta;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        Camera.orthographicSize = zoom;
    }

    public void OnMouse(InputAction.CallbackContext context)
    {
        var newMousePos = context.ReadValue<Vector2>();
        mousePos ??= newMousePos; //ensure no jump for first move
        var delta = newMousePos - mousePos.Value;
        mousePos = newMousePos;
        if (!pan) return;
        var dx_norm = delta.x / Camera.pixelHeight;
        var dy_norm = delta.y / Camera.pixelHeight;
        transform.position -= new Vector3(dx_norm, 0, dy_norm) * Camera.orthographicSize * 2f;
    }
}
