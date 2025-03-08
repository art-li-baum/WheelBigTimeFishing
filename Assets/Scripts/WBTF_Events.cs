using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WanderMarch.WBTF
{
    public static class WBTF_Events
    {
        public static UnityEvent<GamePhases> PhaseChange = new();
    }
}