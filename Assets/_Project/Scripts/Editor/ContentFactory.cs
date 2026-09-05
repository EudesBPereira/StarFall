using System.Collections.Generic;
using Starfall.Audio;
using Starfall.Bosses;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using Starfall.Player;
using Starfall.PowerUps;
using Starfall.Waves;
using UnityEditor;
using UnityEngine;

namespace Starfall.EditorTools
{
    /// <summary>
    /// Creates every ScriptableObject of the game (ships, weapons, power-ups, enemies, bosses, waves, stages, config).
    /// This is the single source of the initial balancing; the numbers are mirrored in docs/BALANCING.md.
    /// Re-running overwrites the generated assets (docs/DECISIONS.md D-006).
    /// </summary>
    public static class ContentFactory
    {
        public sealed class DataSet
        {
            public GameConfig Config;
            public AudioLibrary Audio;
            public ShipDefinition Vanguard;
            public List<ShipDefinition> Ships = new List<ShipDefinition>();
            public List<WeaponDefinition> Weapons = new List<WeaponDefinition>();
            public List<StageDefinition> Stages = new List<StageDefinition>();
            public StageDefinition MenuLook;
            public StageDefinition SurvivalLook;
        }

        private static string D(string sub, string name) => $"{ProjectBootstrap.DataRoot}/{sub}/{name}.asset";

