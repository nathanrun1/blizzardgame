using System;
using Blizzard.Obstacles;
using UnityEngine;
using Blizzard.Obstacles.Harvestables;

namespace Blizzard.Player.Tools
{
    /// <summary>
    /// Type of tool, determines which tools can harvest which harvestables.
    /// Interpreted as a bit field.
    /// </summary>
    [Flags]
    public enum ToolType
    {
        Axe = 1 << 0,
        Pickaxe = 1 << 1
    }

    /// <summary>
    /// Base class for Tool Behaviour Control classes that implement tool behavior
    /// </summary>
    public abstract class ToolBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Base damage that this tool does to harvestables
        /// </summary>
        [Header("Tool Config")] [SerializeField]
        protected int _baseDamage;

        /// <summary>
        /// Tool type
        /// </summary>
        [SerializeField] protected ToolType _toolType;


        /// <summary>
        /// Apply damage to a harvestable
        /// </summary>
        protected void ApplyHit(IHittable hittable, int damage, out bool death)
        {
            hittable.Hit(damage, _toolType, out death);
        }
    }
}