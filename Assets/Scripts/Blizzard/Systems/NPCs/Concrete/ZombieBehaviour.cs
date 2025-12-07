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
    public class ZombieBehaviour : NPCBehaviour
    {
        [System.Serializable]
        public class BehaviourConfig
        {
            public float walkSpeed;

            public float attackRange;
            public int attackDamage;
            public float attackDelay;

            /// <summary>
            /// How close player needs to be to attempt an attack
            /// </summary>
            public float playerAgroRange;

            /// <summary>
            /// Queries up to this many closest player-built structures to choose from for attack
            /// </summary>
            public int targetListSize;

            /// <summary>
            /// Max (Manhattan) distance of potential obstacle target when queried in the obstacle quadtree.
            /// </summary>
            public int maxTargetDistance;

            public float idleUpdateDelay;

            // Square values for range checking
            [HideInInspector] public float attackRangeSqr;
            [HideInInspector] public float playerAgroRangeSqr;

            public void CalculateSquareRanges()
            {
                attackRangeSqr = attackRange * attackRange;
                playerAgroRangeSqr = playerAgroRange * playerAgroRange;
            }
        }

        private class ZombieContext : IStateContext
        {
            public StateMachine StateMachine { get; set; }

            public ObstacleGridService obstacleGridService;
            public PlayerService playerService;
            public PathfindingService pathfindingService;

            public GameObject gameObject;
            public Rigidbody2D rigidbody;

            public Damageable targetObstacle;

            public BehaviourConfig config;
        }

        #region States

        // -- STATES --

        /// <summary>
        /// Base class for zombie states
        /// </summary>
        private abstract class ZombieState : IState
        {
            protected ZombieContext context;

            public virtual void Enter(IStateContext ctx)
            {
                context = ctx as ZombieContext;
            }

            public virtual void Exit()
            {
            }

            public abstract void Update(float deltaTime);

            /// <summary>
            /// Checks if player can be targeted (i.e. visible and in range)
            /// </summary>
            /// <returns>Whether player can be targeted</returns>
            protected bool TryGetPlayerTarget()
            {
                var plrDirection = context.playerService.PlayerPosition -
                                   (Vector2)context.gameObject.transform.position;
                if (plrDirection.sqrMagnitude > context.config.playerAgroRangeSqr)
                    return false; // Player is too far away

                var raycast = Physics2D.Raycast(context.gameObject.transform.position, plrDirection,
                    context.config.playerAgroRange, (int)CollisionAssistant.Visible);

                // BLog.Log(raycast.collider);
                if (raycast.collider && raycast.collider.gameObject.CompareTag("Player"))
                    return true; // Player is visible and in range!
                return false;
            }

            /// <summary>
            /// Attempts to pick the closest damageable obstacle as a target. If successful, assigns target obstacle
            /// in the state context.
            /// </summary>
            /// <returns>True if target found and assigned, false otherwise</returns>
            protected bool TryGetDamageableTarget()
            {
                var selfGridPos = context.obstacleGridService.GetMainGrid()
                    .WorldToCellPos(context.gameObject.transform.position);
                var targetOptions = context.obstacleGridService
                    .GetQuadtree(ObstacleFlags.PlayerBuilt | ObstacleFlags.Detectable)
                    .GetKNearestObstacles
                    (
                        selfGridPos,
                        context.config.targetListSize,
                        context.config.maxTargetDistance
                    );
                if (targetOptions.Count == 0)
                {
                    context.targetObstacle = null;
                    return false; // No targets available
                }

                // Choose the closest target
                context.targetObstacle = targetOptions[0] as Damageable;
                return true;
            }
        }

        private class IdleState : ZombieState
        {
            private float _clock = 0;

            public override void Update(float deltaTime)
            {
                _clock += deltaTime;
                if (_clock <= context.config.idleUpdateDelay) return;

                _clock -= context.config.idleUpdateDelay;

                // TEMP: disable player targeting
                if (TryGetPlayerTarget()) 
                {
                    context.StateMachine.ChangeState(new NavPlayerState());
                    return;
                }

                // No player target, navigate to obstacle.
                context.StateMachine.ChangeState(new NavObstacleState());
            }
        }

        private class NavPlayerState : ZombieState
        {
            private Vector2 _movementVector;
            private float _clock;

            public override void Update(float deltaTime)
            {
                _clock += deltaTime;
                if (_clock > context.config.idleUpdateDelay)
                {
                    _clock -= context.config.idleUpdateDelay;
                    if (!TryGetPlayerTarget())
                        // Can no longer target player, return to idle
                        context.StateMachine.ChangeState(new IdleState());
                }

                _movementVector = context.playerService.PlayerPosition -
                                  (Vector2)context.gameObject.transform.position;
                context.rigidbody.MovePosition(context.rigidbody.position + _movementVector.normalized *
                    (context.config.walkSpeed * Time.fixedDeltaTime));
                if (_movementVector.sqrMagnitude <= context.config.attackRangeSqr)
                    context.StateMachine.ChangeState(new AttackPlayerState());
            }
        }

        private class AttackPlayerState : ZombieState
        {
            private float _clock = 0;

            public override void Update(float deltaTime)
            {
                _clock += deltaTime;
                context.rigidbody.linearVelocity = Vector2.zero;
                if (_clock > context.config.attackDelay)
                {
                    _clock -= context.config.attackDelay;
                    AttackPlayer();
                }
            }

            private void AttackPlayer()
            {
                if ((context.playerService.PlayerPosition - (Vector2)context.gameObject.transform.position)
                    .sqrMagnitude > context.config.attackRangeSqr)
                    // Out of attack range, back to navigation
                    context.StateMachine.ChangeState(new NavPlayerState());

                context.playerService.DamagePlayer(context.config.attackDamage, DamageFlags.Enemy);
            }
        }

        private class NavObstacleState : ZombieState
        {
            private Vector2Int _curGridPos;
            private Vector2Int _nextGridPos;
            private float _clock = 0;

            public override void Update(float deltaTime)
            {
                _clock += deltaTime;
                if (_clock > context.config.idleUpdateDelay)
                {
                    // Reset clock with slight random offset to avoid synchronizing with other enemies.
                    _clock -= context.config.idleUpdateDelay + Random.Range(-0.5f, 0.5f);
                    
                    if (TryGetPlayerTarget())
                        context.StateMachine.ChangeState(new NavPlayerState()); // Prioritize player as target

                    if (_nextGridPos == _curGridPos)
                        // Try getting new target
                        GetNextTarget();
                }

                var curGridPosCheck = context.obstacleGridService.GetMainGrid()
                    .WorldToCellPos(context.gameObject.transform.position);
                if (curGridPosCheck != _curGridPos)
                {
                    _curGridPos = curGridPosCheck;
                    GetNextTarget();
                }

                if (_nextGridPos == _curGridPos)
                {
                    // Already at target position, ensure no movement.
                    context.rigidbody.linearVelocity = Vector2.zero;
                    context.rigidbody.angularVelocity = 0f;
                    return;
                }
                
                // Next position is not an obstacle, keep moving (if we need to)
                // Only navigate if not already at next destination
                Vector2 nextPos = context.obstacleGridService.GetMainGrid()
                    .CellToWorldPosCenter(_nextGridPos);
                
                Vector2 movementVector = nextPos - context.rigidbody.position;
                context.rigidbody.MovePosition(context.rigidbody.position
                                                    + context.config.walkSpeed * Time.fixedDeltaTime *
                                                    movementVector.normalized);
            }


            /// <summary>
            /// Retrieves next target position based on current grid position. 
            /// </summary>
            private void GetNextTarget()
            {
                if (!context.pathfindingService.TryGetNextTargetGridPosition(_curGridPos, out _nextGridPos,
                        out var targetObstacle))
                {
                    BLog.Log("No next target position!");
                    // Not in flow field, move directly towards closest player-built obstacle instead

                    if (!TryGetDamageableTarget())
                    {
                        // No damageable obstacles at all, just navigate to the player.
                        context.StateMachine.ChangeState(new NavPlayerState());
                        _nextGridPos = _curGridPos; // TEMP
                        return;
                    }

                    // Set next grid position to direct path
                    BLog.Log($"Moving instead toward {context.targetObstacle}");
                    Vector2 movementVector = (context.targetObstacle.transform.position
                                              - context.gameObject.transform.position).normalized;
                    Vector2Int movementVectorInt =
                        new(Mathf.RoundToInt(movementVector.x), Mathf.RoundToInt(movementVector.y));
                    _nextGridPos = _curGridPos + movementVectorInt;

                    // Check if target obstacle at next grid position
                    context.obstacleGridService.TryGetObstacleAt(_nextGridPos, out targetObstacle);
                }
                else
                {
                    BLog.Log($"Retrieved next position {_nextGridPos} from current position {_curGridPos}");
                }

                // Check if next position is obstacle
                if (!targetObstacle) return;
                
                // Next position is obstacle, obstacle is "next on the path", thus we attack it.
                context.targetObstacle = targetObstacle as Damageable;
                context.StateMachine.ChangeState(new AttackObstacleState());
            }
        }

        private class AttackObstacleState : ZombieState
        {
            private float _clock = 0;

            public override void Enter(IStateContext ctx)
            {
                base.Enter(ctx);
                Assert.IsTrue(context.targetObstacle as Damageable); // Sanity check, target must be damageable
            }

            public override void Update(float deltaTime)
            {
                _clock += deltaTime;
                context.rigidbody.linearVelocity = Vector2.zero;
                context.rigidbody.angularVelocity = 0f;

                if (_clock <= context.config.attackDelay) return;
                if (TryGetPlayerTarget())
                    context.StateMachine.ChangeState(new NavPlayerState()); // Prioritize player as target
                _clock -= context.config.attackDelay;
                AttackTarget();
            }

            private void AttackTarget()
            {
                Damageable target = context.targetObstacle;
                bool targetDestroyed = true;  // Default to true, thus considered destroyed if target is null
                BLog.Log($"Attacking target {target}");
                if (target && target.gameObject)
                {
                    target?.Damage(context.config.attackDamage, DamageFlags.Enemy,
                        context.gameObject.transform.position, out targetDestroyed);
                }
                if (targetDestroyed)
                    context.StateMachine.ChangeState(new IdleState()); // Target obstacle destroyed, return to idle
            }
        }

        #endregion

        [Header("Zombie Config")] 
        [SerializeField] private BehaviourConfig _behaviour;

        private StateMachine _stateMachine;

        [Inject] private ObstacleGridService _obstacleGridService;
        [Inject] private PlayerService _playerService;
        [Inject] private PathfindingService _pathfindingService;

        protected override void Awake()
        {
            _behaviour.CalculateSquareRanges();

            var stateContext = new ZombieContext
            {
                obstacleGridService = _obstacleGridService,
                playerService = _playerService,
                pathfindingService = _pathfindingService,
                gameObject = gameObject,
                rigidbody = GetComponent<Rigidbody2D>(),
                targetObstacle = null,

                config = _behaviour
            };
            _stateMachine = new StateMachine(stateContext);

            base.Awake();
        }

        protected override void OnEnable()
        {
            _stateMachine.ChangeState(new IdleState());
            base.OnEnable();
        }

        private void FixedUpdate()
        {
            _stateMachine.Update(Time.fixedDeltaTime);
        }
    }
}