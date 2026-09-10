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
            if (walletText != null) walletText.text = Loc.F("{0} CREDITS   {1} COMPONENTS", save.credits.ToString("N0"), save.components);
            for (int i = 0; i < rows.Length && i < UpgradeCatalog.NodeCount; i++)
            {
                var node = (UpgradeNode)i;
                int level = save.GetUpgradeLevel(i);
                bool maxed = level >= UpgradeCatalog.MaxLevel;
                string pips = new string('#', level) + new string('-', UpgradeCatalog.MaxLevel - level);
                string cost = maxed
                    ? Loc.T("MAX")
                    : Loc.F("{0} CR", UpgradeCatalog.CreditCost(node, level).ToString("N0")) +
                      (UpgradeCatalog.ComponentCost(node, level) > 0 ? " + " + UpgradeCatalog.ComponentCost(node, level) + " " + Loc.T("PARTS") : "");
                rows[i].Set(Loc.T(UpgradeCatalog.Category(node)) + "  " + Loc.Upper(Loc.T(UpgradeCatalog.DisplayName(node))),
                    Loc.F("[{0}]   {1} per level", pips, Loc.T(UpgradeCatalog.BonusLabel(node))), cost, UpgradeCatalog.CanPurchase(save, node), false);
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
