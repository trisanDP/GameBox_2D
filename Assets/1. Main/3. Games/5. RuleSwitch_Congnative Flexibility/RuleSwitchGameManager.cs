    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [DisallowMultipleComponent]
    public class RuleSwitchGameManager : GameStateManager {
        #region Core refs
        [Header("Core references")]
        [Tooltip("Logic controlling trials, scoring and events")]
        public SymbolMatchGameLogic logic;
        [Tooltip("UI component that contains all panels/elements")]
        public SymbolMatchGameUI ui;
        #endregion

        #region Config
        [Header("Config")]
        [Tooltip("Main menu scene name (optional)")]
        public string mainMenuSceneName = "MainMenu";
        #endregion

        #region Internal
        private Coroutine countdownRoutine;
        private bool stateInitialized = false;
        private float roundStartRealtime = 0f;
        #endregion

        #region Unity lifecycle
        private void Reset() {
            if(logic == null) logic = FindFirstObjectByType<SymbolMatchGameLogic>();
            if(ui == null) ui = FindFirstObjectByType<SymbolMatchGameUI>();
        }

        private void Start() {
            // ensure refs
            if(logic == null) Debug.LogError("RuleSwitchGameManager: SymbolMatchGameLogic not assigned.");
            if(ui == null) Debug.LogError("RuleSwitchGameManager: SymbolMatchGameUI not assigned.");

            WireUIButtonsAndLogic();
            // ensure initial state is Guide (GameStateManager sets to Guide in Awake)
            SetState(GameState.Guide);
        }

        private void OnDestroy() {
            UnwireUIButtonsAndLogic();
        }
        #endregion

        #region Wiring helpers
        private void WireUIButtonsAndLogic() {
            if(ui == null || logic == null) return;

            // Guide OK
            if(ui.guideOkButton != null) ui.guideOkButton.onClick.AddListener(OnGuideOkPressed);

            // Pause / Resume / Exit (pause panel buttons)
            if(ui.pauseButton != null) ui.pauseButton.onClick.AddListener(OnPausePressed);
            if(ui.resumeButton != null) ui.resumeButton.onClick.AddListener(OnResumePressed);
            if(ui.pauseExitButton != null) ui.pauseExitButton.onClick.AddListener(OnExitToMainMenuPressed);

            // Game Over buttons
            if(ui.gameOverMainMenuButton != null) ui.gameOverMainMenuButton.onClick.AddListener(OnExitToMainMenuPressed);
            if(ui.gameOverPlayNextButton != null) ui.gameOverPlayNextButton.onClick.AddListener(OnPlayNextPressed);

            // Hook Yes/No to logic via manager (use named methods so we can remove later)
            if(ui.tickButton != null) ui.tickButton.onClick.AddListener(OnTickPressed);
            if(ui.crossButton != null) ui.crossButton.onClick.AddListener(OnCrossPressed);

            // Subscribe to logic events
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

            logic.OnSymbolShown -= Logic_OnSymbolShown;
            logic.OnTrialResult -= Logic_OnTrialResult;
            logic.OnRoundComplete -= Logic_OnRoundComplete;
            logic.OnGameComplete -= Logic_OnGameComplete;
            logic.OnScoreUpdated -= Logic_OnScoreUpdated;
        }
        #endregion

        #region State implementations
        protected override void GuideState() {
            if(!stateInitialized) {
                stateInitialized = true;
                // show guide panel through UI (ui owns the references)
                ui.ShowGuide(true);
                ui.ShowCountdown(false);
                ui.ShowGamePanel(false);
                ui.ShowPausePanel(false);
                ui.ShowGameOverPanel(false);
            }
            // waiting for guide OK (OnGuideOkPressed)
        }

        protected override void CountdownState() {
            if(!stateInitialized) {
                stateInitialized = true;
                ui.ShowGuide(false);
                ui.ShowCountdown(true);
                ui.SetCountdownText(ui.countdownStart.ToString());
                // start countdown coroutine
                if(countdownRoutine != null) StopCoroutine(countdownRoutine);
                countdownRoutine = StartCoroutine(RunCountdown(ui.countdownStart));
            }
        }

        protected override void InGameState() {
            if(!stateInitialized) {
                stateInitialized = true;
                // hide panels, show game panel
                ui.ShowCountdown(false);
                ui.ShowGamePanel(true);
                ui.ShowPausePanel(false);
                ui.ShowGameOverPanel(false);

                // reset round timer and UI
                roundStartRealtime = Time.realtimeSinceStartup;
                ui.UpdateTimer(0f);

                // start logic
                logic.StartGame();
            }

            // update runtime UI timer
            if(logic.IsRunning && !logic.IsPaused) {
                float t = Time.realtimeSinceStartup - roundStartRealtime;
                ui.UpdateTimer(t);
            }
        }

        protected override void PausedState() {
            if(!stateInitialized) {
                stateInitialized = true;
                ui.ShowPausePanel(true);
                logic.PauseToggle(); 
            }
        }

        protected override void GameOverState() {
            if(!stateInitialized) {
                stateInitialized = true;

                logic.StopGame();
                ui.ShowGamePanel(false);
                ui.ShowPausePanel(false);
                ui.ShowGameOverPanel(true);
                ui.ShowCountdown(false);
                ui.ShowGuide(false);

                ui.ShowFinalResult(logic.TotalScore, logic.TotalCorrect, 0f);
            }
        }

        protected override void VictoryState() {

        }
        #endregion

        #region --- UI callback handlers (manager-side) ---
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
            // go to pause state
            SetState(GameState.Paused);
        }

        private void OnResumePressed() {
            // hide pause UI, resume logic, go back to InGame
            ui.ShowPausePanel(false);
            logic.PauseToggle(); // toggles pause off
            SetState(GameState.InGame);
        }

        private void OnExitToMainMenuPressed() {
            if(!string.IsNullOrEmpty(mainMenuSceneName)) SceneManager.LoadScene(mainMenuSceneName);
            else Debug.LogWarning("Main menu scene name not set.");
        }

        private void OnPlayNextPressed() {
            // simple level-up: increase initialSymbolsPerRound and go back to Guide
            logic.initialSymbolsPerRound += logic.symbolsPerRoundIncrement;
            SetState(GameState.Guide);
        }

        private void OnTickPressed() {
            logic.PressTick();
        }

        private void OnCrossPressed() {
            logic.PressCross();
        }
        #endregion

        #region --- Logic event handlers (update UI) ---
        private void Logic_OnSymbolShown(SymbolType symbol, bool isFirst) {
            ui.SetSymbol(symbol, isFirst);
        }

        private void Logic_OnTrialResult(bool correct, int points, float rt) {
            // show feedback flash handled in UI if desired (we keep it simple)
            ui.UpdateScore(logic.TotalScore);
            ui.UpdateMultiplier(logic.enableMultiplier ? (1f + Mathf.Max(0, logic.TotalCorrect - 1) * logic.multiplierIncrement) : 1f);
        }

        private void Logic_OnRoundComplete(int roundIndex, int roundCorrect, float timeTaken, int roundScore, int roundTotalTrials) {
            ui.ShowRoundResult(roundIndex, roundCorrect, roundRoundTotalOrDefault(roundIndex, roundScore, roundTotalTrials), roundScore, timeTaken);
            // after showing round result, proceed depending on your flow (here go to GameOver for last round)
            if(roundIndex >= logic.totalRounds) {
                SetState(GameState.GameOver);
            } else {
                // let manager decide: show guide again or continue to next round automatically
                // Here we return to Guide to show the guide for next (level up) if you like:
                SetState(GameState.Guide);
            }
        }

        // helper fallback to provide a total if needed
        private int roundRoundTotalOrDefault(int roundIndex, int roundScore, int roundTotalTrials) {
            return roundTotalTrials;
        }

        private void Logic_OnGameComplete(int totalScore, int totalCorrect, float totalTime) {
            ui.ShowFinalResult(totalScore, totalCorrect, totalTime);
            SetState(GameState.GameOver);
        }

        private void Logic_OnScoreUpdated(int totalScore) {
            ui.UpdateScore(totalScore);
        }
        #endregion

        #region Debug / safety
        protected override void OnStateEnter(GameState from, GameState to) {
            // reset per-state guard
            stateInitialized = false;
        }
        #endregion
    }
