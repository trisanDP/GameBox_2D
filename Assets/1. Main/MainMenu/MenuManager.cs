using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#region --- MenuManager.cs ---
public class MenuManager : MonoBehaviour {
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject gameSelectPanel;
    public GameObject optionsPanel;
    public GameObject scorePanel;

    // Footer Menu Buttons
    [Header("FooterMenu Buttons")]
    public Button Home;
    public Button Games;
    public Button Option;
    public Button ScoreButton;

    //Home Menu Buttons
    [Header("Home Menu Buttons")]
    public Button LetsPlay;

    // Score Panel
/*    [Header("Score Panel")]
    public RectTransform contentParent;
    public GameObject scoreItemPrefab;
    public Button DeleteScore;
    public Button ScoreBackBTN;
*/
    // Option Panel
    [Header("Option Panel")]
    public Button ResetProgress;
    public Button OptionBack;

    // Game Selection
    [Header("Game Selection")]
    public Button KoiGameButton;
    public Button NumberGameButton;
    public Button ColorClashGameButton;

/*    public Button GameSelectionBackButton;*/

    private readonly List<string> gameNames = new() { "KoiGame", "NumberGame", "ColorClash" };

    void Start() {
        // init panels
        mainPanel.SetActive(true);
        gameSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);
        scorePanel.SetActive(false);

        // Footer Menu Buttons
        Home.onClick.AddListener(OnHomeClicked);
        Games.onClick.AddListener(OnGameListClicked);
        Option.onClick.AddListener(OnOptionsClicked);
        ScoreButton.onClick.AddListener(OnScoreClicked);

        // Home Menu Buttons
        LetsPlay.onClick.AddListener(() => OnGameListClicked());

        ResetProgress.onClick.AddListener(() => {
            KoiLevelManager.Instance.ResetProgress();
            Debug.Log("[MenuManager] Progress reset");
        });

        // scores
        ScoreButton.onClick.AddListener(OnScoreClicked);
/*        DeleteScore.onClick.AddListener(OnDeleteScores);
*//*        ScoreBackBTN.onClick.AddListener(OnScoreBack);*/

        // game selection
        KoiGameButton.onClick.AddListener(() => LoadScene("KoiGame_Attention"));
        NumberGameButton.onClick.AddListener(() => LoadScene("NumberGame_Memory"));
        ColorClashGameButton.onClick.AddListener(() => LoadScene("ColorClash_Inhivitory Control"));
/*        GameSelectionBackButton.onClick.AddListener(OnGameSelectBack);*/
    }



    #region FooterButtons Functions
    void OnHomeClicked() {
        CloseAllPanels();
        mainPanel.SetActive(true);
    }

    private void OnGameListClicked() {
        CloseAllPanels();
        gameSelectPanel.SetActive(true);
    }
    private void OnScoreClicked() {
        CloseAllPanels();
        scorePanel.SetActive(true);
    }
    private void OnOptionsClicked() {
        CloseAllPanels();
        optionsPanel.SetActive(true);
    }

    void CloseAllPanels() {
        mainPanel.SetActive(false);
        gameSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);
        scorePanel.SetActive(false);
    }


    #endregion

    #region ScorePanel


    #endregion


    #region Hold

    /*private void OnQuitClicked() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    private void OnGameSelectBack() {
        gameSelectPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    private void OnOptionsBack() {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
*/
    #endregion
    private void LoadScene(string name) => SceneManager.LoadScene(name);
}
#endregion