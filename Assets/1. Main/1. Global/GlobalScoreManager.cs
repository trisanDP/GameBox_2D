// GlobalScoreManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class GameScoresJson {
    public GameType gameType;                   // enum instead of string
    public List<string> entriesJson = new List<string>();
}

[Serializable]
public class AllGameScoresJson {
    public List<GameScoresJson> games = new List<GameScoresJson>();
}


[Serializable]
public class ScoreRecord {
    public int scoreValue;
    public string timestamp;
    public string originalType; // optional
    public string rawJson;      // optional
}

public class GlobalScoreManager : MonoBehaviour {
    public static GlobalScoreManager Instance { get; private set; }

    const string Key = "GlobalScores";
    AllGameScoresJson allScores = new AllGameScoresJson();

    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllScores();
        } else {
            Destroy(gameObject);
        }
    }

    // ----- Public API (enum-based) -----
    public void AddScore<T>(GameType gameType, T entry) where T : ScoreEntry {
        if(entry == null) return;

        var record = new ScoreRecord {
            scoreValue = entry.GetScoreValue(),
            timestamp = string.IsNullOrEmpty(entry.timestamp) ? DateTime.Now.ToString("s") : entry.timestamp,
            originalType = typeof(T).Name,
            rawJson = JsonUtility.ToJson(entry)
        };

        string recordJson = JsonUtility.ToJson(record);

        var bucket = allScores.games.Find(g => g.gameType == gameType);
        if(bucket == null) {
            bucket = new GameScoresJson { gameType = gameType };
            allScores.games.Add(bucket);
        }

        bucket.entriesJson.Add(recordJson);
        SaveAllScores();
    }

    public int GetBestScoreForGame(GameType gameType) {
        var bucket = allScores.games.Find(g => g.gameType == gameType);
        if(bucket == null || bucket.entriesJson == null || bucket.entriesJson.Count == 0) return 0;

        int best = 0;
        foreach(var json in bucket.entriesJson) {
            if(string.IsNullOrEmpty(json)) continue;

            // wrapper path
            try {
                var rec = JsonUtility.FromJson<ScoreRecord>(json);
                if(rec != null && (rec.scoreValue != 0 || !string.IsNullOrEmpty(rec.rawJson) || !string.IsNullOrEmpty(rec.originalType))) {
                    best = Math.Max(best, rec.scoreValue);
                    continue;
                }
            } catch { }

            // fallback parsing
            try { var c = JsonUtility.FromJson<ColorClashScoreEntry>(json); if(c != null && c.finalScore != 0) { best = Math.Max(best, c.GetScoreValue()); continue; } } catch { }
            try { var k = JsonUtility.FromJson<KoiScoreEntry>(json); if(k != null && (k.fedCount != 0 || k.totalEntities != 0)) { best = Math.Max(best, k.GetScoreValue()); continue; } } catch { }
            try { var n = JsonUtility.FromJson<NumberGameLevelScoreEntry>(json); if(n != null && (n.levelPassed != 0 || n.scorePerLevel != 0)) { best = Math.Max(best, n.GetScoreValue()); continue; } } catch { }
            try { var f = JsonUtility.FromJson<FastMathScoreEntry>(json); if(f != null && f.earnedPoints != 0) { best = Math.Max(best, f.GetScoreValue()); continue; } } catch { }
            try { var s = JsonUtility.FromJson<SymbolMatchScoreEntry>(json); if(s != null && s.totalScore != 0) { best = Math.Max(best, s.GetScoreValue()); continue; } } catch { }
        }
        return best;
    }

    public List<T> GetScores<T>(GameType gameType) where T : class {
        var result = new List<T>();
        var bucket = allScores.games.Find(g => g.gameType == gameType);
        if(bucket == null || bucket.entriesJson == null) return result;

        foreach(var json in bucket.entriesJson) {
            if(string.IsNullOrEmpty(json)) continue;

            try {
                var rec = JsonUtility.FromJson<ScoreRecord>(json);
                if(rec != null && !string.IsNullOrEmpty(rec.rawJson)) {
                    var inner = JsonUtility.FromJson<T>(rec.rawJson);
                    if(inner != null) { result.Add(inner); continue; }
                }
            } catch { }

            try {
                var direct = JsonUtility.FromJson<T>(json);
                if(direct != null) { result.Add(direct); continue; }
            } catch { }
        }
        return result;
    }

    public void ClearAllScores() {
        allScores.games.Clear();
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }

    public void ClearScoresForGame(GameType gameType) {
        allScores.games.RemoveAll(g => g.gameType == gameType);
        SaveAllScores();
    }

    // ----- Persistence -----
    void SaveAllScores() {
        try {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(allScores));
            PlayerPrefs.Save();
        } catch(Exception ex) {
            Debug.LogError("[GlobalScoreManager] Save failed: " + ex.Message);
        }
    }

    void LoadAllScores() {
        if(!PlayerPrefs.HasKey(Key)) { allScores = new AllGameScoresJson(); return; }
        try {
            allScores = JsonUtility.FromJson<AllGameScoresJson>(PlayerPrefs.GetString(Key)) ?? new AllGameScoresJson();
        } catch {
            allScores = new AllGameScoresJson();
        }
    }

    public bool HasScoresForGame(GameType gameType) {
        var game = allScores.games.Find(g => g.gameType == gameType);
        return game != null && game.entriesJson != null && game.entriesJson.Count > 0;
    }
}

#region ScoreEntry classes (simple shapes)
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
    public override int GetScoreValue() => levelPassed * scorePerLevel;
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
    public override int GetScoreValue() => earnedPoints;
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
