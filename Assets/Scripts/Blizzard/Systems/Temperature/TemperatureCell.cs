using System.Drawing;
using UnityEngine;

namespace Blizzard.Temperature
{
    public struct TemperatureCell
    {
        /// <summary>
        /// The current temperature of the cell
        /// </summary>
        public float temperature;

        /// <summary>
        /// How much this cell's temperature increases per second
        /// </summary>
        public float heat;

        /// <summary>
        /// How much this cell insulates heat (range from 0 -> 1)
        /// </summary>
        public float insulation;

        /// <summary>
        /// Whether this cell naturally regresses to ambient temperature
        /// 0 if false, 1 if true
        /// </summary>
        public int ambient;

        public static int GetSize()
        {
            return sizeof(float) * 4;
        }
    }
}