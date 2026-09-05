using UnityEngine;
using UnityEngine.SceneManagement;

namespace Starfall.Core
{
    /// <summary>Thin wrapper around SceneManager. Always restores time scale before switching scenes.</summary>
    public static class SceneLoader
    {
        public static void LoadMainMenu(GameConfig config)
        {
            Load(config != null ? config.MainMenuScene : "MainMenu");
        }

        public static void LoadGameplay(GameConfig config)
        {
            Load(config != null ? config.GameplayScene : "Gameplay");
        }

        public static void ReloadCurrent()
        {
            Load(SceneManager.GetActiveScene().name);
        }

        public static void Load(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
