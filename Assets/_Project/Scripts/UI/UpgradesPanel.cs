using Starfall.Audio;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Save;
using TMPro;
using UnityEngine;

namespace Starfall.UI
{
    /// <summary>Permanent upgrade tree (GDD §14).</summary>
    public sealed class UpgradesPanel : MenuPanel
    {
        [SerializeField] internal ListRow[] rows;
        [SerializeField] internal TMP_Text walletText;

        public override void Initialize(GameConfig config)
        {
            base.Initialize(config);
            for (int i = 0; i < rows.Length; i++)
            {
                int node = i;
                if (rows[i] != null && rows[i].button != null) rows[i].button.onClick.AddListener(() => Buy(node));
            }
        }

        public override void Show()
        {
            Refresh();
            base.Show();
        }

        private void Refresh()
        {
            var save = SaveService.Data;
            if (save == null) return;
            if (walletText != null) walletText.text = $"{save.credits:N0} CREDITS   {save.components} COMPONENTS";
            for (int i = 0; i < rows.Length && i < UpgradeCatalog.NodeCount; i++)
            {
                var node = (UpgradeNode)i;
                int level = save.GetUpgradeLevel(i);
                bool maxed = level >= UpgradeCatalog.MaxLevel;
                string pips = new string('#', level) + new string('-', UpgradeCatalog.MaxLevel - level);
                string cost = maxed
                    ? "MAX"
                    : $"{UpgradeCatalog.CreditCost(node, level):N0} CR" +
                      (UpgradeCatalog.ComponentCost(node, level) > 0 ? $" + {UpgradeCatalog.ComponentCost(node, level)} PARTS" : "");
                rows[i].Set($"{UpgradeCatalog.Category(node)}  {UpgradeCatalog.DisplayName(node).ToUpperInvariant()}",
                    $"[{pips}]   {UpgradeCatalog.BonusLabel(node)} per level", cost, UpgradeCatalog.CanPurchase(save, node), false);
            }
        }

        private void Buy(int node)
        {
            var save = SaveService.Data;
            if (save != null && UpgradeCatalog.Purchase(save, (UpgradeNode)node))
            {
                AudioManager.PlaySfx(SfxId.Purchase);
                SaveService.Save();
                AchievementService.Evaluate(null, Config != null ? Config.StageCount : 5, Config != null ? Config.BossCount : 4);
            }
            else
            {
                AudioManager.PlaySfx(SfxId.UiError);
            }
            Refresh();
        }
    }
}
