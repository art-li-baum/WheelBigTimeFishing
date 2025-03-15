using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WanderMarch.Scripts.ActionList;


namespace WanderMarch.WBTF
{
    public class FishCatch : MonoBehaviour
    {
        private static readonly int Nothing = Shader.PropertyToID("nothing");
        [SerializeField,ShowOnly] private Material _fishMat;
        [SerializeField] private TMP_Text label;

        private Texture2D _clearTexture;
        private void Start()
        {
            _fishMat = GetComponent<MeshRenderer>().sharedMaterial;
            WBTF_Events.FishCaught.AddListener(CatchFish);
            WBTF_Events.PhaseChange.AddListener(ThrowBack);
            
            _clearTexture = new Texture2D(1, 1);
            _clearTexture.SetPixel(0,0,Color.clear);
            
            _fishMat.mainTexture = _clearTexture;
            label.gameObject.SetActive(false);
        }

        public void ThrowBack(GamePhases phase)
        {
            if (phase == GamePhases.Baiting)
            {
                ActionList.Instance.QueueAction(new MoveAction(transform,new Vector3(-43,-11,43),1f));
                label.gameObject.SetActive(false);
            }
        }

        public void CatchFish(FishData caught)
        {
            if (caught != null)
            {
                Debug.Log("Caught!: " + caught.fishName);
                _fishMat.mainTexture = caught.fishTexture;
                transform.localScale = Vector3.one * caught.fishSize ;
                label.text = caught.fishName;
                ActionList.Instance.QueueAction(new MoveAction(transform,Vector3.zero,caught.fishSize,true));
                ActionList.Instance.QueueAction(new ActivateAction(label.gameObject,true));
            }
            else
            {
                Debug.Log("Didn't Catch nuttun :(");
                _fishMat.mainTexture = _clearTexture;
            }
        }
    }
}