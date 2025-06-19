using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject gameSelectPanel;
    public GameObject optionsPanel;

    #region Menu Buttons
    public Button Games;
    public Button Option;
    public Button Quit;
    #endregion

    #region Options Panel Buttons   
    public Button OptionBack; // For going back to the main menu from game selection or options
    #endregion

    #region Game Selection Buttons

    public Button KoiGameButton;
    public Button GameSelectionoBackButton;

    #endregion

    // === Main Panel Buttons ===
    private void Start() {
        // Ensure only the main panel is active at the start
        mainPanel.SetActive(true);
        gameSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);

        Games.onClick.AddListener(OnPlayClicked);
        Option.onClick.AddListener(OnOptionsClicked);  
        Quit.onClick.AddListener(OnQuitClicked);

        OptionBack.onClick.AddListener(OnOptionsBack);

        KoiGameButton.onClick.AddListener(OnKoiGameSelected);
        GameSelectionoBackButton.onClick.AddListener(OnGameSelectBack);
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
        SceneManager.LoadScene("KoiGame");
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
