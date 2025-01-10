using System;
using UnityEngine;

// 【利用方法】
// 1. float.Ease(float start, float from, float easeTime, EaseType type)              // 拡張メソッド
//
// 2. Easing.Ease(float start, float from, float easeTime, EaseType type, float time) // 静的メソッド
//
// 3. var easing = new Easing(float start, float from, float easeTime, EaseType type) // 構造体をつくって
//    easing.Ease(float time)                                                         // 使う

namespace Novel
{
    static class ValueEaseExtension
    {
        public static float Ease(this int self, float start, float from, float easeTime, EaseType type)
        {
            return Easing.Ease(start, from, easeTime, type, self);
        }
        public static float Ease(this float self, float start, float from, float easeTime, EaseType type)
        {
            return Easing.Ease(start, from, easeTime, type, self);
        }
    }

    public readonly struct Easing
    {
        readonly float start;
        readonly float inversedEaseTime;
        readonly float delta;
        readonly EaseType type;
        public EaseType Type => type;

        public Easing(float start, float from, float easeTime, EaseType type)
        {
            this.type = easeTime == 0 ? EaseType.INTERNAL_Zero : type;
            this.start = start;
            this.inversedEaseTime = 1f / (easeTime == 0 ? 0.001f : easeTime);
            this.delta = from - start;
        }

        public static float Ease(float start, float from, float easeTime, EaseType type, float time)
        {
            float t = time / easeTime;
            if (easeTime == 0)
            {
                type = EaseType.INTERNAL_Zero;
            }
            float v = Operate(type, t);
            return start + (from - start) * v;
        }

        public float Ease(float time)
        {
            float t = time * inversedEaseTime;
            float v = Operate(type, t);
            return start + delta * v;
        }

        static float Operate(EaseType type, float t)
        {
            return type switch
            {
                EaseType.Linear => t,

                EaseType.InQuad => Pow(t, 2),
                EaseType.InCubic => Pow(t, 3),
                EaseType.InQuart => Pow(t, 4),
                EaseType.InQuint => Pow(t, 5),

                EaseType.OutQuad => 1 - Pow(1 - t, 2),
                EaseType.OutCubic => 1 - Pow(1 - t, 3),
                EaseType.OutQuart => 1 - Pow(1 - t, 4),
                EaseType.OutQuint => 1 - Pow(1 - t, 5),

                EaseType.InOutQuad =>
                    t < 0.5f
                        ? 2 * Pow(t, 2)
                        : 1 - Pow(-2 * t + 2, 2) / 2f,
                EaseType.InOutCubic =>
                    t < 0.5f
                        ? 4 * Pow(t, 3)
                        : 1 - Pow(-2 * t + 2, 3) / 2f,
                EaseType.InOutQuart =>
                    t < 0.5f
                        ? 8 * Pow(t, 4)
                        : 1 - Pow(-2 * t + 2, 4) / 2f,
                EaseType.InOutQuint =>
                    t < 0.5f
                        ? 16 * Pow(t, 5)
                        : 1 - Pow(-2 * t + 2, 5) / 2f,

                EaseType.InSine => 1 - Mathf.Cos(t * Mathf.PI / 2f),
                EaseType.OutSine => Mathf.Sin(t * Mathf.PI / 2f),
                EaseType.InOutSine => -(Mathf.Cos(t * Mathf.PI) - 1) / 2f,

                EaseType.InExpo => t == 0 ? 0
                    : Mathf.Pow(2, 10 * t - 10),
                EaseType.OutExpo => t == 1 ? 1
                    : 1 - Mathf.Pow(2, -10 * t),
                EaseType.InOutExpo => t == 0 ? 0 : t == 1 ? 1
                    : t < 0.5f
                        ? Mathf.Pow(2, 20 * t - 10) / 2f
                        : 2 - Mathf.Pow(2, -20 * t + 10) / 2f,

                EaseType.InCirc => 1 - Mathf.Sqrt(1 - Pow(t, 2)),
                EaseType.OutCirc => Mathf.Sqrt(1 - Pow(t - 1, 2)),
                EaseType.InOutCirc =>
                    t < 0.5f
                        ? (1 - Mathf.Sqrt(1 - 4 * t)) / 2f
                        : (Mathf.Sqrt(1 - Pow(-2 * t + 2, 2)) + 1) / 2f,

                EaseType.InBack => (GetOption(type) + 1) * Pow(t, 3) - GetOption(type) * Pow(t, 2),
                EaseType.OutBack => 1 + (GetOption(type) + 1) * Pow(t - 1, 3) + GetOption(type) * Pow(t - 1, 2),
                EaseType.InOutBack =>
                    t < 0.5f
                        ? Pow(2 * t, 2) * ((GetOption(type) + 1) * 2 * t - GetOption(type)) / 2f
                        : Pow(2 * t - 2, 2) * ((GetOption(type) + 1) * (t * 2 - 2) + GetOption(type)) + 2 / 2f,

                EaseType.InElastic => t == 0 ? 0 : t == 1 ? 1
                    : -Mathf.Pow(2, 10f * t - 10) * Mathf.Sin((t * 10f - 10.75f) * GetOption(type)),
                EaseType.OutElastic => t == 0 ? 0 : t == 1 ? 1
                    : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10f - 0.75f) * GetOption(type)) + 1,
                EaseType.InOutElastic => t == 0 ? 0 : t == 1 ? 1
                    : t < 0.5f
                        ? -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * GetOption(type))) / 2f
                        : Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * GetOption(type)) / 2 + 1,

                EaseType.InBounce => 1 - EaseOutBounce(1 - t),
                EaseType.OutBounce => EaseOutBounce(t),
                EaseType.InOutBounce =>
                    t < 0.5f
                        ? (1 - EaseOutBounce(1 - 2 * t)) / 2f
                        : (1 + EaseOutBounce(2 * t - 1)) / 2f,

                EaseType.INTERNAL_Zero => 1f,

                _ => throw new ArgumentOutOfRangeException()
            };


            static float Pow(float value, int p)
            {
                float result = 1;
                for (int i = 0; i < p; i++)
                {
                    result *= value;
                }
                return result;
            }

            static float GetOption(EaseType type) => type switch
            {
                EaseType.InBack or EaseType.OutBack => 1.70158f,
                EaseType.InOutBack => 1.70158f * 1.525f,
                EaseType.InElastic or EaseType.OutElastic => 2.0944f, //2 * Mathf.PI / 3f,
                EaseType.InOutElastic => 1.396f, // 2 * Mathf.PI / 4.5f,
                _ => throw new ArgumentOutOfRangeException()
            };

            static float EaseOutBounce(float t)
            {
                float n = 7.5625f;
                float d = 2.75f;
                if (t < 1 / d)
                {
                    return n * t * t;
                }
                else if (t < 2 / d)
                {
                    return n * (t -= 1.5f / d) * t + 0.75f;
                }
                else if (t < 2.5f / d)
                {
                    return n * (t -= 2.25f / d) * t + 0.9375f;
                }
                else
                {
                    return n * (t -= 2.625f / d) * t + 0.984375f;
                }
            }
        }
    }

    public enum EaseType
    {
        None,
        Linear,

        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InOutCubic,
        InQuart,
        OutQuart,
        InOutQuart,
        InQuint,
        OutQuint,
        InOutQuint,

        InSine,
        OutSine,
        InOutSine,
        InExpo,
        OutExpo,
        InOutExpo,
        InCirc,
        OutCirc,
        InOutCirc,

        InBack,
        OutBack,
        InOutBack,
        InElastic,
        OutElastic,
        InOutElastic,
        InBounce,
        OutBounce,
        InOutBounce,

        INTERNAL_Zero,
    }
}