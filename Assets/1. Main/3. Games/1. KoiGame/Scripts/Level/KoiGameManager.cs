// ===== KoiGameManager.cs =====
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KoiGameManager : MonoBehaviour {
    public static KoiGameManager Instance { get; private set; }

    [Header("Gameplay Prefabs")] public GameObject entityPrefab, projectilePrefab;
    [Header("Timer Settings")] public float extraTime = 4f;
    [Header("Runtime State")] public float LevelTimeRemaining { get; private set; }

    private LevelParameters levelParams;
    private List<InteractableEntity> entities = new();
    private int fedCount, wrongFeedCount;
    private bool canSelect;
    private Rect spawnBounds;


    void Awake() {
        // Ensure a fresh GameManager each scene load\
        Time.timeScale = 1f;
        if(Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
    }

    void Start() {
        levelParams = KoiLevelManager.Instance.GetCurrentParameters();

        KoiUIManager.Instance.InitializeUI(levelParams.advanceThreshold, levelParams.entityCount);

        var boundary = FindFirstObjectByType<MovementBoundary>().GetWorldBounds();
        var srProto = entityPrefab.GetComponentInChildren<SpriteRenderer>();
        if(srProto == null) {
            Debug.LogError("entityPrefab requires a SpriteRenderer child for bounds calculation.");
            return;
        }
        Vector2 halfSize = srProto.bounds.extents;

        spawnBounds = new Rect(
            boundary.xMin + halfSize.x,
            boundary.yMin + halfSize.y,
            boundary.width - halfSize.x * 2f,
            boundary.height - halfSize.y * 2f
        );

    }

    public void StartGame() {

        SpawnEntities(levelParams.entityCount, levelParams.entitySpeed);
        LevelTimeRemaining = levelParams.entityCount * levelParams.cooldownDuration + extraTime;
        KoiUIManager.Instance.UpdateTimer(LevelTimeRemaining);
        canSelect = true;

        StartCoroutine(LevelTimerRoutine());
    }

    private void SpawnEntities(int count, float speed) {
        // Clear any old entities (if reusing the same manager)
        foreach(var e in entities) if(e != null) Destroy(e.gameObject);
        entities.Clear();

        for(int i = 0; i < count; i++) {
            var go = Instantiate(entityPrefab);
            float x = UnityEngine.Random.Range(spawnBounds.xMin, spawnBounds.xMax);
            float y = UnityEngine.Random.Range(spawnBounds.yMin, spawnBounds.yMax);
            go.transform.position = new Vector3(x, y, 0);

            var entity = go.GetComponent<InteractableEntity>();
            speed = 1.3f;
            entity.speed = speed;
            entities.Add(entity);
        }
    }

    private IEnumerator LevelTimerRoutine() {
        while(LevelTimeRemaining > 0f) {
            yield return null;
            LevelTimeRemaining -= Time.deltaTime;
            KoiUIManager.Instance.UpdateTimer(LevelTimeRemaining);
        }
        EndRound(false);
    }

    public bool CanSelect => canSelect && fedCount < levelParams.entityCount;

    public void OnEntitySelected(InteractableEntity entity) {
        if(!CanSelect) return;

        canSelect = false;
        KoiUIManager.Instance.StartCooldown(levelParams.cooldownDuration);

        var spawnPos = new Vector3(0, -4f, 0);
        var projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projGO.GetComponent<ProjectileEntity>().Initialize(entity);

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine() {
        yield return new WaitForSeconds(levelParams.cooldownDuration);
        canSelect = true;
    }

    public void OnEntityFed() {
        fedCount++;
        KoiUIManager.Instance.UpdateRemaining(levelParams.entityCount - fedCount);
        if(fedCount >= levelParams.entityCount) EndRound(true);
    }

    public void OnWrongFeed() {
        wrongFeedCount++;
        KoiUIManager.Instance.UpdateWrongFeeds(wrongFeedCount);
        if(wrongFeedCount > levelParams.advanceThreshold) EndRound(false);
    }

    private void EndRound(bool success) {
        StopAllCoroutines();
        bool perfect = (wrongFeedCount == 0);
        KoiLevelManager.Instance.ReportRoundEnd(success, perfect);

        // Save with GlobalScoreManager using generic KoiScoreEntry
        var entry = new KoiScoreEntry {
            levelIndex = levelParams.levelIndex,
            fedCount = fedCount,
            totalEntities = levelParams.entityCount,
            timeLeft = LevelTimeRemaining,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        GlobalScoreManager.Instance.AddScore(GameType.KoiGame, entry);

        if(success) KoiUIManager.Instance.ShowLevelComplete(fedCount, levelParams.entityCount, LevelTimeRemaining);
        else KoiUIManager.Instance.ShowGameOver();
    }

    // UI button handlers
    public void RetryLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void NextLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void ReturnToMenu() => SceneManager.LoadScene("MainMenu");
}
