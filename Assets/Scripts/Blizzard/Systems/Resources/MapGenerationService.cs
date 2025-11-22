using System;
using System.Collections.Generic;
using Blizzard.Constants;
using Blizzard.Obstacles;
using Blizzard.Utilities.DataTypes;
using UnityEngine;
using Zenject;

namespace Blizzard.Resources
{
    /// <summary>
    /// Manages procedural map generation
    /// </summary>
    public class MapGenerationService : IInitializable
    {
        private readonly ResourceLocationGenerator _resourceLocationGenerator = new(
            UnityEngine.Random.Range(int.MinValue, int.MaxValue),
            new AABBInt2D(GameConstants.MapBounds)
        );
        
        public void Initialize()
        {
            throw new System.NotImplementedException();
        }
        
        public void GenerateResources(List<ObstacleData> resourceTypes)
        {
            
        }
    }
}