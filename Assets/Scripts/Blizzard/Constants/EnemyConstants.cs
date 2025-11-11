namespace Blizzard.Constants
{
    public static class EnemyConstants
    {
        /// <summary>
        /// Starting pool size for inactive enemy pool, per enemy.
        /// </summary>
        public const int StartInactivePoolSize = 5;
        
        /// <summary>
        /// Maximum invalid entries in query before query is ended when querying nearest enemies
        /// in EnemyQuadtree
        /// </summary>
        public const int QTMaxNearestInvalid = 5;

        /// <summary>
        /// Minimum enemy spawn distance from the player
        /// </summary>
        public const float MinSpawnDistance = 15;

        /// <summary>
        /// Maximum enemy spawn distance from the player
        /// </summary>
        public const float MaxSpawnDistance = 30;
        
        /// <summary>
        /// Maximum temperature that an enemy can spawn within
        /// </summary>
        public const int MaxSpawnTemperature = 5;

        /// <summary>
        /// Amount of enemy spawn range update ticks to run per tick
        /// </summary>
        public const int SpawnRangeUpdateIterationsPerTick = 20;
    }
}