// ===== KoiLevelManager.cs =====
using UnityEngine;

public class KoiLevelManager : MonoBehaviour {
    public static KoiLevelManager Instance { get; private set; }

    [Header("Base Level Settings")] public int baseCount = 4;
    public float baseSpeed = 2f;
    public float baseCooldown = 3f;
    public int baseThreshold = 1;

    [Header("Step Increments")] public int countStep = 1;
    public float speedStep = 0.1f;
    public float cooldownStep = 0.1f;

    [Header("Try Settings")] public int maxTries = 5;
    public int perfectTries = 3;

    private int currentLevelIndex = 0;
    private int triesLeft;
    private const string LevelKey = "CurrentLevelIndex";

    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            triesLeft = maxTries;
        } else Destroy(gameObject);

        currentLevelIndex = PlayerPrefs.GetInt(LevelKey, 0);
    }

    public void ResetProgress() {
        currentLevelIndex = 0;
        PlayerPrefs.DeleteKey(LevelKey);
        PlayerPrefs.Save();
        Debug.Log("[LevelManager] Progress reset to level 0");
    }

    public LevelParameters GetCurrentParameters() {
        return new LevelParameters(
            currentLevelIndex,
            baseCount, baseSpeed, baseCooldown, baseThreshold,
            countStep, speedStep, cooldownStep
        );
    }

    public void ReportRoundEnd(bool success, bool perfect) {
        if(success) {
            currentLevelIndex++;
            triesLeft = maxTries;
        } else {
            currentLevelIndex = Mathf.Max(currentLevelIndex - 1, 0);
            triesLeft = maxTries;
        }
        PlayerPrefs.SetInt(LevelKey, currentLevelIndex);
        PlayerPrefs.Save();
    }

    void Start() {
        currentLevelIndex = PlayerPrefs.GetInt(LevelKey, 0);
    }
}