using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Core game logic for the 1-back symbol match game.
/// This class does NOT control UI flow — it only exposes events and public API.
/// RuleSwitchGameManager controls states and calls StartGame()/PauseToggle()/StopGame() etc.
/// </summary>
public enum SymbolType { Circle = 0, Triangle = 1, Rectangle = 2 }

public class SymbolMatchGameLogic : MonoBehaviour {
    #region Inspector - Gameplay settings
    [Header("Rounds")]
    public string gameName = "SymbolMatch";
    public int totalRounds = 3;
    [Tooltip("Symbols shown in round 1 (includes the initial, non-responsive symbol)")]
    public int initialSymbolsPerRound = 10;
    [Tooltip("How many extra symbols to add each next round")]
    private bool waitForNextRound = false;
    public int symbolsPerRoundIncrement = 2;

    [Header("Timing")]
    [Tooltip("Time between first symbol and second symbol (seconds). Default 2s")]
    public float initialDelay = 2f;
    [Tooltip("Short pause after each response before showing next symbol (seconds)")]
    public float isi = 0.05f;

    [Header("Scoring")]
    public int baseScorePerCorrect = 10;
    public bool enableMultiplier = true;
    [Tooltip("Increase to multiplier per consecutive correct. 0.1 => 1.0, 1.1, 1.2, ...")]
    public float multiplierIncrement = 0.1f;

    [Header("Input")]
    public bool allowKeyboardInput = true; // space = tick, left ctrl = cross
    #endregion

    #region Public runtime state (read-only)
    public SymbolType? PreviousSymbol { get; private set; } = null;
    public SymbolType CurrentSymbol { get; private set; }
    public bool IsRunning { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;
    public bool IsAcceptingInput { get; private set; } = false;

    public int TotalScore { get; private set; } = 0;
    public int TotalCorrect { get; private set; } = 0;
    public int TotalTrials { get; private set; } = 0;
    #endregion

    #region Events (UI / Manager subscribe to these)
    /// <summary>Fired when a symbol is shown. bool = isFirstSymbol (no input allowed)</summary>
    public event Action<SymbolType, bool> OnSymbolShown;

    /// <summary>Fired when a trial is evaluated. parameters: correct, pointsAwarded, reactionTime</summary>
    public event Action<bool, int, float> OnTrialResult;

    /// <summary>Fired when a round completes: roundIndex, roundCorrect, timeTaken, roundScore, roundTotalTrials</summary>
    public event Action<int, int, float, int, int> OnRoundComplete;

    /// <summary>Fired when whole game completes: totalScore, totalCorrect, totalTime</summary>
    public event Action<int, int, float> OnGameComplete;

    /// <summary>Fired when total score updates</summary>
    public event Action<int> OnScoreUpdated;
    #endregion

    #region Private runtime fields
    private Coroutine gameCoroutine;
    private float trialStartRealtime;
    private bool awaitingResponse;
    private int consecutiveCorrect = 0;

    // round-local aggregates
    private int roundLocalCorrect = 0;
    private int roundLocalScore = 0;
    #endregion

    #region Public API (called by manager / UI)
    /// <summary>Begin the game loop. Manager should call this when entering InGame state.</summary>
    public void StartGame() {
        if(IsRunning) return;
        ResetGameState();
        IsRunning = true;
        IsPaused = false;
        gameCoroutine = StartCoroutine(RunGameCoroutine());
    }

    public void ContinueToNextRound() {
        waitForNextRound = false;
    }

    /// <summary>Toggle pause/resume. Manager uses this when entering/exiting Paused state.</summary>
    public void PauseToggle() {
        IsPaused = !IsPaused;
    }

    /// <summary>Stop the game immediately (used by manager when entering GameOver or cleanup).</summary>
    public void StopGame() {
        if(!IsRunning) return;
        if(gameCoroutine != null) StopCoroutine(gameCoroutine);
        IsRunning = false;
    }

    /// <summary>Called by UI (or manager) when player presses Tick/Yes.</summary>
    public void PressTick() => RegisterInput(true);

    /// <summary>Called by UI (or manager) when player presses Cross/No.</summary>
    public void PressCross() => RegisterInput(false);
    #endregion

    #region Unity update (keyboard shortcuts)
    private void Update() {
        if(!IsRunning || IsPaused || !allowKeyboardInput) return;
        if(Input.GetKeyDown(KeyCode.Space)) PressTick();
        if(Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) PressCross();
    }
    #endregion

    #region Core coroutine (game flow)
    private IEnumerator RunGameCoroutine() {
        float gameStartRealtime = Time.realtimeSinceStartup;

        for(int round = 1; round <= totalRounds; round++) {
            int symbolsThisRound = initialSymbolsPerRound + (round - 1) * symbolsPerRoundIncrement;
            roundLocalCorrect = 0;
            roundLocalScore = 0;
            PreviousSymbol = null;
            float roundStartRealtime = Time.realtimeSinceStartup;

            // 1) Show first random symbol (no input allowed)
            CurrentSymbol = PickRandomSymbol();
            OnSymbolShown?.Invoke(CurrentSymbol, true);

            // wait initialDelay (pausable)
            yield return WaitRealtimeFor(initialDelay);

            // set previous to first symbol
            PreviousSymbol = CurrentSymbol;

            // Remaining symbols: each trial waits for player's response
            for(int i = 1; i < symbolsThisRound; i++) {
                // pause-aware loop
                while(IsPaused) yield return null;

                // show next symbol
                CurrentSymbol = PickRandomSymbol();
                OnSymbolShown?.Invoke(CurrentSymbol, false);

                // enable input and block until response
                IsAcceptingInput = true;
                awaitingResponse = true;
                trialStartRealtime = Time.realtimeSinceStartup;

                while(awaitingResponse) {
                    if(IsPaused) break;
                    yield return null;
                }

                // if paused, wait until resumed
                while(IsPaused) yield return null;

                // bookkeeping (RegisterInput / EvaluateResponse already updated round locals)
                PreviousSymbol = CurrentSymbol;
                TotalTrials++;

                // brief ISI
                yield return WaitRealtimeFor(isi);
            }

            float roundTime = Time.realtimeSinceStartup - roundStartRealtime;
            // fire round complete
            OnRoundComplete?.Invoke(round, roundLocalCorrect, roundTime, roundLocalScore, symbolsThisRound);

            // WAIT HERE if not the final round — manager must call ContinueToNextRound()
            if(round < totalRounds) {
                waitForNextRound = true;
                while(waitForNextRound) {
                    // If paused, still allow pause to work
                    if(IsPaused) yield return null;
                    yield return null;
                }
            }

            // reset round-local counters
            roundLocalCorrect = 0;
            roundLocalScore = 0;

            // small inter-round gap
            yield return WaitRealtimeFor(0.25f);
        }

        float totalTime = Time.realtimeSinceStartup - gameStartRealtime;
        IsRunning = false;
        OnGameComplete?.Invoke(TotalScore, TotalCorrect, totalTime);

        // Persist score (optional)
        TrySaveScore(totalTime);
    }

    // Wait helper (unchanged)
    private IEnumerator WaitRealtimeFor(float seconds) {
        float start = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - start < seconds) {
            yield return null;
        }
    }

