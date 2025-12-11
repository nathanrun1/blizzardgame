using System.Collections.Generic;
using Blizzard.NPCs.BaseStates;
using Blizzard.NPCs.Core;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Utilities;
using Blizzard.Utilities.Logging;
using UnityEngine;

namespace Blizzard.NPCs.Concrete.Wolf
{
    public class WolfContext : PassiveNPCContext
    {
        public NPCService npcService;
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
        /// Determines whether the wolf should engage in combat with the player.
        /// </summary>
        protected bool ShouldEngage()
        {
            // Is player in engagement range AND it hasn't been long enough since most recent engagement?
            return (_ctx.playerService.PlayerPosition - (Vector2)_ctx.transform.position).magnitude <= _cfg.engagementRange 
                   && Time.time - WolfBehaviour.whenLastEngagedPlayer < _cfg.packHostileCooldown;
        }

        /// <summary>
        /// Determines whether, when the player gets too close, the wolf should immediately defend rather than flee.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldDefend()
        {
            List<NPCBehaviour> wolvesNearby = _wCtx.npcService.Quadtrees[NPCID.Wolf]
                .GetKNearestNPCs(_wCtx.transform.position, _cfg.packSize + 1, _cfg.packRange);
            return wolvesNearby.Count - 1 >= _cfg.packSize;  // Exclude self
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
            
            if (ShouldEngage())
                _ctx.StateMachine.ChangeState(new HostileState());
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

            _wanderDirection = GetWanderDirection();
            SetDirectionTo(_wanderDirection);
        }

        public override void Update(float deltaTime)
        {
            _clock += deltaTime;

            if (ShouldEngage())
                _ctx.StateMachine.ChangeState(new HostileState());
            if (ShouldStartFlee())
                _ctx.StateMachine.ChangeState(ShouldDefend() ? new HostileState() : new FleeState());
            if (_clock >= _wanderTime)
                _ctx.StateMachine.ChangeState(new IdleState());
            
            AdjustDirectionToward(TryTravelInDirection(_wanderDirection), deltaTime);
            Move();
        }

        /// <summary>
        /// Determines the wandering direction, weighted towards the nearest wolf.
        /// </summary>
        private Vector2 GetWanderDirection()
        {
            List<NPCBehaviour> nearbyWolves = _wCtx.npcService.Quadtrees[NPCID.Wolf]
                .GetKNearestNPCs(_wCtx.transform.position, 2, float.MaxValue);
            
            if (nearbyWolves.Count <= 1)
            {
                float randAngle = Random.Range(0, 2 * Mathf.PI);
                return Vector2.right * Mathf.Cos(randAngle) + Vector2.up * Mathf.Sin(randAngle);
            }
            
            Vector2 packDirection = (nearbyWolves[1].transform.position - _wCtx.transform.position).normalized;
            float angleFromPack = _cfg.wanderTowardPackDistribution.Evaluate(Random.value) * (Random.Range(0, 2) * 2 - 1) * 180;
            BLog.Log($"Wandering {angleFromPack} degrees from nearest wolf direction");
            return Quaternion.Euler(0, 0, angleFromPack) * packDirection;
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
            if (ShouldEngage())
                _ctx.StateMachine.ChangeState(new HostileState());
            
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
        private float _whenBecameHostile;
        
        public override void Enter(IStateContext ctx)
        {
            BLog.Log("HostileState");
            
            base.Enter(ctx);

            _whenBecameHostile = Time.time;
            Vector2 chaseDirection = _ctx.playerService.PlayerPosition - (Vector2)_ctx.transform.position;
            SetDirectionTo(chaseDirection);
        }

        public override void Update(float deltaTime)
        {
            Vector2 chaseDirection = _ctx.playerService.PlayerPosition - (Vector2)_ctx.transform.position;
            if (ShouldExitHostile(chaseDirection.magnitude))
                _ctx.StateMachine.ChangeState(new IdleState());
            if (chaseDirection.magnitude <= _cfg.attackRange)
                _wCtx.wolfBehaviour.AttackPlayer();
            
            AdjustDirectionToward(TryTravelInDirection(chaseDirection.normalized), deltaTime);
            Move();
        }
        
        private bool ShouldExitHostile(float distanceToPlayer)
        {
            return (Time.time - _whenBecameHostile) > _cfg.minHostileDuration &&
                   distanceToPlayer >= _cfg.exitHostileRange;
        }
    }
}