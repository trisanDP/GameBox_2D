// FastMathUIManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FastMathUIManager : MonoBehaviour {
    public static FastMathUIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject tutorialPanel;
    public GameObject countdownPanel;
    public GameObject gamePanel;
    public GameObject questionPanel;
    public GameObject nextLevelPanel;
    public GameObject retryPanel;

    [Header("UI Elements")]
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI numberDisplayText;
    public TextMeshProUGUI questionText;
    public Transform answersContainer;
    public Button answerButtonPrefab;
    public TextMeshProUGUI resultTextNext;
    public TextMeshProUGUI resultTextRetry;
    public Button nextLevelButton;
    public Button retryButton;
    public Button mainMenu;
    public Button MainMenuReady;

    [Header("Tutorial")]
    public Button tutorialOk;

    private FastMathLogic logic;

    void Awake() {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() {
        HideAllPanel();
        logic = FindFirstObjectByType<FastMathLogic>();
        logic.NumbersComplete += OnNumbersComplete;
        nextLevelButton.onClick.AddListener(() => OnNextLevel());
        retryButton.onClick.AddListener(() => OnRetryLevel());
        tutorialOk.onClick.AddListener(() => OnTutorialOk());
        mainMenu.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
        MainMenuReady.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
        ShowTutorial();
    }

    public void HideAllPanel() {
        questionPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        countdownPanel.SetActive(false);
        gamePanel.SetActive(false);
        nextLevelPanel.SetActive(false);
        retryPanel.SetActive(false);
    }
    private void ShowTutorial() {
        tutorialPanel.SetActive(true);
    }

    public void OnTutorialOk() {
        tutorialPanel.SetActive(false);
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown() {
        countdownPanel.SetActive(true);
        for(int i = 3; i > 0; i--) {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        gamePanel.SetActive(true);
        countdownPanel.SetActive(false);
        logic.StartLevel(0);

    }

    public void ShowNumber(int value) {
        numberDisplayText.text = value.ToString();
    }

    private void OnNumbersComplete(int correctAnswer, List<int> choices, float answerDuration) {
        questionText.text = "What is the addition of all numbers?";
        foreach(Transform child in answersContainer) Destroy(child.gameObject);
        foreach(int choice in choices) {
            Debug.Log("Count1");
            var btn = Instantiate(answerButtonPrefab, answersContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.ToString();
            btn.onClick.AddListener(() => logic.SubmitAnswer(choice));
        }
        gamePanel.SetActive(true);
        // Optionally start a timer to enforce answerDuration
    }

    public void ShowResult(bool correct, int pointsAwarded) {
        gamePanel.SetActive(false);
        if(correct) {
            resultTextNext.text = $"Correct! +{pointsAwarded} pts";
            nextLevelPanel.SetActive(true);
        } else {
            resultTextRetry.text = "Incorrect! Try again.";
            retryPanel.SetActive(true);
        }
    }

    private void OnNextLevel() {
        HideAllPanel();
        gamePanel.SetActive(true);
        if(logic.CurrentLevel >= logic.levels.Count - 1) {
            // If it's the last level, show a message or reset
            Debug.Log("Last level reached. Resetting to first level.");
            logic.StartLevel(0);
            return;
        }
        logic.StartLevel(logic.CurrentLevel + 1);
    }

    private void OnRetryLevel() {
        HideAllPanel();
        logic.StartLevel(logic.CurrentLevel);
        gamePanel.SetActive(true);
    }
}