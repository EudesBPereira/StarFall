using Starfall.Logic;
using TMPro;
using UnityEngine;

namespace Starfall.UI
{
    /// <summary>
    /// Keeps a static TextMeshPro label translated. The key is the English text baked into the scene; it is only
    /// applied when the current language has an entry, so dynamic labels (scores, timers) are never overwritten.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] internal string key;

        private TMP_Text _text;
        private string _lastApplied;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (string.IsNullOrEmpty(key) && _text != null) key = _text.text;
        }

        private void OnEnable()
        {
            Apply();
            Loc.Changed += Apply;
        }

        private void OnDisable()
        {
            Loc.Changed -= Apply;
        }

        /// <summary>Only touches the label while it still shows the baked key (or our last translation), so code-driven text wins.</summary>
        public void Apply()
        {
            if (_text == null || string.IsNullOrEmpty(key) || !Loc.Has(key)) return;
            string current = _text.text;
            if (current != key && current != _lastApplied) return;
            _lastApplied = Loc.T(key);
            _text.text = _lastApplied;
        }
    }
}
