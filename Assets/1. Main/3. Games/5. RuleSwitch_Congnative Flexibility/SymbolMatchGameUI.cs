using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SymbolMatchGameUI : MonoBehaviour {
    #region --- Guide Panel ---
    [Header("Guide Panel")]
    [Tooltip("Panel with instructions that appears at scene start")]
    public GameObject guidePanel;
    [Tooltip("Button inside guide panel that starts countdown")]
    public Button guideOkButton;
    #endregion

    #region --- Countdown Panel ---
    [Header("Countdown Panel")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;
    [Tooltip("Countdown start value (3 => 3,2,1)")]
    public int countdownStart = 3;
    #endregion

    #region --- Game Panel (symbol + controls) ---
    [Header("Game Panel (Symbol Display)")]
    public GameObject gamePanel;
    [Tooltip("Image showing the current symbol")]
    public Image symbolImage;
    [Tooltip("Sprites in order: 0 = Circle, 1 = Triangle, 2 = Rectangle")]
    public Sprite[] symbolSprites = new Sprite[3];

    [Header("Game Panel (Buttons)")]
    public Button tickButton;   // ✓ (Yes / Same)
    public Button crossButton;  // ✗ (No / Different)
    public Button pauseButton;  // pause overlay trigger
    #endregion

    #region --- Pause Panel ---
    [Header("Pause Panel")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseExitButton; // exit to menu from pause
    #endregion

    #region --- Game Over Panel ---
    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverTimeText;
    public Button gameOverMainMenuButton;
    public Button gameOverPlayNextButton;
    #endregion

    #region --- HUD (shared) ---
    [Header("HUD Elements")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI timerText;
    #endregion

    #region --- Optional fallback (text) ---
    [Header("Fallback")]
    public TextMeshProUGUI symbolFallbackText;
    #endregion

    #region --- Public helper API (called by Manager / Logic) ---
    public void ShowGuide(bool v) => GuidePanelActive(v);
    public void ShowCountdown(bool v) => CountdownPanelActive(v);
    public void ShowGamePanel(bool v) => GamePanelActive(v);
    public void ShowPausePanel(bool v) => PausePanelActive(v);
    public void ShowGameOverPanel(bool v) => GameOverPanelActive(v);

    public void SetCountdownText(string txt) {
        if(countdownText != null) countdownText.text = txt;
    }

    public void SetSymbol(SymbolType symbol, bool isFirst) {
        int idx = (int)symbol;
        if(symbolSprites != null && idx >= 0 && idx < symbolSprites.Length && symbolSprites[idx] != null) {
            symbolImage.sprite = symbolSprites[idx];
            symbolImage.color = isFirst ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
        } else if(symbolFallbackText != null) {
            symbolFallbackText.text = symbol.ToString();
        }
    }

    public void UpdateScore(int score) {
        if(scoreText != null) scoreText.text = $"Score: {score}";
    }

    public void UpdateMultiplier(float mult) {
        if(multiplierText != null) multiplierText.text = $"x{mult:F2}";
    }

    public void UpdateRound(int roundIndex) {
        if(roundText != null) roundText.text = $"Round {roundIndex}";
    }

    public void UpdateTimer(float seconds) {
        if(timerText != null) timerText.text = $"T: {seconds:F2}s";
    }

    public void ShowRoundResult(int roundIndex, int correct, int total, int roundScore, float timeTaken) {
        if(gameOverPanel == null) return;
        // If you prefer a dedicated round panel, implement it here. For now we reuse gameOverPanel for simplicity:
        ShowGameOverPanel(true);
        if(gameOverScoreText != null) gameOverScoreText.text = $"Round {roundIndex} Score: {roundScore}\nCorrect: {correct}/{total}";
        if(gameOverTimeText != null) gameOverTimeText.text = $"Time: {timeTaken:F2}s";
    }

    public void ShowFinalResult(int totalScore, int totalCorrect, float totalTime) {
        ShowGameOverPanel(true);
        if(gameOverScoreText != null) gameOverScoreText.text = $"Score: {totalScore}\nCorrect: {totalCorrect}";
        if(gameOverTimeText != null) gameOverTimeText.text = $"Time: {totalTime:F2}s";
    }
    #endregion

    #region --- Small internal helpers (visibility) ---
    private void GuidePanelActive(bool v) {
        if(guidePanel != null) guidePanel.SetActive(v);
    }
    private void CountdownPanelActive(bool v) {
        if(countdownPanel != null) countdownPanel.SetActive(v);
    }
    private void GamePanelActive(bool v) {
        if(gamePanel != null) gamePanel.SetActive(v);
    }
    private void PausePanelActive(bool v) {
        if(pausePanel != null) pausePanel.SetActive(v);
    }
    private void GameOverPanelActive(bool v) {
        if(gameOverPanel != null) gameOverPanel.SetActive(v);
    }
    #endregion

    #region --- Editor-time safety ---
    private void Reset() {
        // keep panels off by default in editor play
        if(guidePanel != null) guidePanel.SetActive(false);
        if(countdownPanel != null) countdownPanel.SetActive(false);
        if(gamePanel != null) gamePanel.SetActive(false);
        if(pausePanel != null) pausePanel.SetActive(false);
        if(gameOverPanel != null) gameOverPanel.SetActive(false);
    }
    #endregion
}
