using System;
using UnityEngine;

namespace ChartAndGraph
{
    /// <summary>
    ///     advanced settings for charts (currently includes fraction digits)
    /// </summary>
    [Serializable]
    internal class ChartAdancedSettings
    {
        private static ChartAdancedSettings mInstance;

        private static string[] FractionDigits =
        {
            "{0:0}",
            "{0:0.#}",
            "{0:0.##}",
            "{0:0.###}",
            "{0:0.####}",
            "{0:0.#####}",
            "{0:0.######}",
            "{0:0.#######}"
        };

        [Range(0, 7)] public int ValueFractionDigits = 2;

        [Range(0, 7)] public int AxisFractionDigits = 2;

        public static ChartAdancedSettings Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new ChartAdancedSettings();
                return mInstance;
            }
        }

        private string InnerFormat(string format, double val)
        {
            try
            {
                return string.Format(format, val);
            }
            catch
            {
            }

            return " ";
        }

        private string getFormat(int value)
        {
            value = Mathf.Clamp(value, 0, 7);
            return FractionDigits[value];
        }

        public string FormatFractionDigits(int digits, double val, Func<double, int, string> format = null)
        {
            if (format == null)
                return InnerFormat(getFormat(digits), val);
            return format(val, digits);
        }
    }
}