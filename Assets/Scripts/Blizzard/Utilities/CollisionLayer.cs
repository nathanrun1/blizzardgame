using System;

namespace Blizzard.Utilities
{
    [Flags]
    public enum CollisionLayer
    {
        Default = 1 << 0,
        TransparentFX = 1 << 1,
        IgnoreRaycast = 1 << 2,
        Player = 1 << 3,
        Water = 1 << 4,
        UI = 1 << 5,
        Obstacle = 1 << 6,
        Enemy = 1 << 7,
        NoCollisionObstacle = 1 << 8,
        Animal = 1 << 9
    }

    public static class CollisionAssistant
    {
        /// <summary>
        /// What can be hit by a tool held by the player
        /// </summary>
        public const CollisionLayer Hittable = CollisionLayer.Obstacle | CollisionLayer.Enemy | CollisionLayer.Animal;
        
        /// <summary>
        /// What can be struck by a player-sided weapon
        /// </summary>
        public const CollisionLayer Strikeable = CollisionLayer.Enemy | CollisionLayer.Animal;

        /// <summary>
        /// What will block enemy vision
        /// </summary>
        public const CollisionLayer Visible = CollisionLayer.Obstacle | CollisionLayer.Player;

        /// <summary>
        /// What is considered an obstacle in the environment
        /// </summary>
        public const CollisionLayer AnyObstacle = CollisionLayer.Obstacle | CollisionLayer.NoCollisionObstacle;
    }
}