    #endregion

    #region Input handling & scoring
    private void RegisterInput(bool pressedTick) {
        if(!IsRunning) return;
        if(!IsAcceptingInput) return;
        if(!awaitingResponse) return;

        IsAcceptingInput = false;
        awaitingResponse = false;
        float rt = Time.realtimeSinceStartup - trialStartRealtime;

        EvaluateResponse(pressedTick, rt, out int pts, out bool correct);

        // Fire trial event for UI/manager
        OnTrialResult?.Invoke(correct, pts, rt);
    }

    private void EvaluateResponse(bool pressedTick, float reactionTime, out int pointsAwarded, out bool correct) {
        bool isSame = PreviousSymbol.HasValue && PreviousSymbol.Value == CurrentSymbol;
        bool expectedTick = isSame; // fixed rule: tick if same, cross if different
        correct = (pressedTick == expectedTick);

        pointsAwarded = 0;
        if(correct) {
            float mult = enableMultiplier ? (1f + consecutiveCorrect * multiplierIncrement) : 1f;
            pointsAwarded = Mathf.RoundToInt(baseScorePerCorrect * mult);
            TotalScore += pointsAwarded;
            TotalCorrect++;
            consecutiveCorrect++;

            // update round-level aggregates
            roundLocalCorrect++;
            roundLocalScore += pointsAwarded;
        } else {
            consecutiveCorrect = 0;
        }

        // notify listeners of updated total score
        OnScoreUpdated?.Invoke(TotalScore);
    }
    #endregion

    #region Persistence
    [Serializable]
    public class SymbolMatchScoreEntry : ScoreEntry {
        public int totalScore;
        public int correctCount;
        public int totalTrials;
        public float timeTaken;
        public int roundsCompleted;
        public override int GetScoreValue() => totalScore;
    }

    private void TrySaveScore(float totalTime) {
        try {
            var entry = new SymbolMatchScoreEntry {
                timestamp = DateTime.UtcNow.ToString("o"),
                totalScore = TotalScore,
                correctCount = TotalCorrect,
                totalTrials = TotalTrials,
                timeTaken = totalTime,
                roundsCompleted = totalRounds
            };

            if(GlobalScoreManager.Instance != null) GlobalScoreManager.Instance.AddScore(gameName, entry);
        } catch(Exception ex) {
            Debug.LogWarning("Failed saving score: " + ex.Message);
        }
    }
    #endregion

    #region Utilities
    private SymbolType PickRandomSymbol() {
        int v = UnityEngine.Random.Range(0, 3);
        return (SymbolType)v;
    }

    private void ResetGameState() {
        PreviousSymbol = null;
        CurrentSymbol = SymbolType.Circle;
        IsRunning = false;
        IsPaused = false;
        IsAcceptingInput = false;
        TotalScore = 0;
        TotalCorrect = 0;
        TotalTrials = 0;
        consecutiveCorrect = 0;
        roundLocalCorrect = 0;
        roundLocalScore = 0;
    }
    #endregion
}
