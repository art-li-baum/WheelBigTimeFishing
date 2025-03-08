using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WanderMarch.Scripts.Math;

namespace WanderMarch.Scripts.ActionList
{
    public class MoveAction : Action
    {
        private Vector3 _startPos;
        private readonly Vector3 _endPos;
        private readonly Transform _transform;

        public MoveAction(Transform trans, Vector3 position, float duration, bool blocking = false, int channel = 1,
            float delay = 0)
            : base(duration, blocking, channel, delay)
        {
            Label = "Move " + trans.gameObject.name;
            _transform = trans;
            _endPos = position;
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            _startPos = _transform.position;
        }

        protected override void Act(float dt)
        {
            var t = CurrentTime / Duration;
            MathUtil.EaseOut(ref t);
            _transform.position = Vector3.Lerp(_startPos, _endPos, t);
        }

        protected override void OnExit()
        {
            base.OnExit();
            _transform.position = _endPos;
        }
    }

    public class SpinAction : Action
    {
        private Quaternion _startRot;
        private readonly Quaternion _endRot;
        private readonly Transform _transform;

        public SpinAction(Transform trans, float targetAngle, float duration, bool blocking = false, int channel = 1,
            float delay = 0)
            : base(duration, blocking, channel, delay)
        {
            Label = "Spin " + trans.gameObject.name;
            _transform = trans;
            _endRot = Quaternion.Euler(0, 0, targetAngle);
        }

        public SpinAction(Transform trans, Quaternion targetRot, float duration, bool blocking = false, int channel = 1,
            float delay = 0)
            : base(duration, blocking, channel, delay)
        {
            Label = "Spin " + trans.gameObject.name;
            _transform = trans;
            _endRot = targetRot;
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            _startRot = _transform.localRotation;
        }

        protected override void Act(float dt)
        {
            var t = CurrentTime / Duration;
            MathUtil.EaseOut(ref t);
            _transform.localRotation = Quaternion.Lerp(_startRot, _endRot, t);
        }

        protected override void OnExit()
        {
            base.OnExit();
            _transform.localRotation = _endRot;
        }
    }

    public class ScaleAction : Action
    {
        private Vector3 _startScale;
        private readonly Vector3 _endScale;
        private readonly Transform _transform;

        public ScaleAction(Transform trans, Vector3 targetScale, float duration, bool blocking = false, int channel = 1,
            float delay = 0)
            : base(duration, blocking, channel, delay)
        {
            Label = "Scale " + trans.gameObject.name;
            _transform = trans;
            _endScale = targetScale;
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            _startScale = _transform.localScale;
        }

        protected override void Act(float dt)
        {
            var t = CurrentTime / Duration;
            _transform.localScale = Vector3.Lerp(_startScale, _endScale, t);
        }

        protected override void OnExit()
        {
            base.OnExit();
            _transform.localScale = _endScale;
        }
    }

    public class ActivateAction : Action
    {
        private readonly GameObject _activeObject;
        private readonly bool _setActive;

        public ActivateAction(GameObject object2Active, bool setActive, bool blocking = false,
            int channel = 1, float delay = 0)
            : base(-1, blocking, channel, delay)
        {
            Label = "Activate " + object2Active.name;
            _activeObject = object2Active;
            _setActive = setActive;
        }

        protected override void Act(float dt)
        {
            _activeObject?.SetActive(_setActive);
            Duration = 0;
        }
    }

    public class WaitAction : Action
    {
        public WaitAction(float duration, int channel = 1) : base(duration, true, channel)
        {
            Label = "Wait";
        }

        public void Resume()
        {
            Duration = 0;
        }
    }
    //TODO: Clear Action DOES NOT WORK
    public class ClearAction : Action
    {
        private readonly List<Action> _clearList;
        private readonly bool _clearAll;
        private int index;

        public ClearAction(List<Action> list, bool clearAll, int channel = 1, float delay = 0) : base(-1, true, channel,
            delay)
        {
            Label = "Clear";
            if (clearAll)
                Label += " All";
            _clearList = list;
            _clearAll = clearAll;
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            if (!_clearAll)
                index = _clearList.IndexOf(this);
        }

        protected override void Act(float dt)
        {
            base.Act(dt);
            if (_clearAll)
            {
                _clearList.Clear();
            }
            else
            {
                _clearList.RemoveRange(index, _clearList.Count);
            }
        }
    }

    public class GroupAction : Action
    {
        private readonly List<Action> _actions = new();

        public GroupAction(bool blocking, int channel = 1, float delay = 0) : base(-1f, blocking, channel, delay)
        {
            Label = "Group";
            IsLive = false;
        }

