using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderMarch.WBTF
{
    
    
    [CreateAssetMenu(fileName = "Bait", menuName = "Data Assets/Bait")]
    public class BaitSO : ScriptableObject
    {
        public Texture Icon;
        public string title;
        public BaitAction[] Actions;
    }

    [Serializable]
    public struct BaitAction
    {
        public BaitAction(string t, Vector3 v)
        {
            title = t;
            value = v;
        }

        public string title;
        public Vector3 value;
    }
    
}