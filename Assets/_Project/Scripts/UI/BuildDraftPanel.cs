using System;
using System.Collections.Generic;
using Starfall.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Offers three temporary build mods; the stage is frozen until one is picked (plan §2 builds temporárias).</summary>
    public sealed class BuildDraftPanel : UiPanel
    {
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text currentBuildText;
        [SerializeField] internal ListRow[] rows;
        [SerializeField] internal Button skipButton;

        private readonly List<BuildModId> _offer = new List<BuildModId>(3);
        private Action<BuildModId?> _onChosen;

        private void Awake()
        {
            for (int i = 0; i < rows.Length; i++)
            {
                int index = i;
                if (rows[i] != null && rows[i].button != null) rows[i].button.onClick.AddListener(() => Pick(index));
            }
            if (skipButton != null) skipButton.onClick.AddListener(() => Finish(null));
        }

        public void Show(IReadOnlyList<BuildModId> offer, IReadOnlyList<BuildModId> current, Action<BuildModId?> onChosen)
        {
            _offer.Clear();
            _offer.AddRange(offer);
            _onChosen = onChosen;
            if (titleText != null) titleText.text = "CHOOSE A MODULE";
            if (currentBuildText != null)
            {
                if (current == null || current.Count == 0) currentBuildText.text = "Current build: none";
                else
                {
                    var sb = new System.Text.StringBuilder("Current build: ");
                    for (int i = 0; i < current.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(BuildMods.DisplayName(current[i]));
                    }
                    currentBuildText.text = sb.ToString();
                }
            }
            for (int i = 0; i < rows.Length; i++)
            {
                bool has = i < _offer.Count;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                var id = _offer[i];
                int stacks = BuildMods.CountOf(current, id);
                rows[i].Set(BuildMods.DisplayName(id).ToUpperInvariant(), BuildMods.Description(id), stacks > 0 ? $"x{stacks + 1}" : "", true, false);
            }
            firstSelected = _offer.Count > 0 && rows.Length > 0 ? rows[0].gameObject : (skipButton != null ? skipButton.gameObject : null);
            Show();
        }

        private void Pick(int index)
        {
            if (index < 0 || index >= _offer.Count) return;
            Finish(_offer[index]);
        }

        private void Finish(BuildModId? id)
        {
            var cb = _onChosen;
            _onChosen = null;
            Hide();
            cb?.Invoke(id);
        }
    }
}
