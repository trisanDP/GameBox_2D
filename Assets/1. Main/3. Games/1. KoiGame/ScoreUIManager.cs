#region --- ScoreUIManager.cs ---
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour {
    [Header("Game Panels")]
    public GameObject scoreGameListPanel;
    public GameObject koiPanel;
    public GameObject numberPanel;
    public GameObject colorClashPanel;
    public GameObject fastMathPanel;
    public GameObject shapeShifterPanel;

    [Header("Content Parents")]
    public RectTransform koiContent;
    public RectTransform numberContent;
    public RectTransform colorClashContent;
    public RectTransform fastMathContent;
    public RectTransform shapeShifterContent;

    [Header("Score Item Prefab")]
    public GameObject scoreItemPrefab;

    [Header("Buttons")]
    public Button koiButton;
    public Button numberButton;
    public Button colorClashButton;
    public Button quickAddButton;
    public Button shapeShifterButton;
    public List<Button> backButton;

    [Header("Delete Buttons")]
    public Button deleteKoiScoresButton;
    public Button deleteNumberGameScoresButton;
    public Button deleteColorClashScoresButton;
    public Button deleteFastMathScoreButton;
    public Button deleteShapeshifterScoreButton;

    void Start() {
        // wire buttons (null-safe)
        if(koiButton) koiButton.onClick.AddListener(() => ShowPanel(koiPanel));
        if(numberButton) numberButton.onClick.AddListener(() => ShowPanel(numberPanel));
        if(colorClashButton) colorClashButton.onClick.AddListener(() => ShowPanel(colorClashPanel));
        if(quickAddButton) quickAddButton.onClick.AddListener(() => ShowPanel(fastMathPanel));
        if(shapeShifterButton) shapeShifterButton.onClick.AddListener(() => ShowPanel(shapeShifterPanel));

        if(deleteKoiScoresButton) deleteKoiScoresButton.onClick.AddListener(() => DeleteScores(GameType.KoiGame, PopulateKoi));
        if(deleteNumberGameScoresButton) deleteNumberGameScoresButton.onClick.AddListener(() => DeleteScores(GameType.NumberGame, PopulateNumberGame));
        if(deleteColorClashScoresButton) deleteColorClashScoresButton.onClick.AddListener(() => DeleteScores(GameType.ColorClash, PopulateColorClash));
        if(deleteFastMathScoreButton) deleteFastMathScoreButton.onClick.AddListener(() => DeleteScores(GameType.QuickAdd, PopulateFastMath));
        if(deleteShapeshifterScoreButton) deleteShapeshifterScoreButton.onClick.AddListener(() => DeleteScores(GameType.ShapeShifter, PopulateShapeShifter));

        if(backButton != null) {
            foreach(var b in backButton) if(b != null) b.onClick.AddListener(OnBackFromPanel);
        }

        // show list panel by default
        ShowPanel(scoreGameListPanel);
    }

    void OnEnable() {
        PopulateAll();
    }

    // show requested panel, hide others
    private void ShowPanel(GameObject panel) {
        HidePanels();
        if(panel != null) panel.SetActive(true);
    }

    public void HidePanels() {
        if(scoreGameListPanel) scoreGameListPanel.SetActive(false);
        if(koiPanel) koiPanel.SetActive(false);
        if(numberPanel) numberPanel.SetActive(false);
        if(colorClashPanel) colorClashPanel.SetActive(false);
        if(fastMathPanel) fastMathPanel.SetActive(false);
        if(shapeShifterPanel) shapeShifterPanel.SetActive(false);
    }

    void OnBackFromPanel() {
        ShowPanel(scoreGameListPanel);
    }

    public void PopulateAll() {
        PopulateKoi();
        PopulateNumberGame();
        PopulateColorClash();
        PopulateFastMath();
        PopulateShapeShifter();
    }

    // ---------- Populate functions (use enum-based API) ----------
    private void PopulateKoi() {
        ClearChildren(koiContent);
        var list = SafeGetScores<KoiScoreEntry>(GameType.KoiGame);
        PopulateList(koiContent, list);
    }

    private void PopulateNumberGame() {
        ClearChildren(numberContent);
        var list = SafeGetScores<NumberGameLevelScoreEntry>(GameType.NumberGame);
        PopulateList(numberContent, list);
    }

    private void PopulateColorClash() {
        ClearChildren(colorClashContent);
        var list = SafeGetScores<ColorClashScoreEntry>(GameType.ColorClash);
        PopulateList(colorClashContent, list);
    }

    private void PopulateFastMath() {
        ClearChildren(fastMathContent);
        var list = SafeGetScores<FastMathScoreEntry>(GameType.QuickAdd);
        PopulateList(fastMathContent, list);
    }

    private void PopulateShapeShifter() {
        ClearChildren(shapeShifterContent);
        var list = SafeGetScores<SymbolMatchScoreEntry>(GameType.ShapeShifter);
        PopulateList(shapeShifterContent, list);
    }

    // helper: safely call GlobalScoreManager.GetScores<GameType>
    private List<T> SafeGetScores<T>(GameType game) where T : ScoreEntry {
        if(GlobalScoreManager.Instance == null) return new List<T>();
        try {
            return GlobalScoreManager.Instance.GetScores<T>(game) ?? new List<T>();
        } catch {
            return new List<T>();
        }
    }

    // create UI list from entries
    private void PopulateList<T>(RectTransform parent, List<T> entries) where T : ScoreEntry {
        if(parent == null) return;

        if(entries == null || entries.Count == 0) {
            // show one placeholder entry with score 0
            CreateItem(parent, 1, 0);
            return;
        }

        var sorted = entries.OrderByDescending(e => e.GetScoreValue()).ToList();
        for(int i = 0; i < sorted.Count; i++) {
            CreateItem(parent, i + 1, sorted[i].GetScoreValue());
        }
    }

    // ---------- Delete helpers ----------
    private void DeleteScores(GameType gameType, System.Action repopulateCallback) {
        if(GlobalScoreManager.Instance == null) return;
        GlobalScoreManager.Instance.ClearScoresForGame(gameType); // enum-based
        // update stats/UI
        if(StatsPanelManager.Instance != null) {
            // call whichever exists on your StatsPanelManager
            var stats = StatsPanelManager.Instance;
            // prefer UpdateAllStats, fallback UpdateStats
            var method = stats.GetType().GetMethod("UpdateAllStats");
            if(method != null) method.Invoke(stats, null);
            else {
                var method2 = stats.GetType().GetMethod("UpdateStats");
                if(method2 != null) method2.Invoke(stats, null);
            }
        }
        repopulateCallback?.Invoke();
    }

    // ---------- Utility ----------
    private void ClearChildren(Transform parent) {
        if(parent == null) return;
        for(int i = parent.childCount - 1; i >= 0; i--) {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void CreateItem(Transform parent, int rank, int score) {
        if(scoreItemPrefab == null || parent == null) return;
        var go = Instantiate(scoreItemPrefab, parent);
        var txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if(txt != null) txt.text = $"{rank}. {score}";
    }
}
#endregion
