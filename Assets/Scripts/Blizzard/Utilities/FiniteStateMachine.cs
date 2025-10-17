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
        public void Update();
        /// <summary>
        /// Exit this state
        /// </summary>
        public void Exit();
    }

    public interface IStateContext 
    {
        public StateMachine stateMachine { get; set; }
    }

    public class StateMachine
    {
        public IState currentState { get; private set; }

        private IStateContext _stateContext;

        public StateMachine(IStateContext stateContext)
        {
            _stateContext = stateContext;
            stateContext.stateMachine = this;
        }

        public void ChangeState(IState newState)
        {
            if (currentState != null)
                currentState.Exit();

            currentState = newState;
            currentState.Enter(_stateContext);
        }

        public void Update()
        {
            if (currentState != null) currentState.Update();
        }
    }
}
