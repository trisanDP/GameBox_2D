using System.Linq;
using TMPro;
using UnityEngine;

public class StatsPanelManager : MonoBehaviour {
    [Header("UI Elements")]
    public TextMeshProUGUI memoryText;
    public TextMeshProUGUI attentionText;
    public TextMeshProUGUI inhibitionText;

    [Header("Configuration")]
    public StatRange[] statMappings; // Editable via Inspector

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


    // Updates all stats
    public void UpdateStats() {
        foreach(var mapping in statMappings) {
            int bestScore = GetBestScore(mapping);
            float stat = CalculateStat(mapping, bestScore);
            DisplayStat(mapping.statType, stat);
            Debug.Log($"Updated {mapping.statType} stat for {mapping.gameName}: {stat} (Best Score: {bestScore})");
        }
    }

    // Gets best score from player data
    private int GetBestScore(StatRange mapping) {
        switch(mapping.gameType.ToString()) {
            case "KoiGame":
            return GetBest<KoiScoreEntry>("KoiGame");
            case "NumberGame":
            return GetBest<NumberGameLevelScoreEntry>("NumberGame");
            case "ColorClash":
            return GetBest<ColorClashScoreEntry>("ColorClash");
            default:
            return 0;
        }
    }

    // Generic max score getter
    private int GetBest<T>(string game) where T : ScoreEntry {
        Debug.Log("getting best score for " + game + " of type " + typeof(T).Name);
        var list = GlobalScoreManager.Instance.GetScores<T>(game);
        if(!list.Any()) return 0;
        return list.Max(e => e.GetScoreValue());
    }

    // Calculates stat from score
    private float CalculateStat(StatRange range, int score) {
        if(score <= 0) return range.baseStat; // If no valid score, return baseStat

        int deltaScore = range.maxScore - range.minScore;
        if(deltaScore <= 0) return range.baseStat;

        float normalized = (score - range.minScore) / (float)deltaScore;
        return range.baseStat + normalized * range.averageStat;
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
        }
    }
}

[System.Serializable]
public class StatRange {
    public string gameName;              // Name of the game
    public GameType gameType;            // Enum linking the game to a specific type
    public StatType statType;            // Enum linking the game to a specific stat
    public int minScore;                 // Lowest score achieved in sample
    public int maxScore;                 // Highest score achieved in sample
    public float averageStat = 30f;      // Stat value for average player
    public int baseStat = 10;            // Stat value for the lowest performer


}

public enum StatType {
    Memory,
    Attention,
    Inhibition
}

public enum GameType {
    KoiGame,
    NumberGame,
    ColorClash
}
