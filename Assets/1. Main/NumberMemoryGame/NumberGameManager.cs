// NumberGameManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberGameManager : MonoBehaviour {
    [Header("Game Settings")]
    public int startCount = 5;            // Starting number of tiles
    public float revealTime = 5f;         // Time to show numbers before hiding
    public GameObject tilePrefab;         // Prefab for the number tile
    public Transform tileContainer;       // Parent for spawned tiles
    public RectTransform spawnArea;       // UI area where tiles can appear
    public  NumberUIManager uiManager;           // Reference to UI manager

    private int currentCount;
    private int nextIndex = 1;
    private int mistakes = 0;
    private List<NumberTile> tiles = new List<NumberTile>();

    void Start() {
        NumberLevelManager.Instance.ResetLevel(startCount);
        BeginRound();
    }

    public void BeginRound() {
        ClearTiles();
        currentCount = NumberLevelManager.Instance.CurrentCount;
        nextIndex = 1;
        mistakes = 0;

        uiManager.SetLevel(currentCount);
        uiManager.ShowFeedback("Memorize the positions!");

        SpawnTiles(currentCount);
        StartCoroutine(HideNumbersAfterDelay());
    }

    void SpawnTiles(int count) {
        tiles.Clear();
        List<int> numbers = new List<int>();
        for(int i = 1; i <= count; i++) numbers.Add(i);

        // Calculate spawn bounds in local UI space
        Rect r = spawnArea.rect;
        Vector2 areaOffset = spawnArea.anchoredPosition;

        for(int i = 0; i < count; i++) {
            GameObject go = Instantiate(tilePrefab, tileContainer);
            var tile = go.GetComponent<NumberTile>();
            int randIdx = Random.Range(0, numbers.Count);
            tile.Initialize(numbers[randIdx], OnTileSelected);
            numbers.RemoveAt(randIdx);

            // Random position within spawnArea bounds
            float x = Random.Range(r.xMin, r.xMax);
            float y = Random.Range(r.yMin, r.yMax);
            RectTransform rt = go.GetComponent<RectTransform>();
            // Add spawnArea's anchored position to get correct local position
            rt.anchoredPosition = new Vector2(areaOffset.x + x, areaOffset.y + y);

            // Optional random rotation
            rt.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            tiles.Add(tile);
        }
    }

    // === MODIFICATIONS ===
    // - SpawnTiles: adjusted position calculation to account for spawnArea's anchoredPosition,
    //   ensuring tiles spawn within the defined RectTransform region in UI space.
    // - No collider-based spawn; instead SpawnArea is a UI RectTransform.
    // - MenuManager unchanged except scene name update.



    IEnumerator HideNumbersAfterDelay() {
    yield return new WaitForSeconds(revealTime);
    foreach(var t in tiles)
        t.HideNumber();

    uiManager.ShowFeedback("Select squares in ascending order!");
}

void OnTileSelected(NumberTile tile) {
    if(tile.Number == nextIndex) {
        tile.MarkCorrect();
        nextIndex++;
        if(nextIndex > currentCount) {
            // Round complete
            uiManager.ShowFeedback("Well done! Level up.");
            NumberLevelManager.Instance.LevelUp();
            BeginRound();
        }
    } else {
        mistakes++;
        tile.MarkWrong();
        uiManager.ShowFeedback("Wrong tile!");
        if(mistakes >= 1) {
            GameOver();
        }
    }
}

void GameOver() {
    uiManager.ShowFeedback("Game Over! Restarting.");
    NumberLevelManager.Instance.ResetLevel(startCount);
    BeginRound();
}

void ClearTiles() {
    foreach(Transform child in tileContainer)
        Destroy(child.gameObject);
}
}
