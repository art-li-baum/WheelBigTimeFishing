using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace WanderMarch.Scripts.Spline
{
    [Serializable]
    public class BezierSegment
    {
        [SerializeField] private Transform[] controlPoints = new Transform[4];

         [Range(50, 500), SerializeField] private int numberOfSegments = 50;


        public Vector3 GetPos(int i) => controlPoints[i].position;

        public Transform GetTrans(int i) => controlPoints[i];

        [NonSerialized] private float[] _segmentLengths;


        private float GetParametricFromLength(float targetDistance)
        {
            int index = Array.BinarySearch(_segmentLengths, targetDistance);

            if (index < 0)
            {
                index = ~index;
                if (_segmentLengths[index] > targetDistance)
                {
                    --index;
                }
            }

            return index / (float)(_segmentLengths.Length - 1);
        }

        public OrientedPoint[] PositionsAlongCurve(float distanceBetweenPoints)
        {
            int numberOfObject = Mathf.FloorToInt(Length / distanceBetweenPoints);

            var points = new OrientedPoint[numberOfObject];

            for (int i = 0; i < numberOfObject; ++i)
            {
                var t = GetParametricFromLength(distanceBetweenPoints * i);
                points[i] = GetBezierPoint(t);
            }

            return points;
        }

        private float _length = 0f;

        public float Length
        {
            //TODO: make it so there is a check when a point is moved
            get => CalculateLength();
            private set => _length = value;
        }

        private float CalculateLength()
        {
            // Initialize the distance to 0
            float arcLength = 0.0f;

            float segmentIncrease = 1f / numberOfSegments;
            _segmentLengths = new float[numberOfSegments + 1];
            _segmentLengths[0] = 0f;

            // Initialize the starting point to the first control point
            Vector3 prevPoint = GetPos(0);
            Vector3 currPoint;


            // Set t to the desired interval (dt)
            float t = segmentIncrease;

            // Find the points on the curve at t = dt, 2 * dt, 3 * dt, etc. until t > 1
            for (int i = 1; i < numberOfSegments + 1; i++)
            {
                // Find the point on the curve at t
                currPoint = GetBezierPoint(t).position;

                // Add the distance between the previous point and the current point to the total distance
                arcLength += Vector3.Distance(prevPoint, currPoint);

                _segmentLengths[i] = arcLength;

                // Set the current point to the previous point for the next iteration
                prevPoint = currPoint;

                // Increment t by the desired interval (dt)
                t += segmentIncrease;
            }

            return _length = arcLength;
        }


        public OrientedPoint GetBezierPoint(float t)
        {
            Vector3 p0 = GetPos(0);
            Vector3 p1 = GetPos(1);
            Vector3 p2 = GetPos(2);
            Vector3 p3 = GetPos(3);

            Vector3 a = Vector3.Lerp(p0, p1, t);
            Vector3 b = Vector3.Lerp(p1, p2, t);
            Vector3 c = Vector3.Lerp(p2, p3, t);

            Vector3 d = Vector3.Lerp(a, b, t);
            Vector3 e = Vector3.Lerp(b, c, t);

            return new OrientedPoint(Vector3.Lerp(d, e, t), (e - d).normalized);
        }

        public void DrawGizmo()
        {
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawSphere(GetPos(i), 0.1f);
                var transPos = GetTrans(i).position;
                float size = HandleUtility.GetHandleSize(transPos) * 0.5f;
                Handles.FreeMoveHandle(transPos, size, Vector3.one * 0.1f,
                    Handles.ArrowHandleCap);
            }

            Handles.DrawBezier
            (
                GetPos(0), GetPos(3),
                GetPos(1), GetPos(2),
                Color.white, EditorGUIUtility.whiteTexture, 1f
                
            );
        }
    }

    public struct OrientedPoint
    {
        public Vector3 position;
        public Quaternion orientation;

        public OrientedPoint(Vector3 pos, Quaternion rot)
        {
            position = pos;
            orientation = rot;
        }

        public OrientedPoint(Vector3 pos, Vector3 forward)
        {
            this.position = pos;
            this.orientation = Quaternion.LookRotation(forward.normalized);
        }

        public Vector3 LocalToWorldPosition(Vector3 localPos)
        {
            return position + orientation * localPos;
        }

        public Vector3 LocalToWorldVector(Vector3 localPos)
        {
            return orientation * localPos;
        }
    }
}