using UnityEngine;

public class LevelManager : MonoBehaviour {
    public static LevelManager Instance { get; private set; }

    [Header("Base Level Settings")]
    public int baseCount = 4;
    public float baseSpeed = 2f;
    public float baseCooldown = 3f;
    public int baseThreshold = 1;

    [Header("Step Increments")]
    public int countStep = 1;
    public float speedStep = 0.1f;
    public float cooldownStep = 0.1f;

    [Header("Try Settings")]
    public int maxTries = 5;
    public int perfectTries = 3;

    private int currentLevelIndex = 0;
    private int triesLeft;

    void Awake() {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
        triesLeft = maxTries;
    }

    /// <summary>
    /// Call at start of each round to get the settings.
    /// </summary>
    public LevelParameters GetCurrentParameters() {
        return new LevelParameters(
            currentLevelIndex,
            baseCount, baseSpeed, baseCooldown, baseThreshold,
            countStep, speedStep, cooldownStep
        );
    }

    /// <summary>
    /// Call when a round ends. 
    /// success = (fedCount >= entityCount && wrongCount <= threshold)
    /// perfect = (wrongCount == 0)
    /// </summary>
    public void ReportRoundEnd(bool success, bool perfect) {
        if(success) {
            if(perfect) {
                // perfect score → level up immediately
                currentLevelIndex++;
                triesLeft = maxTries;
            } else {
                triesLeft--;
                if(triesLeft > 1) {
                    // repeat same level until tries exhaust
                } else {
                    // last non‑perfect try → require perfect
                    triesLeft = perfectTries;
                }
            }
        } else {
            // failure → backtrack one level
            currentLevelIndex = Mathf.Max(currentLevelIndex - 1, 0);
            triesLeft = maxTries;
        }

        // (Optional) clamp levelIndex to some max if you like
        currentLevelIndex = Mathf.Max(currentLevelIndex, 0);
    }
}
