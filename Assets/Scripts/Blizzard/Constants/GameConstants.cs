using NativeTrees;
using Unity.Mathematics;

namespace Blizzard.Constants
{
    public class GameConstants
    {
        /// <summary>
        /// Full bounds of map (assuming non-infinite map is chosen)
        /// </summary>
        public static readonly AABB2D MapBounds = new AABB2D(new float2(-500, -500), new float2(500, 500));
    }
}