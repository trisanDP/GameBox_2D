using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SymbolMatchGameManager : GameStateManager {
    [Header("Core references")]
    public SymbolMatchGameLogic logic;
    public SymbolMatchGameUI ui;

    [Header("Config")]
    public string mainMenuSceneName = "MainMenu";

    private Coroutine countdownRoutine;
    private bool stateInitialized = false;
    private float roundStartRealtime = 0f;

    private void Reset() {
        if(logic == null) logic = FindFirstObjectByType<SymbolMatchGameLogic>();
        if(ui == null) ui = FindFirstObjectByType<SymbolMatchGameUI>();
    }

    private void Start() {
        if(logic == null) Debug.LogError("SymbolMatchGameLogic not assigned.");
        if(ui == null) Debug.LogError("SymbolMatchGameUI not assigned.");
        WireUIButtonsAndLogic();
        SetState(GameState.Guide);
    }

    private void OnDestroy() {
        UnwireUIButtonsAndLogic();
    }

    private void WireUIButtonsAndLogic() {
        if(ui == null || logic == null) return;
        if(ui.guideOkButton != null) ui.guideOkButton.onClick.AddListener(OnGuideOkPressed);
        if(ui.pauseButton != null) ui.pauseButton.onClick.AddListener(OnPausePressed);
        if(ui.resumeButton != null) ui.resumeButton.onClick.AddListener(OnResumePressed);
        if(ui.pauseExitButton != null) ui.pauseExitButton.onClick.AddListener(OnExitToMainMenuPressed);
        if(ui.gameOverMainMenuButton != null) ui.gameOverMainMenuButton.onClick.AddListener(OnExitToMainMenuPressed);
        if(ui.gameOverPlayNextButton != null) ui.gameOverPlayNextButton.onClick.AddListener(OnPlayNextPressed);
        if(ui.tickButton != null) ui.tickButton.onClick.AddListener(OnTickPressed);
        if(ui.crossButton != null) ui.crossButton.onClick.AddListener(OnCrossPressed);
        if(ui.roundResultNextButton != null) ui.roundResultNextButton.onClick.AddListener(OnRoundResultNextPressed);

        logic.OnSymbolShown += Logic_OnSymbolShown;
        logic.OnTrialResult += Logic_OnTrialResult;
        logic.OnRoundComplete += Logic_OnRoundComplete;
        logic.OnGameComplete += Logic_OnGameComplete;
        logic.OnScoreUpdated += Logic_OnScoreUpdated;
    }

    private void UnwireUIButtonsAndLogic() {
        if(ui == null || logic == null) return;
        if(ui.guideOkButton != null) ui.guideOkButton.onClick.RemoveListener(OnGuideOkPressed);
        if(ui.pauseButton != null) ui.pauseButton.onClick.RemoveListener(OnPausePressed);
        if(ui.resumeButton != null) ui.resumeButton.onClick.RemoveListener(OnResumePressed);
        if(ui.pauseExitButton != null) ui.pauseExitButton.onClick.RemoveListener(OnExitToMainMenuPressed);
        if(ui.gameOverMainMenuButton != null) ui.gameOverMainMenuButton.onClick.RemoveListener(OnExitToMainMenuPressed);
        if(ui.gameOverPlayNextButton != null) ui.gameOverPlayNextButton.onClick.RemoveListener(OnPlayNextPressed);
        if(ui.tickButton != null) ui.tickButton.onClick.RemoveListener(OnTickPressed);
        if(ui.crossButton != null) ui.crossButton.onClick.RemoveListener(OnCrossPressed);
        if(ui.roundResultNextButton != null) ui.roundResultNextButton.onClick.RemoveListener(OnRoundResultNextPressed);

        logic.OnSymbolShown -= Logic_OnSymbolShown;
        logic.OnTrialResult -= Logic_OnTrialResult;
        logic.OnRoundComplete -= Logic_OnRoundComplete;
        logic.OnGameComplete -= Logic_OnGameComplete;
        logic.OnScoreUpdated -= Logic_OnScoreUpdated;
    }

    protected override void GuideState() {
        if(!stateInitialized) {
            stateInitialized = true;
            ui.ShowGuide(true);
            ui.ShowCountdown(false);
            ui.ShowGamePanel(false);
            ui.ShowPausePanel(false);
            ui.ShowRoundResultPanel(false);
            ui.ShowGameOverPanel(false);
        }
    }

    protected override void CountdownState() {
        if(!stateInitialized) {
            stateInitialized = true;
            ui.ShowGuide(false);
            ui.ShowCountdown(true);
            ui.SetCountdownText(ui.countdownStart.ToString());
            if(countdownRoutine != null) StopCoroutine(countdownRoutine);
            countdownRoutine = StartCoroutine(RunCountdown(ui.countdownStart));
        }
    }

    protected override void InGameState() {
        if(!stateInitialized) {
            stateInitialized = true;
            ui.ShowCountdown(false);
            ui.ShowGamePanel(true);
            ui.ShowPausePanel(false);
            ui.ShowRoundResultPanel(false);
            ui.ShowGameOverPanel(false);
            roundStartRealtime = Time.realtimeSinceStartup;
            ui.UpdateTimer(0f);
            if(!logic.IsRunning) logic.StartGame();
        }
        if(logic.IsRunning && !logic.IsPaused) {
            float t = Time.realtimeSinceStartup - roundStartRealtime;
            ui.UpdateTimer(t);
        }
    }

    protected override void RoundSummaryState() {
        if(!stateInitialized) {
            stateInitialized = true;
            ui.ShowRoundResultPanel(true);
        }
    }

    protected override void PausedState() {
        if(!stateInitialized) {
            stateInitialized = true;
            ui.ShowPausePanel(true);
            if(!logic.IsPaused) logic.PauseToggle();
        }
    }

    protected override void GameOverState() {
        if(!stateInitialized) {
            stateInitialized = true;
            logic.StopGame();
            ui.ShowGamePanel(false);
            ui.ShowPausePanel(false);
            ui.ShowRoundResultPanel(false);
            ui.ShowGameOverPanel(true);
        }
    }

    protected override void VictoryState() { }

    private void OnGuideOkPressed() {
        SetState(GameState.Countdown);
    }

    private IEnumerator RunCountdown(int start) {
        for(int i = start; i >= 1; i--) {
            ui.SetCountdownText(i.ToString());
            yield return new WaitForSecondsRealtime(1f);
        }
        ui.ShowCountdown(false);
        SetState(GameState.InGame);
        yield break;
    }

    private void OnPausePressed() {
        SetState(GameState.Paused);
    }

    private void OnResumePressed() {
        ui.ShowPausePanel(false);
        if(logic.IsPaused) logic.PauseToggle();
        SetState(GameState.InGame);
    }

    private void OnExitToMainMenuPressed() {
        if(!string.IsNullOrEmpty(mainMenuSceneName)) SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnPlayNextPressed() {
        logic.initialSymbolsPerRound += logic.symbolsPerRoundIncrement;
        SetState(GameState.Guide);
    }

    private void OnTickPressed() {
        logic.PressTick();
    }

    private void OnCrossPressed() {
        logic.PressCross();
    }

    private void OnRoundResultNextPressed() {
        ui.ShowRoundResultPanel(false);
        logic.ContinueToNextRound();
        roundStartRealtime = Time.realtimeSinceStartup;
        SetState(GameState.InGame);
    }

    private void Logic_OnSymbolShown(SymbolType symbol, bool isFirst) {
        ui.SetSymbol(symbol, isFirst);
    }

    private void Logic_OnTrialResult(bool correct, int points, float rt) {
        ui.UpdateScore(logic.TotalScore);
        ui.UpdateMultiplier(logic.enableMultiplier ? (1f + Mathf.Max(0, logic.TotalCorrect - 1) * logic.multiplierIncrement) : 1f);
    }

    private void Logic_OnRoundComplete(int roundIndex, int roundCorrect, float timeTaken, int roundScore, int roundTotalTrials) {
        if(roundIndex < logic.totalRounds) {
            ui.ShowRoundResult(roundIndex, roundCorrect, roundTotalTrials, roundScore, timeTaken);
            SetState(GameState.RoundSummary);
        } else {
            // last round: do not change state here; wait for OnGameComplete
        }
    }

    private void Logic_OnGameComplete(int totalScore, int totalCorrect, float totalTime) {
        ui.ShowFinalResult(totalScore, totalCorrect, totalTime);

        // create a SymbolMatchScoreEntry and save it via GlobalScoreManager
        try {
            var entry = new SymbolMatchScoreEntry {
                timestamp = System.DateTime.UtcNow.ToString("o"),
                totalScore = totalScore,
                strike = totalCorrect,               // using TotalCorrect as "strike"; adjust if you track something else
                timeTaken = totalTime,
                roundsCompleted = logic != null ? logic.totalRounds : 0
            };

            string name = (logic != null && !string.IsNullOrEmpty(logic.gameName)) ? logic.gameName : "SymbolMatch";
            if(GlobalScoreManager.Instance != null) {
                GlobalScoreManager.Instance.AddScore(GameType.ShapeShifter, entry);
                Debug.Log($"Saved score for {name}: {entry.totalScore} pts");
            } else {
                Debug.LogWarning("GlobalScoreManager.Instance is null — score not saved.");
            }
        } catch(System.Exception ex) {
            Debug.LogWarning("Failed to save SymbolMatch score: " + ex.Message);
        }

        SetState(GameState.GameOver);
    }

    private void Logic_OnScoreUpdated(int totalScore) {
        ui.UpdateScore(totalScore);
    }

    protected override void OnStateEnter(GameState from, GameState to) {
        stateInitialized = false;
    }
}
