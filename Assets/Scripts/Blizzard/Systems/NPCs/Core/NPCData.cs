using UnityEngine;
using UnityEngine.Serialization;

namespace Blizzard.NPCs.Core
{
    /// <summary>
    /// An NPC prefab
    /// </summary>
    [CreateAssetMenu(fileName = "NPCData", menuName = "ScriptableObjects/NPCs/NPCData")]
    public class NPCData : ScriptableObject
    {
        /// <summary>
        /// Unique ID of enemy type
        /// </summary>
        public int ID;
        /// <summary>
        /// Enemy prefab
        /// </summary>
        [FormerlySerializedAs("enemyPrefab")] 
        public NPCBehaviour npcPrefab;
    }
}