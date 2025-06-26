using UnityEngine;

namespace Blizzard.Temperature
{
    public static class TemperatureConstants
    {
        public const float NeutralTemperature = 0f;
        public static readonly Vector2Int ActiveSubgridDimensions = new(32, 32);
        public const float DiffusionFactor = 0.9f;

        public static readonly Vector2Int ComputeThreadGroupDimensions = new(8, 8);

        public const float DefaultHeatValue = 0f;
        public const float DefaultInsulationValue = 0f;
    }
}