        public GroupAction AddAction(Action action)
        {
            if (!IsLive)
            {
                IsLive = true;
            }

            _actions.Add(action);
            return this;
        }

        public void SetBlocking(bool isBlocking)
        {
            IsBlocking = isBlocking;
        }

        protected override void Act(float dt)
        {
            base.Act(dt);

            if (_actions.Count < 1)
            {
                Duration = 0;
                return;
            }

            var message = "";

            for (var index = 0; index < _actions.Count; index++)
            {
                var action = _actions[index];
                message += action.PrintStatus() + '\n';
                action.Tick(dt);
                if (action.IsBlocking) break;
            }
            
            AL.GroupMessage(message);

            //Remove all actions that aren't live
            _actions.RemoveAll(a => !a.IsLive);
        }
    }

    public class DelegateAction : Action
    {
        private System.Action _function;

        public DelegateAction(bool blocking, System.Action func, int channel = 1, float delay = 0) : base(-1, blocking,
            channel, delay)
        {
            Label = "Delegate" + func;
            _function = func;
        }

        protected override void Act(float dt)
        {
            base.Act(dt);
            //Do the thing
            _function();
            Duration = 0;
        }
    }

    public class DelegateAction<T> : Action
    {
        private System.Action<T> _function;
        private T _input;

        public DelegateAction(bool blocking, System.Action<T> func, T input,int channel = 1,  float delay = 0) : base(-1, blocking,
            channel,delay)
        {
            Label = "Delegate" + func;
            _function = func;
            _input = input;
        }

        protected override void Act(float dt)
        {
            base.Act(dt);
            //Do the thing
            _function(_input);
            Duration = 0;
        }
    }

    public class FadeAction : Action
    {
        private readonly SpriteRenderer _spriteRenderer;
        private readonly Image _uiImage;
        private readonly TMP_Text _tmpText;
        private Color _currentColorAlpha;
        private float _currentFloatAlpha;
        private readonly Color _goalColorAlpha;
        private readonly float _goalFloatAlpha;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="blocking"></param>
        /// <param name="sprite"></param>
        /// <param name="fadeIn"></param>
        /// <param name="alpha">Level of opacity when fading in, does nothing in fadeout</param>
        /// <param name="channel"></param>
        /// <param name="delay"></param>
        public FadeAction(float duration, bool blocking, SpriteRenderer sprite, bool fadeIn,float alpha =1, int channel = 1, float delay = 0) : base(
            duration, blocking,channel, delay)
        {
            Label = "Fade " + sprite.gameObject.name;
            _spriteRenderer = sprite;
            var color = sprite.color;
            _goalColorAlpha = fadeIn ? new Color(color.r,color.g,color.b,alpha): new Color(color.r,color.g,color.b,0);
        }

        public FadeAction(float duration, bool blocking, Image image, bool fadeIn, float alpha =1, int channel = 1, float delay = 0) : base(duration,
            blocking,channel, delay)
        {
            Label = "Fade " + image.gameObject.name;
            _uiImage = image;
            var color = image.color;
            _goalColorAlpha = fadeIn ? new Color(color.r,color.g,color.b,alpha): new Color(color.r,color.g,color.b,0);
        }

        public FadeAction(float duration, bool blocking, TMP_Text text, bool fadeIn, float alpha =1, int channel = 1, float delay = 0) : base(duration,
            blocking,channel, delay)
        {
            Label = "Fade " + text.gameObject.name;
            _tmpText = text;
            _goalFloatAlpha = fadeIn ? alpha : 0f;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            if (_spriteRenderer)
                _currentColorAlpha = _spriteRenderer.color;
            else if (_uiImage)
                _currentColorAlpha = _uiImage.color;
            else if (_tmpText)
                _currentFloatAlpha = _tmpText.alpha;
        }

        protected override void Act(float dt)
        {
            base.Act(dt);
            var t = CurrentTime / Duration;
            MathUtil.EaseOut(ref t);


            if (_spriteRenderer)
                _spriteRenderer.color = Color.Lerp(_currentColorAlpha, _goalColorAlpha, t);
            else if (_uiImage)
                _uiImage.color = Color.Lerp(_currentColorAlpha, _goalColorAlpha, t);
            else if (_tmpText)
                _tmpText.alpha = Mathf.Lerp(_currentFloatAlpha, _goalFloatAlpha, t);
        }

        protected override void OnExit()
        {
            base.OnExit();
            if (_spriteRenderer)
                _spriteRenderer.color = _goalColorAlpha;
            else if (_uiImage)
                _uiImage.color = _goalColorAlpha;
            else if (_tmpText)
                _tmpText.alpha = _goalFloatAlpha;
        }
    }
}