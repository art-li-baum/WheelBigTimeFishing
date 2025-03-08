using UnityEngine;


namespace WanderMarch.Scripts.Math
{
    public static class MathUtil
    {
        public static Vector2 Vector2Decay(Vector2 a, Vector2 b, float decay, float dt)
        {
            return b + (a - b) * Mathf.Exp(-decay * dt);
        }

        public static float FloatDecay(float a, float b, float decay, float dt)
        {
            return b + (a - b) * Mathf.Exp(-decay * dt);
        }
        
        public static void SmoothStep(ref float t)
        {
            t = t * t * (3f - 2f * t);
        }

        public static void SmootherStep(ref float t)
        {
            t = t * t * t * (t * (6f * t - 15f) + 10f);
        }

        public static void EaseOut(ref float t)
        {
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
        }

        public static void EaseIn(ref float t)
        {
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
        }
    }
}