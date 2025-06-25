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
    }

    public void HideUI() {
        gameOverPanel.SetActive(false);
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
        remainingText.text = $"Remaining: {remaining}";
    }

    public void UpdateTimer(float timeLeft) {
        timerText.text = $"Time: {timeLeft:0.0}s";
    }

    public void ShowGameOver() {
        gameOverPanel.SetActive(true);
    }

    public void ShowLevelComplete(int fedCount, int totalEntities, float timeLeft) {
        levelCompletePanel.SetActive(true);
        scoreText.text = $"Fed: {fedCount}/{totalEntities}";
        timeText.text = $"Time Left: {timeLeft:0.0}s";
    }
}