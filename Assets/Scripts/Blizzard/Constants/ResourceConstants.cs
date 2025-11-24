namespace Blizzard.Constants
{
    public class ResourceConstants
    {
        /// <summary>
        /// Seed to use for resource generation
        /// </summary>
        public const int ResourceGenerationSeed = 0;
        
        /// <summary>
        /// Whether to use the set seed for resource generation. If set to false, will instead use a random seed.
        /// </summary>
        public const bool SeededGeneration = false;
        
        /// <summary>
        /// The maximum absolute offset value used on Perlin noise input coordinates
        /// </summary>
        public const float PerlinInputOffsetRadius = 1000f;
        
        /// <summary>
        /// Maximum amount of points to attempt to place when placing the neighbor of an active point
        /// </summary>
        public const int MaxPoissonDiskRetries = 20;
    }
}