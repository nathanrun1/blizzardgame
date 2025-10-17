namespace Blizzard.Pathfinding
{
    /// <summary>
    /// Constant values used as configuration for the pathfinding system
    /// </summary>
    static class PathfindingConstants
    {
        /// <summary>
        /// Side length of flow field chunks
        /// </summary>
        public const int ffChunkSideLength = 32;
        /// <summary>
        /// Amount of padding (# of cells) given to the overall flow-field
        /// covering player-built obstacles
        /// </summary>
        public const int ffPadding = 25;
    }
}