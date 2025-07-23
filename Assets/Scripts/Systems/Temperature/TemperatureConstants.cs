using UnityEngine;

namespace Blizzard.Temperature
{
    public static class TemperatureConstants
    {
        public const float StartingAmbientTemperature = 5f;
        public static readonly Vector2Int ActiveSubgridDimensions = new(32, 32);
        /// <summary>
        /// Factor by which sum of neighbor temperature difference is multiplied by before
        /// added to cell
        /// </summary>
        public const float DiffusionFactor = 0.3f;
        /// <summary>
        /// Factor by which difference to ambient temperature is multiplied
        /// by before added to cell
        /// </summary>
        public const float AmbientFactor = 0.0005f;

        public static readonly Vector2Int ComputeThreadGroupDimensions = new(8, 8);

        public const float DefaultHeatValue = 0f;
        public const float DefaultInsulationValue = 0f;
        public const int DefaultAmbientValue = 1;
    }
}