using Blizzard.NPCs.Concrete.Wolf;
using Blizzard.NPCs.Core;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Utilities;
using Blizzard.Utilities.DataTypes;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Blizzard.NPCs.Concrete
{
    public class WolfBehaviour : NPCBehaviour
    {
        [Inject] private ObstacleGridService _obstacleGridService;
        [Inject] private PlayerService _playerService;
        [Inject] private NPCService _npcService;
        
        [Header("Behaviour Config")] 
        [SerializeField] private WolfConfig _config;

        private StateMachine _stateMachine;
        private float _whenLastAttack = float.MinValue;
        
        /// <summary>
        /// The last time any wolf has engaged in combat with the player (attacked or was attacked). Useful
        /// for pack behavior.
        /// </summary>
        public static float whenLastEngagedPlayer = float.MinValue;

        protected override void Awake()
        {
            WolfContext ctx = new WolfContext
            {
                wolfBehaviour = this,
                config = _config,
                
                obstacleGridService = _obstacleGridService,
                playerService = _playerService,
                npcService = _npcService,
                collisionRadius = 0.2f, // TODO: get this value from somewhere
                transform = GetComponent<Transform>(),
                rigidbody = GetComponent<Rigidbody2D>()
            };
            _stateMachine = new StateMachine(ctx);
            
            base.Awake();
        }

        protected override void OnEnable()
        {
            _stateMachine.ChangeState(new IdleState());
            base.OnEnable();
        }

        protected override void TakeDamage(int damage, out bool death, DamageFlags damageFlags)
        {
            if (!damageFlags.HasFlag(DamageFlags.Player))
            {
                death = false;
                return;
            }

            whenLastEngagedPlayer = Time.time;
            if (_stateMachine.CurrentState is not HostileState) 
                _stateMachine.ChangeState(new HostileState());
            
            base.TakeDamage(damage, out death, damageFlags);
        }

        private void FixedUpdate()
        {
            _stateMachine.Update(Time.fixedDeltaTime);
        }

        
        /// <summary>
        /// Attempts to attack the player
        /// </summary>
        public void AttackPlayer()
        {
            if (!(Time.time - _whenLastAttack >= _config.attackDelay)) return;
            _whenLastAttack = Time.time;
            _playerService.DamagePlayer(_config.attackDamage, DamageFlags.Enemy);
            whenLastEngagedPlayer = Time.time;
        }

        /// <summary>
        /// Forces this wolf to become hostile toward the player
        /// </summary>
        public void BecomeHostile()
        {
            
        }
    }
}