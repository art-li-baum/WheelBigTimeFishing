using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderMarch.Scripts.ActionList
{
    public class ActionList : Singleton<ActionList>
    {
        [SerializeField, Range(0, 10f)] private float speed = 1f;

        public float Speed
        {
            get => speed;
            set
            {
                if (value > 0) speed = value;
            }
        }
        [SerializeField, ShowOnly] private int activeBitChannels = 255;
        [SerializeField] private bool showLog = false;
        [SerializeField, Range(0, 1000)] private int logOffset = 0;
        private readonly List<Action> _actions = new();
        private readonly List<Action> _queueActions = new();


        private string _messageBuffer;
        private string _groupBuffer0 = "";
        private string _groupBuffer1 = "";
        private string _groupBuffer2 = "";
        private string _groupBuffer3 = "";
        private string _groupBuffer4 = "";
        private string _groupBuffer5 = "";

        private void Update()
        {
            if (showLog)
            {
                _messageBuffer = _groupBuffer0 =
                    _groupBuffer1 = _groupBuffer2 = _groupBuffer3 = _groupBuffer4 = _groupBuffer5 = "";
            }

            var dt = Time.deltaTime * speed;

            foreach (var action in _actions)
            {
                if (showLog)
                    _messageBuffer += action.PrintStatus() + '\n';
                if (!action.ActiveChannel(activeBitChannels)) continue;
                action.Tick(dt);
                if (action.IsBlocking) break;
            }

            //Remove all actions that aren't live
            _actions.RemoveAll(a => !a.IsLive);

            //Add all actions in the queue to the list
            FlushQueue();
        }

        private void OnGUI()
        {
            if (!showLog) return;
            var style = new GUIStyle();
            style.fontSize = (int)(20.0f * (Screen.width / 1920.0f));
            GUI.Label(new Rect(logOffset, 0, 200, Screen.height), _messageBuffer);
            GUI.Label(new Rect(logOffset + 150, 0, 200, Screen.height), _groupBuffer0);
            GUI.Label(new Rect(logOffset + 250, 0, 200, Screen.height), _groupBuffer1);
            GUI.Label(new Rect(logOffset + 350, 0, 200, Screen.height), _groupBuffer2);
            GUI.Label(new Rect(logOffset + 450, 0, 200, Screen.height), _groupBuffer3);
            GUI.Label(new Rect(logOffset + 550, 0, 200, Screen.height), _groupBuffer4);
            GUI.Label(new Rect(logOffset + 650, 0, 200, Screen.height), _groupBuffer5);
        }

        public void AddAction(Action action)
        {
            _actions.Add(action);
        }

        public void QueueAction(Action action)
        {
            _queueActions.Add(action);
        }

        private void FlushQueue()
        {
            _actions.AddRange(_queueActions);
            _queueActions.Clear();
        }

        public void GroupMessage(string message)
        {
            if (_groupBuffer0 == "")
                _groupBuffer0 = message;
            else if (_groupBuffer1 == "")
                _groupBuffer1 = message;
            else if (_groupBuffer2 == "")
                _groupBuffer2 = message;
            else if (_groupBuffer3 == "")
                _groupBuffer3 = message;
            else if (_groupBuffer4 == "")
                _groupBuffer4 = message;
            else if (_groupBuffer5 == "")
                _groupBuffer5 = message;
            else
                Debug.LogWarning("Ran Out of Group MessageBuffers");
        }

        public void ActivateChannel(int channel)
        {
            activeBitChannels |= 1 << channel;
        }

        public void DeactivateChannel(int channel)
        {
            activeBitChannels &= ~(1 << channel);
        }
    }

    [Serializable]
    public abstract class Action
    {
        protected ActionList AL { get; private set; }
        private readonly int _channel;
        public bool IsLive { get; protected set; } = true;
        public bool IsBlocking { get; protected set; } = false;
        protected float CurrentTime;
        protected float Duration;
        protected float Delay;
        protected string Label = "Action";
        public bool IsRunning { get; protected set; } = false;


        protected Action(float duration, bool blocking, int channel = 1, float delay = 0)
        {
            AL = ActionList.Instance;
            IsBlocking = blocking;
            Duration = duration;
            CurrentTime = 0f;
            Delay = delay;
            _channel = channel;
        }

        protected virtual void Act(float dt)
        {
        }

        public void Tick(float dt)
        {
            if (IsDelayed(dt)) return;

            CurrentTime += dt;

            //Duration of -1 makes it indefinite
            if (Duration < 0 || CurrentTime < Duration)
            {
                if (!IsRunning)
                    OnEnter();

                Act(dt);
                return;
            }

            IsLive = false;
            OnExit();
        }

        public bool IsDelayed(float dt)
        {
            if (Delay <= 0) return false;
            return (Delay -= dt) > 0;
        }

        protected virtual void OnEnter()
        {
            IsRunning = true;
        }

        protected virtual void OnExit()
        {
        }

        public string PrintStatus()
        {
            var s = _channel + " : " + Label;
            if (IsRunning)
                s += " (" + System.Math.Truncate((CurrentTime / Duration) * 100f) + ") ";
            if (IsBlocking)
                s += " X ";
            return s;
        }

        public bool ActiveChannel(int channelMask)
        {
            var bitflag = 1 << _channel;
            return (bitflag & channelMask) == bitflag;
        }
    }
}