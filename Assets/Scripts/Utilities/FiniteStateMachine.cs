namespace Blizzard.Utilities
{
    public interface IState
    {
        /// <summary>
        /// Enter this state
        /// </summary>
        public void Enter();
        /// <summary>
        /// Call state's update function, to be invoked once per frame
        /// </summary>
        public void Update();
        /// <summary>
        /// Exit this state
        /// </summary>
        public void Exit();
    }

    public class StateMachine
    {
        public IState currentState { get; private set; }

        public void ChangeState(IState newState)
        {
            if (currentState != null)
                currentState.Exit();

            currentState = newState;
            currentState.Enter();
        }

        public void Update()
        {
            if (currentState != null) currentState.Update();
        }
    }
}
