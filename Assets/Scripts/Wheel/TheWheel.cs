using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using WanderMarch.Scripts.App;
using WanderMarch.WBTF;

// CODE (C) Colin McInerney 2025

public enum WheelState { AwaitingSelection, Rotating, NoInput, Finished }

public class TheWheel : Singleton<TheWheel>
{
    /*
        Hello and welcome to the Persuasion Wheel Game Jam AKA WHEELJAM!
    
        This code is free to use and distribute for the purposes of WHEELJAM and any games you choose to make with it besides. Please provide attribution if you use it outside the jam. :)
        This code is also not required for WHEELJAM -- you are free to make your own implementation of the wheel, use your own assets, etc.
        Everything to make the wheel function is in this single script -- this is a deliberate choice to keep it plug-n-play for the jam. I don't recommend coding like this!
    
        Have fun and enjoy WHEELJAM!
        Love, Colin
    */

    #region Events
    public UnityEvent<WheelPayload> newDirChosen;
    public UnityEvent<WheelPayload> rotationStarted;
    public UnityEvent rotationFinished;
    public UnityEvent puzzleFinished;
    #endregion

    #region Private Fields
    [SerializeField] private Transform[] slices; // these are the triangles of varying sizes that rotate and multiply by x1, x2, x3 or x4
    [SerializeField] private Transform[] covers; // once a quadrant is selected, it becomes disabled for selection, and these appear overlaid on the quadrant to communicate that
    [SerializeField] private Transform selector; // players input to move this -- controlled by sending a Vector into the ProcessInput function
    [SerializeField] private Transform sliceGimbal; // this is the thing that actually gets rotated
    
    [SerializeField] private int[] baseNumbers = new int[] { -2, -1, 1, 2 }; // order these from lowest value to greatest
    [SerializeField] private int[] sliceValues = new int[] { 1, 2, 3, 4 }; // these should also be lowest to greatest

    [SerializeField] private AnimationCurve curve; // used to evaluate how the turn animates
    
    private Dictionary<Vector3, int> _valueMappings; // this gets set in Awake -- ties a direction to a baseNumber value randomly
    [SerializeField, ShowOnly]  private WheelState currentState = WheelState.NoInput; // don't touch this unless you know what you're doing lol
    [SerializeField, ShowOnly] private int _numSelections = 0; // how many selections total have been made
    private WheelPayload _currentValue; // this is the actual current value selected
    
    // declaring directions as rotations here
    private readonly Vector3 _dirUp = new Vector3(0f, 180f, 0f);
    private readonly Vector3 _dirRight = new Vector3(0f, 270f, 0f);
    private readonly Vector3 _dirDown = new Vector3(0f, 0f, 0f);
    private readonly Vector3 _dirLeft = new Vector3(0f, 90f, 0f);
    
    private readonly int _targetSelections = 4; // how many selections _should_ be made

    private VectorAction _moveAction;
    private BoolAction _action;
    #endregion

    #region Unity Methods

    private void Start()
    {
        currentState = WheelState.NoInput;
        
        _moveAction =  InputManager.Instance.GetInput("Fishing","Move") as VectorAction;
        _action = InputManager.Instance.GetInput("Fishing", "Action") as BoolAction;

        WBTF_Events.PhaseChange.AddListener(StartCheck);
    }
    
