using Starfall.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Starfall.UI
{
    /// <summary>Plays select/confirm sounds for any Selectable (mouse, touch, keyboard and gamepad).</summary>
    public sealed class UiButtonSfx : MonoBehaviour, ISelectHandler, IPointerEnterHandler, ISubmitHandler, IPointerClickHandler
    {
        public void OnSelect(BaseEventData eventData) => AudioManager.PlaySfx(SfxId.UiSelect, 0.6f);
        public void OnPointerEnter(PointerEventData eventData) => AudioManager.PlaySfx(SfxId.UiSelect, 0.4f);
        public void OnSubmit(BaseEventData eventData) => AudioManager.PlaySfx(SfxId.UiConfirm);
        public void OnPointerClick(PointerEventData eventData) => AudioManager.PlaySfx(SfxId.UiConfirm);
    }
}
