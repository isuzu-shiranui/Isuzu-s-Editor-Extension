using System.Collections.Generic;
using UnityEngine;

namespace IsuzuEditorExtension.Layout
{
    internal sealed class TickHandler
    {
        private int m_BiggestTick = -1;
        private float m_MaxValue = 1f;
        private float m_MinValue;
        private float m_PixelRange = 1f;
        private int m_SmallestTick;
        private float[] m_TickModulos = new float[0];
        private float[] m_TickStrengths = new float[0];

        internal int TickLevels
        {
            get { return this.m_BiggestTick - this.m_SmallestTick + 1; }
        }

        internal int GetLevelWithMinSeparation(float pixelSeparation)
        {
            for (var i = 0; i < this.m_TickModulos.Length; i++)
                if (this.m_TickModulos[i] * this.m_PixelRange / (this.m_MaxValue - this.m_MinValue) >= pixelSeparation)
                    return i - this.m_SmallestTick;

            return -1;
        }


        internal float GetPeriodOfLevel(int level)
        {
            return this.m_TickModulos[Mathf.Clamp(this.m_SmallestTick + level, 0, this.m_TickModulos.Length - 1)];
        }


        internal float GetStrengthOfLevel(int level)
        {
            return this.m_TickStrengths[this.m_SmallestTick + level];
        }


        internal float[] GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels)
        {
            var num = Mathf.Clamp(this.m_SmallestTick + level, 0, this.m_TickModulos.Length - 1);
            var list = new List<float>();
            var num2 = Mathf.FloorToInt(this.m_MinValue / this.m_TickModulos[num]);
            var num3 = Mathf.CeilToInt(this.m_MaxValue / this.m_TickModulos[num]);
            for (var i = num2; i <= num3; i++)
                if (!excludeTicksFromHigherlevels || num >= this.m_BiggestTick ||
                    i % Mathf.RoundToInt(this.m_TickModulos[num + 1] / this.m_TickModulos[num]) != 0)
                    list.Add(i * this.m_TickModulos[num]);

            return list.ToArray();
        }


        internal void SetRanges(float minValue, float maxValue, float minPixel, float maxPixel)
        {
            this.m_MinValue = minValue;
            this.m_MaxValue = maxValue;
            this.m_PixelRange = maxPixel - minPixel;
        }


        internal void SetTickModulos(float[] tickModulos)
        {
            this.m_TickModulos = tickModulos;
        }


        internal void SetTickModulosForFrameRate(float frameRate)
        {
            if (frameRate != Mathf.Round(frameRate))
            {
                var tickModulos = new[]
                {
                    1f / frameRate,
                    5f / frameRate,
                    10f / frameRate,
                    50f / frameRate,
                    100f / frameRate,
                    500f / frameRate,
                    1000f / frameRate,
                    5000f / frameRate,
                    10000f / frameRate,
                    50000f / frameRate,
                    100000f / frameRate,
                    500000f / frameRate
                };
                this.SetTickModulos(tickModulos);
                return;
            }

            var list = new List<int>();
            var num = 1;
            while (num < frameRate && num != frameRate)
            {
                var num2 = Mathf.RoundToInt(frameRate / num);
                if (num2 % 60 == 0)
                {
                    num *= 2;
                    list.Add(num);
                }
                else if (num2 % 30 == 0)
                {
                    num *= 3;
                    list.Add(num);
                }
                else if (num2 % 20 == 0)
                {
                    num *= 2;
                    list.Add(num);
                }
                else if (num2 % 10 == 0)
                {
                    num *= 2;
                    list.Add(num);
                }
                else if (num2 % 5 == 0)
                {
                    num *= 5;
                    list.Add(num);
                }
                else if (num2 % 2 == 0)
                {
                    num *= 2;
                    list.Add(num);
                }
                else if (num2 % 3 == 0)
                {
                    num *= 3;
                    list.Add(num);
                }
                else
                {
                    num = Mathf.RoundToInt(frameRate);
                }
            }

            var array = new float[9 + list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var array2 = array;
                var num3 = i;
                const float num4 = 1f;
                var list2 = list;
                array2[num3] = num4 / list2[list2.Count - i - 1];
            }

            var array3 = array;
            array3[array3.Length - 1] = 3600f;
            var array4 = array;
            array4[array4.Length - 2] = 1800f;
            var array5 = array;
            array5[array5.Length - 3] = 600f;
            var array6 = array;
            array6[array6.Length - 4] = 300f;
            var array7 = array;
            array7[array7.Length - 5] = 60f;
            var array8 = array;
            array8[array8.Length - 6] = 30f;
            var array9 = array;
            array9[array9.Length - 7] = 10f;
            var array10 = array;
            array10[array10.Length - 8] = 5f;
            var array11 = array;
            array11[array11.Length - 9] = 1f;
            this.SetTickModulos(array);
        }


        internal void SetTickStrengths(float tickMinSpacing, float tickMaxSpacing, bool sqrt)
        {
            this.m_TickStrengths = new float[this.m_TickModulos.Length];
            this.m_SmallestTick = 0;
            this.m_BiggestTick = this.m_TickModulos.Length - 1;
            for (var i = this.m_TickModulos.Length - 1; i >= 0; i--)
            {
                var num = this.m_TickModulos[i] * this.m_PixelRange / (this.m_MaxValue - this.m_MinValue);
                this.m_TickStrengths[i] = (num - tickMinSpacing) / (tickMaxSpacing - tickMinSpacing);
                if (this.m_TickStrengths[i] >= 1f) this.m_BiggestTick = i;

                if (!(num <= tickMinSpacing)) continue;
                this.m_SmallestTick = i;
                break;
            }

            for (var j = this.m_SmallestTick; j <= this.m_BiggestTick; j++)
            {
                this.m_TickStrengths[j] = Mathf.Clamp01(this.m_TickStrengths[j]);
                if (sqrt) this.m_TickStrengths[j] = Mathf.Sqrt(this.m_TickStrengths[j]);
            }
        }
    }
}