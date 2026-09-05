using UnityEngine;
using UnityEngine.EventSystems;

namespace Starfall.UI
{
    /// <summary>
    /// Base for overlay panels. Handles show/hide and keeps a focused element for keyboard/gamepad navigation.
    /// </summary>
    public class UiPanel : MonoBehaviour
    {
        [SerializeField] internal GameObject firstSelected;

        public bool IsVisible => gameObject.activeSelf;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            Focus();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Focus()
        {
            var es = EventSystem.current;
            if (es == null || firstSelected == null) return;
            es.SetSelectedGameObject(null);
            es.SetSelectedGameObject(firstSelected);
        }

        protected virtual void Update()
        {
            // Re-acquire focus if the selection was lost (mouse click on empty space) so keyboards/gamepads keep
            // working. Touch-only devices skip this so the first button is not permanently highlighted.
            if (!HasNavigationDevice()) return;
            var es = EventSystem.current;
            if (es != null && firstSelected != null && es.currentSelectedGameObject == null && firstSelected.activeInHierarchy)
                es.SetSelectedGameObject(firstSelected);
        }

        private static bool HasNavigationDevice()
        {
            return UnityEngine.InputSystem.Gamepad.current != null || UnityEngine.InputSystem.Keyboard.current != null;
        }
    }
}
