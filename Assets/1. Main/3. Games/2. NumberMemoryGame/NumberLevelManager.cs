// NumberLevelManager.cs
using UnityEngine;

public class NumberLevelManager : MonoBehaviour {
    public static NumberLevelManager Instance { get; private set; }
    public int CurrentCount { get; private set; }

    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else Destroy(gameObject);
    }

    public void ResetLevel(int startCount) => CurrentCount = startCount;
    public void LevelUp() => CurrentCount++;
}
