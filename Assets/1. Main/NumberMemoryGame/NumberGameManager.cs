// NumberGameManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberGameManager : MonoBehaviour {
    [Header("Game Settings")]
    public int startCount = 5;
    public float revealTime = 5f;
    public GameObject tilePrefab;
    public Transform tileContainer;
    public RectTransform spawnArea;
    public NumberUIManager uiManager;
    public int maxMistakes = 1;

    private int currentCount;
    private int nextIndex;
    private int mistakes;
    private List<NumberTile> tiles = new List<NumberTile>();
    private List<Rect> occupiedRects = new List<Rect>();
    private float levelStartTime;

    void Start() {
        Time.timeScale = 1f; // Ensure game unpaused on start
        NumberLevelManager.Instance.ResetLevel(startCount);
        uiManager = GetComponent<NumberUIManager>();
        uiManager.onPauseRequested += PauseGame;
        StartRound();
    }

    void StartRound() {
        ClearTiles();
        Debug.Log("Starting new round with count: " + currentCount);
        occupiedRects.Clear();
        currentCount = NumberLevelManager.Instance.CurrentCount;
        nextIndex = 1;
        mistakes = 0;
        levelStartTime = Time.time;

        uiManager.SetLevel(currentCount);
        uiManager.HideAllPanels();
        uiManager.ShowFeedback("Memorize the positions!");

        SpawnTiles(currentCount);
        StartCoroutine(HideNumbersAfterDelay());
    }

    void SpawnTiles(int count) {
        Rect area = spawnArea.rect;
        Vector2 areaPos = spawnArea.anchoredPosition;

        for(int i = 1; i <= count; i++) {
            GameObject go = Instantiate(tilePrefab, tileContainer);
            var tile = go.GetComponent<NumberTile>();
            tile.Initialize(i, OnTileSelected);
            tile.EnableInteraction(false); // disable until numbers hidden
            RectTransform rt = go.GetComponent<RectTransform>();
            Vector2 size = rt.sizeDelta;

            Rect newRect;
            int attempts = 0;
            do {
                float x = Random.Range(area.xMin, area.xMax);
                float y = Random.Range(area.yMin, area.yMax);
                Vector2 pos = new Vector2(areaPos.x + x, areaPos.y + y);
                rt.anchoredPosition = pos;
                newRect = new Rect(pos - size * 0.5f, size);
                attempts++;
                if(attempts > 50) break;
            }
            while(occupiedRects.Exists(r => r.Overlaps(newRect)));

            occupiedRects.Add(newRect);
            tiles.Add(tile);
        }
    }

    IEnumerator HideNumbersAfterDelay() {
        yield return new WaitForSeconds(revealTime);
        foreach(var t in tiles) {
            t.HideNumber();
            t.EnableInteraction(true); // enable after hiding
        }
        uiManager.ShowFeedback("Select in ascending order!");
    }

    void OnTileSelected(NumberTile tile) {
        if(tile.Number == nextIndex) {
            tile.MarkCorrect(Color.blue);
            nextIndex++;
            if(nextIndex > currentCount) {
                float timeTaken = Time.time - levelStartTime;
                uiManager.ShowNextLevelPanel(timeTaken);
            }
        } else {
            mistakes++;
            tile.MarkWrong(Color.red);
            if(mistakes > maxMistakes) {
                uiManager.ShowGameOverPanel();
            }
        }
    }

    public void OnRetry() {
        StartRound();
    }

    public void OnNextLevel() {
        NumberLevelManager.Instance.LevelUp();
        StartRound();
    }

    void PauseGame() {
        Time.timeScale = 0f;
        uiManager.ShowPausePanel();
    }

    public void UnpauseGame() {
        Time.timeScale = 1f;
        uiManager.HidePausePanel();
    }

    void ClearTiles() {
        foreach(Transform c in tileContainer)
            Destroy(c.gameObject);
        tiles.Clear();
    }
}
