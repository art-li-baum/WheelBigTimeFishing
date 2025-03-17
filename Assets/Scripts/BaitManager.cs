using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace WanderMarch.WBTF
{
    public class BaitManager : Singleton<BaitManager>
    {
        [SerializeField] private bool available;

        [SerializeField] private GameObject baitPanel;
        [SerializeField] private Button baitButton;
        [field: SerializeField, ShowOnly] public BaitSO equippedBait { get; private set; }

        [SerializeField] private BaitSO defaultBait;
        protected override void Awake()
        {
            base.Awake();

           
        }

        private void Start()
        {
            WBTF_Events.PhaseChange.AddListener(SetAvailable);
            SetBait(defaultBait);
        }

        private void OnDisable()
        {
            WBTF_Events.PhaseChange.RemoveListener(SetAvailable);
        }

        public void SetBait(BaitSO newBait)
        {
            equippedBait = newBait;
            WBTF_Events.SwappedBait.Invoke(newBait);
        }

        private void SetAvailable(GamePhases phase)
        {
            baitButton.interactable = phase == GamePhases.Baiting;
        }
        
        
        public void ToggleBaitBox()
        {
                baitPanel.SetActive(!baitPanel.activeSelf);
        }
    }
    
}