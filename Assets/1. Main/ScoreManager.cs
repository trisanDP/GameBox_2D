using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScoreEntry {
    public string gameName;
    public int levelIndex;
    public int fedCount;
    public int totalEntities;
    public float timeLeft;
    public string timestamp;
}

public class ScoreManager : MonoBehaviour {
    public static ScoreManager Instance { get; private set; }
    private List<ScoreEntry> scores = new List<ScoreEntry>();
    private const string Key = "SavedScores";

    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScores();
        } else Destroy(gameObject);
    }

    public void AddScore(string gameName, int level, int fed, int total, float timeLeft) {
        var entry = new ScoreEntry {
            gameName = gameName,
            levelIndex = level,
            fedCount = fed,
            totalEntities = total,
            timeLeft = timeLeft,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        scores.Add(entry);
        SaveScores();
    }
    public void ClearScores() {
        scores.Clear();                    // wipe the in‑memory list
        PlayerPrefs.DeleteKey(Key);        // remove the saved JSON
        PlayerPrefs.Save();
    }

    public List<ScoreEntry> GetScores() => scores;

    private void SaveScores() {
        string json = JsonUtility.ToJson(new Serialization<ScoreEntry>(scores));
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }

    private void LoadScores() {
        if(PlayerPrefs.HasKey(Key)) {
            string json = PlayerPrefs.GetString(Key);
            var wrapper = JsonUtility.FromJson<Serialization<ScoreEntry>>(json);
            scores = new List<ScoreEntry>(wrapper.items);
        }
    }
}

[Serializable]
public class Serialization<T> {
    public T[] items;
    public Serialization(List<T> list) { items = list.ToArray(); }
}