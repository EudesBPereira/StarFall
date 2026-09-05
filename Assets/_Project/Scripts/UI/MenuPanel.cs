using System;
using Starfall.Core;
using Starfall.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Base for menu sub-screens with a Back button that saves and returns to the main panel.</summary>
    public abstract class MenuPanel : UiPanel
    {
        [SerializeField] internal Button backButton;
        protected GameConfig Config;
        public event Action Closed;

        public virtual void Initialize(GameConfig config)
        {
            Config = config;
            if (backButton != null) backButton.onClick.AddListener(Close);
        }

        public void Close()
        {
            SaveService.Save();
            Hide();
            Closed?.Invoke();
        }
    }
}
