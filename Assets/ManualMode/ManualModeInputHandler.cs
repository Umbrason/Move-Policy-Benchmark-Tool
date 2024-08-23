using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManualModeInputHandler : MonoBehaviour, ManualModeInputs.IManualGameActions
{
    [SerializeField] Camera Camera;
    [SerializeField] UnityGame manualGame;
    [SerializeField] GameObject MoveOptionMarker;
    Vector3 MouseWorldPosition;
    Vector3 KeyboardWorldDirection;
    private ManualModeInputs input;

    public static bool HasPendingSelection => requests.Count > 0;

    public class MoveRequest
    {
        public MoveRequest(Agent agent)
        {
            Agent = agent;
            Fulfilled = null;
            MoveOptions = Agent.OccupiedNode.Neighbours.Append(Agent.OccupiedNode).ToArray();
        }

        public event Action Fulfilled;
        public readonly Agent Agent;
        public readonly Node[] MoveOptions;
        public bool TryResolve(Node node)
        {
            if (!MoveOptions.Contains(node)) return false;
            Fulfilled?.Invoke();
            return true;
        }
    }
    private static readonly Queue<MoveRequest> requests = new();

    public static MoveRequest RequestMoveSelection(Agent agent)
    {
        var moveRequest = new MoveRequest(agent);
        requests.Enqueue(moveRequest);
        return moveRequest;
    }

    MoveRequest CurrentRequest;
    void Update()
    {
        if (CurrentRequest != null || requests.Count == 0) return;
        CurrentRequest = requests.Peek();
        PlaceMarkers(CurrentRequest);
    }

    void Awake()
    {
        input = new ManualModeInputs();
        input.ManualGame.SetCallbacks(this);
        input.ManualGame.Enable();
    }

    private readonly List<GameObject> markers = new();
    void ClearMarkers()
    {
        foreach (var marker in markers) Destroy(marker);
        markers.Clear();
    }
    void PlaceMarkers(MoveRequest request)
    {
        var options = request.MoveOptions;
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
        if (!context.performed || CurrentRequest == null) return;
        var gridPos = Vector2Int.RoundToInt(MouseWorldPosition._xz());
        TryResolveCurrentMoveRequest(gridPos);
    }

    public void OnConfirmRelativeMove(InputAction.CallbackContext context)
    {
        if (!context.performed || CurrentRequest == null) return;
        var dir = Vector2Int.RoundToInt(KeyboardWorldDirection._xz());
        var gridPos = CurrentRequest.Agent.OccupiedNode.position + dir;
        TryResolveCurrentMoveRequest(gridPos);
    }

    private void TryResolveCurrentMoveRequest(Vector2Int position)
    {
        if (CurrentRequest == null) return;
        var request = CurrentRequest;
        if (!request.MoveOptions.Any(option => option.position == position)) return;
        var option = request.MoveOptions.First(option => option.position == position);
        if (!request.TryResolve(option)) return;
        request.Agent.Move(option);
        ClearMarkers();
        requests.Dequeue();
        CurrentRequest = null;
    }
}
