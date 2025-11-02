using UnityEngine;

namespace Blizzard.Enemies.Core
{
    /// <summary>
    /// Database of enemy prefabs instantiatable through EnemyService
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "ScriptableObjects/Enemies/EnemyDatabase")]
    public class EnemyDatabase : ScriptableObject
    {
        public EnemyData[] enemyDatas;
    }
}