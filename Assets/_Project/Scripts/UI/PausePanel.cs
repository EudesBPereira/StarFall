using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Pause overlay (GDD §16): continue, settings, restart, main menu.</summary>
    public sealed class PausePanel : UiPanel
    {
        [SerializeField] internal Button continueButton;
        [SerializeField] internal Button settingsButton;
        [SerializeField] internal Button restartButton;
        [SerializeField] internal Button menuButton;
    }
}
