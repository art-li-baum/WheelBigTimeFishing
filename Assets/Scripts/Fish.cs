using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WanderMarch.WBTF
{
    public class Fish
    {
        public FishData fishData;
        public Vector2 sizeRange;
        public int numInPool = 0;

        public float Distance(Vector3 state)
        {
            return Mathf.Abs((state - fishData.fishStats).magnitude);
        }

        public float Weight(float distance)
        {
            return distance + 10 * (int)fishData.fishRarity;
        }
    }


    [Serializable]
    public class FishData
    {
        public FishData(Fish fish)
        {
            fishName = fish.fishData.fishName;
            fishStats = fish.fishData.fishStats;
            fishTexture = fish.fishData.fishTexture;
            fishSize = Random.Range(fish.sizeRange.x, fish.sizeRange.y);
            fishRarity = fish.fishData.fishRarity;
        }

        public enum Rarity
        {
            Exotic = 1,
            Rare = 2,
            Uncommon = 4,
            Common = 8
        }

        public FishData()
        {
        }

        public string fishName = "default";
        public Vector3 fishStats;
        public Texture2D fishTexture;
        public float fishSize = 1f;
        public Rarity fishRarity = Rarity.Common;
    }
}

