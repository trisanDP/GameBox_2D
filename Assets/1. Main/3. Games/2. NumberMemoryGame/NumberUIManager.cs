// NumberUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NumberUIManager : MonoBehaviour {
    public TMP_Text levelText;
    public TMP_Text feedbackText;
    public GameObject gameOverPanel;
    public GameObject nextLevelPanel;
    public GameObject pausePanel;
    public Button pauseButton;
    public Button retryButton;
    public Button menuButton_GameOver;
    public Button nextButton;
    public Button menuButton_Next;
    public Button menuButton_Pause;
    public Button unpauseButton;
    public TMP_Text timeText;
    public event Action onPauseRequested;

    public GameObject guidePanel;
    public Button CloseButton;


    void Start() {
        CloseButton.onClick.AddListener(OnCloseGuideButton);
        pauseButton.onClick.AddListener(() => onPauseRequested?.Invoke());
        retryButton.onClick.AddListener(() => { HideAllPanels(); FindFirstObjectByType<NumberGameManager>().OnRetry(); });
        menuButton_GameOver.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
        nextButton.onClick.AddListener(() => { HideAllPanels(); FindFirstObjectByType<NumberGameManager>().OnNextLevel(); });
        menuButton_Next.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
        menuButton_Pause.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
        unpauseButton.onClick.AddListener(() => FindFirstObjectByType<NumberGameManager>().UnpauseGame());
        HideAllPanels();
        guidePanel.SetActive(true);
    }

    void OnCloseGuideButton() {
        guidePanel.SetActive(false);
        FindFirstObjectByType<NumberGameManager>().StartRound();
    }

    public void SetLevel(int level) => levelText.text = "Level: " + level;
    public void ShowFeedback(string msg) { feedbackText.text = msg; feedbackText.gameObject.SetActive(true); Invoke("HideFeedback", 2f); }
    void HideFeedback() => feedbackText.gameObject.SetActive(false);
    public void ShowGameOverPanel() { gameOverPanel.SetActive(true); }
    public void ShowNextLevelPanel(float timeTaken) {
        timeText.text = "Time: " + timeTaken.ToString("F1") + "s";
        nextLevelPanel.SetActive(true);
    }
    public void ShowPausePanel() => pausePanel.SetActive(true);
    public void HidePausePanel() => pausePanel.SetActive(false);
    public void HideAllPanels() {
        feedbackText.gameObject.SetActive(false);
        gameOverPanel.SetActive(false);
        nextLevelPanel.SetActive(false);
        pausePanel.SetActive(false);
    }
}