        public static DataSet CreateData(PlaceholderArt.Set art, ProjectBootstrap.PrefabSet prefabs)
        {
            var data = new DataSet();

            // ---------------------------------------------------------------- Weapons (GDD §4)
            var laser = Weapon(WeaponId.Laser, "Laser", "Reliable energy beam. Levels add parallel and angled shots.", 0, art, prefabs,
                fireInterval: 0.14f, damage: 4f, speed: 18f, lifetime: 2.5f, scale: 1f, color: new Color(0.35f, 0.9f, 1f), sfx: SfxId.Laser, type: DamageType.Laser,
                sprite: art.Projectile, levels: new[]
                {
                    Level(1f, S(0, 0)),
                    Level(1.15f, S(-0.11f, 0), S(0.11f, 0)),
                    Level(1.3f, S(0, 0), S(-0.11f, -9f), S(0.11f, 9f)),
                    Level(1.45f, S(-0.11f, 0), S(0.11f, 0), S(-0.2f, -10f), S(0.2f, 10f)),
                    Level(1.6f, S(0, 0), S(-0.22f, 0), S(0.22f, 0), S(-0.3f, -12f), S(0.3f, 12f)),
                });
            var doubleLaser = Weapon(WeaponId.DoubleLaser, "Double Laser", "Twin beams. More shots per volley at every level.", ProgressionRules.WeaponCreditCost(WeaponId.DoubleLaser), art, prefabs,
                0.13f, 3.5f, 19f, 2.5f, 0.9f, new Color(0.5f, 1f, 0.7f), SfxId.Laser, DamageType.Laser, art.Projectile, new[]
                {
                    Level(1f, S(-0.16f, 0), S(0.16f, 0)),
                    Level(1.1f, S(-0.16f, 0), S(0.16f, 0), S(0, 0)),
                    Level(1.2f, S(-0.16f, 0), S(0.16f, 0), S(-0.28f, -6f), S(0.28f, 6f)),
                    Level(1.3f, S(-0.16f, 0), S(0.16f, 0), S(-0.32f, 0), S(0.32f, 0)),
                    Level(1.4f, S(-0.16f, 0), S(0.16f, 0), S(-0.32f, 0), S(0.32f, 0), S(-0.4f, -8f), S(0.4f, 8f)),
                });
            var plasma = Weapon(WeaponId.Plasma, "Plasma", "Slow, heavy bolts with high damage per hit.", ProgressionRules.WeaponCreditCost(WeaponId.Plasma), art, prefabs,
                0.32f, 11f, 12f, 3f, 1.6f, new Color(0.6f, 0.4f, 1f), SfxId.Plasma, DamageType.Plasma, art.Plasma, new[]
                {
                    Level(1f, S(0, 0)),
                    Level(1.25f, S(0, 0)),
                    Level(1.25f, S(-0.24f, 0), S(0.24f, 0)),
                    Level(1.5f, S(-0.24f, 0), S(0.24f, 0)),
                    Level(1.5f, S(0, 0), S(-0.36f, -5f), S(0.36f, 5f)),
                });
            var spread = Weapon(WeaponId.SpreadShot, "Spread Shot", "Fan of shots. Great coverage, lower damage per bullet.", ProgressionRules.WeaponCreditCost(WeaponId.SpreadShot), art, prefabs,
                0.2f, 3f, 16f, 2.2f, 0.85f, new Color(1f, 0.8f, 0.3f), SfxId.Spread, DamageType.Laser, art.Bullet, new[]
                {
                    Level(1f, S(0, -12f), S(0, 0), S(0, 12f)),
                    Level(1.1f, S(0, -18f), S(0, -6f), S(0, 6f), S(0, 18f)),
                    Level(1.2f, S(0, -24f), S(0, -12f), S(0, 0), S(0, 12f), S(0, 24f)),
                    Level(1.3f, S(0, -30f), S(0, -18f), S(0, -6f), S(0, 6f), S(0, 18f), S(0, 30f)),
                    Level(1.4f, S(0, -36f), S(0, -24f), S(0, -12f), S(0, 0), S(0, 12f), S(0, 24f), S(0, 36f)),
                });
            var railgun = Weapon(WeaponId.Railgun, "Railgun", "Hyper-velocity slug that pierces through several enemies.", ProgressionRules.WeaponCreditCost(WeaponId.Railgun), art, prefabs,
                0.55f, 16f, 32f, 1.2f, 1.1f, new Color(0.9f, 0.95f, 1f), SfxId.Railgun, DamageType.Kinetic, art.Rail, new[]
                {
                    Level(1f, S(0, 0)),
                    Level(1.25f, S(0, 0)),
                    Level(1.25f, S(-0.2f, 0), S(0.2f, 0)),
                    Level(1.5f, S(-0.2f, 0), S(0.2f, 0)),
                    Level(1.5f, S(0, 0), S(-0.3f, 0), S(0.3f, 0)),
                }, pierce: 3);
            var missiles = Weapon(WeaponId.Missiles, "Missiles", "Homing warheads with splash damage. Never miss, but reload slowly.", ProgressionRules.WeaponCreditCost(WeaponId.Missiles), art, prefabs,
                0.45f, 12f, 10f, 4f, 1f, new Color(1f, 0.55f, 0.35f), SfxId.Missile, DamageType.Explosive, art.Missile, new[]
                {
                    Level(1f, S(-0.3f, -20f), S(0.3f, 20f)),
                    Level(1.15f, S(-0.3f, -20f), S(0.3f, 20f)),
                    Level(1.15f, S(-0.3f, -25f), S(0.3f, 25f), S(0, 0)),
                    Level(1.3f, S(-0.3f, -25f), S(0.3f, 25f), S(0, 0)),
                    Level(1.3f, S(-0.3f, -30f), S(0.3f, 30f), S(-0.15f, -8f), S(0.15f, 8f)),
                }, homing: true, homingTurnRate: 240f, splash: 0.9f);
            var cannon = Weapon(WeaponId.EnergyCannon, "Energy Cannon", "Hold to charge, release to unleash a massive orb.", ProgressionRules.WeaponCreditCost(WeaponId.EnergyCannon), art, prefabs,
                0.6f, 10f, 14f, 3f, 1.2f, new Color(0.4f, 1f, 0.9f), SfxId.EnergyCannon, DamageType.Energy, art.Plasma, new[]
                {
                    Level(1f, S(0, 0)),
                    Level(1.2f, S(0, 0)),
                    Level(1.4f, S(0, 0)),
                    Level(1.6f, S(0, 0)),
                    Level(1.8f, S(0, 0)),
                }, chargeSeconds: 1.2f, chargeDamage: 4f, chargeScale: 2.6f, pierce: 1);
            data.Weapons.AddRange(new[] { laser, doubleLaser, plasma, spread, railgun, missiles, cannon });

            // ---------------------------------------------------------------- Ships (GDD §15)
            data.Vanguard = Ship(ShipId.Vanguard, "SF-01 Vanguard", "Balanced experimental interceptor of the Earth Defense Fleet.", 0, art.Ship, Color.white,
                hull: 100f, shield: 50f, regen: 0f, speed: 9f, accel: 60f, decel: 80f, crit: 0.05f, critMult: 2f, dmg: 1f, ultCharge: 1f, ultPower: 1f, weapon: laser,
                thruster: new Color(0.4f, 0.8f, 1f, 0.9f));
            var falcon = Ship(ShipId.Falcon, "SF-02 Falcon", "Very fast strike craft. Thin hull, sharp reflexes.", ProgressionRules.ShipCreditCost(ShipId.Falcon), art.Falcon, new Color(1f, 0.85f, 0.5f),
                75f, 40f, 0f, 12f, 90f, 110f, 0.08f, 2f, 1f, 1.15f, 0.9f, laser, new Color(1f, 0.7f, 0.3f, 0.9f));
            var titan = Ship(ShipId.Titan, "SF-03 Titan", "Heavy assault frame. Slow, but shrugs off punishment.", ProgressionRules.ShipCreditCost(ShipId.Titan), art.Titan, new Color(0.75f, 0.85f, 1f),
                160f, 90f, 1.5f, 6.8f, 40f, 55f, 0.03f, 1.8f, 1.1f, 0.9f, 1.3f, laser, new Color(0.5f, 0.6f, 1f, 0.9f));
            var phantom = Ship(ShipId.Phantom, "SF-04 Phantom", "Precision specialist. High critical chance and brutal criticals.", ProgressionRules.ShipCreditCost(ShipId.Phantom), art.Phantom, new Color(0.85f, 0.6f, 1f),
                85f, 45f, 0.5f, 10f, 70f, 90f, 0.25f, 3f, 0.95f, 1f, 1f, laser, new Color(0.8f, 0.4f, 1f, 0.9f));
            var novax = Ship(ShipId.NovaX, "Nova-X", "Legendary prototype recovered from the Hive Core. Excels at everything.", 0, art.NovaX, new Color(1f, 0.95f, 0.75f),
                130f, 80f, 2f, 11f, 85f, 100f, 0.15f, 2.5f, 1.2f, 1.25f, 1.5f, laser, new Color(1f, 0.9f, 0.5f, 0.95f));
            novax.UnlockHint = "Finish the campaign";
            data.Ships.AddRange(new[] { data.Vanguard, falcon, titan, phantom, novax });

            // ---------------------------------------------------------------- Power-ups (GDD §6)
            var puBlue = PowerUp(PowerUpKind.LaserLevel, "Weapon Power", "WEAPON UP", new Color(0.35f, 0.6f, 1f), 0f, 1f, 1.2f, prefabs);
            var puGreen = PowerUp(PowerUpKind.ShieldRestore, "Shield Restore", "SHIELD +50", new Color(0.35f, 1f, 0.5f), 0f, 50f, 1.4f, prefabs);
            var puRed = PowerUp(PowerUpKind.DamageBoost, "Damage Boost", "DAMAGE x2", new Color(1f, 0.3f, 0.3f), 8f, 2f, 1f, prefabs);
            var puYellow = PowerUp(PowerUpKind.SpeedBoost, "Speed Boost", "SPEED UP", new Color(1f, 0.9f, 0.3f), 8f, 1.4f, 1f, prefabs);
            var puPurple = PowerUp(PowerUpKind.Energy, "Ultimate Energy", "ENERGY +40", new Color(0.7f, 0.35f, 1f), 0f, 40f, 0.9f, prefabs);
            var puWhite = PowerUp(PowerUpKind.Invincibility, "Invincibility", "INVINCIBLE", Color.white, 5f, 1f, 0.5f, prefabs);
            var allPowerUps = new[] { puBlue, puGreen, puRed, puYellow, puPurple, puWhite };
            var dropStandard = DropTable("Standard", 0.12f, allPowerUps);
            var dropRich = DropTable("Rich", 0.5f, allPowerUps);
            var dropGuaranteed = DropTable("Guaranteed", 1f, allPowerUps);

            // ---------------------------------------------------------------- Enemies (GDD §9)
            var drone = Enemy("Drone", art.Drone, new Color(0.7f, 0.9f, 1f), 0.9f, 0.38f, hull: 12f, shield: 0f, contact: 20f, score: 100, energy: 6f,
                MovementKind.StraightDown, Move(speed: 2.6f), AttackKind.Forward, Attack(interval: 2.8f, delay: 1f, damage: 8f, speed: 5f, color: new Color(1f, 0.5f, 0.3f)), dropStandard);
            var interceptor = Enemy("Interceptor", art.Interceptor, new Color(1f, 0.75f, 0.35f), 0.85f, 0.36f, 18f, 0f, 22f, 100, 8f,
                MovementKind.Weave, Move(4f, amplitude: 1.6f, frequency: 2.4f), AttackKind.Aimed, Attack(1.5f, 0.6f, 8f, 8f, new Color(1f, 0.7f, 0.2f), count: 1, burst: 2, burstInterval: 0.12f), dropStandard);
            var bomber = Enemy("Bomber", art.Bomber, new Color(0.8f, 0.55f, 1f), 1.25f, 0.5f, 60f, 0f, 30f, 100, 12f,
                MovementKind.StraightDown, Move(1.4f), AttackKind.Forward, Attack(2.6f, 1.2f, 18f, 4.5f, new Color(1f, 0.35f, 0.6f), count: 3, spread: 30f, scale: 1.7f, splash: 0.9f), dropStandard);
            var kamikaze = Enemy("Kamikaze", art.Kamikaze, new Color(1f, 0.45f, 0.35f), 0.8f, 0.34f, 10f, 0f, 35f, 100, 6f,
                MovementKind.Chase, Move(5.5f, turnRate: 140f), AttackKind.None, Attack(), dropStandard, selfDestruct: true);
            var shieldDrone = Enemy("ShieldDrone", art.ShieldDrone, new Color(0.5f, 1f, 0.85f), 1f, 0.42f, 20f, 30f, 22f, 100, 10f,
                MovementKind.HoverStrafe, Move(2.5f, holdHeight: 0.28f, holdDuration: 7f, strafeSpeed: 2f), AttackKind.Forward, Attack(2f, 1f, 10f, 6f, new Color(0.4f, 1f, 0.9f), count: 2, spread: 14f), dropStandard);
            shieldDrone.ShieldColor = new Color(0.4f, 1f, 0.85f, 0.7f);
            var turret = Enemy("Turret", art.Turret, new Color(0.75f, 0.75f, 0.8f), 1.1f, 0.45f, 55f, 0f, 25f, 100, 10f,
                MovementKind.Hold, Move(2.2f, holdHeight: 0.22f), AttackKind.Ring, Attack(2.4f, 1.2f, 9f, 5f, new Color(1f, 0.6f, 0.2f), count: 8), dropStandard);
            turret.HasThruster = false;
            turret.MinLifetime = 4f;
            var supply = Enemy("SupplyDrone", art.Drone, new Color(0.5f, 1f, 0.6f), 0.8f, 0.36f, 8f, 0f, 0f, 50, 2f,
                MovementKind.Weave, Move(1.8f, amplitude: 2.5f, frequency: 1.2f), AttackKind.None, Attack(), dropGuaranteed);
            var asteroid = Enemy("Asteroid", art.Asteroid, new Color(0.6f, 0.55f, 0.5f), 1.1f, 0.45f, 40f, 0f, 30f, 0, 0f,
                MovementKind.StraightDown, Move(2.2f), AttackKind.None, Attack(), null);
            asteroid.IsObstacle = true;
            asteroid.ImmuneToUltimate = true;
            asteroid.HasThruster = false;
            asteroid.ExplosionColor = new Color(0.7f, 0.65f, 0.6f);

            var droneElite = Elite(drone, "DroneElite", "Elite Drone", dropRich);
            var interceptorElite = Elite(interceptor, "InterceptorElite", "Elite Interceptor", dropRich);
            var bomberElite = Elite(bomber, "BomberElite", "Elite Bomber", dropRich);
            var kamikazeElite = Elite(kamikaze, "KamikazeElite", "Elite Kamikaze", dropRich);
            var shieldElite = Elite(shieldDrone, "ShieldDroneElite", "Elite Shield Drone", dropRich);

            // ---------------------------------------------------------------- Mini-bosses (GDD §10)
            var sentinel = Boss("SentinelX", "SENTINEL-X", BossId.MiniBoss, prefabs.SentinelX, art.SentinelX, new Color(0.8f, 0.6f, 1f), 1.4f, 1.05f,
                hull: 900f, shield: 0f, contact: 30f, score: 2500, energy: 40f, components: 1, drop: dropGuaranteed, entrance: 2.5f, entranceHeight: 0.2f, explosions: 8, deathSeconds: 1.6f,
                phases: new[]
                {
                    Phase("SENTINEL-X", 1f, MovementKind.LateralPatrol, Move(1f, amplitude: 3.2f, frequency: 0.9f),
                        BossAtk(BossAttackKind.Ring, Attack(2.2f, 1.5f, 10f, 4.5f, new Color(0.85f, 0.6f, 1f), count: 10, scale: 1.1f)),
                        BossAtk(BossAttackKind.Aimed, Attack(1.6f, 0.8f, 10f, 7f, new Color(1f, 0.5f, 0.8f), count: 1))),
                    Phase("SENTINEL-X OVERDRIVE", 0.5f, MovementKind.LateralPatrol, Move(1f, amplitude: 3.6f, frequency: 1.4f),
                        BossAtk(BossAttackKind.Ring, Attack(1.6f, 0.2f, 10f, 5f, new Color(1f, 0.5f, 0.9f), count: 14)),
                        BossAtk(BossAttackKind.Aimed, Attack(1.2f, 0.4f, 10f, 8f, new Color(1f, 0.5f, 0.8f), count: 3, spread: 22f))),
                });
            var widow = Boss("Widow", "WIDOW", BossId.MiniBoss, prefabs.Widow, art.Widow, new Color(0.8f, 0.95f, 1f), 1.5f, 1f,
                1100f, 0f, 30f, 2500, 40f, 1, dropGuaranteed, 2.5f, 0.18f, 8, 1.6f, new[]
                {
                    Phase("WIDOW", 1f, MovementKind.LateralPatrol, Move(1f, amplitude: 3f, frequency: 0.7f),
                        BossAtk(BossAttackKind.Web, Attack(2.6f, 1.2f, 14f, 3.5f, new Color(0.8f, 0.95f, 1f), slow: 3f)),
                        BossAtk(BossAttackKind.Aimed, Attack(1.8f, 0.6f, 9f, 7f, new Color(0.6f, 0.9f, 1f), count: 2, spread: 14f))),
                    Phase("WIDOW FRENZY", 0.5f, MovementKind.LateralPatrol, Move(1f, amplitude: 3.5f, frequency: 1.1f),
                        BossAtk(BossAttackKind.Web, Attack(1.8f, 0.3f, 14f, 4f, new Color(0.8f, 0.95f, 1f), slow: 3.5f)),
                        BossAtk(BossAttackKind.Ring, Attack(2.4f, 1f, 9f, 5f, new Color(0.6f, 0.9f, 1f), count: 8)),
                        BossAtk(BossAttackKind.Aimed, Attack(1.4f, 0.5f, 9f, 8f, new Color(0.6f, 0.9f, 1f), count: 3, spread: 20f))),
                });
            var reaper = Boss("ReaperWing", "REAPER WING", BossId.MiniBoss, prefabs.ReaperWing, art.ReaperWing, new Color(1f, 0.55f, 0.45f), 1.4f, 0.9f,
                1000f, 0f, 35f, 2500, 40f, 1, dropGuaranteed, 2f, 0.22f, 8, 1.6f, new[]
                {
                    Phase("REAPER WING", 1f, MovementKind.Dash, Move(11f, amplitude: 1.6f, holdHeight: 0.25f, strafeSpeed: 0.55f),
                        BossAtk(BossAttackKind.Aimed, Attack(1.4f, 0.8f, 9f, 8.5f, new Color(1f, 0.6f, 0.4f), count: 1, burst: 3, burstInterval: 0.1f))),
                    Phase("REAPER WING BERSERK", 0.5f, MovementKind.Dash, Move(14f, amplitude: 2f, holdHeight: 0.3f, strafeSpeed: 0.35f),
                        BossAtk(BossAttackKind.Aimed, Attack(1.2f, 0.3f, 9f, 9.5f, new Color(1f, 0.6f, 0.4f), count: 2, spread: 10f, burst: 3, burstInterval: 0.1f)),
                        BossAtk(BossAttackKind.Stream, Attack(0.16f, 0.5f, 7f, 7f, new Color(1f, 0.4f, 0.3f), spread: 35f))),
                });
            var sentinelMk2 = Variant(sentinel, "SentinelXMk2", "SENTINEL-X MK.II", new Color(1f, 0.55f, 0.35f), 1800f, 1.3f, 2);
            var widowPrime = Variant(widow, "WidowPrime", "WIDOW PRIME", new Color(1f, 0.8f, 0.95f), 2200f, 1.35f, 2);

            // ---------------------------------------------------------------- Bosses (GDD §11)
            var destroyer = Boss("Destroyer", "THE DESTROYER", BossId.Destroyer, prefabs.Destroyer, art.Destroyer, new Color(1f, 0.45f, 0.45f), 1.8f, 1.4f,
                2400f, 0f, 40f, 10000, 60f, 3, dropGuaranteed, 3.5f, 0.2f, 14, 2.2f, new[]
                {
                    Phase("THE DESTROYER", 1f, MovementKind.LateralPatrol, Move(1f, amplitude: 2.5f, frequency: 0.6f),
                        BossAtk(BossAttackKind.SideCannons, Attack(0.9f, 1f, 10f, 7f, new Color(1f, 0.5f, 0.4f), count: 1), muzzle: 1.7f),
                        BossAtk(BossAttackKind.Ring, Attack(3.5f, 2f, 10f, 4.5f, new Color(1f, 0.35f, 0.5f), count: 12))),
                    Phase("DESTROYER: MISSILE BARRAGE", 0.66f, MovementKind.LateralPatrol, Move(1f, amplitude: 3f, frequency: 0.8f),
                        BossAtk(BossAttackKind.SideCannons, Attack(0.7f, 0.4f, 10f, 7.5f, new Color(1f, 0.5f, 0.4f), count: 2, spread: 12f), muzzle: 1.7f),
                        BossAtk(BossAttackKind.Missiles, Attack(3f, 1f, 16f, 5f, new Color(1f, 0.7f, 0.3f), scale: 1.2f, lifetime: 6f, splash: 0.8f), homing: 80f, muzzle: 1.4f)),
                    Phase("DESTROYER: FINAL FURY", 0.33f, MovementKind.LateralPatrol, Move(1f, amplitude: 3.2f, frequency: 1.1f),
                        BossAtk(BossAttackKind.FrontLaser, Attack(6f, 2f, 0f, 0f, new Color(1f, 0.35f, 0.55f)), telegraph: 1.2f, beam: 1.6f, beamWidth: 1.2f, beamDps: 45f),
                        BossAtk(BossAttackKind.Missiles, Attack(3.5f, 2f, 16f, 5.5f, new Color(1f, 0.7f, 0.3f), scale: 1.2f, lifetime: 6f, splash: 0.8f), homing: 90f, muzzle: 1.4f),
                        BossAtk(BossAttackKind.Ring, Attack(2.8f, 1f, 10f, 5f, new Color(1f, 0.35f, 0.5f), count: 16))),
                });
            var leviathan = Boss("Leviathan", "LEVIATHAN", BossId.Leviathan, prefabs.Leviathan, art.LeviathanHead, new Color(0.45f, 0.9f, 0.75f), 1.5f, 0.8f,
                2800f, 0f, 35f, 10000, 60f, 3, dropGuaranteed, 3f, 0.25f, 14, 2.4f, new[]
                {
                    Phase("LEVIATHAN", 1f, MovementKind.Serpentine, Move(1f, amplitude: 3.4f, frequency: 0.8f, strafeSpeed: 1.2f),
                        BossAtk(BossAttackKind.Stream, Attack(0.22f, 1f, 8f, 6.5f, new Color(0.5f, 1f, 0.8f), spread: 40f))),
                    Phase("LEVIATHAN COILS", 0.6f, MovementKind.Serpentine, Move(1f, amplitude: 3.6f, frequency: 1.1f, strafeSpeed: 1.6f),
                        BossAtk(BossAttackKind.Stream, Attack(0.18f, 0.3f, 8f, 7f, new Color(0.5f, 1f, 0.8f), spread: 45f)),
                        BossAtk(BossAttackKind.Ring, Attack(3f, 1.5f, 10f, 4.5f, new Color(0.3f, 0.9f, 0.7f), count: 10))),
                    Phase("LEVIATHAN RAMPAGE", 0.3f, MovementKind.Serpentine, Move(1f, amplitude: 3.8f, frequency: 1.5f, strafeSpeed: 2f),
                        BossAtk(BossAttackKind.Stream, Attack(0.12f, 0.2f, 8f, 8f, new Color(0.6f, 1f, 0.85f), spread: 50f)),
                        BossAtk(BossAttackKind.Aimed, Attack(1.6f, 0.5f, 12f, 8f, new Color(0.3f, 0.9f, 0.7f), count: 3, spread: 18f))),
                }, segments: 8, segmentSprite: art.LeviathanSegment, segmentSpacing: 0.95f, segmentScale: 0.85f);
            var hiveQueen = Boss("HiveQueen", "HIVE QUEEN", BossId.HiveQueen, prefabs.HiveQueen, art.HiveQueen, new Color(1f, 0.7f, 0.9f), 1.9f, 1.4f,
                3400f, 200f, 40f, 10000, 60f, 3, dropGuaranteed, 3.5f, 0.18f, 16, 2.6f, new[]
                {
                    Phase("HIVE QUEEN", 1f, MovementKind.LateralPatrol, Move(1f, amplitude: 2f, frequency: 0.5f),
                        BossAtk(BossAttackKind.Summon, Attack(4f, 1.5f, 0f, 0f, Color.white, count: 3), summon: drone, cap: 8),
                        BossAtk(BossAttackKind.Ring, Attack(2.4f, 1f, 10f, 4.5f, new Color(1f, 0.6f, 0.9f), count: 10))),
                    Phase("HIVE QUEEN: SWARM", 0.6f, MovementKind.LateralPatrol, Move(1f, amplitude: 2.5f, frequency: 0.7f),
                        BossAtk(BossAttackKind.Summon, Attack(5f, 0.5f, 0f, 0f, Color.white, count: 2), summon: kamikaze, cap: 8),
                        BossAtk(BossAttackKind.Aimed, Attack(1.5f, 0.5f, 10f, 7.5f, new Color(1f, 0.5f, 0.85f), count: 3, spread: 20f)),
                        BossAtk(BossAttackKind.Ring, Attack(3f, 1f, 10f, 5f, new Color(1f, 0.6f, 0.9f), count: 12))),
                    Phase("HIVE QUEEN: LAST BROOD", 0.3f, MovementKind.LateralPatrol, Move(1f, amplitude: 3f, frequency: 1f),
                        BossAtk(BossAttackKind.Summon, Attack(4f, 0.5f, 0f, 0f, Color.white, count: 2), summon: interceptor, cap: 8),
                        BossAtk(BossAttackKind.Ring, Attack(2f, 0.5f, 10f, 5.5f, new Color(1f, 0.6f, 0.9f), count: 16)),
                        BossAtk(BossAttackKind.Aimed, Attack(1.2f, 0.4f, 10f, 8f, new Color(1f, 0.5f, 0.85f), count: 2, spread: 12f, burst: 2, burstInterval: 0.15f))),
                });
            hiveQueen.ShieldColor = new Color(1f, 0.6f, 0.9f, 0.7f);
            var destroyerMk2 = Variant(destroyer, "DestroyerMk2", "DESTROYER MK.II", new Color(0.9f, 0.3f, 0.3f), 4200f, 1.15f, 3);
            destroyerMk2.BossId = BossId.Destroyer;
            var omega = Boss("OmegaCore", "OMEGA CORE", BossId.OmegaCore, prefabs.OmegaCore, art.OmegaCore, new Color(0.6f, 0.9f, 1f), 2f, 1.5f,
                6000f, 400f, 45f, 10000, 80f, 5, dropGuaranteed, 4f, 0.2f, 20, 3.2f, new[]
                {
                    Phase("OMEGA CORE", 1f, MovementKind.LateralPatrol, Move(1f, amplitude: 2.2f, frequency: 0.5f),
                        BossAtk(BossAttackKind.Ring, Attack(2.4f, 1.5f, 11f, 4.5f, new Color(0.6f, 0.9f, 1f), count: 12)),
                        BossAtk(BossAttackKind.SideCannons, Attack(0.8f, 1f, 11f, 7f, new Color(0.5f, 0.8f, 1f), count: 1), muzzle: 1.9f)),
                    Phase("OMEGA CORE: TRANSFORMATION", 0.75f, MovementKind.LateralPatrol, Move(1f, amplitude: 2.8f, frequency: 0.8f),
                        new[]
                        {
                            BossAtk(BossAttackKind.Missiles, Attack(2.8f, 1f, 16f, 5.5f, new Color(1f, 0.6f, 0.4f), scale: 1.2f, lifetime: 6f, splash: 0.9f), homing: 90f, muzzle: 1.6f),
                            BossAtk(BossAttackKind.Stream, Attack(0.2f, 0.5f, 9f, 7f, new Color(1f, 0.5f, 0.6f), spread: 40f)),
                        }, sprite: art.OmegaCoreForm2, tint: new Color(1f, 0.6f, 0.5f), scale: 1.65f, transformSeconds: 1.6f),
                    Phase("OMEGA CORE: ANNIHILATOR", 0.5f, MovementKind.LateralPatrol, Move(1f, amplitude: 3f, frequency: 0.9f),
                        new[]
                        {
                            BossAtk(BossAttackKind.FrontLaser, Attack(6.5f, 2.5f, 0f, 0f, new Color(1f, 0.35f, 0.55f)), telegraph: 1.2f, beam: 1.8f, beamWidth: 1.4f, beamDps: 50f),
                            BossAtk(BossAttackKind.Ring, Attack(2.2f, 1f, 11f, 5f, new Color(0.9f, 0.5f, 1f), count: 16)),
                            BossAtk(BossAttackKind.Summon, Attack(6f, 3f, 0f, 0f, Color.white, count: 2), summon: kamikaze, cap: 4),
                        }, sprite: art.OmegaCoreForm3, tint: new Color(0.9f, 0.5f, 1f), scale: 1.8f, transformSeconds: 1.8f),
                    Phase("OMEGA OVERLOAD", 0.25f, MovementKind.LateralPatrol, Move(1f, amplitude: 3.4f, frequency: 1.3f),
                        new[]
                        {
                            BossAtk(BossAttackKind.SideCannons, Attack(0.55f, 0.3f, 11f, 8f, new Color(1f, 0.4f, 0.4f), count: 2, spread: 12f), muzzle: 1.9f),
                            BossAtk(BossAttackKind.Missiles, Attack(3.5f, 1.5f, 16f, 6f, new Color(1f, 0.6f, 0.4f), scale: 1.2f, lifetime: 6f, splash: 0.9f), homing: 100f, muzzle: 1.6f),
                            BossAtk(BossAttackKind.FrontLaser, Attack(5f, 3f, 0f, 0f, new Color(1f, 0.35f, 0.55f)), telegraph: 1f, beam: 1.6f, beamWidth: 1.4f, beamDps: 55f),
                            BossAtk(BossAttackKind.Ring, Attack(2f, 0.5f, 11f, 5.5f, new Color(1f, 0.6f, 0.6f), count: 18)),
                        }, sprite: art.OmegaCoreForm2, tint: new Color(1f, 0.85f, 0.8f), scale: 1.9f, transformSeconds: 1.4f),
                });
            omega.ShieldColor = new Color(0.6f, 0.9f, 1f, 0.7f);

            // ---------------------------------------------------------------- Stages (GDD §12)
            var s1 = Stage("Stage1_OrbitalSector", "Orbital Sector", "Low Earth orbit - debris field",
                "The Swarm vanguard is tearing through our satellite grid.\nClear the orbital sector and stop their scout carrier, The Destroyer.\n\nDrag to move. Your laser fires automatically.\nBlue crates upgrade your weapon; green ones restore shields.",
                "- Destroy every Swarm wave\n- Defeat Sentinel-X\n- Destroy The Destroyer",
                MusicId.Stage1, AmbientId.Space, new Color(0.03f, 0.05f, 0.14f), new Color(0.06f, 0.03f, 0.14f), new Color(0.5f, 0.2f, 0.8f, 0f), 0.75f, Color.white,
                debris: new[] { art.Satellite }, debrisInterval: 5f, debrisTint: new Color(0.6f, 0.65f, 0.75f, 0.55f), backdrop: art.Planet, backdropTint: new Color(0.15f, 0.35f, 0.6f, 0.45f), backdropScale: 14f,
                bonus: 1000, asteroid: null, events: new[]
                {
                    Ev.Message("SECTOR 1: ORBITAL", 2f),
                    Ev.Wave(Wave("S1_W1", E(drone, 4, 0.7f, SpawnPattern.TopLine))),
                    Ev.Wave(Wave("S1_W2", E(drone, 5, 0.5f, SpawnPattern.TopVee), E(drone, 3, 0.6f, SpawnPattern.TopAlternate, 1.5f))),
                    Ev.Message("INTERCEPTORS INBOUND", 1.5f),
                    Ev.Wave(Wave("S1_W3", E(interceptor, 3, 0.8f, SpawnPattern.TopAlternate), E(drone, 4, 0.5f, SpawnPattern.TopLine, 1f))),
                    Ev.Wave(Wave("S1_W4", E(interceptor, 4, 0.6f, SpawnPattern.TopRandom), E(drone, 6, 0.4f, SpawnPattern.TopVee, 1f))),
                    Ev.MiniBoss(sentinel, "WARNING: SENTINEL-X APPROACHING"),
                    Ev.Wave(Wave("S1_W5", E(drone, 6, 0.4f, SpawnPattern.TopLine), E(interceptor, 3, 0.6f, SpawnPattern.TopAlternate, 1f), E(droneElite, 1, 0f, SpawnPattern.TopCenter, 2f))),
                    Ev.Wave(Wave("S1_W6", E(interceptor, 5, 0.5f, SpawnPattern.TopVee), E(drone, 6, 0.4f, SpawnPattern.TopRandom, 1.5f))),
                    Ev.Delay(1.5f),
                    Ev.Boss(destroyer, "WARNING: THE DESTROYER"),
                });
            var s2 = Stage("Stage2_AsteroidField", "Asteroid Field", "Kuiper drift - rogue asteroids",
                "The Swarm is using the asteroid field as cover.\nWatch for rocks: they damage your hull on contact but can be destroyed.\nThe Widow lurks here, and something big is coiling beneath the rocks.",
                "- Survive the asteroid field\n- Defeat the Widow\n- Destroy the Leviathan",
                MusicId.Stage2, AmbientId.Asteroids, new Color(0.08f, 0.06f, 0.05f), new Color(0.03f, 0.03f, 0.06f), new Color(0.5f, 0.2f, 0.8f, 0f), 0.4f, new Color(0.9f, 0.85f, 0.8f),
                null, 0f, Color.white, null, Color.white, 6f, 1500, asteroid, new[]
                {
                    Ev.Message("SECTOR 2: ASTEROID FIELD", 2f),
                    Ev.Asteroids(true),
                    Ev.Wave(Wave("S2_W1", E(drone, 5, 0.5f, SpawnPattern.TopLine), E(bomber, 2, 1f, SpawnPattern.TopAlternate, 1.5f))),
                    Ev.Wave(Wave("S2_W2", E(kamikaze, 4, 0.7f, SpawnPattern.TopRandom), E(interceptor, 3, 0.6f, SpawnPattern.TopVee, 1.5f))),
                    Ev.Message("SHIELDED UNITS DETECTED", 1.5f),
                    Ev.Wave(Wave("S2_W3", E(shieldDrone, 3, 1f, SpawnPattern.TopLine), E(drone, 4, 0.4f, SpawnPattern.TopRandom, 2f))),
                    Ev.MiniBoss(widow, "WARNING: WIDOW"),
                    Ev.Wave(Wave("S2_W4", E(bomber, 3, 0.9f, SpawnPattern.TopLine), E(kamikaze, 5, 0.5f, SpawnPattern.TopRandom, 1f), E(bomberElite, 1, 0f, SpawnPattern.TopCenter, 2.5f))),
                    Ev.Wave(Wave("S2_W5", E(shieldDrone, 3, 0.8f, SpawnPattern.TopVee), E(interceptor, 4, 0.5f, SpawnPattern.TopAlternate, 1f), E(kamikazeElite, 1, 0f, SpawnPattern.TopRandom, 2f))),
                    Ev.Asteroids(false),
                    Ev.Delay(1.5f),
                    Ev.Boss(leviathan, "WARNING: LEVIATHAN"),
                });
            var s3 = Stage("Stage3_VioletNebula", "Violet Nebula", "Ionized gas cloud - low visibility",
                "Sensors are useless in the nebula. The Swarm breeds here.\nExpect every enemy type at once, elite escorts, and their fastest ace: the Reaper Wing.\nThe Hive Queen must not leave this cloud.",
                "- Break through the nebula\n- Shoot down the Reaper Wing\n- Kill the Hive Queen",
                MusicId.Stage3, AmbientId.Nebula, new Color(0.16f, 0.05f, 0.24f), new Color(0.08f, 0.02f, 0.14f), new Color(0.55f, 0.25f, 0.85f, 0.22f), 0.5f, new Color(0.9f, 0.8f, 1f),
                null, 0f, Color.white, null, Color.white, 6f, 2000, null, new[]
                {
                    Ev.Message("SECTOR 3: VIOLET NEBULA", 2f),
                    Ev.Wave(Wave("S3_W1", E(interceptor, 5, 0.4f, SpawnPattern.TopVee), E(drone, 6, 0.35f, SpawnPattern.TopLine, 1f))),
                    Ev.Wave(Wave("S3_W2", E(kamikaze, 6, 0.4f, SpawnPattern.TopRandom), E(shieldDrone, 3, 0.8f, SpawnPattern.TopLine, 1.5f), E(interceptorElite, 1, 0f, SpawnPattern.TopCenter, 2f))),
                    Ev.Wave(Wave("S3_W3", E(bomber, 4, 0.7f, SpawnPattern.TopAlternate), E(interceptor, 4, 0.5f, SpawnPattern.TopVee, 1f), E(kamikaze, 4, 0.5f, SpawnPattern.TopRandom, 2f))),
                    Ev.MiniBoss(reaper, "WARNING: REAPER WING"),
                    Ev.Wave(Wave("S3_W4", E(shieldDrone, 4, 0.6f, SpawnPattern.TopLine), E(bomber, 3, 0.8f, SpawnPattern.TopAlternate, 1f), E(kamikaze, 6, 0.35f, SpawnPattern.TopRandom, 2f), E(shieldElite, 1, 0f, SpawnPattern.TopCenter, 3f))),
                    Ev.Wave(Wave("S3_W5", E(interceptor, 6, 0.35f, SpawnPattern.TopVee), E(drone, 8, 0.3f, SpawnPattern.TopLine, 1f), E(bomberElite, 1, 0f, SpawnPattern.TopLeft, 2f), E(droneElite, 1, 0f, SpawnPattern.TopRight, 2f))),
                    Ev.Delay(1.5f),
                    Ev.Boss(hiveQueen, "WARNING: HIVE QUEEN"),
                });
            var s4 = Stage("Stage4_MechanicalFortress", "Mechanical Fortress", "Swarm forward base - turret grid",
                "A fortress the size of a moon, bristling with cannons.\nTurrets hold their position and fire in every direction: take them out fast.\nThe Swarm rebuilt The Destroyer. It is waiting at the core.",
                "- Silence the turret grid\n- Defeat Sentinel-X Mk.II\n- Destroy the Destroyer Mk.II",
                MusicId.Stage4, AmbientId.Fortress, new Color(0.12f, 0.08f, 0.05f), new Color(0.05f, 0.04f, 0.05f), new Color(1f, 0.5f, 0.2f, 0.06f), 0.3f, new Color(1f, 0.8f, 0.6f),
                new[] { art.Satellite, art.Turret }, 4f, new Color(0.5f, 0.45f, 0.4f, 0.5f), art.FortressWall, new Color(0.35f, 0.3f, 0.32f, 0.6f), 12f, 2500, null, new[]
                {
                    Ev.Message("SECTOR 4: MECHANICAL FORTRESS", 2f),
                    Ev.Wave(Wave("S4_W1", E(turret, 2, 1f, SpawnPattern.TopAlternate), E(drone, 6, 0.4f, SpawnPattern.TopLine, 1.5f))),
                    Ev.Wave(Wave("S4_W2", E(turret, 3, 0.8f, SpawnPattern.TopLine), E(interceptor, 4, 0.5f, SpawnPattern.TopVee, 1.5f), E(kamikaze, 4, 0.5f, SpawnPattern.TopRandom, 2f))),
                    Ev.Message("HEAVY CANNONS ONLINE", 1.5f),
                    Ev.Wave(Wave("S4_W3", E(turret, 4, 0.6f, SpawnPattern.TopLine), E(shieldDrone, 3, 0.7f, SpawnPattern.TopAlternate, 1.5f), E(bomberElite, 1, 0f, SpawnPattern.TopCenter, 2.5f))),
                    Ev.MiniBoss(sentinelMk2, "WARNING: SENTINEL-X MK.II"),
                    Ev.Wave(Wave("S4_W4", E(turret, 3, 0.6f, SpawnPattern.TopLine), E(bomber, 4, 0.7f, SpawnPattern.TopAlternate, 1f), E(interceptorElite, 2, 1f, SpawnPattern.TopAlternate, 2f))),
                    Ev.Wave(Wave("S4_W5", E(turret, 4, 0.5f, SpawnPattern.TopLine), E(kamikaze, 8, 0.3f, SpawnPattern.TopRandom, 1.5f), E(shieldElite, 2, 1f, SpawnPattern.TopAlternate, 2f), E(kamikazeElite, 2, 0.8f, SpawnPattern.TopRandom, 3f))),
                    Ev.Delay(1.5f),
                    Ev.Boss(destroyerMk2, "WARNING: DESTROYER MK.II"),
                });
            var s5 = Stage("Stage5_HiveCore", "Hive Core", "Swarm homeworld - the final assault",
                "This is it: the Swarm homeworld.\nEndless swarms, elite guards and the Omega Core, the machine intelligence behind the invasion.\nIt will transform as it takes damage. Finish it.",
                "- Reach the core\n- Defeat the Widow Prime\n- Destroy the Omega Core",
                MusicId.Stage5, AmbientId.Hive, new Color(0.12f, 0.03f, 0.1f), new Color(0.04f, 0.01f, 0.06f), new Color(0.9f, 0.3f, 0.6f, 0.1f), 0.45f, new Color(1f, 0.75f, 0.9f),
                null, 0f, Color.white, art.HiveWall, new Color(0.45f, 0.15f, 0.35f, 0.55f), 12f, 4000, null, new[]
                {
                    Ev.Message("SECTOR 5: HIVE CORE", 2f),
                    Ev.Wave(Wave("S5_W1", E(drone, 10, 0.25f, SpawnPattern.TopVee), E(kamikaze, 6, 0.35f, SpawnPattern.TopRandom, 1f))),
                    Ev.Wave(Wave("S5_W2", E(interceptor, 6, 0.35f, SpawnPattern.TopLine), E(shieldDrone, 4, 0.6f, SpawnPattern.TopAlternate, 1f), E(droneElite, 2, 0.8f, SpawnPattern.TopAlternate, 2f))),
                    Ev.Wave(Wave("S5_W3", E(bomber, 4, 0.6f, SpawnPattern.TopLine), E(turret, 2, 0.8f, SpawnPattern.TopAlternate, 1f), E(kamikaze, 8, 0.3f, SpawnPattern.TopRandom, 2f), E(interceptorElite, 2, 0.8f, SpawnPattern.TopVee, 3f))),
                    Ev.MiniBoss(widowPrime, "WARNING: WIDOW PRIME"),
                    Ev.Wave(Wave("S5_W4", E(shieldElite, 2, 1f, SpawnPattern.TopAlternate), E(bomberElite, 2, 1f, SpawnPattern.TopAlternate, 1f), E(drone, 10, 0.25f, SpawnPattern.TopLine, 2f), E(kamikazeElite, 3, 0.6f, SpawnPattern.TopRandom, 3f))),
                    Ev.Wave(Wave("S5_W5", E(interceptor, 8, 0.3f, SpawnPattern.TopVee), E(turret, 3, 0.6f, SpawnPattern.TopLine, 1f), E(kamikaze, 10, 0.25f, SpawnPattern.TopRandom, 2f), E(droneElite, 2, 0.6f, SpawnPattern.TopAlternate, 3f), E(shieldElite, 2, 0.8f, SpawnPattern.TopAlternate, 4f))),
                    Ev.Message("THE OMEGA CORE AWAKENS", 2f),
                    Ev.Delay(1.5f),
                    Ev.Boss(omega, "WARNING: OMEGA CORE"),
                });
            data.Stages.AddRange(new[] { s1, s2, s3, s4, s5 });

            data.MenuLook = Stage("MenuLook", "Menu", "", "", "", MusicId.Menu, AmbientId.Space, new Color(0.03f, 0.05f, 0.14f), new Color(0.05f, 0.02f, 0.12f), new Color(0.5f, 0.2f, 0.8f, 0f), 0.8f, Color.white,
                null, 0f, Color.white, art.Planet, new Color(0.15f, 0.35f, 0.6f, 0.35f), 14f, 0, null, new StageEvent[0]);
            data.SurvivalLook = Stage("SurvivalLook", "Deep Space", "Endless", "", "", MusicId.Survival, AmbientId.Space, new Color(0.02f, 0.06f, 0.12f), new Color(0.04f, 0.02f, 0.1f), new Color(0.3f, 0.5f, 0.9f, 0.05f), 0.7f, new Color(0.85f, 0.95f, 1f),
                null, 0f, Color.white, null, Color.white, 6f, 0, null, new StageEvent[0]);

            // ---------------------------------------------------------------- Config + audio
            var config = ProjectBootstrap.CreateOrLoad<GameConfig>(D("Config", "GameConfig"));
            config.Stages = data.Stages.ToArray();
            config.Ships = data.Ships.ToArray();
            config.Weapons = data.Weapons.ToArray();
            config.Bosses = new[] { destroyer, leviathan, hiveQueen, omega };
            config.SurvivalLook = data.SurvivalLook;
            config.ProceduralEnemies = new[] { drone, interceptor, bomber, kamikaze, shieldDrone, supply, interceptorElite, turret };
            config.SurvivalMiniBosses = new[] { sentinel, widow, reaper };
            config.SurvivalMiniBossEvery = 8;
            config.SurvivalWavePause = 2.5f;
            config.DailyEnemySpeedMultiplier = 1.15f;
            config.DailyDropChanceMultiplier = 0.6f;
            config.StartingLives = 3;
            config.MaxLives = 9;
            config.RespawnDelay = 1.5f;
            config.RespawnInvulnerability = 2.5f;
            config.HitInvulnerability = 1f;
            config.SpawnHeightFraction = 0.15f;
            config.PlayAreaWidth = 10f;
            config.PlayerEdgePadding = 0.45f;
            config.DespawnMargin = 2.5f;
            config.MaxMultiplier = 10;
            config.KillsPerMultiplierStep = 4;
            config.UltimateEnergyMax = 100f;
            config.UltimateEliteDamage = 150f;
            config.UltimateClearsEnemyProjectiles = true;
            data.Config = config;

            var audio = ProjectBootstrap.CreateOrLoad<AudioLibrary>(D("Audio", "AudioLibrary"));
            audio.UseSynthesizedPlaceholders = true;
            data.Audio = audio;

            AssetDatabase.SaveAssets();
            return data;
        }

