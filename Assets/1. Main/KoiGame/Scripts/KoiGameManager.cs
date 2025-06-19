// KoiGameManager.cs
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KoiGameManager : MonoBehaviour {
    public static KoiGameManager Instance { get; private set; }

    #region Inspector Settings
    [Header("Gameplay Prefabs")]
    [Tooltip("Generic entity prefab (e.g. square/fish)")]
    public GameObject entityPrefab;
    [Tooltip("Generic projectile prefab (e.g. circle)")]
    public GameObject projectilePrefab;
    #endregion

    #region UI References
    [Header("UI Elements")]
    [Tooltip("Cooldown bar UI")]
    public CooldownUI cooldownUI;
    [Tooltip("Text showing wrong feeds count")]
    public TextMeshProUGUI wrongFeedText;
    [Tooltip("Text showing how many remain")]
    public TextMeshProUGUI remainingText;
    #endregion

    #region Runtime State
    private List<InteractableEntity> entities = new List<InteractableEntity>();
    private int entityCount;           // now driven from LevelManager
    private float cooldownDuration;    // driven from LevelManager
    private int advanceThreshold;      // from LevelManager
    private int fedCount = 0;
    private int wrongFeedCount = 0;
    private bool canSelect = true;
    public bool CanSelect => canSelect && fedCount < entityCount;
    #endregion

    void Awake() {
        // Singleton pattern
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() {
        // 1) Pull in the dynamic level parameters
        var p = LevelManager.Instance.GetCurrentParameters();
        entityCount = p.entityCount;
        cooldownDuration = p.cooldownDuration;
        advanceThreshold = p.advanceThreshold;

        // 2) Initialize UI
        fedCount = 0;
        wrongFeedCount = 0;
        UpdateWrongFeedUI();
        UpdateRemainingUI();

        // 3) Spawn entities with the correct count & speed
        SpawnEntities(entityCount, p.entitySpeed);
    }

    /// <summary>
    /// Spawns `count` entities and applies `speed` to each.
    /// </summary>
    private void SpawnEntities(int count, float speed) {
        for(int i = 0; i < count; i++) {
            GameObject go = Instantiate(entityPrefab);
            var entity = go.GetComponent<InteractableEntity>();
            entity.speed = speed;
            entities.Add(entity);
        }
    }

    /// <summary>
    /// Called by an InteractableEntity when the player taps it (and CanSelect is true).
    /// </summary>
    public void OnEntitySelected(InteractableEntity entity) {
        canSelect = false;

        // Start the visual cooldown
        if(cooldownUI != null)
            cooldownUI.StartCooldown(cooldownDuration);

        // Spawn and launch a projectile at the tapped entity
        Vector3 spawnPos = new Vector3(0, -4f, 0);  // adjust as needed
        var projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        var proj = projGO.GetComponent<ProjectileEntity>();
        proj.Initialize(entity);

        // Begin the cooldown before next selection
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine() {
        yield return new WaitForSeconds(cooldownDuration);
        canSelect = true;
    }

    /// <summary>
    /// Called by ProjectileEntity when it successfully feeds an entity.
    /// </summary>
    public void OnEntityFed(InteractableEntity entity) {
        fedCount++;
        UpdateRemainingUI();

        // Check for level completion (all fed or within threshold)
        if(fedCount >= entityCount)
            EndRound(success: true);
    }

    /// <summary>
    /// Called by InteractableEntity when the player attempts to feed an already-fed entity.
    /// </summary>
    public void OnWrongFeed() {
        wrongFeedCount++;
        UpdateWrongFeedUI();

        // If too many wrong feeds, end the round as failure
        if(wrongFeedCount > advanceThreshold)
            EndRound(success: false);
    }

    private void UpdateWrongFeedUI() {
        if(wrongFeedText != null)
            wrongFeedText.text = $"Wrong Feeds: {wrongFeedCount}/{advanceThreshold}";
    }

    private void UpdateRemainingUI() {
        if(remainingText != null)
            remainingText.text = $"Remaining: {entityCount - fedCount}";
    }

    private void EndRound(bool success) {
        // Report to LevelManager: perfect if zero wrong feeds
        bool perfect = (wrongFeedCount == 0);
        LevelManager.Instance.ReportRoundEnd(success, perfect);

        // (Optional) Show Win or GameOver UI here…

        // Return to menu after a short delay
        Invoke(nameof(ReturnToMenu), 2f);
    }

    private void ReturnToMenu() {
        SceneManager.LoadScene("MainMenu");
    }
}
