using UnityEngine;

namespace Blizzard.Enemies.Core
{
    /// <summary>
    /// A UI prefab that can be instantiated through UIService
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/Enemies/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        /// <summary>
        /// Unique ID of enemy type
        /// </summary>
        public int id;
        /// <summary>
        /// Enemy prefab
        /// </summary>
        public GameObject enemyPrefab;
    }
}