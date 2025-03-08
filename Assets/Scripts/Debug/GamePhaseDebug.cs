using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace WanderMarch.WBTF
{


    public class GamePhaseDebug : MonoBehaviour
    {
        [SerializeField] private TMP_Text output;

        // Start is called before the first frame update
        void Start()
        {
            WBTF_Events.PhaseChange.AddListener(OutputUpdate);
        }

        void OutputUpdate(GamePhases phase)
        {
            output.text = phase.ToString();
        }
    }
}
