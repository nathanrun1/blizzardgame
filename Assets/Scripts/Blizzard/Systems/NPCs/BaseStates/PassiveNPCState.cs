using System;
using Blizzard.Constants;
using Blizzard.NPCs.Concrete.Rabbit;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Utilities;
using Blizzard.Utilities.Assistants;
using Blizzard.Utilities.Logging;
using UnityEngine;

namespace Blizzard.NPCs.BaseStates
{
    public class PassiveNPCContext : IStateContext
    {
        public StateMachine StateMachine { get; set; }

        public PassiveNPCConfig config;

        public ObstacleGridService obstacleGridService;
        public PlayerService playerService;
        public float collisionRadius;
        public Transform transform;
        public Rigidbody2D rigidbody;
    }
    
    /// <summary>
    /// Base abstract state for passive NPCs, containing necessary functionality for passive behavior, e.g.
    /// wandering & fleeing
    /// </summary>
    public abstract class PassiveNPCState : IState
    {
        protected PassiveNPCContext _ctx { get; set; }

        private Vector2 _curDirection;
        
        public virtual void Enter(IStateContext ctx)
        {
            _ctx = ctx as PassiveNPCContext;
        }

        public abstract void Update(float deltaTime);

        public void Exit()
        {
        }

        
        /// <summary>
        /// Move in the current intended direction
        /// </summary>
        protected void Move()
        {
            _ctx.rigidbody.MovePosition(_ctx.rigidbody.position + _curDirection.normalized *
                (_ctx.config.walkSpeed * Time.fixedDeltaTime));
        }
        
        /// <summary>
        /// Attempt to stay in the same position
        /// </summary>
        protected void Freeze()
        {
            _ctx.rigidbody.MovePosition(_ctx.transform.position);
        }

        /// <summary>
        /// Sets current direction to given target direction
        /// </summary>
        protected void SetDirectionTo(Vector2 targetDir)
        {
            _curDirection = targetDir;
        }
        
        /// <summary>
        /// Smoothly adjusts (interpolates) the current travel direction toward the intended travel direction.
        /// </summary>
        protected void AdjustDirectionToward(Vector2 targetDir, float deltaTime)
        {
            float signedAngle = Mathf.Atan2(
                    _curDirection.x * targetDir.y - _curDirection.y * targetDir.x,  // cross product
                    Vector2.Dot(_curDirection, targetDir)                   // dot product
            );
            float rotDir = Mathf.Sign(signedAngle);
            Quaternion rot = Quaternion.Euler(0, 0, rotDir * deltaTime * _ctx.config.dirAdjustmentRate);
            _curDirection = rot * _curDirection;
        }

        /// <summary>
        /// Picks the next direction to travel in to based on the desired direction to travel in.
        /// 
        /// Assumes direction is normalized.
        /// </summary>
        /// <param name="direction">Direction to attempt travelling in</param>
        protected Vector2 TryTravelInDirection(Vector2 direction)
        {
            // Ensure that next travel position is at most one grid square away
            Vector2 curDir = direction * GameConstants.CellSideLength;

            // We alternate by checking some angle both rotated clockwise & counterclockwise, increasing that angle
            //   iteratively.
            int maxDoubleChecks = PathfindingConstants.maxDirectionRotationChecks / 2;
            for (int i = 0; i < maxDoubleChecks; ++i)
            {
                float clockwiseAngle = i / (float)maxDoubleChecks * 180f;
                float counterclockwiseAngle = (i + 1) / (float)maxDoubleChecks * -180f;
                Quaternion rotClockwise = Quaternion.Euler(0, 0, clockwiseAngle);
                Quaternion rotCounterclockwise = Quaternion.Euler(0, 0, counterclockwiseAngle);
                
                Vector2 targetPos = (Vector2)_ctx.transform.position + (Vector2)(rotClockwise * curDir);
                if (!_ctx.obstacleGridService.IsOccupied(targetPos.ToCellPos()))
                    return rotClockwise * curDir;
                
                targetPos = (Vector2)_ctx.transform.position + (Vector2)(rotCounterclockwise * curDir);
                if (!_ctx.obstacleGridService.IsOccupied(targetPos.ToCellPos()))
                    return rotCounterclockwise * curDir;
            }

            return _ctx.transform.position;  // No available options. Stand still.
        }
        
        /// <summary>
        /// Determines if within range to starting fleeing from the player
        /// </summary>
        protected bool ShouldStartFlee()
        {
            return (_ctx.playerService.PlayerPosition - (Vector2)_ctx.transform.position).magnitude <=
                   _ctx.config.playerEnterFleeRange;
        }

        /// <summary>
        /// Determines if far enough from the player to stop fleeing
        /// </summary>
        protected bool ShouldStopFlee()
        {
            return (_ctx.playerService.PlayerPosition - (Vector2)_ctx.transform.position).magnitude >=
                   _ctx.config.playerExitFleeRange;
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
            Vector2 newPoint = (0.5f * GameConstants.CellSideLength + _ctx.collisionRadius)
                * (Vector2)diff + GridAssistant.CellToWorldPosCenter(curSquare);
            newPoint *= new Vector2(Math.Sign(diff.x), Math.Sign(diff.y));
            if (newPoint.x == 0)
                newPoint.x = 1 - newPoint.y * newPoint.y;  // Constraint is y component
            else
                newPoint.y = 1 - newPoint.x * newPoint.x;  // Constraint is x component

            return newPoint;
        }
    }
}