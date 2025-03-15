using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WanderMarch.WBTF
{
    public static class WBTF_Events
    {
        public static UnityEvent<GamePhases> PhaseChange = new();
        public static UnityEvent<FishData> FishAdded2Pool = new();
        public static UnityEvent<FishData> FishCaught = new();

        public static UnityEvent<Vector3> ReelSelected = new();
        public static UnityEvent<BaitSO> SwappedBait = new();

        public static UnityEvent Cast = new();
    }
}