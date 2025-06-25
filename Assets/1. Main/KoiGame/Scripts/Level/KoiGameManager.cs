// ===== KoiGameManager.cs =====
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KoiGameManager : MonoBehaviour {
    public static KoiGameManager Instance { get; private set; }

    [Header("Gameplay Prefabs")]
    public GameObject entityPrefab;
    public GameObject projectilePrefab;

    [Header("Level Timer Settings")]
    [Tooltip("Additional seconds to add on top of entityCount * cooldown + extraTime")]
    public float extraTime = 4f;

    [Header("Runtime State (read-only)")]
    public float LevelTimeRemaining { get; private set; }

    private LevelParameters levelParams;
    private List<InteractableEntity> entities = new List<InteractableEntity>();
    private int fedCount, wrongFeedCount;
    private bool canSelect;
    private Rect spawnBounds;

    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
    }

    void Start() {
        levelParams = KoiLevelManager.Instance.GetCurrentParameters();

        var raw = FindFirstObjectByType<MovementBoundary>().GetWorldBounds();
        var srProto = entityPrefab.GetComponentInChildren<SpriteRenderer>();
        if(srProto == null) {
            Debug.LogError("entityPrefab requires a SpriteRenderer child for bounds calculation.");
            return;
        }
        Vector2 halfSize = srProto.bounds.extents;

        spawnBounds = new Rect(
            raw.xMin + halfSize.x,
            raw.yMin + halfSize.y,
            raw.width - halfSize.x * 2f,
            raw.height - halfSize.y * 2f
        );

        SpawnEntities(levelParams.entityCount, levelParams.entitySpeed);
        LevelTimeRemaining = levelParams.entityCount * levelParams.cooldownDuration + extraTime;
        KoiUIManager.Instance.UpdateRemaining(levelParams.entityCount);
        KoiUIManager.Instance.UpdateTimer(LevelTimeRemaining);
        canSelect = true;

        StartCoroutine(LevelTimerRoutine());
    }   

    private void SpawnEntities(int count, float speed) {
        for(int i = 0; i < count; i++) {
            var go = Instantiate(entityPrefab);
            float x = Random.Range(spawnBounds.xMin, spawnBounds.xMax);
            float y = Random.Range(spawnBounds.yMin, spawnBounds.yMax);
            go.transform.position = new Vector3(x, y, 0);

            var entity = go.GetComponent<InteractableEntity>();
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
        if(fedCount >= levelParams.entityCount)
            EndRound(true);
    }

    public void OnWrongFeed() {
        wrongFeedCount++;
        KoiUIManager.Instance.UpdateWrongFeeds(wrongFeedCount);
        if(wrongFeedCount > levelParams.advanceThreshold)
            EndRound(false);
    }

    private void EndRound(bool success) {
        StopAllCoroutines();
        bool perfect = (wrongFeedCount == 0);

        KoiLevelManager.Instance.ReportRoundEnd(success, perfect);
        ScoreManager.Instance.AddScore(
            "KoiGame",
            levelParams.levelIndex,
            fedCount,
            levelParams.entityCount,
            LevelTimeRemaining
        );

        if(success)
            KoiUIManager.Instance.ShowLevelComplete(fedCount, levelParams.entityCount, LevelTimeRemaining);
        else
            KoiUIManager.Instance.ShowGameOver();
    }

    public void RetryLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void NextLevel() {
        Debug.Log("Loading next level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }
    public void ReturnToMenu() => SceneManager.LoadScene("MainMenu");
}
