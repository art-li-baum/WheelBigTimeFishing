using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WanderMarch.WBTF
{
    public class FishPool : Singleton<FishPool>
    {

        [field:SerializeField] public List<FishSO> possibleFish { get; private set; }

        private List<FishData> _poolFish = new();

        [SerializeField] private float threshold = 4f;
        
        // Start is called before the first frame update
        void Start()
        {
            PopulateFish();
        }

        public void AddFish2Pool(FishSO fish2add)
        {
            var fd = new FishData(fish2add);
            
            
            WBTF_Events.FishAdded2Pool.Invoke(fd);
            _poolFish.Add(fd);
            ++fish2add.numInPool;
        }

        private FishData TakeFishFromPool(List<Pull> pulls)
        {
            //if there are no fish
            if (pulls.Count < 1 || _poolFish.Count < 1)
                return null;
            
            var totalWeight = pulls.Sum(pull => pull.weight);

            var randomNumber = Random.Range(0, totalWeight);

            FishSO fishType = null; 
            
            foreach (var pull in pulls)
            {
                if (randomNumber < pull.weight)
                {
                    fishType = pull.fish;
                    break;
                }

                randomNumber -= pull.weight;
            }

            var selectedFish = _poolFish.First();
            
            //remove fish from pool
            foreach (var fish in _poolFish)
            {
                if (fishType.fishData.fishName != fish.fishName) continue;
                
                selectedFish = fish;
                break;

            }

            _poolFish.Remove(selectedFish);
            --fishType.numInPool;

            return selectedFish;
        }


        private void PopulateFish()
        {
            foreach (var fish in possibleFish)
            {
                var number = Mathf.RoundToInt((int)fish.fishData.fishRarity * Random.Range(0.65f,1.25f));

                fish.numInPool = 0;
                
                for (var i = 0; i <= number; ++i)
                {
                    AddFish2Pool(fish);
                }
            }
        }

        public void PullFish(Vector3 stats, out FishData fishPullData)
        {
            var validFish = new List<Pull>();
                
            foreach (var fish in possibleFish)
            {
                if (fish.numInPool < 1) continue;
                
                //Evaluate "distance" from each fish
                var distance = fish.Distance(stats);
                
                if(distance > threshold) continue;
                
                //determine weight
                var weight = fish.Weight(distance);
                var pull = new Pull( fish, weight );
                
                validFish.Add(pull);
            }
            //

            fishPullData = TakeFishFromPool(validFish);
        }

        public struct Pull
        {
            public Pull(FishSO f, float w)
            {
                fish = f;
                weight = w;
            }
            public FishSO fish;
            public float weight;
        }
    }
}
