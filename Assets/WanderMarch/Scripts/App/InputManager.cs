using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace WanderMarch.Scripts.App
{
    /// <summary>
    /// Managing class in the App space that converts InputSystem actions into GameActions for global callback referencing
    /// </summary>
    public class InputManager : Singleton<InputManager>
    {
        #region Variables

        [SerializeField] private InputActionAsset actions;

        [FormerlySerializedAs("_currentInputMap")] [ShowOnly,SerializeField]private InputActionMap currentInputMap;

        //Dictionaries for InputMaps, These must correspond with the settings in the Input Action Asset
        private readonly Dictionary<string, Dictionary<string, GameAction>> _inputMap = new();


        // Universal Input tracking
        public static InputAction AnyKeyPressed { get; protected set; }
        public static InputDevice CurrentDevice { get; protected set; } = new InputDevice();
        public static Action DeviceChanged;
        #endregion

        /// <summary>
        /// Property that handles input switching behavior internally 
        /// </summary>
        private InputActionMap CurrentInputMap
        {
            get => currentInputMap;
            set
            {
                //we null out the current action map while we disable it.
                var oldMap = currentInputMap;
                currentInputMap = null;
                oldMap?.Disable();

                // Switch to new map.
                currentInputMap = value;
                currentInputMap?.Enable();
            }
        }

        /// <summary>
        /// Used to confirm which input map we're in
        /// </summary>
        ///
        /// <returns>
        /// the name of the current map
        /// </returns>
        public string CurrentMapName => currentInputMap.name;


        /// <summary>
        /// Unity Awake function from MonoBehaviour
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            //initialize dictionaries
            
            //for each action map in input action
            foreach (var map in actions.actionMaps)
            {
                _inputMap.Add(map.name,InitializeMapDictionary(map.name));
            }
            
        }

        private void CheckInputType(InputAction.CallbackContext ctx)
        {
            // Check if any devices are connected
            if (AnyKeyPressed.activeControl == null) return;
            
            // Check if the current device is the same as the new device
            if (AnyKeyPressed.activeControl.device == CurrentDevice) return;
            
            // Update the stored device and invoke the event
            CurrentDevice = AnyKeyPressed.activeControl.device;
            DeviceChanged.Invoke();
        }

        /// <summary>
        /// Swap input maps by string
        /// </summary>
        /// <param name="newMap"></param>
        public void SwapInputMaps(string newMap)
        {
            // Must have map.
            var actionMap = actions.FindActionMap(newMap);
            if (actionMap == null)
            {
                Debug.LogError($"Cannot find action map '{newMap}' in actions '{actions}'", this);
                return;
            }

      
            CurrentInputMap = actionMap;

        }

        public GameAction GetInput(string map, string action)
        {
            var input = _inputMap[map][action];

            if(input == null) Debug.LogError("Action: " + action + " was not found in map: " + map );
            
            return input;
        }
        

        public bool IsCurrentMap(string map)
        {
            if (currentInputMap == null)
                return false;
            return currentInputMap.name == map;
        }
        
        /// <summary>
        /// Performs the steps to make a functioning InputMap reference dictionary
        /// </summary>
        /// <param name="mapName"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Dictionary<string, GameAction> InitializeMapDictionary(string mapName)
        {
            var map = new Dictionary<string, GameAction>();
            //Fill with custom action classes
            foreach (var a in actions.FindActionMap(mapName))
            {
                GameAction g;
                switch (a.type)
                {
                    case InputActionType.PassThrough:
                    case InputActionType.Value:
                        // yo this this wild https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/switch-expression
                        g = a.expectedControlType switch
                        {
                            "Vector2" => new VectorAction(a),
                            "Axis" => new FloatAction(a),
                            _ => new BoolAction(a)
                        };
                        break;
                    case InputActionType.Button:
                        g = new BoolAction(a);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                map.Add(a.name, g);
            }

            return map;
        }
    }

    #region Input Types

    /// <summary>
    /// Abstract class that handles action pairing and basic queries 
    /// </summary>
    public abstract class GameAction
    {
        protected readonly InputAction Action;

        protected GameAction(InputAction action)
        {
            Action = action;
        }

        public bool IsTriggered => Action.WasPressedThisFrame();
        public bool IsReleased => Action.WasReleasedThisFrame();
        public bool IsPressed => Action.IsPressed();
    }

    /// <summary>
    /// Used for Buttons and other On/off type inputs
    /// </summary>
    public class BoolAction : GameAction
    {
        public BoolAction(InputAction action) : base(action)
        {
        }

        public bool Held => Action.ReadValue<float>() >= 1;
    }

    /// <summary>
    /// Used for joystick or directional inputs
    /// </summary>
    public class VectorAction : GameAction
    {
        public VectorAction(InputAction action) : base(action)
        {
        }

        public new bool IsPressed => Action.ReadValue<Vector2>().magnitude > Single.Epsilon;

        public Vector2 Value => Action.ReadValue<Vector2>();
    }

    /// <summary>
    /// Used for triggers or other pressure sensitive input
    /// </summary>
    public class FloatAction : GameAction
    {
        public FloatAction(InputAction action) : base(action)
        {
        }

        public float Value => Action.ReadValue<float>();
    }

    #endregion
}