        // ================================================================ builders

        private static ShotSpec S(float offsetX, float angle) => new ShotSpec(offsetX, angle);

        private static WeaponLevel Level(float damageMultiplier, params ShotSpec[] shots) => new WeaponLevel { Shots = shots, DamageMultiplier = damageMultiplier };

        private static WeaponDefinition Weapon(WeaponId id, string name, string description, int cost, PlaceholderArt.Set art, ProjectBootstrap.PrefabSet prefabs,
            float fireInterval, float damage, float speed, float lifetime, float scale, Color color, SfxId sfx, DamageType type, Sprite sprite, WeaponLevel[] levels,
            int pierce = 0, bool homing = false, float homingTurnRate = 180f, float chargeSeconds = 0f, float chargeDamage = 4f, float chargeScale = 2.5f, float splash = 0f)
        {
            var w = ProjectBootstrap.CreateOrLoad<WeaponDefinition>(D("Weapons", id.ToString()));
            w.Id = id;
            w.DisplayName = name;
            w.Description = description;
            w.Cost = cost;
            w.ProjectilePrefab = prefabs.PlayerProjectile;
            w.ProjectileSprite = sprite;
            w.FireInterval = fireInterval;
            w.Damage = damage;
            w.ProjectileSpeed = speed;
            w.ProjectileLifetime = lifetime;
            w.ProjectileScale = scale;
            w.ProjectileColor = color;
            w.DamageType = type;
            w.FireSfx = sfx;
            w.Pierce = pierce;
            w.Homing = homing;
            w.HomingTurnRate = homingTurnRate;
            w.ChargeSeconds = chargeSeconds;
            w.ChargeDamageMultiplier = chargeDamage;
            w.ChargeScaleMultiplier = chargeScale;
            w.SplashRadius = splash;
            w.Levels = levels;
            return w;
        }

