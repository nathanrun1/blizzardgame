using Blizzard.NPCs.BaseStates;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Utilities;
using Blizzard.Utilities.Logging;
using UnityEngine;

namespace Blizzard.NPCs.Concrete.Wolf
{
    public class WolfContext : PassiveNPCContext
    {
        public WolfBehaviour wolfBehaviour;
    }
    
    public abstract class WolfState : PassiveNPCState
    {
        protected WolfContext _wCtx => _ctx as WolfContext;
        protected WolfConfig _cfg => _ctx.config as WolfConfig;

        public override void Enter(IStateContext ctx)
        {
            _ctx = ctx as WolfContext;
        }

        /// <summary>
        /// Determines whether, when the player is in flee range, the wolf should instead attack the player
        /// rather than flee.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldDefend()
        {
            // TODO: Function to determine if wolf defends itself (based on pack presence)
            // Check where nearest wolf is somehow
            // If close enough, return true, else false
            return false;
        }
    }

    public class IdleState : WolfState
    {
        private float _clock = 0f;
        private float _wanderPeriod;

        public override void Enter(IStateContext ctx)
        {
            base.Enter(ctx);

            _wanderPeriod = Random.Range(_ctx.config.wanderPeriodRange.x, _ctx.config.wanderPeriodRange.y);
            SetDirectionTo((new Vector2(1, 1)).normalized);
        }

        public override void Update(float deltaTime)
        {
            _clock += deltaTime;
            
            if (ShouldStartFlee())
                _ctx.StateMachine.ChangeState(ShouldDefend() ? new HostileState() : new FleeState());
            if (_clock >= _wanderPeriod)
                _ctx.StateMachine.ChangeState(new WanderState());
            
            Freeze();
        }
    }
    
    /// <summary>
    /// Wander in a random direction for some amount of time
    /// </summary>
    public class WanderState : WolfState
    {
        private float _clock = 0f;
        private float _wanderTime;
        private Vector2 _wanderDirection;
        
        public override void Enter(IStateContext ctx)
        {
            BLog.Log("WanderState");
            base.Enter(ctx);

            _wanderTime = Random.Range(_ctx.config.wanderTimeRange.x, _ctx.config.wanderTimeRange.y);
            float randAngle = Random.Range(0, 2 * Mathf.PI);
            _wanderDirection = Vector2.right * Mathf.Cos(randAngle) + Vector2.up * Mathf.Sin(randAngle);
            
            SetDirectionTo(_wanderDirection);
        }

        public override void Update(float deltaTime)
        {
            _clock += deltaTime;

            if (ShouldStartFlee()) 
                _ctx.StateMachine.ChangeState(new FleeState());
            if (_clock >= _wanderTime)
                _ctx.StateMachine.ChangeState(new IdleState());
            
            AdjustDirectionToward(TryTravelInDirection(_wanderDirection), deltaTime);
            Move();
        }
    }
    
    /// <summary>
    /// Flee from the player until out of range
    /// </summary>
    public class FleeState : WolfState
    {
        public override void Enter(IStateContext ctx)
        {
            BLog.Log("FleeState");
            base.Enter(ctx);

            Vector2 fleeDirection = (Vector2)_ctx.transform.position - _ctx.playerService.PlayerPosition;
            SetDirectionTo(fleeDirection);
        }
        
        public override void Update(float deltaTime)
        {
            Vector2 fleeDirection = (Vector2)_ctx.transform.position - _ctx.playerService.PlayerPosition;
            if (fleeDirection.magnitude >= _ctx.config.playerExitFleeRange)
                _ctx.StateMachine.ChangeState(new IdleState());
            
            AdjustDirectionToward(TryTravelInDirection(fleeDirection.normalized), deltaTime);
            Move();
        }
    }

    /// <summary>
    /// Try and kill the player
    /// </summary>
    public class HostileState : WolfState
    {
        public override void Enter(IStateContext ctx)
        {
            BLog.Log("HostileState");
            
            base.Enter(ctx);

            Vector2 chaseDirection = _ctx.playerService.PlayerPosition - (Vector2)_ctx.transform.position;
            SetDirectionTo(chaseDirection);
        }

        public override void Update(float deltaTime)
        {
            Vector2 chaseDirection = _ctx.playerService.PlayerPosition - (Vector2)_ctx.transform.position;
            if (chaseDirection.magnitude >= _cfg.exitHostileRange)
                _ctx.StateMachine.ChangeState(new IdleState());
            if (chaseDirection.magnitude <= _cfg.attackRange)
                _wCtx.wolfBehaviour.AttackPlayer();
            
            AdjustDirectionToward(TryTravelInDirection(chaseDirection.normalized), deltaTime);
            Move();
        }
    }
}