using System;
using System.Collections.Generic;
using Blizzard.Constants;
using UnityEngine;
using Zenject;
using Blizzard.Grid;
using Blizzard.Obstacles;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class ObstacleGridServiceInstaller : MonoInstaller
    {
        [SerializeField] private Transform _obstaclesParent;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            // Create one grid per obstacle layer
            Dictionary<ObstacleLayer, ISparseWorldGrid<Obstacle>> grids = new();
            foreach (ObstacleLayer layer in Enum.GetValues(typeof(ObstacleLayer)))
                grids.Add(layer, new HashWorldGrid<Obstacle>(GameConstants.CellSideLength, GameConstants.CellSideLength));

            Container.Bind<ObstacleGridService>()
                .FromNew()
                .AsSingle()
                .WithArguments(grids, _obstaclesParent);

            BLog.Log("Installed Obstacle Grid Service");
        }
    }
}