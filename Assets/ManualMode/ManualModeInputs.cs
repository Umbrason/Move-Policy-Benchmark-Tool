//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.6.3
//     from Assets/ManualMode/ManualModeInputs.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @ManualModeInputs: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @ManualModeInputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""ManualModeInputs"",
    ""maps"": [
        {
            ""name"": ""ManualGame"",
            ""id"": ""48a3beee-4b2c-4813-8b4c-258bb5cd0af4"",
            ""actions"": [
                {
                    ""name"": ""MoveRelative"",
                    ""type"": ""Value"",
                    ""id"": ""bac181ac-e6f2-47f9-89df-dfa20c27cb08"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MoveToMouse"",
                    ""type"": ""Button"",
                    ""id"": ""9a1be945-6a5c-44a3-9f76-1df2e6aa89dd"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""340d78ce-224f-46f7-a2a6-946d3f0c79fe"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""ConfirmRelativeMove"",
                    ""type"": ""Button"",
                    ""id"": ""6dd96a62-e55a-44c2-bf3f-a43259f4328f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD [Keyboard]"",
                    ""id"": ""2c8249af-fe2c-4028-87da-b32351d9ea76"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveRelative"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""1bd42536-7949-4e17-87ad-f7ce960ae636"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveRelative"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""128d72d9-25b2-4a84-9ce3-dcb49ffc7e88"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveRelative"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""3401ee3d-25f8-4d69-a726-fc0481ce84d8"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveRelative"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""3545ed35-6e9c-4d56-940f-a87587114392"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveRelative"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""f22f7b57-1c1b-4d01-85f4-45952db1be37"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveToMouse"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""21366803-0a07-4d7f-be41-1a0842a91fbc"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0a25a5b3-5670-4b1a-b17f-9a650ea63582"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ConfirmRelativeMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // ManualGame
        m_ManualGame = asset.FindActionMap("ManualGame", throwIfNotFound: true);
        m_ManualGame_MoveRelative = m_ManualGame.FindAction("MoveRelative", throwIfNotFound: true);
        m_ManualGame_MoveToMouse = m_ManualGame.FindAction("MoveToMouse", throwIfNotFound: true);
        m_ManualGame_MousePosition = m_ManualGame.FindAction("MousePosition", throwIfNotFound: true);
        m_ManualGame_ConfirmRelativeMove = m_ManualGame.FindAction("ConfirmRelativeMove", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // ManualGame
    private readonly InputActionMap m_ManualGame;
    private List<IManualGameActions> m_ManualGameActionsCallbackInterfaces = new List<IManualGameActions>();
    private readonly InputAction m_ManualGame_MoveRelative;
    private readonly InputAction m_ManualGame_MoveToMouse;
    private readonly InputAction m_ManualGame_MousePosition;
    private readonly InputAction m_ManualGame_ConfirmRelativeMove;
    public struct ManualGameActions
    {
        private @ManualModeInputs m_Wrapper;
        public ManualGameActions(@ManualModeInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @MoveRelative => m_Wrapper.m_ManualGame_MoveRelative;
        public InputAction @MoveToMouse => m_Wrapper.m_ManualGame_MoveToMouse;
        public InputAction @MousePosition => m_Wrapper.m_ManualGame_MousePosition;
        public InputAction @ConfirmRelativeMove => m_Wrapper.m_ManualGame_ConfirmRelativeMove;
        public InputActionMap Get() { return m_Wrapper.m_ManualGame; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ManualGameActions set) { return set.Get(); }
        public void AddCallbacks(IManualGameActions instance)
        {
            if (instance == null || m_Wrapper.m_ManualGameActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_ManualGameActionsCallbackInterfaces.Add(instance);
            @MoveRelative.started += instance.OnMoveRelative;
            @MoveRelative.performed += instance.OnMoveRelative;
            @MoveRelative.canceled += instance.OnMoveRelative;
            @MoveToMouse.started += instance.OnMoveToMouse;
            @MoveToMouse.performed += instance.OnMoveToMouse;
            @MoveToMouse.canceled += instance.OnMoveToMouse;
            @MousePosition.started += instance.OnMousePosition;
            @MousePosition.performed += instance.OnMousePosition;
            @MousePosition.canceled += instance.OnMousePosition;
            @ConfirmRelativeMove.started += instance.OnConfirmRelativeMove;
            @ConfirmRelativeMove.performed += instance.OnConfirmRelativeMove;
            @ConfirmRelativeMove.canceled += instance.OnConfirmRelativeMove;
        }

        private void UnregisterCallbacks(IManualGameActions instance)
        {
            @MoveRelative.started -= instance.OnMoveRelative;
            @MoveRelative.performed -= instance.OnMoveRelative;
            @MoveRelative.canceled -= instance.OnMoveRelative;
            @MoveToMouse.started -= instance.OnMoveToMouse;
            @MoveToMouse.performed -= instance.OnMoveToMouse;
            @MoveToMouse.canceled -= instance.OnMoveToMouse;
            @MousePosition.started -= instance.OnMousePosition;
            @MousePosition.performed -= instance.OnMousePosition;
            @MousePosition.canceled -= instance.OnMousePosition;
            @ConfirmRelativeMove.started -= instance.OnConfirmRelativeMove;
            @ConfirmRelativeMove.performed -= instance.OnConfirmRelativeMove;
            @ConfirmRelativeMove.canceled -= instance.OnConfirmRelativeMove;
        }

        public void RemoveCallbacks(IManualGameActions instance)
        {
            if (m_Wrapper.m_ManualGameActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IManualGameActions instance)
        {
            foreach (var item in m_Wrapper.m_ManualGameActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_ManualGameActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public ManualGameActions @ManualGame => new ManualGameActions(this);
    public interface IManualGameActions
    {
        void OnMoveRelative(InputAction.CallbackContext context);
        void OnMoveToMouse(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
        void OnConfirmRelativeMove(InputAction.CallbackContext context);
    }
}
