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
            if (walletText != null) walletText.text = Loc.F("{0} CREDITS   {1} COMPONENTS", save.credits.ToString("N0"), save.components);

            for (int i = 0; i < shipRows.Length; i++)
            {
                var row = shipRows[i];
                if (row == null) continue;
                var ship = Config.GetShip((ShipId)i);
                bool owned = save.IsShipUnlocked(i);
                string cost = owned ? (save.selectedShip == i ? Loc.T("EQUIPPED") : Loc.T("OWNED"))
                    : ProgressionRules.ShipRequiresCampaign((ShipId)i) ? Loc.T("CAMPAIGN") : Loc.F("{0} CR", ProgressionRules.ShipCreditCost((ShipId)i).ToString("N0"));
                row.Set(ship != null ? Loc.Upper(Loc.T(ship.DisplayName)) : ((ShipId)i).ToString().ToUpperInvariant(),
                    ship != null ? ShortStats(ship) : "", cost, ship != null, _viewWeapon < 0 && _viewShip == i);
                if (row.icon != null && ship != null) { row.icon.sprite = ship.Sprite; row.icon.color = ship.Tint; }
            }
            for (int i = 0; i < weaponRows.Length; i++)
            {
                var row = weaponRows[i];
                if (row == null) continue;
                var weapon = Config.GetWeapon((WeaponId)i);
                bool owned = save.IsWeaponUnlocked(i);
                string cost = owned ? (save.selectedWeapon == i ? Loc.T("EQUIPPED") : Loc.T("OWNED")) : Loc.F("{0} CR", ProgressionRules.WeaponCreditCost((WeaponId)i).ToString("N0"));
                row.Set(weapon != null ? Loc.Upper(Loc.T(weapon.DisplayName)) : ((WeaponId)i).ToString().ToUpperInvariant(),
                    weapon != null ? Loc.T(weapon.Description) : "", cost, weapon != null, _viewWeapon == i);
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
            if (previewName != null) previewName.text = Loc.Upper(Loc.T(ship.DisplayName));
            if (previewDescription != null) previewDescription.text = Loc.T(ship.Description);
            var mods = UpgradeCatalog.Compute(save.upgradeLevels);
            var loadout = new PlayerLoadout(ship, ship.Weapon, mods);
            if (statsText != null)
                statsText.text = Loc.F("HULL {0}   SHIELD {1}   SPEED {2}\n", loadout.MaxHull.ToString("0"), loadout.MaxShield.ToString("0"), loadout.MoveSpeed.ToString("0.0")) +
                                 Loc.F("DAMAGE x{0}   CRIT {1}% (x{2})\n", loadout.DamageMultiplier.ToString("0.00"), (loadout.CritChance * 100f).ToString("0"), loadout.CritMultiplier.ToString("0.0")) +
                                 Loc.F("SHIELD REGEN {0}/s   ULTIMATE x{1}\n", loadout.ShieldRegenPerSecond.ToString("0.0"), loadout.UltimatePowerMultiplier.ToString("0.00")) + KitLine(ship);

            bool owned = save.IsShipUnlocked(_viewShip);
            bool equipped = save.selectedShip == _viewShip;
            if (actionButton != null && actionLabel != null)
            {
                if (owned)
                {
                    actionButton.interactable = !equipped;
                    actionLabel.text = Loc.T(equipped ? "EQUIPPED" : "EQUIP");
                }
                else if (ProgressionRules.ShipRequiresCampaign((ShipId)_viewShip))
                {
                    actionButton.interactable = false;
                    actionLabel.text = string.IsNullOrEmpty(ship.UnlockHint) ? Loc.T("FINISH THE CAMPAIGN") : Loc.Upper(Loc.T(ship.UnlockHint));
                }
                else
                {
                    actionButton.interactable = ProgressionRules.CanBuyShip(save, (ShipId)_viewShip);
                    actionLabel.text = Loc.F("BUY  {0} CR", ProgressionRules.ShipCreditCost((ShipId)_viewShip).ToString("N0"));
                }
            }
        }

        private void RefreshWeaponPreview(SaveData save)
        {
            var weapon = Config.GetWeapon((WeaponId)_viewWeapon);
            if (weapon == null) return;
            if (previewImage != null) { previewImage.sprite = weapon.ProjectileSprite; previewImage.color = weapon.ProjectileColor; previewImage.preserveAspect = true; }
            if (preview != null) preview.SetTarget(previewImage != null ? previewImage.rectTransform : null);
            if (previewName != null) previewName.text = Loc.Upper(Loc.T(weapon.DisplayName));
            if (previewDescription != null) previewDescription.text = Loc.T(weapon.Description);
            if (statsText != null)
            {
                float dps = weapon.FireInterval > 0f ? weapon.Damage * weapon.GetLevel(1).Shots.Length / weapon.FireInterval : 0f;
                string extra = weapon.Pierce > 0 ? Loc.F("   PIERCE {0}", weapon.Pierce)
                    : weapon.Homing ? Loc.T("   HOMING")
                    : weapon.ChargeSeconds > 0f ? Loc.F("   CHARGE {0}s", weapon.ChargeSeconds.ToString("0.0")) : "";
                statsText.text = Loc.F("DAMAGE {0}   RATE {1}/s   DPS {2}{3}\nMAX LEVEL {4}", weapon.Damage.ToString("0.#"), (1f / weapon.FireInterval).ToString("0.0"), dps.ToString("0"), extra, weapon.MaxLevel);
            }
            bool owned = save.IsWeaponUnlocked(_viewWeapon);
            bool equipped = save.selectedWeapon == _viewWeapon;
            if (actionButton != null && actionLabel != null)
            {
                if (owned)
                {
                    actionButton.interactable = !equipped;
                    actionLabel.text = Loc.T(equipped ? "EQUIPPED" : "EQUIP");
                }
                else
                {
                    actionButton.interactable = ProgressionRules.CanBuyWeapon(save, (WeaponId)_viewWeapon);
                    actionLabel.text = Loc.F("BUY  {0} CR", ProgressionRules.WeaponCreditCost((WeaponId)_viewWeapon).ToString("N0"));
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

        private static string ShortStats(ShipDefinition s) => Loc.F("{0}  HULL {1}  SHD {2}  SPD {3}", Loc.T(FactionRules.FactionName(s.Faction)), s.MaxHull.ToString("0"), s.MaxShield.ToString("0"), s.MoveSpeed.ToString("0.0"));

        private static string KitLine(ShipDefinition s)
        {
            string passive = s.Faction == FactionId.Biomech ? Loc.F("LIFESTEAL {0}%/KILL", (s.LifestealPerKill * 100f).ToString("0.#"))
                : s.Faction == FactionId.Cyber ? Loc.T(s.HasCompanionDrone ? "DRONE + MARK" : "MARK TARGETS")
                : Loc.F("PRECISION +{0}%/HIT", (s.PrecisionBonusPerHit * 100f).ToString("0.#"));
            return Loc.F("{0}   ULT: {1}   PASSIVE: {2}", Loc.T(FactionRules.FactionName(s.Faction)), Loc.T(FactionRules.UltimateName(s.Ultimate)), passive);
        }
    }
}
