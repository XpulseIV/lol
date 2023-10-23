using System.Collections.Generic;

namespace AstralAssault
{
    public class GameStateMachine
    {
        private GameState _currentState;

        public GameStateMachine(GameState initialState) {
            this._currentState = initialState;
            this._currentState.Enter();
        }

        public void ChangeState(GameState newState) {
            this._currentState?.Exit();
            this._currentState = newState;
            this._currentState.Enter();
        }

        public List<DrawTask> GetDrawTasks() => this._currentState.GetDrawTasks();
    }
}