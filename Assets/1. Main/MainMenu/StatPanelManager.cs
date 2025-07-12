#region --- StatsPanelManager.cs ---
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Converts scores into cognitive stats and displays them.
/// </summary>
public class StatsPanelManager : MonoBehaviour {
    [Header("UI Elements")]
    public TextMeshProUGUI memoryText;
    public TextMeshProUGUI attentionText;
    public TextMeshProUGUI flexibilityText;
    public TextMeshProUGUI inhibitionText;
    public TextMeshProUGUI speedText;

    public static StatsPanelManager Instance { get; private set; }

    public void Awake() {
        if(Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void OnEnable() {
        UpdateStats();
    }

    public void UpdateStats() {
        var koiBest = GetBest<KoiScoreEntry>("KoiGame");
        var numBest = GetBest<NumberGameScoreEntry>("NumberGame");
        var colorBest = GetBest<ColorClashScoreEntry>("ColorClash");

        float memoryVal = numBest;
        float attentionVal = koiBest;
        float flexibilityVal = numBest * 0.5f + colorBest * 0.5f;
        float inhibitionVal = colorBest;
        float speedVal = koiBest * 0.3f + numBest * 0.7f;

        memoryText.text = $"{memoryVal:0}";
        attentionText.text = $"{attentionVal:0}";
        flexibilityText.text = $"{flexibilityVal:0}";
        inhibitionText.text = $"{inhibitionVal:0}";
        speedText.text = $"{speedVal:0}";
    }

    private int GetBest<T>(string game) where T : ScoreEntry {
        var list = GlobalScoreManager.Instance.GetScores<T>(game);
        if(!list.Any()) return 0;
        return list.Max(e => e.GetScoreValue());
    }
}
#endregion
