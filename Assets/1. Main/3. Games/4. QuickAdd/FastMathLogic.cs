// FastMathLevelSettings.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class FastMathLevelSettings {
    [Tooltip("How many numbers to display this level")] public int numberCount;
    [Tooltip("Total time (in seconds) to display all numbers")] public float displayDuration;
    [Tooltip("Time (in seconds) allowed to choose answer")] public float answerDuration;
    [Tooltip("Points awarded for correct answer on this level")] public int pointsOnCorrect;
    [Tooltip("Minimum value for generated numbers")] public int minValue;
    [Tooltip("Maximum value for generated numbers")] public int maxValue;
}




public class FastMathLogic : MonoBehaviour {
    [SerializeField] public List<FastMathLevelSettings> levels;
    private int currentLevel = 0;
    private List<int> displayedNumbers = new List<int>();
    private int sum = 0;

    public int CurrentLevel => currentLevel;

    public delegate void OnNumbersComplete(int correctAnswer, List<int> choices, float answerDuration);
    public event OnNumbersComplete NumbersComplete;

    private Coroutine displayCoroutine;

    public void StartLevel(int levelIndex) {
        if(levelIndex < 0 || levelIndex >= levels.Count) {
            Debug.LogError("Invalid level index");
            return;
        }
        currentLevel = levelIndex;
        displayedNumbers.Clear();
        sum = 0;
        if(displayCoroutine != null) StopCoroutine(displayCoroutine);
        displayCoroutine = StartCoroutine(DisplayNumbersCoroutine(levels[levelIndex]));
    }

    private IEnumerator DisplayNumbersCoroutine(FastMathLevelSettings settings) {
        Debug.Log("Take1");
        float interval = settings.displayDuration / settings.numberCount;
        for(int i = 0; i < settings.numberCount; i++) {
            int value = UnityEngine.Random.Range(settings.minValue, settings.maxValue + 1);
            displayedNumbers.Add(value);
            sum += value;
            FastMathUIManager.Instance.ShowNumber(value);
            yield return new WaitForSeconds(interval);
        }
        GenerateChoicesAndNotify();
    }

    private void GenerateChoicesAndNotify() {
        var settings = levels[currentLevel];
        List<int> choices = new List<int> { sum };
        while(choices.Count < 4) {
            int delta = UnityEngine.Random.Range(-settings.maxValue, settings.maxValue);
            int distractor = sum + delta;
            if(distractor != sum && !choices.Contains(distractor)) choices.Add(distractor);
        }
        // Shuffle
        for(int i = 0; i < choices.Count; i++) {
            int j = UnityEngine.Random.Range(i, choices.Count);
            var tmp = choices[i]; choices[i] = choices[j]; choices[j] = tmp;
        }
        NumbersComplete?.Invoke(sum, choices, settings.answerDuration);

        //UI
        FastMathUIManager.Instance.HideAllPanel();
        FastMathUIManager.Instance.questionPanel.SetActive(true);
    }

    public void SubmitAnswer(int answer) {
        var settings = levels[currentLevel];
        bool correct = (answer == sum);
        if(correct) {
            var entry = new FastMathScoreEntry {
                levelIndex = currentLevel,
                earnedPoints = settings.pointsOnCorrect
            };
            GlobalScoreManager.Instance.AddScore("FastMath", entry);
        }
        FastMathUIManager.Instance.ShowResult(correct, correct ? levels[currentLevel].pointsOnCorrect : 0);
    }
}