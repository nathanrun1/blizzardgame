using UnityEngine;

namespace Blizzard.Temperature
{
    public static class TemperatureConstants
    {
        public static float NeutralTemperature = 0f;
        public static Vector2Int ActiveSubgridDimensions = new(32, 32);
        public static float DiffusionFactor = 0.9f;

        public static Vector2Int ComputeThreadGroupDimensions = new(8, 8);

        public static float DefaultHeatValue = 0f;
        public static float DefaultInsulationValue = 0f;
    }
}