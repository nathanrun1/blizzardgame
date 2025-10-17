using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Blizzard.Grid;
using Blizzard.Obstacles;

namespace Blizzard.Installers
{
    public class ObstacleGridServiceInstaller : MonoInstaller
    {
        const float CELL_SIDE_LENGTH = 0.5f;

        [SerializeField] private Transform _obstaclesParent;

        public override void InstallBindings()
        {
            // Create one grid per obstacle layer
            Dictionary<ObstacleLayer, ISparseWorldGrid<Obstacle>> grids = new();
            foreach (ObstacleLayer layer in Enum.GetValues(typeof(ObstacleLayer)))
            {
                grids.Add(layer, new HashWorldGrid<Obstacle>(CELL_SIDE_LENGTH, CELL_SIDE_LENGTH));
            }

            Container.Bind<ObstacleGridService>()
                .FromNew()
                .AsSingle()
                .WithArguments(grids, _obstaclesParent);

            Debug.Log("Installed Obstacle Grid Service");
        }
    }
}