        private static ShipDefinition Ship(ShipId id, string name, string description, int cost, Sprite sprite, Color tint,
            float hull, float shield, float regen, float speed, float accel, float decel, float crit, float critMult, float dmg, float ultCharge, float ultPower,
            WeaponDefinition weapon, Color thruster)
        {
            var s = ProjectBootstrap.CreateOrLoad<ShipDefinition>(D("Ships", id.ToString()));
            s.Id = id;
            s.DisplayName = name;
            s.Description = description;
            s.Cost = cost;
            s.MaxHull = hull;
            s.MaxShield = shield;
            s.ShieldRegenPerSecond = regen;
            s.ShieldRegenDelay = 4f;
            s.MoveSpeed = speed;
            s.Acceleration = accel;
            s.Deceleration = decel;
            s.BankAngle = 18f;
            s.CritChance = crit;
            s.CritMultiplier = critMult;
            s.DamageMultiplier = dmg;
            s.UltimateChargeMultiplier = ultCharge;
            s.UltimatePowerMultiplier = ultPower;
            s.Weapon = weapon;
            s.Sprite = sprite;
            s.Tint = tint;
            s.ThrusterColor = thruster;
            s.HitboxRadius = 0.28f;
            s.VisualScale = 1f;
            return s;
        }

        private static PowerUpDefinition PowerUp(PowerUpKind kind, string name, string label, Color color, float duration, float magnitude, float weight, ProjectBootstrap.PrefabSet prefabs)
        {
            var p = ProjectBootstrap.CreateOrLoad<PowerUpDefinition>(D("PowerUps", kind.ToString()));
            p.Kind = kind;
            p.DisplayName = name;
            p.Label = label;
            p.Color = color;
            p.Duration = duration;
            p.Magnitude = magnitude;
            p.Weight = weight;
            p.PickupPrefab = prefabs.Pickup;
            p.FallSpeed = 1.6f;
            p.SwayAmplitude = 0.4f;
            return p;
        }

