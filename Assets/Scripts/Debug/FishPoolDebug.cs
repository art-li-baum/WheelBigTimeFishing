using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderMarch.WBTF
{
    public class FishPoolDebug : MonoBehaviour
    {
        [SerializeField] private GameObject panelPrefab;
        
        // Start is called before the first frame update
        void OnEnable()
        {
            WBTF_Events.FishAdded2Pool.AddListener(AddFish);
        }

        private void OnDisable()
        {
            WBTF_Events.FishAdded2Pool.RemoveListener(AddFish);
        }

        void AddFish(FishData data)
        {
            var panel = Instantiate(panelPrefab, transform);
            
            panel.GetComponent<FishPanelDataUpdate>().UpdateFishData(data);
        }
        
        
    }
}