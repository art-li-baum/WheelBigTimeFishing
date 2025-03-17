using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderMarch.Scripts.ActionList;

namespace WanderMarch.WBTF
{
    public class Fishualizer : Singleton<Fishualizer>
    {
        [SerializeField] private float boxScale = 10f;

        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _hook;
        
        private Transform _indicators;

        protected override void Awake()
        {
            base.Awake();
            
            _indicators = transform.Find("Indicators");
        }

        private void OnEnable()
        {
            WBTF_Events.PhaseChange.AddListener(UpdateFishualizer);
            WBTF_Events.ReelSelected.AddListener(MoveHook);
        }

        private void OnDisable()
        {
            WBTF_Events.PhaseChange.RemoveListener(UpdateFishualizer);
            WBTF_Events.ReelSelected.RemoveListener(MoveHook);
        }

        private void MoveHook(Vector3 position)
        {
            ActionList.Instance.QueueAction(new MoveAction(_hook,_hook.position + (position / (boxScale * 2f)),0.2f,true));
            ActionList.Instance.QueueAction(new ActivateAction(_lineRenderer.gameObject,false));
            //_hook.localPosition = position / (boxScale * 2f);
        }

        public void ShowValue(WheelPayload wp)
        {
            _lineRenderer.gameObject.SetActive(true);
            _lineRenderer.SetPosition(0, _hook.localPosition);
            _lineRenderer.SetPosition(1, _hook.localPosition + wp.TotalValue/(boxScale * 2f));
        }

        public void UpdateFishualizer(GamePhases phase)
        {
            if (phase != GamePhases.Baiting) return;

            var possibleFish = FishPool.Instance.possibleFish;
            
            foreach (Transform child in _indicators)
            {
                child.gameObject.SetActive(false);
            }
            
            _hook.localPosition = Vector3.zero;
            
            
            foreach (var fish in possibleFish)
            {
                if(fish.numInPool < 1) continue;

                foreach (Transform child in _indicators)
                {
                    //TODO: change color if hasn't been caught
                    if(child.gameObject.activeSelf) continue;

                    child.localPosition = fish.fishData.fishStats / (boxScale * 2f);
                    child.gameObject.SetActive(true);
                    break;
                }
            }

        }
    }
}