using Blizzard.Constants;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Utilities;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.Logging;
using Unity.Mathematics.Geometry;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Math = System.Math;

namespace Blizzard.NPCs.Concrete.Rabbit
{
    [System.Serializable]
    public class RabbitConfig
    {
        public float walkSpeed;

        public float playerEnterFleeRange;
        public float playerExitFleeRange;
        public Vector2 wanderPeriodRange;
        public Vector2 wanderTimeRange;

        public float idleUpdateDelay;
    }
    
    public class RabbitContext : IStateContext
    {
        public StateMachine StateMachine { get; set; }

        public RabbitConfig config;

        public ObstacleGridService obstacleGridService;
        public PlayerService playerService;
        public float collisionRadius;
        public Transform transform;
        public Rigidbody2D rigidbody;
    }
    
    public abstract class RabbitState : IState
    {
        protected RabbitContext context;
        
        public virtual void Enter(IStateContext ctx)
        {
            context = ctx as RabbitContext;;
        }

        public abstract void Update(float deltaTime);

        public void Exit()
        {
        }

        
        /// <summary>
        /// Travels toward the given position
        /// </summary>
        protected void MoveToward(Vector2 position)
        {
            Vector2 movementVector = (position - (Vector2)context.transform.position).normalized;
            context.rigidbody.MovePosition(context.rigidbody.position + movementVector.normalized *
                (context.config.walkSpeed * Time.fixedDeltaTime));
        }
        
        /// <summary>
        /// Picks the next position to travel to based on the desired direction to travel in.
        ///
        /// Assumes direction is normalized.
        /// </summary>
        /// <param name="direction">Direction to attempt travelling in</param>
        /// <param name="adjRot">Rotation direction when checking similar alternative directions</param>
        protected Vector2 TryTravelInDirection(Vector2 direction, AdjRotation adjRot)
        {
            // Ensure that next travel position is at most one grid square away
            Vector2 curDir = direction *= GameConstants.CellSideLength;

            if (curDir.ToCellPos() == Vector2Int.zero)
            {
                // Next position not in adj grid square, degrade to brute force (check a lot of rotations)
                for (int i = 0; i < PathfindingConstants.maxDirectionRotationChecks; ++i) 
                {
                    Quaternion rot = Quaternion.Euler(0, 0, i / 50f * 360f * (int)adjRot);
                    Vector2 targetPos = (Vector2)context.transform.position + (Vector2)(rot * curDir);
                    if (!context.obstacleGridService.IsOccupied(targetPos.ToCellPos()))
                        return targetPos;
                }
            }
            else
            {
                for (int i = 0; i < 8; ++i)
                {
                    Vector2 targetPos = (Vector2)context.transform.position + curDir;
                
                    if (!context.obstacleGridService.IsOccupied(targetPos.ToCellPos()))
                        return targetPos;

                    curDir = RotateToNextGridSquare(curDir, adjRot);
                
                }    
            }

            return context.transform.position;  // No available options. Stand still.
        }

        /// <summary>
        /// Determines if the rabbit is within range to starting fleeing from the player
        /// </summary>
        protected bool ShouldStartFlee()
        {
            return (context.playerService.PlayerPosition - (Vector2)context.transform.position).magnitude <=
                   context.config.playerEnterFleeRange;
        }

        /// <summary>
        /// Determines if the rabbit is far enough from the player to stop fleeing
        /// </summary>
        protected bool ShouldStopFlee()
        {
            return (context.playerService.PlayerPosition - (Vector2)context.transform.position).magnitude >=
                   context.config.playerExitFleeRange;
        }

        /// <summary>
        /// Rotates the given point to the next adjacent grid coordinate in the given rotation direction, choosing
        /// the point within it such that the total rotation from the given point is minimal.
        ///
        /// Assumes magnitude of point is 1
        /// </summary>
        private Vector2 RotateToNextGridSquare(Vector2 point, AdjRotation rot)
        {
            Vector2Int curSquare = GridAssistant.WorldToCellPos(point);
            Vector2Int nextSquare = GridAssistant.RotateAdjacentCoordinate(curSquare, rot);

            Vector2Int diff = nextSquare - curSquare;
            
            // Constrain one component to be as close as possible to grid square's border, and then adjust other
            //   to maintain magnitude of 1.
            Vector2 newPoint = (0.5f * GameConstants.CellSideLength + context.collisionRadius)
                * (Vector2)diff + GridAssistant.CellToWorldPosCenter(curSquare);
            newPoint *= new Vector2(Math.Sign(diff.x), Math.Sign(diff.y));
            if (newPoint.x == 0)
                newPoint.x = 1 - newPoint.y * newPoint.y;  // Constraint is y component
            else
                newPoint.y = 1 - newPoint.x * newPoint.x;  // Constraint is x component

            return newPoint;
        }
    }

    public class IdleState : RabbitState
    {
        private float _clock = 0f;
        private float _wanderPeriod;

        public override void Enter(IStateContext ctx)
        {
            base.Enter(ctx);

            _wanderPeriod = Random.Range(context.config.wanderPeriodRange.x, context.config.wanderPeriodRange.y);
        }

        public override void Update(float deltaTime)
        {
            _clock += deltaTime;

            if (ShouldStartFlee())
                context.StateMachine.ChangeState(new FleeState());
            if (_clock >= _wanderPeriod)
                context.StateMachine.ChangeState(new WanderState());
            
            MoveToward(context.transform.position); // Stand still
        }
    }

    /// <summary>
    /// Wander in a random direction for some amount of time
    /// </summary>
    public class WanderState : RabbitState
    {
        private float _clock = 0f;
        private float _wanderTime;
        private Vector2 _wanderDirection;
        private AdjRotation _adjRot;
        
        public override void Enter(IStateContext ctx)
        {
            base.Enter(ctx);

            _wanderTime = Random.Range(context.config.wanderTimeRange.x, context.config.wanderTimeRange.y);
            float randAngle = Random.Range(0, 2 * Mathf.PI);
            _wanderDirection = Vector2.right * Mathf.Cos(randAngle) + Vector2.up * Mathf.Sin(randAngle);
            _adjRot = Random.Range(0, 2) == 0 ? AdjRotation.Clockwise : AdjRotation.Counterclockwise;
        }

        public override void Update(float deltaTime)
        {
            _clock += deltaTime;

            if (ShouldStartFlee()) 
                context.StateMachine.ChangeState(new FleeState());
            if (_clock >= _wanderTime)
                context.StateMachine.ChangeState(new IdleState());
            
            MoveToward(TryTravelInDirection(_wanderDirection, _adjRot));
        }
    }

    /// <summary>
    /// Flee from the player until out of range
    /// </summary>
    public class FleeState : RabbitState
    {
        private AdjRotation _adjRot;

        public override void Enter(IStateContext ctx)
        {
            base.Enter(ctx);

            _adjRot = Random.Range(0, 2) == 0 ? AdjRotation.Clockwise : AdjRotation.Counterclockwise;
        }
        
        public override void Update(float deltaTime)
        {
            Vector2 fleeDirection = (Vector2)context.transform.position - context.playerService.PlayerPosition;
            if (fleeDirection.magnitude >= context.config.playerExitFleeRange)
                context.StateMachine.ChangeState(new IdleState());
            
            MoveToward(TryTravelInDirection(fleeDirection.normalized, _adjRot));
        }
    }
}