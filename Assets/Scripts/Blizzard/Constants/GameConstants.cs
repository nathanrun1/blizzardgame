using NativeTrees;
using Unity.Mathematics;

namespace Blizzard.Constants
{
    public class GameConstants
    {
        /// <summary>
        /// Side length of grid cells
        /// </summary>
        public const float CellSideLength = 0.5f;
        /// <summary>
        /// Full bounds of map (assuming non-infinite map is chosen as design)
        /// </summary>
        public static readonly AABB2D MapBounds = new AABB2D(new float2(-500, -500), new float2(500, 500));
    }
}