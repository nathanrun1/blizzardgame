using Blizzard.Obstacles;
using ModestTree;
using UnityEngine;

namespace Blizzard
{
    public abstract class Tool
    {
        [Header("Tool Config")]
        /// <summary>
        /// Base damage that this tool does to harvestables
        /// </summary>
        [SerializeField] int baseDamage;
        /// <summary>
        /// Tool type
        /// </summary>
        [SerializeField] ToolType toolType;


        protected virtual void Harvest(Harvestable harvestable, int damage = -1)
        {
            if ((harvestable.ToolType & (uint)toolType) == 0) return; // Tool type does not match

            int damageToApply = damage == -1 ? baseDamage : damage;
            harvestable.Damage(damageToApply);
        }
    }
}
