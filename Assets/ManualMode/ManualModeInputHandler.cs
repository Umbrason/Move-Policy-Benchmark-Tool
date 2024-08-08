using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManualModeInputHandler : MonoBehaviour, ManualModeInputs.IManualGameActions
{
    [SerializeField] Camera Camera;
    [SerializeField] ManualGame manualGame;
    [SerializeField] GameObject MoveOptionMarker;
    Vector3 MouseWorldPosition;
    Vector3 KeyboardWorldDirection;
    private ManualModeInputs input;
    void Start()
    {
        input = new ManualModeInputs();
        input.ManualGame.SetCallbacks(this);
        input.ManualGame.Enable();
        manualGame.GameStart += PlaceMarkers;
        manualGame.GameTick += PlaceMarkers;
        manualGame.GameStop += ClearMarkers;
    }

    void OnDestroy()
    {
        manualGame.GameStart -= PlaceMarkers;
        manualGame.GameTick -= PlaceMarkers;
        manualGame.GameStop -= ClearMarkers;
    }

    Agent PlayerAgent => manualGame.Game.Robbers.agents[0];
    Node[] MoveOptions => PlayerAgent.OccupiedNode.Neighbours.Append(PlayerAgent.OccupiedNode).ToArray();


    private readonly List<GameObject> markers = new();
    void ClearMarkers()
    {
        foreach (var marker in markers) Destroy(marker);
        markers.Clear();
    }
    void PlaceMarkers()
    {
        var options = MoveOptions;
        while (options.Length != markers.Count)
        {
            if (options.Length > markers.Count)
            {
                var instance = Instantiate(MoveOptionMarker, transform);
                markers.Add(instance);
            }
            else
            {
                Destroy(markers[0]);
                markers.RemoveAt(0);
            }
        }
        for (int i = 0; i < options.Length; i++)
            markers[i].transform.position = ((Vector2)options[i].position)._x0y();
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        var screenPosition = context.ReadValue<Vector2>();
        MouseWorldPosition = Camera.ScreenToWorldPoint(screenPosition);
    }

    public void OnMoveRelative(InputAction.CallbackContext context)
    {
        KeyboardWorldDirection = context.ReadValue<Vector2>()._x0y();
    }

    public void OnMoveToMouse(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        var gridPos = Vector2Int.RoundToInt(MouseWorldPosition._xz());
        if (!MoveOptions.Any(node => node.position == gridPos)) return;
        manualGame.MovePlayer(MoveOptions.First(node => node.position == gridPos));
    }

    public void OnConfirmRelativeMove(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        var dir = Vector2Int.RoundToInt(KeyboardWorldDirection._xz());
        var gridPos = PlayerAgent.OccupiedNode.position + dir;
        if (!MoveOptions.Any(node => node.position == gridPos)) return;
        manualGame.MovePlayer(MoveOptions.First(node => node.position == gridPos));

    }
}
