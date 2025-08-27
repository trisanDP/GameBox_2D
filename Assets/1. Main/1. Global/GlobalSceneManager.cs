using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalSceneManager : MonoBehaviour {
    public static GlobalSceneManager Instance { get; private set; }

    private void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void LoadSceneByGameState(AppState state) {
        string sceneName = GetSceneNameFromGameState(state);
        if(!string.IsNullOrEmpty(sceneName)) {
            SceneManager.LoadScene(sceneName);
        } else {
            Debug.LogError("Scene name not found for state: " + state);
        }
    }

    private string GetSceneNameFromGameState(AppState state) {
        switch(state) {
            case AppState.MainMenu:
            return "MainMenuScene";
            case AppState.Game1_KoiGame:
            return "KoiGameScene";
            case AppState.Game2_NumberGame:
            return "NumberGameScene";
            case AppState.Game3_ColorClash:
            return "ColorClashScene";
            case AppState.Game4_QuickAdd:
            return "QuickAddScene";
            case AppState.Game5_RuleSwitch:
            return "RuleSwitchScene";
            default:
            return null;
        }
    }
}
