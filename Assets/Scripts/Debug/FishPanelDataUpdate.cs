using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WanderMarch.WBTF
{
    public class FishPanelDataUpdate : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text stats;

        [SerializeField] private Image image;

        private FishData _fishData;

        private void Start()
        {
            WBTF_Events.FishCaught.AddListener(RemoveFish);
        }

        private void OnDisable()
        {
            WBTF_Events.FishCaught.RemoveListener(RemoveFish);
        }

        public void UpdateFishData(FishData data)
        {
            _fishData = data;
            
            nameText.text = data.fishName;
            stats.text = "Stats:" + data.fishSize + " Vec:" + data.fishStats;
            image.sprite = Sprite.Create(data.fishTexture,
                new Rect(0, 0, data.fishTexture.width, data.fishTexture.height), Vector2.one * 0.5f);
        }

        public void RemoveFish(FishData data)
        {
            if(data == _fishData)
                gameObject.SetActive(false);
        }
    }
}