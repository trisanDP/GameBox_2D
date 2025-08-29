using System.Linq;
using TMPro;
using UnityEngine;

public class StatsPanelManager : MonoBehaviour {
    [Header("UI Elements")]
    public TextMeshProUGUI memoryText;
    public TextMeshProUGUI attentionText;
    public TextMeshProUGUI inhibitionText;
    public TextMeshProUGUI cognitiveFlexText;
    public TextMeshProUGUI processingSpeedText;

    [Header("Remarks UI")]
    public TextMeshProUGUI memoryRemarkText;
    public TextMeshProUGUI attentionRemarkText;
    public TextMeshProUGUI inhibitionRemarkText;
    public TextMeshProUGUI cognitiveFlexRemarkText;
    public TextMeshProUGUI processingSpeedRemarkText;

    [Header("Configuration")]
    public StatRange[] statMappings; // Editable via Inspector

    [Header("Remark thresholds (editable)")]
    [Tooltip("Normalized < lowThreshold => Low; between low and high => Average; >= high => High")]
    public float remarkLowThreshold = 0.33f;
    public float remarkHighThreshold = 0.66f;

    public static StatsPanelManager Instance { get; private set; }

    void Awake() {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable() {
        UpdateStats();
    }

    private void Start() {
        if(statMappings == null || statMappings.Length == 0) {
            Debug.LogWarning("StatMappings not set! Please configure in the Inspector.");
            return;
        }
        UpdateStats();
    }

    // Updates all stats for configured mappings

    public void UpdateStats() {
        if(GlobalScoreManager.Instance == null) {
            Debug.LogWarning("[StatsPanelManager] No GlobalScoreManager instance found.");
            return;
        }

        foreach(var mapping in statMappings) {
            bool hasScores = GlobalScoreManager.Instance.HasScoresForGame(mapping.gameType);
            if(!hasScores) {
                Debug.LogWarning($"No scores found for game {mapping.gameType}.");
            }
            int bestScore = GlobalScoreManager.Instance.GetBestScoreForGame(mapping.gameType);

            if(bestScore <= 0 && hasScores) {
                bestScore = GetBestScoreFallback(mapping);
            }

            float stat = CalculateStat(mapping, bestScore);
            string remark = CalculateRemark(mapping, bestScore, hasScores);

            DisplayStat(mapping.statType, stat);
            DisplayRemark(mapping.statType, remark);
        }
    }

    private string CalculateRemark(StatRange range, int score, bool hasScores) {
        if(!hasScores) return "Not Played";
        if(score <= 0) return "Low";

        int deltaScore = range.maxScore - range.minScore;
        if(deltaScore <= 0) return "Unknown";

        float normalized = Mathf.Clamp01((score - range.minScore) / (float)deltaScore);

        if(normalized < remarkLowThreshold) return "Low";
        if(normalized < remarkHighThreshold) return "Average";
        return "High";
    }

/*    // Helper to inspect stored bucket (for debug only)
    private GameScoresJson GetGameBucketDebug(string gameName) {
        // access private field via GlobalScoreManager reflection would be messy — instead add a helper in GlobalScoreManager later.
        // For now, attempt to call GetScores<ScoreEntry> to inspect count (best-effort).
        try {
            // This will show how many typed entries can be parsed as ScoreEntry (legacy)
            var list = GlobalScoreManager.Instance.GetScores<ScoreEntry>(gameName);
            // Build a fake bucket for debug (not perfect, just count)
            var fake = new GameScoresJson { gameName = gameName, entriesJson = new System.Collections.Generic.List<string>() };
            // Try reading raw PlayerPrefs string for deeper inspection
            if(PlayerPrefs.HasKey("GlobalScores")) {
                string json = PlayerPrefs.GetString("GlobalScores");
                if(!string.IsNullOrEmpty(json)) {
                    // crude parse: look for the game entry and extract first matched entry
                    // (we won't attempt complex parsing here; this is just for debug output)
                    // Return fake bucket so caller can know whether entries parsed as ScoreEntry exist
                    foreach(var item in list) fake.entriesJson.Add(JsonUtility.ToJson(item));
                    return fake;
                }
            }
        } catch { *//* ignore *//* }
        return null;
    }*/

    // Fallback: attempt typed parsing using the mapping's GameType
    private int GetBestScoreFallback(StatRange mapping) {
        switch(mapping.gameType) {
            case GameType.KoiGame:
            return GetBest<KoiScoreEntry>(mapping.gameType);
            case GameType.NumberGame:
            return GetBest<NumberGameLevelScoreEntry>(mapping.gameType);
            case GameType.ColorClash:
            return GetBest<ColorClashScoreEntry>(mapping.gameType);
            case GameType.QuickAdd:
            return GetBest<FastMathScoreEntry>(mapping.gameType);
            case GameType.ShapeShifter:
            return GetBest<SymbolMatchScoreEntry>(mapping.gameType);
            default:
            return 0;
        }
    }

    // Generic typed getter
    private int GetBest<T>(GameType game) where T : ScoreEntry {
        Debug.Log("getting best score for " + game + " of type " + typeof(T).Name);
        var list = GlobalScoreManager.Instance.GetScores<T>(game);
        if(!list.Any()) return 0;
        return list.Max(e => e.GetScoreValue());
    }

    // Calculates stat from score (returns baseStat when score <= 0); never returns below baseStat
    private float CalculateStat(StatRange range, int score) {
        if(score <= 0) return range.baseStat;
        int deltaScore = range.maxScore - range.minScore;
        if(deltaScore <= 0) return range.baseStat;
        float normalized = Mathf.Max(0f, (score - range.minScore) / (float)deltaScore);
        return range.baseStat + normalized * range.averageStat;
    }

    // Calculates remark
    private string CalculateRemark(StatRange range, int score) {
        if(score <= 0) return "Not Played";
        int deltaScore = range.maxScore - range.minScore;
        if(deltaScore <= 0) return "Unknown";
        float normalized = (score - range.minScore) / (float)deltaScore;
        float clamped = Mathf.Clamp01(normalized);
        if(clamped < remarkLowThreshold) return "Low";
        if(clamped < remarkHighThreshold) return "Average";
        return "High";
    }

    // UI update helpers
    private void DisplayStat(StatType type, float value) {
        string val = Mathf.RoundToInt(value).ToString();
        switch(type) {
            case StatType.Memory:
            if(memoryText != null) memoryText.text = val;
            break;
            case StatType.Attention:
            if(attentionText != null) attentionText.text = val;
            break;
            case StatType.Inhibition:
            if(inhibitionText != null) inhibitionText.text = val;
            break;
            case StatType.CognitiveFlexibility:
            if(cognitiveFlexText != null) cognitiveFlexText.text = val;
            break;
            case StatType.ProcessingSpeed:
            if(processingSpeedText != null) processingSpeedText.text = val;
            break;
        }
    }

    private void DisplayRemark(StatType type, string remark) {
        switch(type) {
            case StatType.Memory:
            if(memoryRemarkText != null) memoryRemarkText.text = "Remarks: " + remark;
            break;

            case StatType.Attention:
            if(attentionRemarkText != null) attentionRemarkText.text = "Remarks: " + remark;
            break;

            case StatType.Inhibition:
            if(inhibitionRemarkText != null) inhibitionRemarkText.text = "Remarks: " + remark;
            break;
            case StatType.CognitiveFlexibility:
            if(cognitiveFlexRemarkText != null) cognitiveFlexRemarkText.text = "Remarks: " + remark;
            break;
            case StatType.ProcessingSpeed:
            if(processingSpeedRemarkText != null) processingSpeedRemarkText.text = "Remarks: " + remark;
            break;
        }
    }
}

[System.Serializable]
public class StatRange {
/*    public string gameName;*/
    public GameType gameType;
    public StatType statType;
    public int minScore;
    public int maxScore;
    public float averageStat = 30f;
    public int baseStat = 10;
}

// Enums.cs
public enum GameType {
    KoiGame,
    NumberGame,
    ColorClash,
    QuickAdd,
    ShapeShifter
}

public enum StatType {
    Memory,
    Attention,
    Inhibition,
    ProcessingSpeed,
    CognitiveFlexibility
}
