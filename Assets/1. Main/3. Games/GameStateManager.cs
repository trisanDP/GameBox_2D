using UnityEngine;

public enum GameState {
    Guide,
    Countdown,
    InGame,
    RoundSummary,
    Paused,
    GameOver,
    Victory
}

public abstract class GameStateManager : MonoBehaviour {
    protected GameState activeState;
    private GameState previousState;

    protected virtual void Awake() {
        activeState = GameState.Guide;
        previousState = activeState;
    }

    protected virtual void Update() {
        HandleState();
        if(previousState != activeState) {
            OnStateEnter(previousState, activeState);
            previousState = activeState;
        }
    }

    protected void SetState(GameState newState) {
        activeState = newState;
    }

    protected virtual void OnStateEnter(GameState from, GameState to) { }

    protected void HandleState() {
        switch(activeState) {
            case GameState.Guide: GuideState(); break;
            case GameState.Countdown: CountdownState(); break;
            case GameState.InGame: InGameState(); break;
            case GameState.RoundSummary: RoundSummaryState(); break;
            case GameState.Paused: PausedState(); break;
            case GameState.GameOver: GameOverState(); break;
            case GameState.Victory: VictoryState(); break;
        }
    }

    protected abstract void GuideState();
    protected abstract void CountdownState();
    protected abstract void InGameState();
    protected abstract void RoundSummaryState();
    protected abstract void PausedState();
    protected abstract void GameOverState();
    protected abstract void VictoryState();
}
