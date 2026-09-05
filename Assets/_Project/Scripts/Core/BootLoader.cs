using System.Collections;
using Starfall.Audio;
using Starfall.Save;
using TMPro;
using UnityEngine;

namespace Starfall.Core
{
    /// <summary>
    /// First scene: splash (studio card, then title), persistent services (save, audio) and jump to the menu.
    /// The Unity logo is shown by the engine itself on Personal licenses.
    /// </summary>
    public sealed class BootLoader : MonoBehaviour
    {
        [SerializeField] internal GameConfig config;
        [SerializeField] internal AudioLibrary audioLibrary;
        [SerializeField] internal CanvasGroup studioCard;
        [SerializeField] internal CanvasGroup titleCard;
        [SerializeField] internal TMP_Text hintText;
        [SerializeField] internal float cardSeconds = 1.4f;

        private IEnumerator Start()
        {
            GameSignals.ClearAll();
            Application.targetFrameRate = 60;
            SaveService.EnsureInitialized(config != null ? config.StageCount : 5);
            AudioManager.Ensure(audioLibrary);
            AudioManager.Instance.PlayMusic(MusicId.Menu);

            if (studioCard != null) yield return Fade(studioCard, cardSeconds);
            if (titleCard != null) yield return Fade(titleCard, cardSeconds + 0.4f);
            SceneLoader.LoadMainMenu(config);
        }

        private static IEnumerator Fade(CanvasGroup group, float seconds)
        {
            group.gameObject.SetActive(true);
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float k = t / seconds;
                group.alpha = k < 0.25f ? k / 0.25f : k > 0.8f ? (1f - k) / 0.2f : 1f;
                yield return null;
            }
            group.alpha = 0f;
            group.gameObject.SetActive(false);
        }
    }
}
