namespace Blizzard.Utilities
{
    public interface IState
    {
        /// <summary>
        /// Enter this state
        /// </summary>
        public void Enter(IStateContext ctx);

        /// <summary>
        /// Call state's update function, to be invoked once per frame
        /// </summary>
        public void Update(float deltaTime);

        /// <summary>
        /// Exit this state
        /// </summary>
        public void Exit();
    }

    public interface IStateContext
    {
        public StateMachine StateMachine { get; set; }
    }

    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        private readonly IStateContext _stateContext;

        public StateMachine(IStateContext stateContext)
        {
            _stateContext = stateContext;
            stateContext.StateMachine = this;
        }

        public void ChangeState(IState newState)
        {
            CurrentState?.Exit();

            CurrentState = newState;
            CurrentState.Enter(_stateContext);
        }

        public void Update(float deltaTime)
        {
            CurrentState?.Update(deltaTime);
        }
    }
}