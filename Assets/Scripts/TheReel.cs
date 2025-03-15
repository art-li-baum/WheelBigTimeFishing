using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WanderMarch.WBTF
{
    public class TheReel : Singleton<TheReel>
    {
        [field: SerializeField,ShowOnly] public Vector3 ReelStatus { get; private set; }

        [SerializeField] private TMP_Text debugText;
        
        // Start is called before the first frame update
        void Start()
        {
            WBTF_Events.ReelSelected.AddListener(AddReel);
            WBTF_Events.FishCaught.AddListener(ClearReel);
        }

        private void OnDisable()
        {
            WBTF_Events.ReelSelected.RemoveListener(AddReel);
            WBTF_Events.FishCaught.RemoveListener(ClearReel);
        }

        void AddReel(Vector3 reel)
        {
            ReelStatus += reel;

            if (debugText)
                debugText.text = ReelStatus.ToString();
        }

        void ClearReel(FishData data)
        {
            ReelStatus = Vector3.zero;
            
            if (debugText)
                debugText.text = ReelStatus.ToString();
        }
    }
}