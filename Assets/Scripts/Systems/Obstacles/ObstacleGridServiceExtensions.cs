using System.Collections.Generic;
using System.Linq;

namespace Blizzard.Obstacles
{
    public static class ObstacleGridServiceExtensions
    {
        public static IEnumerable<Obstacle> GetAllObstaclesWithFlags(this ObstacleGridService obstacleGridService, ObstacleFlags obstacleFlags)
        {
            return obstacleGridService.Grid.Values.Where(o => (o.ObstacleFlags & obstacleFlags) == obstacleFlags);
        }
    }
}