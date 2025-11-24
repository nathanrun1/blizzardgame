using System;
using Blizzard.Obstacles;

namespace Blizzard.Utilities.DataTypes
{
    /// <summary>
    /// Description of a resource that is procedurally generated
    /// </summary>
    [Serializable]
    public struct ResourceGenInfo
    {
        /// <summary>
        /// Obstacle of resource to generate
        /// </summary>
        public ObstacleData obstacleData;
        /// <summary>
        /// Minimum resource density at any location
        /// </summary>
        public float minDensity;
        /// <summary>
        /// Maximum resource density at any location
        /// </summary>
        public float maxDensity;
        /// <summary>
        /// Minimum density such that resources are allowed to spawn
        /// </summary>
        public float densityThreshold;
        /// <summary>
        /// How "sparse" density changes are. Scales input to perlin noise map by this factor.
        /// </summary>
        public float sparsity;
        /// <summary>
        /// How "sharp" density changes are. Density value set to the power of this quantity.
        /// </summary>
        public float sharpness;
    }
}