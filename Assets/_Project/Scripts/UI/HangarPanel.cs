using Starfall.Audio;
using Starfall.Core;
using Starfall.Logic;
using Starfall.Player;
using Starfall.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Starfall.UI
{
    /// <summary>Ship and weapon selection with a holographic preview (GDD §16 Seleção de Nave).</summary>
    public sealed class HangarPanel : MenuPanel
    {
        [SerializeField] internal ListRow[] shipRows;
        [SerializeField] internal ListRow[] weaponRows;
        [SerializeField] internal Image previewImage;
        [SerializeField] internal HologramPreview preview;
        [SerializeField] internal TMP_Text previewName;
        [SerializeField] internal TMP_Text previewDescription;
        [SerializeField] internal TMP_Text statsText;
        [SerializeField] internal TMP_Text walletText;
        [SerializeField] internal Button actionButton;
        [SerializeField] internal TMP_Text actionLabel;

        private int _viewShip;
        private int _viewWeapon = -1; // -1 = viewing a ship

        public override void Initialize(GameConfig config)
        {
            base.Initialize(config);
            for (int i = 0; i < shipRows.Length; i++)
            {
                int index = i;
                if (shipRows[i] != null && shipRows[i].button != null) shipRows[i].button.onClick.AddListener(() => ViewShip(index));
            }
            for (int i = 0; i < weaponRows.Length; i++)
            {
                int index = i;
                if (weaponRows[i] != null && weaponRows[i].button != null) weaponRows[i].button.onClick.AddListener(() => ViewWeapon(index));
            }
            if (actionButton != null) actionButton.onClick.AddListener(Action);
        }

        public override void Show()
        {
            var save = SaveService.Data;
            _viewShip = save != null ? save.selectedShip : 0;
            _viewWeapon = -1;
            Refresh();
            base.Show();
        }

        private void ViewShip(int id) { _viewShip = id; _viewWeapon = -1; Refresh(); }
        private void ViewWeapon(int id) { _viewWeapon = id; Refresh(); }

        private void Refresh()
        {
            var save = SaveService.Data;
            if (Config == null || save == null) return;
            if (walletText != null) walletText.text = $"{save.credits:N0} CREDITS   {save.components} COMPONENTS";

            for (int i = 0; i < shipRows.Length; i++)
            {
                var row = shipRows[i];
                if (row == null) continue;
                var ship = Config.GetShip((ShipId)i);
                bool owned = save.IsShipUnlocked(i);
                string cost = owned ? (save.selectedShip == i ? "EQUIPPED" : "OWNED")
                    : ProgressionRules.ShipRequiresCampaign((ShipId)i) ? "CAMPAIGN" : $"{ProgressionRules.ShipCreditCost((ShipId)i):N0} CR";
                row.Set(ship != null ? ship.DisplayName.ToUpperInvariant() : ((ShipId)i).ToString().ToUpperInvariant(),
                    ship != null ? ShortStats(ship) : "", cost, ship != null, _viewWeapon < 0 && _viewShip == i);
                if (row.icon != null && ship != null) { row.icon.sprite = ship.Sprite; row.icon.color = ship.Tint; }
            }
            for (int i = 0; i < weaponRows.Length; i++)
            {
                var row = weaponRows[i];
                if (row == null) continue;
                var weapon = Config.GetWeapon((WeaponId)i);
                bool owned = save.IsWeaponUnlocked(i);
                string cost = owned ? (save.selectedWeapon == i ? "EQUIPPED" : "OWNED") : $"{ProgressionRules.WeaponCreditCost((WeaponId)i):N0} CR";
                row.Set(weapon != null ? weapon.DisplayName.ToUpperInvariant() : ((WeaponId)i).ToString().ToUpperInvariant(),
                    weapon != null ? weapon.Description : "", cost, weapon != null, _viewWeapon == i);
                if (row.icon != null && weapon != null) { row.icon.sprite = weapon.ProjectileSprite; row.icon.color = weapon.ProjectileColor; }
            }

            if (_viewWeapon >= 0) RefreshWeaponPreview(save);
            else RefreshShipPreview(save);
        }

        private void RefreshShipPreview(SaveData save)
        {
            var ship = Config.GetShip((ShipId)_viewShip);
            if (ship == null) return;
            if (previewImage != null) { previewImage.sprite = ship.Sprite; previewImage.color = ship.Tint; previewImage.preserveAspect = true; }
            if (preview != null) preview.SetTarget(previewImage != null ? previewImage.rectTransform : null);
            if (previewName != null) previewName.text = ship.DisplayName.ToUpperInvariant();
            if (previewDescription != null) previewDescription.text = ship.Description;
            var mods = UpgradeCatalog.Compute(save.upgradeLevels);
            var loadout = new PlayerLoadout(ship, ship.Weapon, mods);
            if (statsText != null)
                statsText.text = $"HULL {loadout.MaxHull:0}   SHIELD {loadout.MaxShield:0}   SPEED {loadout.MoveSpeed:0.0}\n" +
                                 $"DAMAGE x{loadout.DamageMultiplier:0.00}   CRIT {loadout.CritChance * 100f:0}% (x{loadout.CritMultiplier:0.0})\n" +
                                 $"SHIELD REGEN {loadout.ShieldRegenPerSecond:0.0}/s   ULTIMATE x{loadout.UltimatePowerMultiplier:0.00}";

            bool owned = save.IsShipUnlocked(_viewShip);
            bool equipped = save.selectedShip == _viewShip;
            if (actionButton != null && actionLabel != null)
            {
                if (owned)
                {
                    actionButton.interactable = !equipped;
                    actionLabel.text = equipped ? "EQUIPPED" : "EQUIP";
                }
                else if (ProgressionRules.ShipRequiresCampaign((ShipId)_viewShip))
                {
                    actionButton.interactable = false;
                    actionLabel.text = string.IsNullOrEmpty(ship.UnlockHint) ? "FINISH THE CAMPAIGN" : ship.UnlockHint.ToUpperInvariant();
                }
                else
                {
                    actionButton.interactable = ProgressionRules.CanBuyShip(save, (ShipId)_viewShip);
                    actionLabel.text = $"BUY  {ProgressionRules.ShipCreditCost((ShipId)_viewShip):N0} CR";
                }
            }
        }

        private void RefreshWeaponPreview(SaveData save)
        {
            var weapon = Config.GetWeapon((WeaponId)_viewWeapon);
            if (weapon == null) return;
            if (previewImage != null) { previewImage.sprite = weapon.ProjectileSprite; previewImage.color = weapon.ProjectileColor; previewImage.preserveAspect = true; }
            if (preview != null) preview.SetTarget(previewImage != null ? previewImage.rectTransform : null);
            if (previewName != null) previewName.text = weapon.DisplayName.ToUpperInvariant();
            if (previewDescription != null) previewDescription.text = weapon.Description;
            if (statsText != null)
            {
                float dps = weapon.FireInterval > 0f ? weapon.Damage * weapon.GetLevel(1).Shots.Length / weapon.FireInterval : 0f;
                string extra = weapon.Pierce > 0 ? $"   PIERCE {weapon.Pierce}"
                    : weapon.Homing ? "   HOMING"
                    : weapon.ChargeSeconds > 0f ? $"   CHARGE {weapon.ChargeSeconds:0.0}s" : "";
                statsText.text = $"DAMAGE {weapon.Damage:0.#}   RATE {1f / weapon.FireInterval:0.0}/s   DPS {dps:0}{extra}\nMAX LEVEL {weapon.MaxLevel}";
            }
            bool owned = save.IsWeaponUnlocked(_viewWeapon);
            bool equipped = save.selectedWeapon == _viewWeapon;
            if (actionButton != null && actionLabel != null)
            {
                if (owned)
                {
                    actionButton.interactable = !equipped;
                    actionLabel.text = equipped ? "EQUIPPED" : "EQUIP";
                }
                else
                {
                    actionButton.interactable = ProgressionRules.CanBuyWeapon(save, (WeaponId)_viewWeapon);
                    actionLabel.text = $"BUY  {ProgressionRules.WeaponCreditCost((WeaponId)_viewWeapon):N0} CR";
                }
            }
        }

        private void Action()
        {
            var save = SaveService.Data;
            if (save == null) return;
            bool changed = false;
            if (_viewWeapon >= 0)
            {
                if (save.IsWeaponUnlocked(_viewWeapon)) { save.selectedWeapon = _viewWeapon; changed = true; AudioManager.PlaySfx(SfxId.UiConfirm); }
                else if (ProgressionRules.BuyWeapon(save, (WeaponId)_viewWeapon)) { save.selectedWeapon = _viewWeapon; changed = true; AudioManager.PlaySfx(SfxId.Purchase); }
                else AudioManager.PlaySfx(SfxId.UiError);
            }
            else
            {
                if (save.IsShipUnlocked(_viewShip)) { save.selectedShip = _viewShip; changed = true; AudioManager.PlaySfx(SfxId.UiConfirm); }
                else if (ProgressionRules.BuyShip(save, (ShipId)_viewShip)) { save.selectedShip = _viewShip; changed = true; AudioManager.PlaySfx(SfxId.Purchase); }
                else AudioManager.PlaySfx(SfxId.UiError);
            }
            if (changed)
            {
                SaveService.Save();
                AchievementService.Evaluate(null, Config.StageCount, Config.BossCount);
            }
            Refresh();
        }

        private static string ShortStats(ShipDefinition s) => $"HULL {s.MaxHull:0}  SHD {s.MaxShield:0}  SPD {s.MoveSpeed:0.0}  CRIT {s.CritChance * 100f:0}%";
    }
}
