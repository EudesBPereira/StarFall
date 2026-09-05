using System.Collections;
using System.Collections.Generic;
using Starfall.Core;
using Starfall.Logic;
using TMPro;
using UnityEngine;

namespace Starfall.UI
{
    /// <summary>Slides a small banner in when an achievement unlocks. Works in menu and gameplay (unscaled time).</summary>
    public sealed class AchievementToast : MonoBehaviour
    {
        [SerializeField] internal RectTransform panel;
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text bodyText;

        private readonly Queue<AchievementId> _queue = new Queue<AchievementId>();
        private Coroutine _routine;

        private void OnEnable()
        {
            GameSignals.AchievementUnlocked += OnUnlocked;
            if (panel != null) panel.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            GameSignals.AchievementUnlocked -= OnUnlocked;
        }

        private void OnUnlocked(AchievementId id)
        {
            _queue.Enqueue(id);
            if (_routine == null) _routine = StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (_queue.Count > 0)
            {
                var id = _queue.Dequeue();
                if (panel == null) yield break;
                if (titleText != null) titleText.text = "ACHIEVEMENT: " + AchievementRules.DisplayName(id).ToUpperInvariant();
                if (bodyText != null) bodyText.text = AchievementRules.Description(id);
                panel.gameObject.SetActive(true);
                float t = 0f;
                Vector2 hidden = new Vector2(0f, 160f);
                while (t < 0.35f)
                {
                    t += Time.unscaledDeltaTime;
                    panel.anchoredPosition = Vector2.Lerp(hidden, Vector2.zero, Mathf.SmoothStep(0f, 1f, t / 0.35f));
                    yield return null;
                }
                float hold = 2.6f;
                while (hold > 0f) { hold -= Time.unscaledDeltaTime; yield return null; }
                t = 0f;
                while (t < 0.3f)
                {
                    t += Time.unscaledDeltaTime;
                    panel.anchoredPosition = Vector2.Lerp(Vector2.zero, hidden, t / 0.3f);
                    yield return null;
                }
                panel.gameObject.SetActive(false);
            }
            _routine = null;
        }
    }
}
