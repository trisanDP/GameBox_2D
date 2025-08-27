using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public AppState currentState;


    private void Awake() {
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
            Destroy(gameObject);
    }

    public void ChangeState(AppState state) {
        currentState = state;
        GlobalSceneManager.Instance.LoadSceneByGameState(state);
    }

}

public enum AppState {
    MainMenu,
    Game1_KoiGame,
    Game2_NumberGame,
    Game3_ColorClash,
    Game4_QuickAdd,
    Game5_RuleSwitch,
}