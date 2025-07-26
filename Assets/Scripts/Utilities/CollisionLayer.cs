using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Blizzard
{
    [Flags] public enum CollisionLayer
    {
        Default = 1 << 0,
        TransparentFX = 1 << 1,
        IgnoreRaycast = 1 << 2,
        Player = 1 << 3,
        Water = 1 << 4,
        UI = 1 << 5,
        Obstacle = 1 << 6,
        Enemy = 1 << 7
    }

    public static class CollisionAssistant
    {
        /// <summary>
        /// What can be hit by a tool held by the player
        /// </summary>
        public static CollisionLayer Hittable = CollisionLayer.Obstacle | CollisionLayer.Enemy;
        /// <summary>
        /// What will block enemy vision
        /// </summary>
        public static CollisionLayer Visible = CollisionLayer.Obstacle | CollisionLayer.Player;
    }
}


