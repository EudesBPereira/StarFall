using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Row widget reused by every menu list (stage, ship, weapon, upgrade, ranking).</summary>
    public sealed class ListRow : MonoBehaviour
    {
        [SerializeField] internal Button button;
        [SerializeField] internal TMP_Text titleText;
        [SerializeField] internal TMP_Text detailText;
        [SerializeField] internal TMP_Text costText;
        [SerializeField] internal Image icon;
        [SerializeField] internal Image highlight;

        public void Set(string title, string detail, string cost, bool interactable, bool selected)
        {
            if (titleText != null) titleText.text = title;
            if (detailText != null) detailText.text = detail;
            if (costText != null) costText.text = cost;
            if (button != null) button.interactable = interactable;
            if (highlight != null) highlight.enabled = selected;
        }
    }
}
