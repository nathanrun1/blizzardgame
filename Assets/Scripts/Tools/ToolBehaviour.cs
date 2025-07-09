using Blizzard.Obstacles;
using UnityEngine;

namespace Blizzard.Player
{
    /// <summary>
    /// Base class for Tool Behaviour Control classes that implement tool behavior
    /// </summary>
    public abstract class ToolBehaviour : MonoBehaviour
    {
        [Header("Tool Config")]
        /// <summary>
        /// Base damage that this tool does to harvestables
        /// </summary>
        [SerializeField] protected int baseDamage;
        /// <summary>
        /// Tool type
        /// </summary>
        [SerializeField] protected ToolType toolType;


        /// <summary>
        /// Apply damage to (or "harvest") a harvestable
        /// </summary>
        protected virtual void Harvest(Harvestable harvestable, int damage = -1)
        {
            if ((harvestable.ToolType & (uint)toolType) == 0) return; // Tool type does not match

            int damageToApply = damage == -1 ? baseDamage : damage;
            harvestable.Damage(damageToApply);
        }
    }
}