        private static DropTable DropTable(string name, float chance, PowerUpDefinition[] powerUps)
        {
            var t = ProjectBootstrap.CreateOrLoad<DropTable>(D("PowerUps", "DropTable_" + name));
            t.DropChance = chance;
            var entries = new DropTable.Entry[powerUps.Length];
            for (int i = 0; i < powerUps.Length; i++) entries[i] = new DropTable.Entry { PowerUp = powerUps[i], Weight = 0f };
            t.Entries = entries;
            return t;
        }

        private static MovementParams Move(float speed, float amplitude = 1.5f, float frequency = 2f, float holdHeight = 0.25f, float holdDuration = 6f, float strafeSpeed = 2f, float turnRate = 90f)
            => new MovementParams { Speed = speed, Amplitude = amplitude, Frequency = frequency, HoldHeight = holdHeight, HoldDuration = holdDuration, StrafeSpeed = strafeSpeed, TurnRate = turnRate };

        private static AttackParams Attack(float interval = 2f, float delay = 0.8f, float damage = 10f, float speed = 6f, Color? color = null,
            int count = 1, float spread = 0f, int burst = 1, float burstInterval = 0.12f, float scale = 1f, float lifetime = 5f, float splash = 0f, float slow = 0f)
            => new AttackParams
            {
                Interval = interval, InitialDelay = delay, Count = count, SpreadAngle = spread, BurstCount = burst, BurstInterval = burstInterval,
                ProjectileSpeed = Mathf.Max(0.1f, speed), Damage = damage, ProjectileScale = scale, Lifetime = lifetime, Color = color ?? new Color(1f, 0.4f, 0.3f),
                OnlyWhenOnScreen = true, SplashRadius = splash, SlowSeconds = slow,
            };

