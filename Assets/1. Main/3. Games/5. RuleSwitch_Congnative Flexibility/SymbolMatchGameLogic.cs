using System;
using System.Collections;
using UnityEngine;

public enum SymbolType { Circle = 0, Triangle = 1, Rectangle = 2 }

public class SymbolMatchGameLogic : MonoBehaviour {
    [Header("Rounds")]
    public string gameName = "ShapeShifter";
    public int totalRounds = 3;
    public int initialSymbolsPerRound = 10;
    public int symbolsPerRoundIncrement = 2;

    [Header("Timing")]
    public float initialDelay = 2f;
    public float isi = 0.05f;

    [Header("Scoring")]
    public int baseScorePerCorrect = 10;
    public bool enableMultiplier = true;
    public float multiplierIncrement = 0.1f;

    [Header("Input")]
    public bool allowKeyboardInput = true;

    public SymbolType? PreviousSymbol { get; private set; } = null;
    public SymbolType CurrentSymbol { get; private set; }
    public bool IsRunning { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;
    public bool IsAcceptingInput { get; private set; } = false;

    public int TotalScore { get; private set; } = 0;
    public int TotalCorrect { get; private set; } = 0;
    public int TotalTrials { get; private set; } = 0;

    public event Action<SymbolType, bool> OnSymbolShown;
    public event Action<bool, int, float> OnTrialResult;
    public event Action<int, int, float, int, int> OnRoundComplete;
    public event Action<int, int, float> OnGameComplete;
    public event Action<int> OnScoreUpdated;

    private Coroutine gameCoroutine;
    private float trialStartRealtime;
    private bool awaitingResponse;
    private int consecutiveCorrect = 0;

    private int roundLocalCorrect = 0;
    private int roundLocalScore = 0;

    private bool waitForNextRound = false;

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

    public void PauseToggle() {
        IsPaused = !IsPaused;
    }

    public void StopGame() {
        if(!IsRunning) return;
        if(gameCoroutine != null) StopCoroutine(gameCoroutine);
        IsRunning = false;
    }

    public void PressTick() => RegisterInput(true);
    public void PressCross() => RegisterInput(false);

    private void Update() {
        if(!IsRunning || IsPaused || !allowKeyboardInput) return;
        if(Input.GetKeyDown(KeyCode.Space)) PressTick();
        if(Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) PressCross();
    }

    private IEnumerator RunGameCoroutine() {
        float gameStartRealtime = Time.realtimeSinceStartup;

        for(int round = 1; round <= totalRounds; round++) {
            int symbolsThisRound = initialSymbolsPerRound + (round - 1) * symbolsPerRoundIncrement;
            roundLocalCorrect = 0;
            roundLocalScore = 0;
            PreviousSymbol = null;
            float roundStartRealtime = Time.realtimeSinceStartup;

            CurrentSymbol = PickRandomSymbol();
            OnSymbolShown?.Invoke(CurrentSymbol, true);
            yield return WaitRealtimeFor(initialDelay);
            PreviousSymbol = CurrentSymbol;

            for(int i = 1; i < symbolsThisRound; i++) {
                while(IsPaused) yield return null;
                CurrentSymbol = PickRandomSymbol();
                OnSymbolShown?.Invoke(CurrentSymbol, false);
                IsAcceptingInput = true;
                awaitingResponse = true;
                trialStartRealtime = Time.realtimeSinceStartup;
                while(awaitingResponse) {
                    if(IsPaused) break;
                    yield return null;
                }
                while(IsPaused) yield return null;
                PreviousSymbol = CurrentSymbol;
                TotalTrials++;
                yield return WaitRealtimeFor(isi);
            }

            float roundTime = Time.realtimeSinceStartup - roundStartRealtime;
            OnRoundComplete?.Invoke(round, roundLocalCorrect, roundTime, roundLocalScore, symbolsThisRound);

            if(round < totalRounds) {
                waitForNextRound = true;
                while(waitForNextRound) {
                    if(IsPaused) yield return null;
                    yield return null;
                }
            }

            roundLocalCorrect = 0;
            roundLocalScore = 0;
            yield return WaitRealtimeFor(0.25f);
        }

        float totalTime = Time.realtimeSinceStartup - gameStartRealtime;
        IsRunning = false;
        OnGameComplete?.Invoke(TotalScore, TotalCorrect, totalTime);
/*        TrySaveScore(totalTime);*/
    }

    private IEnumerator WaitRealtimeFor(float seconds) {
        float start = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup - start < seconds) {
            yield return null;
        }
    }

    private void RegisterInput(bool pressedTick) {
        if(!IsRunning) return;
        if(!IsAcceptingInput) return;
        if(!awaitingResponse) return;
        IsAcceptingInput = false;
        awaitingResponse = false;
        float rt = Time.realtimeSinceStartup - trialStartRealtime;
        EvaluateResponse(pressedTick, rt, out int pts, out bool correct);
        OnTrialResult?.Invoke(correct, pts, rt);
    }

    private void EvaluateResponse(bool pressedTick, float reactionTime, out int pointsAwarded, out bool correct) {
        bool isSame = PreviousSymbol.HasValue && PreviousSymbol.Value == CurrentSymbol;
        bool expectedTick = isSame;
        correct = (pressedTick == expectedTick);
        pointsAwarded = 0;
        if(correct) {
            float mult = enableMultiplier ? (1f + consecutiveCorrect * multiplierIncrement) : 1f;
            pointsAwarded = Mathf.RoundToInt(baseScorePerCorrect * mult);
            TotalScore += pointsAwarded;
            TotalCorrect++;
            consecutiveCorrect++;
            roundLocalCorrect++;
            roundLocalScore += pointsAwarded;
        } else {
            consecutiveCorrect = 0;
        }
        OnScoreUpdated?.Invoke(TotalScore);
    }

    [Serializable]
    public class SymbolMatchScoreEntry : ScoreEntry {
        public int totalScore;
        public int correctCount;
        public int totalTrials;
        public float timeTaken;
        public int roundsCompleted;
        public override int GetScoreValue() => totalScore;
    }

/*    private void TrySaveScore(float totalTime) {
        try {
            var entry = new SymbolMatchScoreEntry {
                timestamp = DateTime.UtcNow.ToString("o"),
                totalScore = TotalScore,
                correctCount = TotalCorrect,
                totalTrials = TotalTrials,
                timeTaken = totalTime,
                roundsCompleted = totalRounds
            };
            if(GlobalScoreManager.Instance != null) GlobalScoreManager.Instance.AddScore(GameType.ShapeShifter, entry);
        } catch(Exception ex) {
            Debug.LogWarning("Failed saving score: " + ex.Message);
        }
    }
*/
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
        waitForNextRound = false;
    }
}
