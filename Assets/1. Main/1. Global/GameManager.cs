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

    public void ActiveState() {
        switch(currentState) {
            case AppState.MainMenu1:
            HandleMenuLogic();
            break;
            case AppState.Game1_KoiGame:
            HandleGame1Logic();
            break;
            case AppState.Game2_NumberGame:
            HandleGame2Logic();
            break;
            case AppState.Game3_ColorClash:
            HandleGame3Logic();
            break;
            case AppState.Game4_QuickAdd:
            HandleGame4Logic();
            break;
            case AppState.Game5_RuleSwitch:
            HandleGame5Logic();
            break;
        }
    }

    private void HandleMenuLogic() {

    }

    private void HandleGame1Logic() {

    }
    private void HandleGame2Logic() {

    }
    private void HandleGame3Logic() {

    }
    private void HandleGame4Logic() {

    }
    private void HandleGame5Logic() {

    }

}

public enum AppState {
    MainMenu1,
    Game1_KoiGame,
    Game2_NumberGame,
    Game3_ColorClash,
    Game4_QuickAdd,
    Game5_RuleSwitch,
}