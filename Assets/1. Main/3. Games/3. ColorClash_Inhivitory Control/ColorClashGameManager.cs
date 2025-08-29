using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ----------------------------------------
// ColorClashGameManager: Game Logic Only
// ----------------------------------------
public class ColorClashGameManager : MonoBehaviour {
    #region Singleton
    public static ColorClashGameManager Instance { get; private set; }
    private void Awake() {
        Time.timeScale = 1f;
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region Settings
    public float baseGameTime = 30f;
    public int pointsPerCorrect = 10;
    public int pointsPerWrong = -3;
    #endregion

    #region Runtime State
    private float remainingTime;
    private int points;
    private string currentWord;
    private Color currentColor;
    private int difficultyLevel = 1;
    #endregion

    #region Events
    public event Action<string, Color> OnRoundGenerated;
    public event Action<int> OnScoreUpdated;
    public event Action<float> OnTimerUpdated;
    public event Action<int> OnGameOver;
    #endregion

    #region Public API
    public void StartGame(List<string> colorNames, List<Color> colorValues) {
        points = 0;
        remainingTime = baseGameTime + (difficultyLevel - 1) * 15f;
        OnScoreUpdated?.Invoke(points);
        OnTimerUpdated?.Invoke(remainingTime);
        OnRoundGenerated?.Invoke(currentWord, currentColor);

        GenerateRound(colorNames, colorValues);
    }

    public void SubmitAnswer(Color selected) {
        bool correct = selected == currentColor;
        points += correct ? pointsPerCorrect : pointsPerWrong;
        OnScoreUpdated?.Invoke(points);
        GenerateNext();
    }

    public void SetDifficulty(int level) {
        difficultyLevel = level;
    }

    #endregion

    #region Internal Flow
    private List<string> names;
    private List<Color> values;

    private void GenerateRound(List<string> colorNames, List<Color> colorValues) {
        names = colorNames;
        values = colorValues;
        GenerateNext();
        // Start timer coroutine
        StopAllCoroutines();
        StartCoroutine(TimerRoutine());
    }

    private void GenerateNext() {
        int idxWord = UnityEngine.Random.Range(0, names.Count);
        int idxColor = UnityEngine.Random.Range(0, values.Count);
        currentWord = names[idxWord];
        currentColor = values[idxColor];
        OnRoundGenerated?.Invoke(currentWord, currentColor);
    }

    private IEnumerator TimerRoutine() {
        while(remainingTime > 0f) {
            yield return null;
            remainingTime -= Time.deltaTime;
            OnTimerUpdated?.Invoke(remainingTime);
        }
        EndGame();
    }

    private void EndGame() {
        StopAllCoroutines();
        OnGameOver?.Invoke(points);
    }
    #endregion
}