    // throw this out and use unity's new input system or whatever plugin you decide, get your input, and pass it off through the ProcessDirectionInput() and ProcessConfirmInput() methods; this is only here for demonstration purposes and is inefficient and inflexible
    private void Update()
    {
        switch (currentState)
        {
            case WheelState.AwaitingSelection:
                if (_action.IsTriggered)
                {
                    ProcessConfirmInput();
                    break;
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Rotate();
                    return;
                }

                if(_moveAction.IsTriggered)
                    ProcessDirectionInput(_moveAction.Value);
                
                break;
            case WheelState.Rotating:
                break;
            case WheelState.NoInput:
                break;
            case WheelState.Finished:
                if (_moveAction.IsTriggered)
                {
                    GameManager.Instance.ChangePhase(new BaitingPhase());
                    currentState = WheelState.NoInput;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Public Methods
    public void ProcessDirectionInput(Vector2 input) // once you get your directional input from whatever, pass it into this function to change the direction of the wheel
    {
        Vector3 cacheDir = selector.localEulerAngles;

        selector.localEulerAngles = input.x switch
        {
            > 0 => _dirRight,
            < 0 => _dirLeft,
            _ => input.y switch
            {
                > 0 => _dirUp,
                < 0 => _dirDown,
                _ => selector.localEulerAngles
            }
        };

        if (selector.localEulerAngles == cacheDir) return;
        
        _currentValue = GetCurrentWheelValue();
        newDirChosen?.Invoke(_currentValue);
    }
    
    public void ProcessConfirmInput()
    {
        foreach (Transform c in covers)
        {
            if (c.localEulerAngles == selector.localEulerAngles)
            {
                if (!c.gameObject.activeSelf)
                {
                    c.gameObject.SetActive(true);
                    Rotate();
                    _numSelections++;
                    return;
                }
            }
        }
    }
    
    // you can call this independently if you want feature parity with TES4
    public void Rotate()
    {
        currentState = WheelState.Rotating;
        rotationStarted?.Invoke(_currentValue);
        
        StartCoroutine(RotateSlices(new Vector3(0, -90, 0f)));
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        selector.localEulerAngles = Vector3.zero; // remove this if you don't want the selector to reset up every time
        sliceGimbal.localEulerAngles = Vector3.zero;
        _numSelections = 0;
        
        List<Vector3> directions = GetDirectionsList();
        
        // hide the covers
        for (int i = 0; i < directions.Count; i++)
        {
            covers[i].localEulerAngles = directions[i];
            covers[i].gameObject.SetActive(false);
        }
        
        _valueMappings = new Dictionary<Vector3, int>();
        
        for (int i = 0; i < slices.Length; i++)
        {
            Vector3 dir = directions[UnityEngine.Random.Range(0, directions.Count)];
            
            _valueMappings.Add(dir, baseNumbers[i]);
            directions.Remove(dir);
        }
        
        InitializeSlices();
        
        _currentValue = GetCurrentWheelValue();
        newDirChosen?.Invoke(_currentValue);
        
        currentState = WheelState.AwaitingSelection;
    }
    #endregion

    #region Private Methods
    private void InitializeSlices()
    {
        // create list of possible directions
        List<Vector3> directions = GetDirectionsList();

        // assign one to each slice
        foreach (Transform slice in slices)
        {
            Vector3 dir = directions[UnityEngine.Random.Range(0, directions.Count)];
            slice.localEulerAngles = dir;
            directions.Remove(dir);
        }
    }

    private List<Vector3> GetDirectionsList()
    {
        List<Vector3> directions = new List<Vector3>
        {
            _dirUp,
            _dirRight,
            _dirDown,
            _dirLeft
        };

        return directions;
    }
    
    private IEnumerator RotateSlices(Vector3 newEuler)
    {
        Quaternion newRotation = Quaternion.Euler(sliceGimbal.localEulerAngles + newEuler);
        Quaternion startRotation = sliceGimbal.localRotation;
        
        float timer = 0f;

        float lastKeyTime = curve.keys[^1].time;
        
        while (timer < lastKeyTime)
        {
            math.remap(0f, lastKeyTime, 0f, 1f, timer);
            sliceGimbal.localRotation = Quaternion.Lerp(startRotation, newRotation, curve.Evaluate(timer));
            timer += Time.deltaTime;
            
            yield return null;
        }

        sliceGimbal.localRotation = newRotation;

        _currentValue = GetCurrentWheelValue();
        newDirChosen?.Invoke(_currentValue);
        rotationFinished?.Invoke();
        EndCheck();
    }

    private void StartCheck(GamePhases phase)
    {
        if(phase == GamePhases.Fishing)
            Reset();
    }

    // this gets added as a listener in awake to rotationFinished
    private void EndCheck()
    {
        if (_numSelections >= _targetSelections)
        {
            currentState = WheelState.Finished;
            
            puzzleFinished?.Invoke();
        }
        else
        {
            currentState = WheelState.AwaitingSelection;
        }
    }

    private WheelPayload GetCurrentWheelValue()
    {
        WheelPayload wp = new WheelPayload();
        
        foreach (KeyValuePair<Vector3, int> kvp in _valueMappings)
        {
            if ((int)selector.localEulerAngles.y == (int)kvp.Key.y)
            {
                for (int i = 0; i < slices.Length; i++)
                {
                    if ((int)slices[i].localEulerAngles.y == (int)kvp.Key.y)
                    {
                        wp.BaseValue = kvp.Value;
                        wp.SliceValue = sliceValues[i];
                        wp.TotalValue = wp.BaseValue * wp.SliceValue;
                        return wp;
                    }
                }
            }
        }
        
        return null; // you fricked up
    }
    #endregion
}

public class WheelPayload
{
    public int BaseValue;
    public int SliceValue;
    public int TotalValue;
}
