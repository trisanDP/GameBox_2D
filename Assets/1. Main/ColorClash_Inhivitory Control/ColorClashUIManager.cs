// ----------------------------------------
// ColorClashUIManager: UI & Prefab Generation
// ----------------------------------------
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ColorClashUIManager : MonoBehaviour {
    [Header("UI References")]
    public GameObject countdownPanel;
    public GameObject roundPanel;


    [Header("Game UI")]
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI timerText;



    public TextMeshProUGUI finalScoreText;
    public Transform buttonContainer;
    public Button buttonPrefab;

    [Header("PauseMenu")]
    public GameObject pausePanel;
    public Button pause_MainMenu;
    public Button pause;
    public Button pause_Resume;

    [Header("GameOver Menu")]
    public GameObject gameOverPanel;
    public Button tryAgainButton;
    public Button mainMenuButton;

    [Header("Game Settings")]
    public List<string> colorNames;
    public TextMeshProUGUI roundText;
    public RectTransform textSpawnArea;       // Area where text appears

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctClip;
    public AudioClip incorrectClip;

    private List<Color> colorValues;

    void Start() {
        // Map names to colors
        colorValues = new List<Color>();
        foreach(var name in colorNames) {
            if(ColorUtility.TryParseHtmlString(name, out Color c)) colorValues.Add(c);
            else colorValues.Add(GetFromPredefined(name));
        }
        roundPanel.SetActive(false);    
        ClearButtons();
        // Instantiate buttons
        for(int idx = 0; idx < colorNames.Count; idx++) {
            var btn = Instantiate(buttonPrefab, buttonContainer);
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log("Calling");
            if(txt == null) {
                Debug.LogError("Button prefab must have a TextMeshProUGUI component in children!");
                continue;
            }
            txt.text = colorNames[idx];
            Color value = colorValues[idx];
            btn.onClick.AddListener(() => OnButtonPressed(value));
        }

        // Subscribe to game events
        var gm = ColorClashGameManager.Instance;
        gm.OnRoundGenerated += HandleRound;

        gm.OnScoreUpdated += s => pointsText.text = $"Points: {s}";
        gm.OnTimerUpdated += t => timerText.text = $"{t:F1}s";
        gm.OnGameOver += HandleGameOver;

        // Start countdown then game
        StartCoroutine(ShowCountdownThenStart());
        pause_MainMenu.onClick.AddListener(ReturnToMenu);
        pause_Resume.onClick.AddListener(ResumeGame);
        pause.onClick.AddListener(PauseGame);
        tryAgainButton.onClick.AddListener(TryAgain);
        mainMenuButton.onClick.AddListener(ReturnToMenu);
    }

    void ClearButtons() {
        foreach(Transform child in buttonContainer) {
            Destroy(child.gameObject);
        }
    }
    private IEnumerator ShowCountdownThenStart() {
        roundPanel.SetActive(false);
        countdownPanel.SetActive(true);

        float count = 3f;
        while(count > 0f) {
            countdownText.text = $"{count:F0}";
            yield return new WaitForSeconds(1f);
            count -= 1f;
        }
        countdownPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        roundPanel.SetActive(true);
        ColorClashGameManager.Instance.StartGame(colorNames, colorValues);
    }

    private void HandleRound(string word, Color fontColor) {
        roundText.text = word;
        roundText.color = fontColor;

        // This is the "spawn" logic 
        if(textSpawnArea != null) {
            var area = textSpawnArea.rect;
            roundText.gameObject.SetActive(true);
        }
    }


    private void OnButtonPressed(Color c) {
        // Submit answer and play sound based on correctness
        bool correct = c == roundText.color;
        ColorClashGameManager.Instance.SubmitAnswer(c);
        audioSource.PlayOneShot(correct ? correctClip : incorrectClip);
    }

    private void HandleGameOver(int finalScore) {
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Score: " + finalScore.ToString();
        // Save score
        var entry = new ColorClashScoreEntry { finalScore = finalScore, timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
        GlobalScoreManager.Instance.AddScore("ColorClash", entry);
    }

    private Color GetFromPredefined(string name) {
        switch(name.ToLower()) {
            case "red": return Color.red;
            case "blue": return Color.blue;
            case "green": return Color.green;
            case "yellow": return Color.yellow;
            case "black": return Color.black;
            case "white": return Color.white;
            default: return Color.gray;
        }
    }

    #region Pause Menu
    public void PauseGame() {
        Time.timeScale = 0f; // Pause the game
        pausePanel.SetActive(true); // Show pause menu
    }

    public void ResumeGame() {
        Time.timeScale = 1f; // Resume the game
        pausePanel.SetActive(false); // Hide pause menu
    }
    public void ReturnToMenu() {
        Debug.Log("Returning to Main Menu");
        SceneManager.LoadScene("MainMenu");
    }

    public void TryAgain() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion
}
