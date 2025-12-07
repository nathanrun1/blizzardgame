using Blizzard.NPCs.Concrete.Rabbit;
using Blizzard.Obstacles;
using Blizzard.Pathfinding;
using Blizzard.Player;
using Blizzard.Utilities;
using UnityEngine;
using Zenject;


namespace Blizzard.NPCs.Concrete
{
    public class RabbitBehaviour : NPCBehaviour
    {
        [Header("Behaviour Config")] 
        [SerializeField] private RabbitConfig _config;

        private StateMachine _stateMachine;

        [Inject] private ObstacleGridService _obstacleGridService;
        [Inject] private PlayerService _playerService;

        protected override void Awake()
        {
            RabbitContext ctx = new RabbitContext
            {
                config = _config,
                obstacleGridService = _obstacleGridService,
                playerService = _playerService,
                collisionRadius = 0.125f, // TODO: get this value from somewhere
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

        private void FixedUpdate()
        {
            _stateMachine.Update(Time.fixedDeltaTime);
        }
    }
}