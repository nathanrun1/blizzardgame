using Blizzard.Grid;
using Blizzard.Obstacles;
using Blizzard.Pathfinding;
using Blizzard.Player;
using Blizzard.Utilities;
using Blizzard.Utilities.DataTypes;
using Blizzard.Utilities.Logging;
using UnityEngine;
using Zenject;
using Assert = UnityEngine.Assertions.Assert;
using Random = UnityEngine.Random;


namespace Blizzard.NPCs.Concrete
{
    public class RabbitBehaviour : NPCBehaviour
    {
        [System.Serializable]
        public class BehaviourConfig
        {
            public float walkSpeed;

            /// <summary>
            /// How close player needs to run away
            /// </summary>
            public float playerFleeRange;

            public float idleUpdateDelay;
            
            [HideInInspector] public float playerFleeRangeSqr;
            
            public void CalculateSquareRanges()
            {
                playerFleeRangeSqr = playerFleeRange * playerFleeRange;
            }
        }

        #region States

        // -- STATES --

        #endregion

        [Header("Zombie Config")] 
        [SerializeField] private BehaviourConfig _behaviour;

        private StateMachine _stateMachine;

        [Inject] private ObstacleGridService _obstacleGridService;
        [Inject] private PlayerService _playerService;
        [Inject] private EnemyPathfindingService _enemyPathfindingService;

        protected override void Awake()
        {
            _behaviour.CalculateSquareRanges();

            //_stateMachine = new StateMachine(stateContext);

            base.Awake();
        }

        protected override void OnEnable()
        {
            //_stateMachine.ChangeState(new IdleState());
            base.OnEnable();
        }

        private void FixedUpdate()
        {
            //_stateMachine.Update(Time.fixedDeltaTime);
        }
    }
}