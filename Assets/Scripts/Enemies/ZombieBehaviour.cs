using UnityEngine;
using UnityEngine.Assertions;
using Blizzard.Utilities.StateMachine;
using Zenject;
using Blizzard.Obstacles;
using Blizzard.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;


namespace Blizzard.NPC.Enemies
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
            protected ZombieContext _stateContext;

            public virtual void Enter(IStateContext stateContext)
            {
                _stateContext = stateContext as ZombieContext;
            }

            public virtual void Exit() { }

            public abstract void Update();

            /// <summary>
            /// Checks if player can be targeted (i.e. visible and in range)
            /// </summary>
            /// <returns>Whether player can be targeted</returns>
            protected bool TryGetPlayerTarget()
            {
                Vector2 plrDirection = _stateContext.playerService.PlayerPosition - (Vector2)_stateContext.gameObject.transform.position;
                if (plrDirection.sqrMagnitude > _stateContext.behaviourConfig.playerAgroRangeSqr) return false; // Player is too far away

                RaycastHit2D raycast = Physics2D.Raycast(_stateContext.gameObject.transform.position, plrDirection, _stateContext.behaviourConfig.playerAgroRange, (int)CollisionAssistant.Visible);

                Debug.Log(raycast.collider);
                if (raycast.collider != null && raycast.collider.gameObject.CompareTag("Player")) return true; // Player is visible and in range!
                else return false;
            }

            /// <summary>
            /// Attempts to pick a damageable obstacle as a target. If successful, assigns target obstacle
            /// in the state context.
            /// </summary>
            /// <returns>True if target found and assigned, false otherwise</returns>
            protected bool TryGetDamageableTarget()
            {
                Obstacle randObstacle = _stateContext.obstacleGridService.GetRandomObstacleWithFlags(ObstacleFlags.PlayerBuilt);

                // Check for a closer player built obstacle in line of sight
                RaycastHit2D raycast = Physics2D.Raycast(_stateContext.gameObject.transform.position,
                                                         randObstacle.transform.position - _stateContext.gameObject.transform.position,
                                                         Mathf.Infinity,
                                                         (int)CollisionAssistant.Visible);
                if (raycast.collider.gameObject != randObstacle.gameObject)
                {
                    Obstacle hit = raycast.collider.gameObject.GetComponent<Obstacle>();
                    if (hit != null && (hit.ObstacleFlags & ObstacleFlags.PlayerBuilt) == ObstacleFlags.PlayerBuilt)
                    {
                        // A closer target, pick this one instead
                        randObstacle = hit;
                    }
                }

                if (randObstacle != null)
                {
                    _stateContext.targetObstacle = randObstacle as Damageable; // PlayerBuilt obstacles must be damageable
                    return true;
                }
                return false;
            }
        }

        private class IdleState : ZombieState
        {
            float _clock = 0;

            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                if (_clock > _stateContext.behaviourConfig.idleUpdateDelay)
                {
                    _clock -= _stateContext.behaviourConfig.idleUpdateDelay;
                    Debug.Log("In idle state, checking for targets...");

                    if (TryGetPlayerTarget())
                    {
                        Debug.Log("player targeting!");
                        _stateContext.stateMachine.ChangeState(new NavPlayerState());
                    }

                    if (TryGetDamageableTarget())
                    {
                        Debug.Log("obstacle identified, navigating!: " + _stateContext.targetObstacle.name);
                        _stateContext.stateMachine.ChangeState(new NavObstacleState());
                    }
                }
            }
        }

        private class NavPlayerState : ZombieState
        {
            Vector2 _movementVector;
            float _clock;

            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                if (_clock > _stateContext.behaviourConfig.idleUpdateDelay)
                {
                    _clock -= _stateContext.behaviourConfig.idleUpdateDelay;
                    if (!TryGetPlayerTarget())
                    {
                        // Can no longer target player, return to idle
                        _stateContext.stateMachine.ChangeState(new IdleState());
                    }
                }
                // TODO: verify player visibility
                _movementVector = _stateContext.playerService.PlayerPosition - (Vector2)_stateContext.gameObject.transform.position;
                _stateContext.rigidBody.MovePosition(_stateContext.rigidBody.position + _movementVector.normalized * _stateContext.behaviourConfig.walkSpeed * Time.fixedDeltaTime);
                if (_movementVector.sqrMagnitude <= _stateContext.behaviourConfig.attackRangeSqr)
                {
                    Debug.Log("Close enough to player, attacking!");
                    _stateContext.stateMachine.ChangeState(new AttackPlayerState());
                }
            }
        }

        private class AttackPlayerState : ZombieState
        {
            float _clock = 0;

            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                if (_clock > _stateContext.behaviourConfig.attackDelay)
                {
                    _clock -= _stateContext.behaviourConfig.attackDelay;
                    AttackPlayer();
                }
            }

            private void AttackPlayer()
            {
                if ((_stateContext.playerService.PlayerPosition - (Vector2)_stateContext.gameObject.transform.position).sqrMagnitude > _stateContext.behaviourConfig.attackRangeSqr)
                {
                    // Out of attack range, back to navigation
                    _stateContext.stateMachine.ChangeState(new NavPlayerState());
                }

                _stateContext.playerService.DamagePlayer(_stateContext.behaviourConfig.attackDamage);
            }
        }

        private class NavObstacleState : ZombieState
        {
            Vector2 _movementVector;
            float _clock = 0;

            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                if (_clock > _stateContext.behaviourConfig.idleUpdateDelay)
                {
                    _clock -= _stateContext.behaviourConfig.idleUpdateDelay;
                    if (TryGetPlayerTarget()) _stateContext.stateMachine.ChangeState(new NavPlayerState()); // Prioritize player as target
                }

                if (_stateContext.targetObstacle == null || _stateContext.targetObstacle.gameObject == null) _stateContext.stateMachine.ChangeState(new IdleState()); // Target no longer exists

                _movementVector = _stateContext.targetObstacle.transform.position - _stateContext.gameObject.transform.position;
                _stateContext.rigidBody.MovePosition(_stateContext.rigidBody.position + _movementVector.normalized * _stateContext.behaviourConfig.walkSpeed * Time.fixedDeltaTime);
                if (_movementVector.sqrMagnitude <= _stateContext.behaviourConfig.attackRangeSqr)
                {
                    Debug.Log("Close enough to target obstacle, attacking!");
                    _stateContext.stateMachine.ChangeState(new AttackObstacleState());
                }
            }
        }

        private class AttackObstacleState : ZombieState
        {
            float _clock = 0;

            public override void Enter(IStateContext stateContext)
            {
                base.Enter(stateContext);
                Assert.IsTrue(_stateContext.targetObstacle as Damageable != null); // Sanity check, target must be damageable
            }

            public override void Update()
            {
                _clock += Time.fixedDeltaTime;
                if (_clock > _stateContext.behaviourConfig.attackDelay)
                {
                    _clock -= _stateContext.behaviourConfig.attackDelay;
                    AttackTarget();
                }
            }

            private void AttackTarget()
            {
                Damageable target = _stateContext.targetObstacle as Damageable;
                if (target.gameObject == null || (target.transform.position - _stateContext.gameObject.transform.position).sqrMagnitude > _stateContext.behaviourConfig.attackRangeSqr)
                {
                    // Target no longer exists or out of attack range, return to idle
                    _stateContext.stateMachine.ChangeState(new IdleState());
                }

                target.Damage(_stateContext.behaviourConfig.attackDamage, out bool destroyed);
                if (destroyed) _stateContext.stateMachine.ChangeState(new IdleState()); // Target obstacle destroyed, return to idle
            }
        }
        #endregion

        [Header("Zombie Config")]
        [SerializeField] BehaviourConfig _behaviour;

        private StateMachine _stateMachine;

        [Inject] ObstacleGridService _obstacleGridService;
        [Inject] PlayerService _playerService;

        private void Awake()
        {
            _behaviour.CalculateSquareRanges();

            ZombieContext stateContext = new ZombieContext
            {
                obstacleGridService = _obstacleGridService,
                playerService = _playerService,
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
