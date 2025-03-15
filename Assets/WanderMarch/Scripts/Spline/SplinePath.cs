using System;
using UnityEngine;

namespace WanderMarch.Scripts.Spline
{
    public class SplinePath : MonoBehaviour
    {
        [SerializeField] private BezierSegment spline = new();

        [SerializeField, Range(0.0f, 1.0f), Tooltip("Used to visualize point along path")]
        private float tTest;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            spline.DrawGizmo();
            OrientedPoint testPoint = spline.GetBezierPoint(tTest);

            Gizmos.color = Color.red;
            
           Gizmos.DrawSphere(testPoint.position, 0.1f);
        }
        #endif
    }
}