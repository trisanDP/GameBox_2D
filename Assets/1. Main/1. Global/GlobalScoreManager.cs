using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameScoresJson {
    public string gameName;
    public List<string> entriesJson;
}

[Serializable]
public class AllGameScoresJson {
    public List<GameScoresJson> games = new List<GameScoresJson>();
}

[Serializable]
public class ScoreRecord {
    public int scoreValue;
    public string timestamp;
    public string originalType; // optional: store the type name
    public string rawJson;      // optional: the original entry JSON
}

public class GlobalScoreManager : MonoBehaviour {
    #region Singleton
    public static GlobalScoreManager Instance { get; private set; }
    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllScores();
        } else Destroy(gameObject);
    }
    #endregion

    #region Fields
    private const string Key = "GlobalScores";
    private AllGameScoresJson allScores = new AllGameScoresJson();
    #endregion

    #region Public Methods

    /// Adds a typed score entry for the specified game and stores a stable ScoreRecord wrapper.
    /// T must inherit ScoreEntry so we can call GetScoreValue().
    public void AddScore<T>(string gameName, T entry) where T : ScoreEntry {
        Debug.Log("Adding score for game: " + gameName);
        // store the raw entry JSON (optional, for migration)
        string entryJson = JsonUtility.ToJson(entry);

        // create stable wrapper
        var record = new ScoreRecord {
            scoreValue = entry.GetScoreValue(),
            timestamp = entry.timestamp,
            originalType = typeof(T).Name,
            rawJson = entryJson
        };
        string recordJson = JsonUtility.ToJson(record);

        // find or create game bucket
        var game = allScores.games.Find(g => g.gameName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
        if(game == null) {
            game = new GameScoresJson { gameName = gameName, entriesJson = new List<string>() };
            allScores.games.Add(game);
        }

        // store the wrapper JSON (instead of or in addition to original)
        game.entriesJson.Add(recordJson);
        SaveAllScores();
    }
    /// Returns the maximum stored scoreValue for the given gameName (reads ScoreRecord JSONs).
    public int GetBestScoreForGame(string gameName) {
        var game = allScores.games.Find(g => g.gameName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
        if(game == null || game.entriesJson == null || game.entriesJson.Count == 0) return 0;

        int best = 0;
        foreach(var json in game.entriesJson) {
            try {
                var rec = JsonUtility.FromJson<ScoreRecord>(json);
                if(rec != null) best = Mathf.Max(best, rec.scoreValue);
            } catch {
                // If parsing fails, skip — keeps old entries from breaking everything
            }
        }
        return best;
    }

    public List<T> GetScores<T>(string gameName) where T : class {
        var game = allScores.games.Find(g => g.gameName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
        var list = new List<T>();
        if(game == null) return list;
        foreach(var json in game.entriesJson) {
            try {
                var obj = JsonUtility.FromJson<T>(json);
                if(obj != null) list.Add(obj);
            } catch {
                Debug.LogWarning($"Failed to parse score entry for {gameName}");
            }
        }
        Debug.Log("Retrieved " + list.Count + " scores for game: " + gameName);
        return list;
    }

    public void ClearAllScores() {
        allScores.games.Clear();
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }



    public void ClearScoresForGame(string gameName) {
        allScores.games.RemoveAll(g => g.gameName.Equals(gameName, StringComparison.OrdinalIgnoreCase));
        SaveAllScores();
    }
    #endregion

    #region Persistence
    private void SaveAllScores() {
        string json = JsonUtility.ToJson(allScores);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }

    private void LoadAllScores() {
        if(PlayerPrefs.HasKey(Key)) {
            string json = PlayerPrefs.GetString(Key);
            allScores = JsonUtility.FromJson<AllGameScoresJson>(json) ?? new AllGameScoresJson();
        }
    }
    #endregion
}

#region --- Updated Base and Derived ScoreEntry Classes ---
[Serializable]
public abstract class ScoreEntry {
    public string timestamp;
    public abstract int GetScoreValue();
}


[Serializable]
public class KoiScoreEntry : ScoreEntry {
    public int levelIndex;
    public int fedCount;
    public int totalEntities;
    public float timeLeft;

    public override int GetScoreValue() {
        float ratio = totalEntities > 0 ? (float)fedCount / totalEntities : 0;
        return Mathf.RoundToInt(ratio * 100 + timeLeft);
    }
}


[Serializable]
public class NumberGameLevelScoreEntry : ScoreEntry {
    public int levelPassed;
    public int scorePerLevel;

    public override int GetScoreValue() {
        return levelPassed * scorePerLevel;
    }
}


[Serializable]
public class ColorClashScoreEntry : ScoreEntry {
    public int finalScore;
    public override int GetScoreValue() => finalScore;
}

[Serializable]
public class FastMathScoreEntry : ScoreEntry {
    public int levelIndex;
    public int earnedPoints;

    public override int GetScoreValue() {
        return earnedPoints;
    }
}

[Serializable]
public class SymbolMatchScoreEntry : ScoreEntry {

    public int totalScore;
    public int strike;
    public float timeTaken;
    public int roundsCompleted;

    public override int GetScoreValue() => totalScore;
}

#endregion


