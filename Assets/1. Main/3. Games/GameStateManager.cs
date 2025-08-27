using UnityEngine;

/// <summary>
/// Abstract base that defines named states and ensures a clear SetState API.
/// Derived classes implement each state's behavior in the abstract methods.
/// </summary>
public abstract class GameStateManager : MonoBehaviour {
    protected GameState activeState;
    private GameState previousState;

    protected virtual void Awake() {
        activeState = GameState.Guide;
        previousState = activeState;
    }

    protected virtual void Update() {
        // Call per-frame handler for the active state
        HandleState();

        // Call Enter logic once on state change
        if(previousState != activeState) {
            OnStateEnter(previousState, activeState);
            previousState = activeState;
        }
    }

    /// <summary>Switch to a new state (use from derived classes / UI callbacks).</summary>
    protected void SetState(GameState newState) {
        activeState = newState;
    }

    /// <summary>Optional hook invoked once when state changes (from, to)</summary>
    protected virtual void OnStateEnter(GameState from, GameState to) {
        // default does nothing; derived classes can override
    }

    protected void HandleState() {
        switch(activeState) {
            case GameState.Guide:
            GuideState();
            break;
            case GameState.Countdown:
            CountdownState();
            break;
            case GameState.InGame:
            InGameState();
            break;
            case GameState.Paused:
            PausedState();
            break;
            case GameState.GameOver:
            GameOverState();
            break;
            case GameState.Victory:
            VictoryState();
            break;
        }
    }

    #region --- abstract state handlers (implement in derived) ---
    protected abstract void GuideState();
    protected abstract void CountdownState();
    protected abstract void InGameState();
    protected abstract void PausedState();
    protected abstract void GameOverState();
    protected abstract void VictoryState();
    #endregion
}

public enum GameState {
    Guide,
    Countdown,
    InGame,
    Paused,
    GameOver,
    Victory
}
