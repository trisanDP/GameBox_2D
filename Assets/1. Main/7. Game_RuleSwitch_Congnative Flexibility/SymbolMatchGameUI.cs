using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SymbolMatchGameLogic))]
public class SymbolMatchGameUI : MonoBehaviour {
    [Header("Logic")]
    public SymbolMatchGameLogic logic;

    [Header("Symbol Display")]
    [Tooltip("Sprites: index 0 = Circle, 1 = Triangle, 2 = Rectangle")]
    public Sprite[] symbolSprites = new Sprite[3];
    public Image symbolImage;
    public TextMeshProUGUI symbolFallbackText; // optional fallback

    [Header("Guide & Countdown")]
    public GameObject guidePanel;            // visible after Play pressed
    public Button guideStartButton;         // inside guide: press to start countdown
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;

    [Header("HUD")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI timerText;

    [Header("Buttons")]
    public Button playButton;   // opens guidePanel
    public Button tickButton;   // ✓
    public Button crossButton;  // ✗
    public Button pauseButton;  // toggles pause

    [Header("Result Panels")]
    public GameObject roundResultPanel;
    public TextMeshProUGUI roundResultText;
    public GameObject finalResultPanel;
    public TextMeshProUGUI finalResultText;

    [Header("Config")]
    [Tooltip("Countdown start number (3 -> 3,2,1).")]
    public int countdownStart = 3;

    private float roundStartRealtime;

    private void Reset() {
        if(logic == null) logic = GetComponent<SymbolMatchGameLogic>();
    }

    private void OnEnable() {
        if(logic == null) {
            Debug.LogError("Logic not assigned");
            enabled = false;
            return;
        }

        // subscribe logic events
        logic.OnSymbolShown += HandleSymbolShown;
        logic.OnTrialResult += HandleTrialResult;
        logic.OnRoundComplete += HandleRoundComplete;
        logic.OnGameComplete += HandleGameComplete;
        logic.OnScoreUpdated += HandleScoreUpdated;

        // wire buttons
        if(playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if(guideStartButton != null) guideStartButton.onClick.AddListener(OnGuideStartPressed);
        if(tickButton != null) tickButton.onClick.AddListener(() => logic.PressTick());
        if(crossButton != null) crossButton.onClick.AddListener(() => logic.PressCross());
        if(pauseButton != null) pauseButton.onClick.AddListener(TogglePause);

        // initial UI state
        guidePanel.SetActive(false);
        countdownPanel.SetActive(false);
        roundResultPanel.SetActive(false);
        finalResultPanel.SetActive(false);
        UpdateScore(0);
        UpdateMultiplierText(1f);
    }

    private void OnDisable() {
        logic.OnSymbolShown -= HandleSymbolShown;
        logic.OnTrialResult -= HandleTrialResult;
        logic.OnRoundComplete -= HandleRoundComplete;
        logic.OnGameComplete -= HandleGameComplete;
        logic.OnScoreUpdated -= HandleScoreUpdated;
    }

    private void Update() {
        // live timer
        if(logic.IsRunning && !logic.IsPaused && roundStartRealtime > 0f) {
            float t = Time.realtimeSinceStartup - roundStartRealtime;
            UpdateTimer(t);
        }
    }

    // Play opens the guide panel (as you requested)
    private void OnPlayPressed() {
        guidePanel.SetActive(true);
    }

    // Inside guide panel: start countdown
    private void OnGuideStartPressed() {
        guidePanel.SetActive(false);
        StartCoroutine(RunCountdownThenStart());
    }

    private IEnumerator RunCountdownThenStart() {
        countdownPanel.SetActive(true);
        for(int i = countdownStart; i >= 1; i--) {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }
        countdownPanel.SetActive(false);

        // start game
        roundResultPanel.SetActive(false);
        finalResultPanel.SetActive(false);
        roundStartRealtime = Time.realtimeSinceStartup;
        logic.StartGame();
    }

    // Logic event handlers
    private void HandleSymbolShown(SymbolType s, bool isFirst) {
        int idx = (int)s;
        if(symbolSprites != null && idx >= 0 && idx < symbolSprites.Length && symbolSprites[idx] != null) {
            symbolImage.sprite = symbolSprites[idx];
            // dim first symbol slightly
            symbolImage.color = isFirst ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
        } else if(symbolFallbackText != null) {
            symbolFallbackText.text = s.ToString();
        }

        // set round text if not set yet
        if(roundStartRealtime == 0f) roundStartRealtime = Time.realtimeSinceStartup;
        // update multiplier display (using consecutive logic is internal; show best-effort)
        UpdateMultiplierText(logic.enableMultiplier ? 1f + (logic.TotalCorrect > 0 ? (logic.TotalCorrect - 1) * logic.multiplierIncrement : 0f) : 1f);
    }

    private void HandleTrialResult(bool correct, int points, float rt) {
        // small visual feedback: flash sprite color
        if(correct) StartCoroutine(FlashColor(symbolImage, new Color(0.8f, 1f, 0.8f), 0.18f));
        else StartCoroutine(FlashColor(symbolImage, new Color(1f, 0.8f, 0.8f), 0.18f));

        UpdateScore(logic.TotalScore);
        // update multiplier text using a safe read (best-effort)
        UpdateMultiplierText(logic.enableMultiplier ? 1f + Mathf.Max(0, logic.TotalCorrect - 1) * logic.multiplierIncrement : 1f);

        // reset round start time if it was zero (for timer)
        if(roundStartRealtime == 0f) roundStartRealtime = Time.realtimeSinceStartup;
    }

    private void HandleRoundComplete(int roundIndex, int roundCorrect, float timeTaken, int roundScore, int roundTotalTrials) {
        roundResultPanel.SetActive(true);
        roundResultText.text = $"Round {roundIndex} Complete\nCorrect: {roundCorrect}/{roundTotalTrials}\nTime: {timeTaken:F2}s\nRound Score: {roundScore}";
        roundStartRealtime = 0f;
    }

    private void HandleGameComplete(int totalScore, int totalCorrect, float totalTime) {
        finalResultPanel.SetActive(true);
        finalResultText.text = $"Game Complete\nScore: {totalScore}\nCorrect: {totalCorrect}\nTime: {totalTime:F2}s";
        roundStartRealtime = 0f;
    }

    private void HandleScoreUpdated(int totalScore) {
        UpdateScore(totalScore);
    }

    // UI utilities
    private void UpdateScore(int score) {
        if(scoreText != null) scoreText.text = $"Score: {score}";
    }

    private void UpdateMultiplierText(float multiplier) {
        if(multiplierText != null) multiplierText.text = $"x{multiplier:F2}";
    }

    private void UpdateTimer(float t) {
        if(timerText != null) timerText.text = $"T: {t:F2}s";
    }

    private IEnumerator FlashColor(Image img, Color flashColor, float duration) {
        if(img == null) yield break;
        Color original = img.color;
        img.color = flashColor;
        yield return new WaitForSecondsRealtime(duration);
        img.color = original;
    }

    private void TogglePause() {
        logic.PauseToggle();
    }
}
