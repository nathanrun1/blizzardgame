using System.Collections.Generic;
using Blizzard.Constants;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.DataTypes;
using Blizzard.Utilities.Extensions;
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
        private readonly System.Random _rand;
        
        private readonly Vector2 _perlinOffset;
        private readonly AABBInt2D _spawnRange;
        private readonly float _cellSideLength;


        /// <summary>
        /// List of grid positions where resources should spawn
        /// </summary>
        public List<Vector2Int> ResourceSpawnPositions { get; private set; } = new();
        
        /// <param name="seed">Seed to use for resource generation</param>
        /// <param name="gridCellSideLength">Associated grid's square cell side length</param>
        /// <param name="spawnRange">Range of integer coordinates where resources can spawn</param>
        public ResourceLocationGenerator(int seed, AABBInt2D spawnRange, float gridCellSideLength = GameConstants.CellSideLength)
        {
            _rand = new System.Random(seed);
            _perlinOffset = new Vector2(
                (float)(_rand.NextDouble() * 2f - 1f) * ResourceConstants.PerlinInputRadius,
                (float)(_rand.NextDouble() * 2f - 1f) * ResourceConstants.PerlinInputRadius
            );
            _spawnRange = spawnRange;
            _cellSideLength = gridCellSideLength;
            
            ReconstructResourceSpawnPositions();
        }

        /// <summary>
        /// Reconstructs the list of resource spawn positions 
        /// </summary>
        public void ReconstructResourceSpawnPositions()
        {
            ResourceSpawnPositions.Clear();
            foreach (Vector2Int gridPosition in _spawnRange.GetCoordinates())
            {
                if (_rand.NextDouble() <= GetResourceDensity(gridPosition))
                    ResourceSpawnPositions.Add(gridPosition);
            }
        }

        /// <summary>
        /// Fetches the resource density at a given grid position, retrieved from a Perlin noise map
        /// </summary>
        private float GetResourceDensity(Vector2Int gridPosition)
        {
            return GetResourceDensity(GridAssistant.CellToWorldPosCenter(gridPosition, _cellSideLength));
        }
        
        /// <summary>
        /// Fetches the resource density at a given arbitrary position, retrieved from a Perlin Noise map
        /// </summary>
        private float GetResourceDensity(Vector2 position)
        {
            Vector2 offsetPosition = position + _perlinOffset;
            return Mathf.Pow(Mathf.PerlinNoise(offsetPosition.x, offsetPosition.y), ResourceConstants.ResourceNoiseExponent);
        }
    }
}