        private static EnemyDefinition Enemy(string name, Sprite sprite, Color tint, float scale, float radius, float hull, float shield, float contact, int score, float energy,
            MovementKind movement, MovementParams move, AttackKind attack, AttackParams atk, DropTable drop, bool selfDestruct = false)
        {
            var e = ProjectBootstrap.CreateOrLoad<EnemyDefinition>(D("Enemies", name));
            Fill(e, name, sprite, tint, scale, radius, hull, shield, contact, score, energy, movement, move, attack, atk, drop, selfDestruct);
            return e;
        }

        private static void Fill(EnemyDefinition e, string name, Sprite sprite, Color tint, float scale, float radius, float hull, float shield, float contact, int score, float energy,
            MovementKind movement, MovementParams move, AttackKind attack, AttackParams atk, DropTable drop, bool selfDestruct)
        {
            e.DisplayName = name;
            e.Sprite = sprite;
            e.Tint = tint;
            e.Scale = scale;
            e.ColliderRadius = radius;
            e.MaxHull = hull;
            e.MaxShield = shield;
            e.ContactDamage = contact;
            e.SelfDestructOnContact = selfDestruct;
            e.ScoreValue = score;
            e.EnergyOnKill = energy;
            e.ComponentReward = 0;
            e.DropTable = drop;
            e.IsElite = false;
            e.IsObstacle = false;
            e.ImmuneToUltimate = false;
            e.Movement = movement;
            e.MovementSettings = move;
            e.Attack = attack;
            e.AttackSettings = atk;
            e.MinLifetime = 1.5f;
            e.ExplosionScale = Mathf.Max(0.6f, scale);
            e.ExplosionColor = Color.Lerp(tint, new Color(1f, 0.6f, 0.25f), 0.5f);
            e.HasThruster = true;
            e.ThrusterColor = Color.Lerp(tint, new Color(1f, 0.5f, 0.2f), 0.4f);
        }

