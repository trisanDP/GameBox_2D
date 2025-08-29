#region --- ScoreUIManager.cs ---
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages displaying per-game score lists in their own panels.
/// </summary>
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


    [Header("DeleteButtons")]
    public Button deleteKoiScoresButton;
    public Button deleteNumberGameScoresButton;
    public Button deleteColorClashScoresButton;
    public Button deleteFastMathScoreButton;
    public Button deleteShapeshifterScoreButton;

    private void Start() {
        // Initialize buttons
        koiButton.onClick.AddListener(() => ShowPanel(koiPanel));
        numberButton.onClick.AddListener(() => ShowPanel(numberPanel));
        colorClashButton.onClick.AddListener(() => ShowPanel(colorClashPanel));
        quickAddButton.onClick.AddListener(() => ShowPanel(fastMathPanel));
        shapeShifterButton.onClick.AddListener(() => ShowPanel(shapeShifterPanel));


        deleteKoiScoresButton.onClick.AddListener(DeleteKoiScores);
        deleteNumberGameScoresButton.onClick.AddListener(DeleteNumberGameScores);
        deleteColorClashScoresButton.onClick.AddListener(DeleteColorClashScores);
        deleteFastMathScoreButton.onClick.AddListener(DeleteFastMathScore);
        deleteShapeshifterScoreButton.onClick.AddListener(DeleteShapeShifter);



        foreach(var btn in backButton) {
            btn.onClick.AddListener(OnBackFromPanel);
        }
        // Start with Koi panel active
        ShowPanel(scoreGameListPanel);
    }
    
    private void ShowPanel(GameObject panel) {
        HidePanels();
        scoreGameListPanel.SetActive(panel == scoreGameListPanel);
        koiPanel.SetActive(panel == koiPanel);
        numberPanel.SetActive(panel == numberPanel);
        colorClashPanel.SetActive(panel == colorClashPanel);
        fastMathPanel.SetActive(panel == fastMathPanel);
        shapeShifterPanel.SetActive(panel == shapeShifterPanel);
    }



    public void HidePanels() {
        scoreGameListPanel.SetActive(false);
        koiPanel.SetActive(false);
        numberPanel.SetActive(false);
        colorClashPanel.SetActive(false);
        fastMathPanel.SetActive(false);
        shapeShifterPanel.SetActive(false);
    }


    void OnBackFromPanel() { 
        HidePanels();
        ShowPanel(scoreGameListPanel);
    }

    void OnEnable() {
        PopulateAll();
    }


    public void PopulateAll() {
        PopulateKoi();
        PopulateNumberGame();
        PopulateColorClash();
        PopulateFastMath();
        PopulateShapeShifter(); 
    }

    private void PopulateKoi() {
        ClearChildren(koiContent);
        var list = GlobalScoreManager.Instance?.GetScores<KoiScoreEntry>("KoiGame");
        if(list == null || list.Count == 0) {
            CreateItem(koiContent, 1, 0);
            return;
        }
        var sorted = list.OrderByDescending(e => e.GetScoreValue()).ToList();
        for(int i = 0; i < sorted.Count; i++) {
            CreateItem(koiContent, i + 1, sorted[i].GetScoreValue());
        }
    }

    private void PopulateNumberGame() {
        ClearChildren(numberContent);
        var list = GlobalScoreManager.Instance?.GetScores<NumberGameLevelScoreEntry>("NumberGame");
        if(list == null || list.Count == 0) {
            CreateItem(numberContent, 1, 0);
            return;
        }
        var sorted = list.OrderByDescending(e => e.GetScoreValue()).ToList();
        for(int i = 0; i < sorted.Count; i++) {
            CreateItem(numberContent, i + 1, sorted[i].GetScoreValue());
        }
    }

    private void PopulateColorClash() {
        ClearChildren(colorClashContent);
        var list = GlobalScoreManager.Instance?.GetScores<ColorClashScoreEntry>("ColorClash");
        if(list == null || list.Count == 0) {
            CreateItem(colorClashContent, 1, 0);
            return;
        }
        var sorted = list.OrderByDescending(e => e.GetScoreValue()).ToList();
        for(int i = 0; i < sorted.Count; i++) {
            CreateItem(colorClashContent, i + 1, sorted[i].GetScoreValue());
        }
    }
    
    private void PopulateFastMath() {
        ClearChildren(fastMathContent);
        var list = GlobalScoreManager.Instance?.GetScores<FastMathScoreEntry>("QuickAdd");
        if(list == null || list.Count == 0) {
            CreateItem(fastMathContent, 1, 0);
            return;
        }
        var sorted = list.OrderByDescending(e => e.GetScoreValue()).ToList();
        for(int i = 0; i < sorted.Count; i++) {
            CreateItem(fastMathContent, i + 1, sorted[i].GetScoreValue());
        }
    }

    private void PopulateShapeShifter() { 
        ClearChildren(shapeShifterContent);
        var list = GlobalScoreManager.Instance?.GetScores<SymbolMatchScoreEntry>("ShapeShifter");
        if(list == null || list.Count == 0) {
            CreateItem(shapeShifterContent, 1, 0);
            return;
        }
        var sorted = list.OrderByDescending(e => e.GetScoreValue()).ToList();
        for(int i = 0; i < sorted.Count; i++) {
            CreateItem(shapeShifterContent, i + 1, sorted[i].GetScoreValue());
        }
    }

    #region Delete Scores
    void DeleteKoiScores() {
        GlobalScoreManager.Instance?.ClearAllScores();
        StatsPanelManager.Instance?.UpdateStats();
        PopulateKoi();
    }

    void DeleteNumberGameScores() {
        GlobalScoreManager.Instance?.ClearAllScores();
        StatsPanelManager.Instance?.UpdateStats();
        PopulateNumberGame();
    }
    
    void DeleteColorClashScores() {
        GlobalScoreManager.Instance?.ClearAllScores();
        StatsPanelManager.Instance?.UpdateStats();
        PopulateColorClash();
    }

    void DeleteShapeShifter() {
        GlobalScoreManager.Instance?.ClearAllScores();
        StatsPanelManager.Instance?.UpdateStats();
        PopulateColorClash();
    }

    void DeleteFastMathScore() {
        GlobalScoreManager.Instance?.ClearAllScores();
        StatsPanelManager.Instance?.UpdateStats();
        PopulateFastMath();
    }
    #endregion
    private void ClearChildren(Transform parent) {
        foreach(Transform child in parent) {
            Destroy(child.gameObject);
        }
    }

    private void CreateItem(Transform parent, int rank, int score) {
        var go = Instantiate(scoreItemPrefab, parent);
        var txt = go.GetComponentInChildren<TextMeshProUGUI>();
        txt.text = $"{rank}. \t\t {score}";
    }
}
#endregion