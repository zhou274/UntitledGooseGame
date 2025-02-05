using UnityEngine;

namespace Watermelon
{
    public class Ease
    {
        /// <summary>
        /// All allowed ease type. See examples <see href="http://easings.net">here</see>.
        /// </summary>
        public enum Type
        {
            Linear,
            QuadIn,
            QuadOut,
            QuadOutIn,
            CubicIn,
            CubicOut,
            CubicInOut,
            QuartIn,
            QuartOut,
            QuartInOut,
            QuintIn,
            QuintOut,
            QuintInOut,
            SineIn,
            SineOut,
            SineInOut,
            CircIn,
            CircOut,
            CircInOut,
            ExpoIn,
            ExpoOut,
            ExpoInOut,
            ElasticIn,
            ElasticOut,
            ElastinInOut,
            BackIn,
            BackOut,
            BackInOut,
            BounceIn,
            BounceOut,
            BounceInOut
        }

        private const float PI = Mathf.PI;
        private const float HALF_PI = Mathf.PI / 2.0f;

        public static float Interpolate(float p, Type ease)
        {
            switch (ease)
            {
                case Type.Linear: return p;
                case Type.QuadIn: return p * p;
                case Type.QuadOut: return -(p * (p - 2));
                case Type.QuadOutIn:
                    if (p < 0.5f)
                    {
                        return 2 * p * p;
                    }
                    else
                    {
                        return (-2 * p * p) + (4 * p) - 1;
                    };
                case Type.CubicIn: return p * p * p;
                case Type.CubicOut:
                    float f1 = (p - 1);
                    return f1 * f1 * f1 + 1;
                case Type.CubicInOut:
                    if (p < 0.5f)
                    {
                        return 4 * p * p * p;
                    }
                    else
                    {
                        float f2 = ((2 * p) - 2);
                        return 0.5f * f2 * f2 * f2 + 1;
                    }
                case Type.QuartIn: return p * p * p * p;
                case Type.QuartOut:
                    float f3 = (p - 1);
                    return f3 * f3 * f3 * (1 - p) + 1;
                case Type.QuartInOut:
                    if (p < 0.5f)
                    {
                        return 8 * p * p * p * p;
                    }
                    else
                    {
                        float f4 = (p - 1);
                        return -8 * f4 * f4 * f4 * f4 + 1;
                    }
                case Type.QuintIn: return p * p * p * p * p;
                case Type.QuintOut:
                    float f5 = (p - 1);
                    return f5 * f5 * f5 * f5 * f5 + 1;
                case Type.QuintInOut:
                    if (p < 0.5f)
                    {
                        return 16 * p * p * p * p * p;
                    }
                    else
                    {
                        float f6 = ((2 * p) - 2);
                        return 0.5f * f6 * f6 * f6 * f6 * f6 + 1;
                    }
                case Type.SineIn: return Mathf.Sin((p - 1) * HALF_PI) + 1;
                case Type.SineOut: return Mathf.Sin(p * HALF_PI);
                case Type.SineInOut: return 0.5f * (1 - Mathf.Cos(p * PI));
                case Type.CircIn: return 1 - Mathf.Sqrt(1 - (p * p));
                case Type.CircOut: return Mathf.Sqrt((2 - p) * p);
                case Type.CircInOut:
                    if (p < 0.5f)
                    {
                        return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (p * p)));
                    }
                    else
                    {
                        return 0.5f * (Mathf.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
                    }
                case Type.ExpoIn: return (p == 0.0f) ? p : Mathf.Pow(2, 10 * (p - 1));
                case Type.ExpoOut: return (p == 1.0f) ? p : 1 - Mathf.Pow(2, -10 * p);
                case Type.ExpoInOut:
                    if (p == 0.0 || p == 1.0) return p;

                    if (p < 0.5f)
                    {
                        return 0.5f * Mathf.Pow(2, (20 * p) - 10);
                    }
                    else
                    {
                        return -0.5f * Mathf.Pow(2, (-20 * p) + 10) + 1;
                    }
                case Type.ElasticIn: return Mathf.Sin(13 * HALF_PI * p) * Mathf.Pow(2, 10 * (p - 1));
                case Type.ElasticOut: return Mathf.Sin(-13 * HALF_PI * (p + 1)) * Mathf.Pow(2, -10 * p) + 1;
                case Type.ElastinInOut:
                    if (p < 0.5f)
                    {
                        return 0.5f * Mathf.Sin(13 * HALF_PI * (2 * p)) * Mathf.Pow(2, 10 * ((2 * p) - 1));
                    }
                    else
                    {
                        return 0.5f * (Mathf.Sin(-13 * HALF_PI * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
                    }
                case Type.BackIn: return p * p * p - p * Mathf.Sin(p * PI);
                case Type.BackOut:
                    float f7 = (1 - p);
                    return 1 - (f7 * f7 * f7 - f7 * Mathf.Sin(f7 * PI));
                case Type.BackInOut:
                    if (p < 0.5f)
                    {
                        float f = 2 * p;
                        return 0.5f * (f * f * f - f * Mathf.Sin(f * PI));
                    }
                    else
                    {
                        float f = (1 - (2 * p - 1));
                        return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * PI))) + 0.5f;
                    }
                case Type.BounceIn: return BounceEaseIn(p);
                case Type.BounceOut: return BounceEaseOut(p);
                case Type.BounceInOut: return BounceEaseInOut(p);
                default: return p;
            }
        }

        public static EaseFunction GetFunction(Type ease)
        {
            switch (ease)
            {
                case Type.Linear: return (float p) => { return p; };
                case Type.QuadIn: return (float p) => { return p * p; };
                case Type.QuadOut: return (float p) => { return -(p * (p - 2)); };
                case Type.QuadOutIn:
                    return (float p) =>
                    {
                        if (p < 0.5f)
                        {
                            return 2 * p * p;
                        }
                        else
                        {
                            return (-2 * p * p) + (4 * p) - 1;
                        };
                    };
                case Type.CubicIn: return (float p) => { return p * p * p; };
                case Type.CubicOut:
                    return (float p) =>
                    {
                        float f1 = (p - 1);
                        return f1 * f1 * f1 + 1;
                    };
                case Type.CubicInOut:
                    return (float p) =>
                    {
                        if (p < 0.5f)
                        {
                            return 4 * p * p * p;
                        }
                        else
                        {
                            float f2 = ((2 * p) - 2);
                            return 0.5f * f2 * f2 * f2 + 1;
                        }
                    };
                case Type.QuartIn: return (float p) => { return p * p * p * p; };
                case Type.QuartOut:
                    return (float p) =>
                    {
                        float f3 = (p - 1);
                        return f3 * f3 * f3 * (1 - p) + 1;
                    };
                case Type.QuartInOut:
                    return (float p) =>
                    {
                        if (p < 0.5f)
                        {
                            return 8 * p * p * p * p;
                        }
                        else
                        {
                            float f4 = (p - 1);
                            return -8 * f4 * f4 * f4 * f4 + 1;
                        }
                    };
                case Type.QuintIn: return (float p) => { return p * p * p * p * p; };
                case Type.QuintOut:
                    return (float p) =>
                    {
                        float f = (p - 1);
                        return f * f * f * f * f + 1;
                    };
                case Type.QuintInOut:
                    return (float p) =>
                    {
                        if (p < 0.5f)
                        {
                            return 16 * p * p * p * p * p;
                        }
                        else
                        {
                            float f = ((2 * p) - 2);
                            return 0.5f * f * f * f * f * f + 1;
                        }
                    };
                case Type.SineIn: return (float p) => { return Mathf.Sin((p - 1) * HALF_PI) + 1; };
                case Type.SineOut: return (float p) => { return Mathf.Sin(p * HALF_PI); };
                case Type.SineInOut: return (float p) => { return 0.5f * (1 - Mathf.Cos(p * PI)); };
                case Type.CircIn: return (float p) => { return 1 - Mathf.Sqrt(1 - (p * p)); };
                case Type.CircOut: return (float p) => { return Mathf.Sqrt((2 - p) * p); };
                case Type.CircInOut:
                    return (float p) =>
                    {
                        if (p < 0.5f)
                        {
                            return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (p * p)));
                        }
                        else
                        {
                            return 0.5f * (Mathf.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
                        }
                    };
                case Type.ExpoIn: return (float p) => { return (p == 0.0f) ? p : Mathf.Pow(2, 10 * (p - 1)); };
                case Type.ExpoOut: return (float p) => { return (p == 1.0f) ? p : 1 - Mathf.Pow(2, -10 * p); };
                case Type.ExpoInOut:
                    return (float p) =>
                    {
                        if (p == 0.0 || p == 1.0) return p;

                        if (p < 0.5f)
                        {
                            return 0.5f * Mathf.Pow(2, (20 * p) - 10);
                        }
                        else
                        {
                            return -0.5f * Mathf.Pow(2, (-20 * p) + 10) + 1;
                        }
                    };
                case Type.ElasticIn: return (float p) => { return Mathf.Sin(13 * HALF_PI * p) * Mathf.Pow(2, 10 * (p - 1)); };
                case Type.ElasticOut: return (float p) => { return Mathf.Sin(-13 * HALF_PI * (p + 1)) * Mathf.Pow(2, -10 * p) + 1; };
                case Type.ElastinInOut:
                    return (float p) =>
                    {
                        if (p < 0.5f)
                        {
                            return 0.5f * Mathf.Sin(13 * HALF_PI * (2 * p)) * Mathf.Pow(2, 10 * ((2 * p) - 1));
                        }
                        else
                        {
                            return 0.5f * (Mathf.Sin(-13 * HALF_PI * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
                        }
                    };
                case Type.BackIn: return (float p) => { return p * p * p - p * Mathf.Sin(p * PI); };
                case Type.BackOut:
                    return (float p) =>
                    {
                        float f = (1 - p);
                        return 1 - (f * f * f - f * Mathf.Sin(f * PI));
                    };
                case Type.BackInOut:
                    return (float p) =>
                    {
                        if (p < 0.5f)
                        {
                            float f = 2 * p;
                            return 0.5f * (f * f * f - f * Mathf.Sin(f * PI));
                        }
                        else
                        {
                            float f = (1 - (2 * p - 1));
                            return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * PI))) + 0.5f;
                        }
                    };
                case Type.BounceIn: return (float p) => { return BounceEaseIn(p); };
                case Type.BounceOut: return (float p) => { return BounceEaseOut(p); };
                case Type.BounceInOut: return (float p) => { return BounceEaseInOut(p); };
                default: return (float p) => { return p; };
            }
        }

        #region Bounce
        private static float BounceEaseIn(float p)
        {
            return 1 - BounceEaseOut(1 - p);
        }

        private static float BounceEaseOut(float p)
        {
            if (p < 4 / 11.0f)
            {
                return (121 * p * p) / 16.0f;
            }
            else if (p < 8 / 11.0f)
            {
                return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
            }
            else if (p < 9 / 10.0f)
            {
                return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
            }
            else
            {
                return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
            }
        }

        private static float BounceEaseInOut(float p)
        {
            if (p < 0.5f)
            {
                return 0.5f * BounceEaseIn(p * 2);
            }
            else
            {
                return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
            }
        }
        #endregion

        public delegate float EaseFunction(float p);
    }
}


// -----------------
// Tween v 1.3.1
// -----------------