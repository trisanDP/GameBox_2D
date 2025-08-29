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
        // Ensure statMappings is initialized
        if(statMappings == null || statMappings.Length == 0) {
            Debug.LogWarning("StatMappings not set! Please configure in the Inspector.");
            return;
        }
        UpdateStats();
    }

    // Updates all stats for configured mappings
    public void UpdateStats() {
        foreach(var mapping in statMappings) {
            int bestScore = GetBestScore(mapping);                // Retrieves best score
            float stat = CalculateStat(mapping, bestScore);      // Calculates RPG-style stat
            string remark = CalculateRemark(mapping, bestScore); // Determines textual remark

            DisplayStat(mapping.statType, stat);                 // Shows numeric value
            DisplayRemark(mapping.statType, remark);             // Shows remark text

            Debug.Log($"Updated {mapping.statType} stat for {mapping.gameName}: {stat} (Best Score: {bestScore}) - {remark}");
        }
    }

    // Gets best score from player data for a mapping
    private int GetBestScore(StatRange mapping) {
        switch(mapping.gameType) {
            case GameType.KoiGame:
            return GetBest<KoiScoreEntry>(mapping.gameName);
            case GameType.NumberGame:
            return GetBest<NumberGameLevelScoreEntry>(mapping.gameName);
            case GameType.ColorClash:
            return GetBest<ColorClashScoreEntry>(mapping.gameName);
            case GameType.QuickAdd:
            return GetBest<FastMathScoreEntry>(mapping.gameName);
            case GameType.ShapeShifter:
            return GetBest<SymbolMatchScoreEntry>(mapping.gameName);
            default:
            return 0;
        }
    }
/*    private int GetBestScore(StatRange mapping) {
        return GlobalScoreManager.Instance.GetBestScoreForGame(mapping.gameName);
    }*/

    // Generic max score getter
    private int GetBest<T>(string game) where T : ScoreEntry {
        Debug.Log("getting best score for " + game + " of type " + typeof(T).Name);
        var list = GlobalScoreManager.Instance.GetScores<T>(game);
        if(!list.Any()) return 0;
        return list.Max(e => e.GetScoreValue());
    }

    // Calculates stat from score (returns baseStat when score <= 0)
    private float CalculateStat(StatRange range, int score) {
        if(score <= 0) return range.baseStat; // If no valid score, return baseStat

        int deltaScore = range.maxScore - range.minScore;
        if(deltaScore <= 0) return range.baseStat;

        float normalized = (score - range.minScore) / (float)deltaScore; // not clamped here so values above max extend stat
        return range.baseStat + normalized * range.averageStat;
    }

    // Calculates a simple remark (Low / Average / High / Not Played)
    private string CalculateRemark(StatRange range, int score) {
        if(score <= 0) return "Not Played"; // Player hasn't played

        int deltaScore = range.maxScore - range.minScore;
        if(deltaScore <= 0) return "Unknown";

        float normalized = (score - range.minScore) / (float)deltaScore;
        float clamped = Mathf.Clamp01(normalized); // clamp to [0,1] for remark classification

        if(clamped < remarkLowThreshold) return "Low";
        if(clamped < remarkHighThreshold) return "Average";
        return "High";
    }

    // Updates UI based on stat type
    private void DisplayStat(StatType type, float value) {
        string val = Mathf.RoundToInt(value).ToString();
        switch(type) {
            case StatType.Memory:
            memoryText.text = val;
            break;
            case StatType.Attention:
            attentionText.text = val;
            break;
            case StatType.Inhibition:
            inhibitionText.text = val;
            break;
            case StatType.CognitiveFlexibility:
            cognitiveFlexText.text = val;
            break;
            case StatType.ProcessingSpeed:
            processingSpeedText.text = val;
            break;
        }
    }

    // Updates remark UI based on stat type
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
    public string gameName;
    public GameType gameType;
    public StatType statType;
    public int minScore;
    public int maxScore;
    public float averageStat = 30f;
    public int baseStat = 10;
}

public enum StatType {
    Memory,
    Attention,
    Inhibition,
    ProcessingSpeed,
    CognitiveFlexibility
}

public enum GameType {
    KoiGame,
    NumberGame,
    ColorClash,
    QuickAdd,
    ShapeShifter
}