        private static EnemyDefinition Elite(EnemyDefinition source, string assetName, string displayName, DropTable drop)
        {
            var e = ProjectBootstrap.CreateOrLoad<EnemyDefinition>(D("Enemies", assetName));
            var atk = source.AttackSettings;
            atk.Interval *= 0.75f;
            atk.Damage *= 1.3f;
            var move = source.MovementSettings;
            move.Speed *= 1.15f;
            Fill(e, displayName, source.Sprite, Color.Lerp(source.Tint, new Color(1f, 0.85f, 0.3f), 0.55f), source.Scale * 1.2f, source.ColliderRadius, source.MaxHull * 2.4f, source.MaxShield * 2f,
                source.ContactDamage * 1.3f, 500, source.EnergyOnKill * 1.5f, source.Movement, move, source.Attack, atk, drop, source.SelfDestructOnContact);
            e.IsElite = true;
            e.ComponentReward = 1;
            e.ShieldColor = new Color(1f, 0.85f, 0.4f, 0.7f);
            e.ThrusterColor = new Color(1f, 0.85f, 0.3f, 0.95f);
            return e;
        }

        private static BossPhase Phase(string name, float threshold, MovementKind movement, MovementParams move, params BossAttack[] attacks)
            => Phase(name, threshold, movement, move, attacks, null, new Color(0, 0, 0, 0), 0f, 0f);

