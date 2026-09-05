using UnityEngine;
using UnityEngine.EventSystems;

namespace Starfall.Input
{
    /// <summary>
    /// Invisible full-screen UI element placed below the HUD buttons. Converts pointer/touch drags into
    /// ship movement deltas. Because it is a regular UI raycast target, buttons on top keep working.
    /// </summary>
    public sealed class TouchPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] internal GameInputReader input;

        private int _activePointer = int.MinValue;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (input == null || _activePointer != int.MinValue) return;
            _activePointer = eventData.pointerId;
            input.BeginDrag();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (input == null || eventData.pointerId != _activePointer) return;
            input.FeedDrag(eventData.delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (input == null || eventData.pointerId != _activePointer) return;
            _activePointer = int.MinValue;
            input.EndDrag();
        }

        private void OnDisable()
        {
            if (_activePointer != int.MinValue && input != null) input.EndDrag();
            _activePointer = int.MinValue;
        }
    }
}
