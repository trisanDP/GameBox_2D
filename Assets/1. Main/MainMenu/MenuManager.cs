using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject gameSelectPanel;
    public GameObject optionsPanel;
    public GameObject scorePanel; // UI panel to show scores

    #region Menu Buttons
    [Header("Menu Buttons")]
    public Button Games;
    public Button Option;
    public Button Quit;
    public Button ScoreButton;
    #endregion

    #region Score Panel
    [Header("Score Panel")]
    public RectTransform contentParent;
    public GameObject scoreItemPrefab; // Simple UI prefab with TextMeshProUGUI
    public Button DeleteScore; // Button to delete scores
    public Button ScoreBackBTN; // Button to go back from score panel to main menu
    #endregion

    #region Options Panel Buttons   
    [Header("Option Panel")]
    public Button ResetProgress;
    public Button OptionBack; // For going back to the main menu from game selection or options
    #endregion

    #region Game Selection Buttons
    [Header("GameSelection Panel")]
    public Button KoiGameButton;
    public Button NumberGameButton;
    public Button GameSelectionoBackButton;

    #endregion

    // === Main Panel Buttons ===
    private void Start() {
        // Ensure only the main panel is active at the start
        mainPanel.SetActive(true);
        gameSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);
        scorePanel.SetActive(false); // Start with panel hidden

        Games.onClick.AddListener(OnPlayClicked);
        Option.onClick.AddListener(OnOptionsClicked);  
        Quit.onClick.AddListener(OnQuitClicked);

        OptionBack.onClick.AddListener(OnOptionsBack);
        ResetProgress.onClick.AddListener(() => {
            KoiLevelManager.Instance.ResetProgress();
            Debug.Log("[MenuManager] Progress reset to level 0");
        });

        ScoreButton.onClick.AddListener(OnScoreClicked);
        DeleteScore.onClick.AddListener(DeleteScores); // Assign delete button functionality
        ScoreBackBTN.onClick.AddListener(OnScoreBack);

        KoiGameButton.onClick.AddListener(OnKoiGameSelected);
        NumberGameButton.onClick.AddListener(OnMemoryGameSelected);
        GameSelectionoBackButton.onClick.AddListener(OnGameSelectBack);
    }

    void OnScoreClicked() {
        mainPanel.SetActive(false);
        scorePanel.SetActive(true);
        PopulateScores(); // Populate scores when panel is opened
        if(scorePanel == null) {
            Debug.LogError("Score panel is not assigned in the MenuManager!");
            return;
        }
    }
    public void PopulateScores() {
        List<ScoreEntry> scores = ScoreManager.Instance.GetScores();
        foreach(var entry in scores) {
            if(entry == null) {
                Debug.LogWarning("Null score entry found! Skipping.");
                continue;
            }
            var go = Instantiate(scoreItemPrefab, contentParent);
            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
            if(text == null) {
                Debug.LogError("Score item prefab must have a TextMeshProUGUI component!");
                continue;
            }
            text.text = $"[{entry.timestamp}] {entry.gameName} L{entry.levelIndex}: {entry.fedCount}/{entry.totalEntities} fed, {entry.timeLeft:0.0}s left";
        }
    }

    private void DeleteScores() {
        ScoreManager.Instance.ClearScores();
        // also remove the UI elements
        foreach(Transform child in contentParent) {
            Destroy(child.gameObject);
        }
        PopulateScores(); // Refresh the score list
    }

    private void OnScoreBack() {
        scorePanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    private void OnPlayClicked() {
        mainPanel.SetActive(false);
        gameSelectPanel.SetActive(true);
    }

    private void OnOptionsClicked() {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    private void OnQuitClicked() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // === Game Selection Panel ===
    public void OnKoiGameSelected() {
        SceneManager.LoadScene("KoiGame_Attention");
    }

    public void OnMemoryGameSelected() {
        SceneManager.LoadScene("NumberGame_Memory");
    }

    public void OnGameSelectBack() {
        gameSelectPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // === Options Panel ===
    public void OnOptionsBack() {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

}
