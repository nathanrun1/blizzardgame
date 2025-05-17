using UnityEngine;
using Blizzard.Obstacles;

[CreateAssetMenu(fileName = "ObstacleData", menuName = "ScriptableObjects/Obstacles/ObstacleData")]
public class ObstacleData : ScriptableObject
{
    /// <summary>
    /// Prefab of the obstacle's gameobject
    /// </summary>
    public Obstacle obstaclePrefab;
}
