using Blizzard.NPCs.BaseStates;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Utilities;
using Blizzard.Utilities.Assistants;
using UnityEngine;

namespace Blizzard.NPCs.Concrete.Rabbit
{
    public class IdleState : PassiveNPCState
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
                _ctx.StateMachine.ChangeState(new FleeState());
            if (_clock >= _wanderPeriod)
                _ctx.StateMachine.ChangeState(new WanderState());
            
            Freeze();
        }
    }
    
    /// <summary>
    /// Wander in a random direction for some amount of time
    /// </summary>
    public class WanderState : PassiveNPCState
    {
        private float _clock = 0f;
        private float _wanderTime;
        private Vector2 _wanderDirection;
        
        public override void Enter(IStateContext ctx)
        {
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
    public class FleeState : PassiveNPCState
    {
        public override void Enter(IStateContext ctx)
        {
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
}