using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderMarch.Scripts.App;
using UnityEngine.Events;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UIElements;
using WanderMarch.Scripts.ActionList;

namespace WanderMarch.WBTF
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField, ShowOnly] private GamePhases currentPhase = GamePhases.Invalid;

        [SerializeField] private GameObject debugPanel;
        private GamePhase _currentPhase;

        private BoolAction _debugToggle;
        
        // Start is called before the first frame update
        void Start()
        {
            currentPhase = GamePhases.Invalid;
            
            //Placeholder initialize
            ChangePhase(new BaitingPhase());

            _debugToggle = InputManager.Instance.GetInput("Fishing", "Debug") as BoolAction;
    
            debugPanel.SetActive(false);
        }

        public void ChangePhase(GamePhase newPhase)
        {
            if (currentPhase == newPhase.phase) return;
            
            if (currentPhase != GamePhases.Invalid)
                _currentPhase.Exit(newPhase.phase);
            
            newPhase.Enter(currentPhase);
            _currentPhase = newPhase;
            currentPhase = _currentPhase.phase;
            
            
            WBTF_Events.PhaseChange.Invoke(currentPhase);
        }

        // Update is called once per frame
        void Update()
        {
            _currentPhase.Update(Time.deltaTime);
            
            if(_debugToggle.IsTriggered)
                debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }

    public enum GamePhases
    {
        Invalid = -1,
        Baiting,
        Fishing,
        Scoring,
    }
    
    
    public abstract class GamePhase
    {
        public GamePhases phase { get; protected set; }
        
        public virtual void Enter(GamePhases previous)
        {}

        public virtual bool Update(float dt)
        {
            return true;
        }
        
        public virtual void Exit(GamePhases next)
        {}
    }

    public class BaitingPhase : GamePhase
    {
        private BoolAction _castAction;

        public BaitingPhase()
        {
            phase = GamePhases.Baiting;
            _castAction = InputManager.Instance.GetInput("Fishing", "Action") as BoolAction;
        }

        public override void Enter(GamePhases previous)
        {
            base.Enter(previous);
            InputManager.Instance.SwapInputMaps("Fishing");
        }

        public override bool Update(float dt)
        {
            if (!base.Update(dt)) return false;

            if (_castAction.IsTriggered)
            {
                GameManager.Instance.ChangePhase(new FishingPhase());
                return false;
            }

            return true;
        }
    }

    public class FishingPhase : GamePhase
    {
        public FishingPhase()
        {
            phase = GamePhases.Fishing;
        }
        
        public override void Enter(GamePhases previous)
        {
            base.Enter(previous);
            InputManager.Instance.SwapInputMaps("Fishing");
        }
    }

    public class ScoringPhase : GamePhase
    {
        private BoolAction _castAction;
        
        public ScoringPhase()
        {
            phase = GamePhases.Scoring;
            _castAction = InputManager.Instance.GetInput("Fishing", "Action") as BoolAction;
        }

        public override bool Update(float dt)
        {
            if (!base.Update(dt)) return false; 

            if (_castAction.IsTriggered)
            {
                GameManager.Instance.ChangePhase(new BaitingPhase());
                return false;
            }

            return true;

        }
    }
    
    
}
