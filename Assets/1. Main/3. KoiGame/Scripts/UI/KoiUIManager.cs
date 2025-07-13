// ===== KoiUIManager.cs =====
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class KoiUIManager : MonoBehaviour {
    public static KoiUIManager Instance { get; private set; }

    [Header("In‑Game UI")]
    public Slider cooldownSlider;
    public TextMeshProUGUI wrongFeedText;
    public TextMeshProUGUI remainingText;
    public TextMeshProUGUI timerText;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Button retryButton;
    public Button gameOverMenuButton;

    [Header("Pause")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button pauseResumeButton;
    public Button pauseMainMenuButton;

    [Header("Level Complete Panel")]
    public GameObject levelCompletePanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public Button nextLevelButton;
    public Button completeMenuButton;

    void Awake() {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);

    }

    void Start() {
        HideUI();

        pauseButton.onClick.AddListener(PauseGame);
        pauseResumeButton.onClick.AddListener(ResumeGame);
        pauseMainMenuButton.onClick.AddListener(KoiGameManager.Instance.ReturnToMenu);

    }

    public void HideUI() {
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        levelCompletePanel.SetActive(false);
    }

    public void InitializeUI(int threshold, int totalEntities) {
        HideUI();
        UpdateWrongFeeds(0);
        UpdateRemaining(totalEntities);
        retryButton.onClick.AddListener(KoiGameManager.Instance.RetryLevel);
        gameOverMenuButton.onClick.AddListener(KoiGameManager.Instance.ReturnToMenu);
        nextLevelButton.onClick.AddListener(KoiGameManager.Instance.NextLevel);
        completeMenuButton.onClick.AddListener(KoiGameManager.Instance.ReturnToMenu);
    }

    public void StartCooldown(float duration) {
        StartCoroutine(CooldownRoutine(duration));
    }

    private IEnumerator CooldownRoutine(float duration) {
        cooldownSlider.value = 0f;
        while(cooldownSlider.value < 1f) {
            cooldownSlider.value += Time.deltaTime / duration;
            yield return null;
        }
    }

    public void UpdateWrongFeeds(int wrongCount) {
        wrongFeedText.text = $"Wrong: {wrongCount}";
    }

    public void UpdateRemaining(int remaining) {
        remainingText.text = $"Remaining {remaining}";
    }

    public void UpdateTimer(float timeLeft) {
        timerText.text = $"Time: {(int)timeLeft:0}s";
    }

    public void ShowGameOver() {
        gameOverPanel.SetActive(true);
    }

    public void ShowLevelComplete(int fedCount, int totalEntities, float timeLeft) {
        levelCompletePanel.SetActive(true);
        scoreText.text = $"Fed: {fedCount}/{totalEntities}";
        timeText.text = $"Time Left: {timeLeft:0.0}s";
    }

    #region Pause Menu

    public void ShowPauseMenu() {
        pausePanel.SetActive(true);
        pauseButton.gameObject.SetActive(false);
        pauseResumeButton.onClick.AddListener(ResumeGame);
        pauseMainMenuButton.onClick.AddListener(KoiGameManager.Instance.ReturnToMenu);
    }
    public void HidePauseMenu() {
        pausePanel.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }
    public void PauseGame() {
        Time.timeScale = 0f;
        ShowPauseMenu();
    }
    public void ResumeGame() {
        Time.timeScale = 1f;
        HidePauseMenu();
    }
    #endregion
}