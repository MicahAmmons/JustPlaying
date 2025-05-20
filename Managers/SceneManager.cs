using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingAround.Managers
{
    public static class SceneManager
    {
        public enum SceneState
        {
            TitleScreen,
            LoadingScreen,
            Play,
            Pause,
            Dialogue,
            SceneTransition,
            Combat
        }

        private static SceneState _currentState = SceneState.TitleScreen;
        public static SceneState CurrentState => _currentState;

        public static event Action<SceneState> OnStateChanged;

        public static void SetState(SceneState newState)
        {
            if (_currentState == newState)
                return;

            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }

        public static bool IsState(SceneState state) => _currentState == state;
    }
}