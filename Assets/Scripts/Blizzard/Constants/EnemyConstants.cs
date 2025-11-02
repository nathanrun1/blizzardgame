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
    }
}