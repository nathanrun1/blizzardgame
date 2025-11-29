using Blizzard.NPCs;
using Blizzard.Utilities.DataTypes;
using Unity.Assertions;
using UnityEngine;
using Zenject;

namespace Blizzard.Obstacles.Concrete
{
    public class Trap : Structure, IInteractable
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _readySprite;
        [SerializeField] private Sprite _triggeredSprite;
        [Header("Config")]
        [SerializeField] private int _damage = 50;

        /// <summary>
        /// Whether the trap is currently ready to be triggered
        /// </summary>
        private bool _ready = true;
        
        public string PrimaryInteractText => "Repair";
        public bool PrimaryInteractReady => !_ready;

        public void OnPrimaryInteract()
        {
            Repair();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_ready) return;
            NPCBehaviour npc = other.GetComponent<NPCBehaviour>();
            Trigger(npc);
        }

        /// <summary>
        /// Triggers the trap, damaging the given enemy
        /// </summary>
        private void Trigger(NPCBehaviour npc)
        {
            npc.Strike(_damage, out _);
            _spriteRenderer.sprite = _triggeredSprite;
            SetFlags(ObstacleFlags | ObstacleFlags.Detectable); // Trap is detectable now, until repaired.
            _ready = false;
        }

        /// <summary>
        /// Repairs the trap, re-activating it to be triggered once more
        /// </summary>
        private void Repair()
        {
            Assert.IsFalse(_ready);
            _spriteRenderer.sprite = _readySprite;
            SetFlags(ObstacleFlags & ~ObstacleFlags.Detectable); // Trap is now undetectable and ready to be triggered
            _ready = true;
        }
    }
}

// "Meaning. Purpose."