        private static BossPhase Phase(string name, float threshold, MovementKind movement, MovementParams move, BossAttack[] attacks, Sprite sprite, Color tint, float scale, float transformSeconds)
            => new BossPhase { Name = name, HealthThreshold = threshold, Movement = movement, MovementSettings = move, Attacks = attacks, Sprite = sprite, Tint = tint, Scale = scale, TransformSeconds = transformSeconds };

        private static BossAttack BossAtk(BossAttackKind kind, AttackParams settings, float homing = 0f, float telegraph = 1f, float beam = 1.5f, float beamWidth = 1.2f, float beamDps = 40f,
            float muzzle = 0f, EnemyDefinition summon = null, int cap = 0)
            => new BossAttack
            {
                Kind = kind, Settings = settings, HomingTurnRate = homing, TelegraphSeconds = telegraph, BeamSeconds = beam, BeamWidth = beamWidth, BeamDamagePerSecond = beamDps,
                MuzzleOffset = muzzle, SummonEnemy = summon, SummonCap = cap,
            };

        private static BossDefinition Boss(string asset, string title, BossId id, Pooling.PooledObject prefab, Sprite sprite, Color tint, float scale, float radius,
            float hull, float shield, float contact, int score, float energy, int components, DropTable drop, float entrance, float entranceHeight, int explosions, float deathSeconds, BossPhase[] phases,
            int segments = 0, Sprite segmentSprite = null, float segmentSpacing = 0.9f, float segmentScale = 0.8f)
        {
            var b = ProjectBootstrap.CreateOrLoad<BossDefinition>(D("Bosses", asset));
            Fill(b, title, sprite, tint, scale, radius, hull, shield, contact, score, energy, MovementKind.LateralPatrol, Move(1f), AttackKind.None, Attack(), drop, false);
            b.ComponentReward = components;
            b.BossId = id;
            b.Title = title;
            b.EntranceDuration = entrance;
            b.EntranceHeight = entranceHeight;
            b.Phases = phases;
            b.DeathExplosions = explosions;
            b.DeathSequenceSeconds = deathSeconds;
            b.Prefab = prefab;
            b.MinLifetime = 999f;
            b.ExplosionScale = scale * 1.2f;
            b.SegmentCount = segments;
            b.SegmentSprite = segmentSprite;
            b.SegmentSpacing = segmentSpacing;
            b.SegmentScale = segmentScale;
            b.HasThruster = segments == 0;
            return b;
        }

        /// <summary>Stronger recolored copy of a boss (Mk.II / Prime variants).</summary>
        private static BossDefinition Variant(BossDefinition source, string asset, string title, Color tint, float hull, float attackSpeed, int components)
        {
            var b = ProjectBootstrap.CreateOrLoad<BossDefinition>(D("Bosses", asset));
            EditorUtility.CopySerialized(source, b);
            b.name = asset;
            b.DisplayName = title;
            b.Title = title;
            b.Tint = tint;
            b.MaxHull = hull;
            b.ComponentReward = components;
            var phases = new BossPhase[source.Phases.Length];
            for (int i = 0; i < phases.Length; i++)
            {
                var p = source.Phases[i];
                var attacks = new BossAttack[p.Attacks != null ? p.Attacks.Length : 0];
                for (int a = 0; a < attacks.Length; a++)
                {
                    var atk = p.Attacks[a];
                    atk.Settings.Interval /= attackSpeed;
                    atk.Settings.ProjectileSpeed *= 1.1f;
                    attacks[a] = atk;
                }
                p.Attacks = attacks;
                p.Name = p.Name.Replace(source.Title, title);
                phases[i] = p;
            }
            b.Phases = phases;
            EditorUtility.SetDirty(b);
            return b;
        }

        private static SpawnEntry E(EnemyDefinition enemy, int count, float interval, SpawnPattern pattern, float delayBefore = 0f, float patternValue = 0.5f)
            => new SpawnEntry { Enemy = enemy, Count = count, Interval = interval, DelayBefore = delayBefore, Pattern = pattern, PatternValue = patternValue };

        private static WaveDefinition Wave(string name, params SpawnEntry[] entries)
        {
            var w = ProjectBootstrap.CreateOrLoad<WaveDefinition>(D("Waves", name));
            w.Label = name;
            w.Entries = entries;
            w.WaitForClear = true;
            w.MaxDuration = 45f;
            return w;
        }

        private static class Ev
        {
            public static StageEvent Wave(WaveDefinition w) => new StageEvent { Type = StageEventType.Wave, Wave = w };
            public static StageEvent Delay(float s) => new StageEvent { Type = StageEventType.Delay, Seconds = s };
            public static StageEvent Message(string m, float s) => new StageEvent { Type = StageEventType.Message, Message = m, Seconds = s };
            public static StageEvent MiniBoss(BossDefinition b, string warning) => new StageEvent { Type = StageEventType.MiniBoss, Boss = b, Message = warning };
            public static StageEvent Boss(BossDefinition b, string warning) => new StageEvent { Type = StageEventType.Boss, Boss = b, Message = warning };
            public static StageEvent Asteroids(bool on) => new StageEvent { Type = StageEventType.AsteroidField, Flag = on };
        }

        private static StageDefinition Stage(string asset, string name, string subtitle, string briefing, string objectives, MusicId music, AmbientId ambient,
            Color top, Color bottom, Color fog, float stars, Color starTint, Sprite[] debris, float debrisInterval, Color debrisTint,
            Sprite backdrop, Color backdropTint, float backdropScale, int bonus, EnemyDefinition asteroid, StageEvent[] events)
        {
            var s = ProjectBootstrap.CreateOrLoad<StageDefinition>(D("Stages", asset));
            s.DisplayName = name;
            s.Subtitle = subtitle;
            s.Briefing = briefing;
            s.Objectives = objectives;
            s.Events = events;
            s.CompletionBonus = bonus;
            s.Music = music;
            s.Ambient = ambient;
            s.KeepBossMusicAfterDefeat = false;
            s.BackgroundTop = top;
            s.BackgroundBottom = bottom;
            s.Fog = fog;
            s.StarDensity = stars;
            s.StarTint = starTint;
            s.DebrisSprites = debris ?? new Sprite[0];
            s.DebrisInterval = debrisInterval;
            s.DebrisTint = debrisTint;
            s.BackdropSprite = backdrop;
            s.BackdropTint = backdropTint;
            s.BackdropScale = backdropScale;
            s.AsteroidDefinition = asteroid;
            s.AsteroidInterval = 1.2f;
            return s;
        }
    }
}
