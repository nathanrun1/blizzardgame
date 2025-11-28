using System.Collections.Generic;
using System.Linq;
using Blizzard.Constants;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.DataTypes;
using Blizzard.Utilities.Extensions;
using Blizzard.Utilities.Logging;
using Unity.Assertions;
using UnityEngine;

namespace Blizzard.Resources
{
    /// <summary>
    /// Constructs and defines a generated list of grid positions within a
    /// given range where a resource should spawn.
    /// </summary>
    public class ResourceLocationGenerator
    {
        public struct Context
        {
            /// <summary>
            /// Range of integer coordinates where resources can spawn
            /// </summary>
            public AABBInt2D spawnRange;
            /// <summary>
            /// Associated grid's square cell side length
            /// </summary>
            public float gridCellSideLength;
            public ResourceGenInfo genInfo;
        }
        
        private readonly System.Random _rand;
        
        private readonly Vector2 _perlinOffset;
        private readonly Context _context;

        /// <summary>
        /// List of grid positions where resources should spawn
        /// </summary>
        public List<Vector2Int> ResourceSpawnLocations { get; private set; } = new();

        /// <param name="seed">Seed to use for resource generation</param>
        /// <param name="context">Resource location generation context</param>
        public ResourceLocationGenerator(int seed, Context context)
        {
            _rand = new System.Random(seed);

            _context = context;
            _perlinOffset = new Vector2(
                (float)(_rand.NextDouble() * 2f - 1f) * ResourceConstants.PerlinInputOffsetRadius,
                (float)(_rand.NextDouble() * 2f - 1f) * ResourceConstants.PerlinInputOffsetRadius
            );

            ReconstructResourceSpawnPositions();
        }

        /// <summary>
        /// Reconstructs the list of resource spawn positions 
        /// </summary>
        public void ReconstructResourceSpawnPositions()
        {
            // -- Perlin noise -> Poisson disk sampling disk radius --
            HashSet<Vector2Int> spawnLocationsSet = new();
            Queue<Vector2Int> activePoints = new();
            activePoints.Enqueue(_context.spawnRange.GetRandomPoint(_rand));
            
            while (activePoints.TryDequeue(out Vector2Int cur))
            {   
                float minDistance = DensityToMinDistance(GetResourceDensity(cur));
                for (int i = 0; i < ResourceConstants.MaxPoissonDiskRetries; ++i)
                {
                    // Pick a random point within distance range of current point, check if any other
                    //   points are too close. If not, candidate is valid and we spawn it.
                    // This enforces a minimum distance between spawned points
                    Vector2Int candidate = RandomPointInAnnulus(minDistance, 2 * minDistance) + cur;
                    if (!_context.spawnRange.Contains(candidate)) continue;
            
                    float candidateMinDistance = DensityToMinDistance(GetResourceDensity(cur));
                    bool otherPointTooClose = GridAssistant.GetPointsInDistanceRange(candidate, 0, candidateMinDistance)
                        .Any(point => spawnLocationsSet.Contains(point));
            
                    if (otherPointTooClose) continue;
                    // Valid candidate! No other points within minimum distance.
                    spawnLocationsSet.Add(candidate);
                    activePoints.Enqueue(candidate);
                }
            }
            
            ResourceSpawnLocations = new List<Vector2Int>(spawnLocationsSet);
            ResourceSpawnLocations.RemoveAll(x => GetResourceDensity(x) < _context.genInfo.densityThreshold);
        }

        /// <summary>
        /// Generates a random integer coordinate point in an annulus centered at (0, 0)
        /// </summary>
        /// <param name="r1">Inner circle radius</param>
        /// <param name="r2">Outer circle radius</param>
        private Vector2Int RandomPointInAnnulus(float r1, float r2)
        {
            float r = Mathf.Lerp(r1, r2, Mathf.Sqrt((float)_rand.NextDouble()));
            float theta = Mathf.Lerp(0, 2 * Mathf.PI, (float)_rand.NextDouble());

            return new Vector2Int(
                Mathf.FloorToInt(r * Mathf.Cos(theta)), 
                Mathf.FloorToInt(r * Mathf.Sin(theta))
            );
        }

        /// <summary>
        /// Converts resource density to the minimum distance between resources within an area of that density.
        /// </summary>
        private static float DensityToMinDistance(float density)
        {
            return 1f / density;
        }
        
        /// <summary>
        /// Fetches the resource density at a given grid position, retrieved from a Perlin noise map
        /// </summary>
        private float GetResourceDensity(Vector2Int gridPosition)
        {
            return GetResourceDensity(GridAssistant.CellToWorldPosCenter(gridPosition, _context.gridCellSideLength));
        }
        
        /// <summary>
        /// Fetches the resource density at a given arbitrary position, retrieved from a Perlin Noise map
        /// </summary>
        private float GetResourceDensity(Vector2 position)
        {
            Vector2 offsetPosition = position * _context.genInfo.sparsity + _perlinOffset;
            return Mathf.Lerp(_context.genInfo.minDensity, _context.genInfo.maxDensity,
                Mathf.Pow(Mathf.PerlinNoise(offsetPosition.x, offsetPosition.y),
                    _context.genInfo.sharpness));
        }
    }
}