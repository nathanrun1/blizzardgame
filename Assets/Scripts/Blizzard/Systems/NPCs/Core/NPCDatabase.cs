using UnityEngine;
using UnityEngine.Serialization;

namespace Blizzard.NPCs.Core
{
    /// <summary>
    /// Database of enemy prefabs instantiatable through EnemyService
    /// </summary>
    [CreateAssetMenu(fileName = "NPCDatabase", menuName = "ScriptableObjects/NPCs/NPCDatabase")]
    public class NPCDatabase : ScriptableObject
    {
        [FormerlySerializedAs("enemyDatas")] public NPCData[] npcDatas;
    }
}