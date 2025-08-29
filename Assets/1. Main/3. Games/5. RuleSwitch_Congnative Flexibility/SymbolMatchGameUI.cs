using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SymbolMatchGameUI : MonoBehaviour {
    [Header("Guide Panel")]
    public GameObject guidePanel;
    public Button guideOkButton;

    [Header("Countdown Panel")]
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;
    public int countdownStart = 3;

    [Header("Game Panel (Symbol Display)")]
    public GameObject gamePanel;
    public Image symbolImage;
    public Sprite[] symbolSprites = new Sprite[3];

    [Header("Game Panel (Buttons)")]
    public Button tickButton;
    public Button crossButton;
    public Button pauseButton;

    [Header("Pause Panel")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseExitButton;

    [Header("Round Result Panel")]
    public GameObject roundResultPanel;
    public TextMeshProUGUI roundResultText;
    public Button roundResultNextButton;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverTimeText;
    public Button gameOverMainMenuButton;
    public Button gameOverPlayNextButton;

    [Header("HUD Elements")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI timerText;

    [Header("Fallback")]
    public TextMeshProUGUI symbolFallbackText;

    public void ShowGuide(bool v) { if(guidePanel != null) guidePanel.SetActive(v); }
    public void ShowCountdown(bool v) { if(countdownPanel != null) countdownPanel.SetActive(v); }
    public void ShowGamePanel(bool v) { if(gamePanel != null) gamePanel.SetActive(v); }
    public void ShowPausePanel(bool v) { if(pausePanel != null) pausePanel.SetActive(v); }
    public void ShowRoundResultPanel(bool v) { if(roundResultPanel != null) roundResultPanel.SetActive(v); }
    public void ShowGameOverPanel(bool v) { if(gameOverPanel != null) gameOverPanel.SetActive(v); }

    public void SetCountdownText(string txt) { if(countdownText != null) countdownText.text = txt; }

    public void SetSymbol(SymbolType symbol, bool isFirst) {
        int idx = (int)symbol;
        if(symbolSprites != null && idx >= 0 && idx < symbolSprites.Length && symbolSprites[idx] != null) {
            if(symbolImage != null) {
                symbolImage.sprite = symbolSprites[idx];
                symbolImage.color = isFirst ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
            }
        } else if(symbolFallbackText != null) {
            symbolFallbackText.text = symbol.ToString();
        }
    }

    public void UpdateScore(int score) { if(scoreText != null) scoreText.text = $"Score: {score}"; }
    public void UpdateMultiplier(float mult) { if(multiplierText != null) multiplierText.text = $"x{mult:F2}"; }
    public void UpdateRound(int roundIndex) { if(roundText != null) roundText.text = $"Round {roundIndex}"; }
    public void UpdateTimer(float seconds) { if(timerText != null) timerText.text = $"T: {seconds:F2}s"; }

    public void ShowRoundResult(int roundIndex, int correct, int total, int roundScore, float timeTaken) {
        if(roundResultPanel == null || roundResultText == null) return;
        roundResultPanel.SetActive(true);
        roundResultText.text = $"Round {roundIndex} Complete\nCorrect: {correct}/{total}\nTime: {timeTaken:F2}s\nRound Score: {roundScore}";
    }

    public void ShowFinalResult(int totalScore, int totalCorrect, float totalTime) {
        if(gameOverPanel == null) return;
        gameOverPanel.SetActive(true);
        if(gameOverScoreText != null) gameOverScoreText.text = $"Score: {totalScore}\nCorrect: {totalCorrect}";
        if(gameOverTimeText != null) gameOverTimeText.text = $"Time: {totalTime:F2}s";
    }

    private void Reset() {
        if(guidePanel != null) guidePanel.SetActive(false);
        if(countdownPanel != null) countdownPanel.SetActive(false);
        if(gamePanel != null) gamePanel.SetActive(false);
        if(pausePanel != null) pausePanel.SetActive(false);
        if(roundResultPanel != null) roundResultPanel.SetActive(false);
        if(gameOverPanel != null) gameOverPanel.SetActive(false);
    }
}
