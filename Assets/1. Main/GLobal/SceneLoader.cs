using UnityEngine.SceneManagement;

public static class SceneLoader {
    /// <summary>
    /// Loads the scene by name.
    /// </summary>
    public static void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Reloads whatever scene is currently active.
    /// </summary>
    public static void ReloadCurrentScene() {
        var active = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(active);
    }
}
