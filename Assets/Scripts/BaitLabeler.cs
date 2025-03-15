using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WanderMarch.WBTF
{
    public class BaitLabeler : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] boxes;

        private void OnEnable()
        {
            WBTF_Events.SwappedBait.AddListener(LabelReel);
        }

        private void OnDisable()
        {
            WBTF_Events.SwappedBait.RemoveListener(LabelReel);
        }

        public void LabelReel(BaitSO bait)
        {
            for (var i = 0; i < 4; ++i)
            {
                boxes[i].text = bait.Actions[i].title;
            }
        }
    }
}