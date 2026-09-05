using System.Collections;
using Starfall.Audio;
using Starfall.Save;
using UnityEngine;

namespace Starfall.Core
{
    /// <summary>
    /// First scene. Initializes persistent services (save, audio) and jumps to the main menu.
    /// </summary>
    public sealed class BootLoader : MonoBehaviour
    {
        [SerializeField] internal GameConfig config;
        [SerializeField] internal AudioLibrary audioLibrary;
        [SerializeField] internal float minimumSplashSeconds = 0.6f;

        private IEnumerator Start()
        {
            GameSignals.ClearAll();
            Application.targetFrameRate = 60;
            SaveService.EnsureInitialized(config != null ? config.StageCount : 3);
            AudioManager.Ensure(audioLibrary);
            AudioManager.Instance.PlayMusic(MusicId.Menu);
            yield return new WaitForSecondsRealtime(minimumSplashSeconds);
            SceneLoader.LoadMainMenu(config);
        }
    }
}
