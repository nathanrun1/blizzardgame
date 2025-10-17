using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;
using Blizzard.Utilities;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Grid;
using Blizzard.Pathfinding;


namespace Blizzard.Enemies
{
    public class ZombieBehaviour : MonoBehaviour
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
            public StateMachine stateMachine { get; set; }

            public ObstacleGridService obstacleGridService;
            public PlayerService playerService;
            public PathfindingService pathfindingService;

            public GameObject gameObject;
            public Rigidbody2D rigidBody;

            public Damageable targetObstacle;

            public BehaviourConfig behaviourConfig;
        }

        #region States

        // -- STATES --

        /// <summary>
        /// Base class for zombie states
        /// </summary>
        private abstract class ZombieState : IState
        {
            protected ZombieContext stateContext;

            public virtual void Enter(IStateContext ctx)
            {
                this.stateContext = ctx as ZombieContext;
            }

            public virtual void Exit()
            {
            }

            public abstract void Update();

            /// <summary>
            /// Checks if player can be targeted (i.e. visible and in range)
            /// </summary>
            /// <returns>Whether player can be targeted</returns>
            protected bool TryGetPlayerTarget()
            {
                Vector2 plrDirection = stateContext.playerService.PlayerPosition -
                                       (Vector2)stateContext.gameObject.transform.position;
                if (plrDirection.sqrMagnitude > stateContext.behaviourConfig.playerAgroRangeSqr)
                    return false; // Player is too far away

                RaycastHit2D raycast = Physics2D.Raycast(stateContext.gameObject.transform.position, plrDirection,
                    stateContext.behaviourConfig.playerAgroRange, (int)CollisionAssistant.Visible);

                // Debug.Log(raycast.collider);
                if (raycast.collider && raycast.collider.gameObject.CompareTag("Player"))
                    return true; // Player is visible and in range!
                else return false;
            }

            /// <summary>
            /// Attempts to pick the closest damageable obstacle as a target. If successful, assigns target obstacle
            /// in the state context.
            /// </summary>
            /// <returns>True if target found and assigned, false otherwise</returns>
            protected bool TryGetDamageableTarget()
            {
                stateContext.obstacleGridService.InitQuadTree(ObstacleFlags
                    .PlayerBuilt); // Ensure PlayerBuilt buildings quadtree exists
                Vector2Int selfGridPos = stateContext.obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                    .WorldToCellPos(stateContext.gameObject.transform.position);
                // Debug.Log($"Quadtree available: {stateContext.obstacleGridService.QuadTrees[ObstacleFlags.PlayerBuilt]}");
                List<Obstacle> targetOptions = stateContext.obstacleGridService.QuadTrees[ObstacleFlags.PlayerBuilt]
                    .GetKNearestObstacles
                    (
                        selfGridPos,
                        stateContext.behaviourConfig.targetListSize,
                        stateContext.behaviourConfig.maxTargetDistance
                    );
                if (targetOptions.Count == 0)
                {
                    stateContext.targetObstacle = null;
                    return false; // No targets available
                }

                // Choose closest target
                stateContext.targetObstacle = targetOptions[0] as Damageable;
                return true;
            }
        }

        private class IdleState : ZombieState
        {
            private float _clock = 0;

            // ReSharper disable Unity.PerformanceAnalysis
            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                if (_clock <= stateContext.behaviourConfig.idleUpdateDelay) return;

                _clock -= stateContext.behaviourConfig.idleUpdateDelay;

                if (TryGetPlayerTarget())
                {
                    stateContext.stateMachine.ChangeState(new NavPlayerState());
                    return;
                }

                // No player target, navigate to obstacle.
                stateContext.stateMachine.ChangeState(new NavObstacleState());
            }
        }

        private class NavPlayerState : ZombieState
        {
            Vector2 _movementVector;
            float _clock;

            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                if (_clock > stateContext.behaviourConfig.idleUpdateDelay)
                {
                    _clock -= stateContext.behaviourConfig.idleUpdateDelay;
                    if (!TryGetPlayerTarget())
                    {
                        // Can no longer target player, return to idle
                        stateContext.stateMachine.ChangeState(new IdleState());
                    }
                }

                _movementVector = stateContext.playerService.PlayerPosition -
                                  (Vector2)stateContext.gameObject.transform.position;
                stateContext.rigidBody.MovePosition(stateContext.rigidBody.position + _movementVector.normalized *
                    (stateContext.behaviourConfig.walkSpeed * Time.fixedDeltaTime));
                if (_movementVector.sqrMagnitude <= stateContext.behaviourConfig.attackRangeSqr)
                {
                    stateContext.stateMachine.ChangeState(new AttackPlayerState());
                }
            }
        }

        private class AttackPlayerState : ZombieState
        {
            float _clock = 0;

            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                stateContext.rigidBody.linearVelocity = Vector2.zero;
                if (_clock > stateContext.behaviourConfig.attackDelay)
                {
                    _clock -= stateContext.behaviourConfig.attackDelay;
                    AttackPlayer();
                }
            }

            private void AttackPlayer()
            {
                if ((stateContext.playerService.PlayerPosition - (Vector2)stateContext.gameObject.transform.position)
                    .sqrMagnitude > stateContext.behaviourConfig.attackRangeSqr)
                {
                    // Out of attack range, back to navigation
                    stateContext.stateMachine.ChangeState(new NavPlayerState());
                }

                stateContext.playerService.DamagePlayer(stateContext.behaviourConfig.attackDamage);
            }
        }

        private class NavObstacleState : ZombieState
        {
            private Vector2Int _curGridPos;
            private Vector2Int _nextGridPos;
            private float _clock = 0;
            
            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                if (_clock > stateContext.behaviourConfig.idleUpdateDelay)
                {
                    // Reset clock with slight random offset to avoid synchronizing with other enemies.
                    _clock -= stateContext.behaviourConfig.idleUpdateDelay + UnityEngine.Random.Range(-0.5f, 0.5f);
                    if (TryGetPlayerTarget())
                        stateContext.stateMachine.ChangeState(new NavPlayerState()); // Prioritize player as target
                    
                    if (_nextGridPos == _curGridPos)
                    {
                        // Try getting new target
                        GetNextTarget();
                    }
                }
                
                Vector2Int curGridPosCheck = stateContext.obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                    .WorldToCellPos(stateContext.gameObject.transform.position);
                if (curGridPosCheck != _curGridPos)
                {
                    _curGridPos = curGridPosCheck;
                    GetNextTarget();
                }
                
                if (_nextGridPos == _curGridPos)
                {
                    // Already at target position, ensure no movement.
                    stateContext.rigidBody.linearVelocity = Vector2.zero;
                    stateContext.rigidBody.angularVelocity = 0f;
                    return;
                }
               
                // Next position is not an obstacle, keep moving (if we need to)
                // Only navigate if not already at next destination
                Vector2 nextPos = stateContext.obstacleGridService.Grids[ObstacleConstants.MainObstacleLayer]
                    .CellToWorldPosCenter(_nextGridPos); 
                
                Vector2 movementVector = nextPos - (Vector2)stateContext.rigidBody.position;
                stateContext.rigidBody.MovePosition(stateContext.rigidBody.position
                                                    + stateContext.behaviourConfig.walkSpeed * Time.fixedDeltaTime *
                                                    movementVector.normalized);
            }

            
            /// <summary>
            /// Retrieves next target position based on current grid position. 
            /// </summary>
            private void GetNextTarget()
            {
                if (!stateContext.pathfindingService.TryGetNextTargetGridPosition(_curGridPos, out _nextGridPos,
                        out Obstacle targetObstacle))
                {
                    // Debug.Log("No next target position!");
                    // Not in flow field, move directory towards closest player-built obstacle instead
                    if (!TryGetDamageableTarget())
                    {
                        // No damageable obstacles at all, just navigate to the player.
                        stateContext.stateMachine.ChangeState(new NavPlayerState());
                        return;
                    }

                    Vector2 movementVector = stateContext.targetObstacle.transform.position
                                             - stateContext.gameObject.transform.position;
                    stateContext.rigidBody.MovePosition(stateContext.rigidBody.position
                                                        + stateContext.behaviourConfig.walkSpeed * Time.fixedDeltaTime *
                                                        movementVector.normalized);
                    // TODO: handle when stuck
                }
                
                // Debug.Log($"Next position is {_nextGridPos}");
                
                // Check if next position is obstacle
                if (targetObstacle)
                {
                    // Next position is obstacle, obstacle is "next on the path", thus we attack it.
                    stateContext.targetObstacle = targetObstacle as Damageable;
                    // Debug.Log("Close enough to target obstacle, attacking!");
                    stateContext.stateMachine.ChangeState(new AttackObstacleState());
                }
            }
        }

        private class AttackObstacleState : ZombieState
        {
            float _clock = 0;

            public override void Enter(IStateContext ctx)
            {
                base.Enter(ctx);
                Assert.IsTrue(
                    base.stateContext.targetObstacle as Damageable); // Sanity check, target must be damageable
            }

            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                stateContext.rigidBody.linearVelocity = Vector2.zero;
                stateContext.rigidBody.angularVelocity = 0f;
                
                if (_clock <= stateContext.behaviourConfig.attackDelay) return;
                _clock -= stateContext.behaviourConfig.attackDelay;
                AttackTarget();
            }

            private void AttackTarget()
            {
                Damageable target = stateContext.targetObstacle as Damageable;
                if (!target || !target.gameObject)
                {
                    // Target no longer exists, return to idle
                    stateContext.stateMachine.ChangeState(new IdleState());
                }

                target.Damage(stateContext.behaviourConfig.attackDamage, DamageFlags.Enemy,
                    stateContext.gameObject.transform.position, out bool destroyed);
                if (destroyed)
                    stateContext.stateMachine.ChangeState(new IdleState()); // Target obstacle destroyed, return to idle
            }
        }

        #endregion

        [Header("Zombie Config")] [SerializeField]
        private BehaviourConfig _behaviour;

        private StateMachine _stateMachine;

        [Inject] ObstacleGridService _obstacleGridService;
        [Inject] PlayerService _playerService;
        [Inject] PathfindingService _pathfindingService;

        private void Awake()
        {
            _behaviour.CalculateSquareRanges();

            ZombieContext stateContext = new ZombieContext
            {
                obstacleGridService = _obstacleGridService,
                playerService = _playerService,
                pathfindingService = _pathfindingService,
                gameObject = gameObject,
                rigidBody = GetComponent<Rigidbody2D>(),
                targetObstacle = null,

                behaviourConfig = _behaviour
            };
            _stateMachine = new StateMachine(stateContext);

            _stateMachine.ChangeState(new IdleState());
        }

        private void FixedUpdate()
        {
            _stateMachine.Update();
        }
    }
}