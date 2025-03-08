using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderMarch.Scripts.App;
using UnityEngine.Events;

namespace WanderMarch.WBTF
{
    
    
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField, ShowOnly] private GamePhases currentPhase = GamePhases.Invalid;

        private GamePhase _currentPhase;
        
        // Start is called before the first frame update
        void Start()
        {
            currentPhase = GamePhases.Invalid;
            
            //Placeholder initialize
            ChangePhase(new BaitingPhase());

        }

        public bool ChangePhase(GamePhase newPhase)
        {
            if (currentPhase == newPhase.phase) return false;
            
            if (currentPhase != GamePhases.Invalid)
                _currentPhase.Exit(newPhase.phase);
            
            newPhase.Enter(currentPhase);
            _currentPhase = newPhase;
            currentPhase = _currentPhase.phase;
            
            WBTF_Events.PhaseChange.Invoke(currentPhase);
            
            return true;
        }

        // Update is called once per frame
        void Update()
        {
            _currentPhase.Update(Time.deltaTime);
        }
    }

    public enum GamePhases
    {
        Invalid = -1,
        Baiting,
        Fishing,
        Reeling,
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
    
    
}
