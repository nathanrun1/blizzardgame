namespace Blizzard.Constants
{
    /// <summary>
    /// Constant values used as configuration for the pathfinding system
    /// </summary>
    internal static class PathfindingConstants
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
        
        /// <summary>
        /// When brute force checking for valid directions, will split all possible directions into this many angles.
        /// </summary>
        public const int maxDirectionRotationChecks = 20;
    }
}