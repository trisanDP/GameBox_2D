using System;
using System.Collections;
using UnityEngine;

public enum SymbolType { Circle = 0, Triangle = 1, Rectangle = 2 }

public class SymbolMatchGameLogic : MonoBehaviour {
    [Header("Rounds")]
    public string gameName = "SymbolMatch";
    public int totalRounds = 3;
    [Tooltip("Symbols shown in round 1 (includes the initial, non-responsive symbol)")]
    public int initialSymbolsPerRound = 10;
    [Tooltip("How many extra symbols to add each next round")]
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

    // runtime state (read-only)
    public SymbolType? PreviousSymbol { get; private set; } = null;
    public SymbolType CurrentSymbol { get; private set; }
    public bool IsRunning { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;
    public bool IsAcceptingInput { get; private set; } = false;

    public int TotalScore { get; private set; } = 0;
    public int TotalCorrect { get; private set; } = 0;
    public int TotalTrials { get; private set; } = 0;

    // Events UI subscribes to
    public event Action<SymbolType, bool /*isFirst*/> OnSymbolShown;
    public event Action<bool /*correct*/, int /*points*/, float /*rt*/> OnTrialResult;
    public event Action<int /*roundIndex*/, int /*roundCorrect*/, float /*timeTaken*/, int /*roundScore*/, int /*roundTotalTrials*/> OnRoundComplete;
    public event Action<int /*totalScore*/, int /*totalCorrect*/, float /*totalTime*/> OnGameComplete;
    public event Action<int /*totalScore*/> OnScoreUpdated;

    // private
    private Coroutine gameCoroutine;
    private float trialStartRealtime;
    private bool awaitingResponse;
    private int consecutiveCorrect = 0;

    // public API - call from UI
    public void StartGame() {
        if(IsRunning) return;
        ResetGameState();
        IsRunning = true;
        IsPaused = false;
        gameCoroutine = StartCoroutine(RunGameCoroutine());
    }

    public void PauseToggle() {
        IsPaused = !IsPaused;
    }

    public void StopGame() {
        if(!IsRunning) return;
        if(gameCoroutine != null) StopCoroutine(gameCoroutine);
        IsRunning = false;
    }

    // Buttons call these:
    public void PressTick() => RegisterInput(true);
    public void PressCross() => RegisterInput(false);

    private void Update() {
        if(!IsRunning || IsPaused || !allowKeyboardInput) return;
        if(Input.GetKeyDown(KeyCode.Space)) PressTick();
        if(Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) PressCross();
    }

    // Core flow:
    private IEnumerator RunGameCoroutine() {
        float gameStartRealtime = Time.realtimeSinceStartup;

        for(int round = 1; round <= totalRounds; round++) {
            int symbolsThisRound = initialSymbolsPerRound + (round - 1) * symbolsPerRoundIncrement;
            int roundCorrectCount = 0;
            int roundScore = 0;
            int roundTrials = 0;
            PreviousSymbol = null;

            float roundStartRealtime = Time.realtimeSinceStartup;

            // 1) Show first random symbol (no input allowed)
            CurrentSymbol = PickRandomSymbol();
            OnSymbolShown?.Invoke(CurrentSymbol, true);

            // Wait initialDelay (pausable)
            yield return WaitRealtimeFor(initialDelay);

            // mark first as previous and now display next symbols (symbolsThisRound - 1 trials needing response)
            PreviousSymbol = CurrentSymbol;

            for(int i = 1; i < symbolsThisRound; i++) {
                // respect pause
                while(IsPaused) yield return null;

                // show next symbol (remains until player responds)
                CurrentSymbol = PickRandomSymbol();
                OnSymbolShown?.Invoke(CurrentSymbol, false);

                // accept input
                IsAcceptingInput = true;
                awaitingResponse = true;
                trialStartRealtime = Time.realtimeSinceStartup;

                // wait until player presses one of the inputs (no timeout)
                while(awaitingResponse) {
                    if(IsPaused) break;
                    yield return null;
                }
                // if was paused, wait till unpause
                while(IsPaused) yield return null;

                // evaluate and update counters (RegisterInput triggered evaluation and invoked OnTrialResult)
                // those updates were already handled in RegisterInput/EvaluateResponse

                // bookkeeping
                PreviousSymbol = CurrentSymbol;
                roundTrials++;
                TotalTrials++;

                // small ISI (configurable)
                yield return WaitRealtimeFor(isi);
            }

            float roundTime = Time.realtimeSinceStartup - roundStartRealtime;
            // compute roundScore & roundCorrect from totals by subtracting previous rounds?
            // Simpler: pass totals for this round by tracking during evaluations - we'll collect via events from EvaluateResponse.
            // For clarity, recompute by assuming UI tracked per-round. To keep things simple, raise OnRoundComplete with round-level aggregates via local tracking:

            // Since EvaluateResponse updates total fields, we need local round aggregates.
            // For that we can compute roundCorrectCount and roundScore by using TotalScore/TotalCorrect snapshots.
            // But we didn't track scoreboard snapshots at round start. To avoid complexity, track round-local counters within EvaluateResponse.
            // --> Implement via private fields updated in EvaluateResponse. (these exist below)
            OnRoundComplete?.Invoke(round, roundLocalCorrect, Time.realtimeSinceStartup - roundStartRealtime, roundLocalScore, roundTrials);

            // reset round-local counters
            roundLocalCorrect = 0;
            roundLocalScore = 0;

            // tiny gap
            yield return WaitRealtimeFor(0.25f);
        }

        float totalTime = Time.realtimeSinceStartup - gameStartRealtime;
        IsRunning = false;
        OnGameComplete?.Invoke(TotalScore, TotalCorrect, totalTime);

        TrySaveScore(totalTime);
    }

    // helper wait unaffected by Time.timeScale and pausable
    private IEnumerator WaitRealtimeFor(float seconds) {
        float start = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - start < seconds) {
            // pause-aware: if paused, just yield and keep start time
            if(!IsPaused) yield return null;
            else yield return null;
        }
    }

    // Register input from UI/keyboard. pressedTick = true if tick pressed
    private void RegisterInput(bool pressedTick) {
        if(!IsRunning) return;
        if(!IsAcceptingInput) return;
        if(!awaitingResponse) return;

        IsAcceptingInput = false;
        awaitingResponse = false;
        float rt = Time.realtimeSinceStartup - trialStartRealtime;

        EvaluateResponse(pressedTick, rt, out int pointsAwarded, out bool correct);

        OnTrialResult?.Invoke(correct, pointsAwarded, rt);
    }

    // Round-local accumulators (used when raising OnRoundComplete)
    private int roundLocalCorrect = 0;
    private int roundLocalScore = 0;

    // Evaluate correctness and update scores
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

            // update round locals
            roundLocalCorrect++;
            roundLocalScore += pointsAwarded;
        } else {
            consecutiveCorrect = 0;
        }

        OnScoreUpdated?.Invoke(TotalScore);
    }

    // pick one of three
    private SymbolType PickRandomSymbol() {
        int v = UnityEngine.Random.Range(0, 3);
        return (SymbolType)v;
    }

    // Persist via GlobalScoreManager
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

            if(GlobalScoreManager.Instance != null)
                GlobalScoreManager.Instance.AddScore(gameName, entry);
            else
                Debug.LogWarning("GlobalScoreManager not found; score not saved.");
        } catch(Exception ex) {
            Debug.LogWarning("Failed saving score: " + ex.Message);
        }
    }

    // reset runtime fields
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
}