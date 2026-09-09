using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Starfall.EditorTools
{
    /// <summary>
    /// Generates clearly-temporary white sprites (tinted at runtime) so the game has no external art dependency.
    /// All shapes are simple polygons/circles rendered with anti-aliased signed distance fields.
    /// Output: Assets/_Project/Art/Placeholders/*.png
    /// </summary>
    public static class PlaceholderArt
    {
        public const string Folder = "Assets/_Project/Art/Placeholders";

        public sealed class Set
        {
            public Sprite Pixel, Dot, Circle, Ring, Gradient, Panel, Spark, Vignette;
            public Sprite Ship, Falcon, Titan, Phantom, NovaX, Symbiont, Nexus, Drone2, Flame;
            public Sprite Projectile, Bullet, Plasma, Missile, Rail, Web, Spore;
            public Sprite Drone, Interceptor, Bomber, Kamikaze, ShieldDrone, Asteroid, Turret;
            public Sprite SentinelX, Widow, ReaperWing, Destroyer, LeviathanHead, LeviathanSegment, HiveQueen, OmegaCore, OmegaCoreForm2, OmegaCoreForm3;
            public Sprite IronWarden, Bastion, CorsairQueen, Assembler, AetherGuardian, RiftWalker, Crystal, Fabricator, TurretPart;
            public Sprite Satellite, Planet, FortressWall, HiveWall, Colony, Ruins, Rift, StarCore, Logo;
        }

        public static Set GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            FinalAssets.ResetCache();
            var set = new Set();

            set.Pixel = Save("pixel", 8, (x, y) => -1f, ppu: 100, fullRect: true);
            set.Dot = Save("dot", 64, (x, y) => SoftCircle(x, y, 0.95f), soft: true);
            set.Spark = Save("spark", 32, (x, y) => Capsule(x, y, 0.18f, 0.7f), soft: true);
            set.Circle = Save("circle", 128, (x, y) => Circle(x, y, 0.95f));
            set.Ring = Save("ring", 128, (x, y) => Ring(x, y, 0.92f, 0.12f));
            // Always procedural (not part of the final-art spec): a smooth edge glow stretched over the screen by RiskVignette.
            set.Vignette = Save("vignette", 128, (x, y) => 0.55f - Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)), soft: true);
            set.Gradient = SaveGradient("gradient", 4, 256);
            set.Panel = Save("panel", 64, (x, y) => RoundedBox(x, y, 0.95f, 0.95f, 0.25f), ppu: 64, fullRect: true);

            // ---- Player ships (GDD §15) ----
            set.Ship = Save("ship", 96, (x, y) => Union(
                Polygon(x, y, P(0f, 1f), P(0.28f, -0.2f), P(0.16f, -0.45f), P(-0.16f, -0.45f), P(-0.28f, -0.2f)),
                Polygon(x, y, P(0.2f, 0.0f), P(0.95f, -0.55f), P(0.9f, -0.85f), P(0.15f, -0.6f)),
                Polygon(x, y, P(-0.2f, 0.0f), P(-0.95f, -0.55f), P(-0.9f, -0.85f), P(-0.15f, -0.6f))));
            set.Falcon = Save("falcon", 96, (x, y) => Union(
                Polygon(x, y, P(0f, 1f), P(0.18f, -0.5f), P(-0.18f, -0.5f)),
                Polygon(x, y, P(0.1f, -0.1f), P(0.7f, -0.9f), P(0.45f, -0.95f), P(0.05f, -0.55f)),
                Polygon(x, y, P(-0.1f, -0.1f), P(-0.7f, -0.9f), P(-0.45f, -0.95f), P(-0.05f, -0.55f))));
            set.Titan = Save("titan", 96, (x, y) => Union(
                RoundedBox(x, y + 0.1f, 0.42f, 0.7f, 0.2f),
                Polygon(x, y, P(0f, 1f), P(0.35f, 0.5f), P(-0.35f, 0.5f)),
                Box(x + 0.72f, y - 0.3f, 0.25f, 0.55f), Box(x - 0.72f, y - 0.3f, 0.25f, 0.55f)));
            set.Phantom = Save("phantom", 96, (x, y) => Union(
                Polygon(x, y, P(0f, 1f), P(0.22f, 0.2f), P(0.1f, -0.7f), P(-0.1f, -0.7f), P(-0.22f, 0.2f)),
                Polygon(x, y, P(0.15f, 0.3f), P(0.95f, -0.2f), P(0.6f, -0.75f), P(0.12f, -0.3f)),
                Polygon(x, y, P(-0.15f, 0.3f), P(-0.95f, -0.2f), P(-0.6f, -0.75f), P(-0.12f, -0.3f))));
            set.NovaX = Save("novax", 112, (x, y) => Union(
                Star(x, y, 4, 1f, 0.32f),
                Polygon(x, y, P(0f, 1f), P(0.2f, -0.1f), P(-0.2f, -0.1f)),
                Circle(x, y - 0.1f, 0.22f)));
            // Biomech: organic hull with tendrils. Cyber: hard hexagon with wing blades and a small drone.
            set.Symbiont = Save("symbiont", 96, (x, y) => Union(
                Capsule(x, y + 0.05f, 0.34f, 0.55f), Circle(x, y + 0.55f, 0.3f),
                Leg(x, y, 0.25f, 0.1f, 0.85f, -0.5f, 0.09f), Leg(x, y, -0.25f, 0.1f, -0.85f, -0.5f, 0.09f),
                Leg(x, y, 0.2f, -0.3f, 0.55f, -0.95f, 0.08f), Leg(x, y, -0.2f, -0.3f, -0.55f, -0.95f, 0.08f),
                Circle(x + 0.85f, y - 0.5f, 0.13f), Circle(x - 0.85f, y - 0.5f, 0.13f)));
            set.Nexus = Save("nexus", 96, (x, y) => Union(
                RegularPolygon(x, y, 6, 0.5f), Polygon(x, y, P(0f, 1f), P(0.2f, 0.4f), P(-0.2f, 0.4f)),
                Polygon(x, y, P(0.35f, 0.1f), P(0.98f, -0.3f), P(0.85f, -0.7f), P(0.3f, -0.4f)),
                Polygon(x, y, P(-0.35f, 0.1f), P(-0.98f, -0.3f), P(-0.85f, -0.7f), P(-0.3f, -0.4f)),
                Ring(x, y, 0.28f, 0.05f) * -1f + 0.1f));
            set.Drone2 = Save("companiondrone", 40, (x, y) => Union(RegularPolygon(x, y, 6, 0.8f), Circle(x, y, 0.3f) * -1f + 0.12f));
            set.Spore = Save("spore", 32, (x, y) => Union(SoftCircle(x, y, 0.75f), SoftCircle(x + 0.35f, y + 0.3f, 0.35f), SoftCircle(x - 0.3f, y - 0.35f, 0.3f)), soft: true);
            set.Flame = Save("flame", 32, (x, y) => Union(Polygon(x, y, P(-0.5f, 1f), P(0.5f, 1f), P(0f, -1f)), Circle(x, y - 0.6f, 0.45f)), soft: true);

            // ---- Projectiles ----
            set.Projectile = Save("projectile", 32, (x, y) => Capsule(x, y, 0.28f, 0.9f), soft: true);
            set.Bullet = Save("bullet", 32, (x, y) => SoftCircle(x, y, 0.9f), soft: true);
            set.Plasma = Save("plasma", 48, (x, y) => SoftCircle(x, y, 0.85f), soft: true);
            set.Missile = Save("missile", 40, (x, y) => Union(Capsule(x, y, 0.22f, 0.75f), Polygon(x, y, P(-0.5f, -0.6f), P(0.5f, -0.6f), P(0f, -0.95f))));
            set.Rail = Save("rail", 32, (x, y) => Capsule(x, y, 0.12f, 0.98f), soft: true);
            set.Web = Save("web", 96, (x, y) => Union(Ring(x, y, 0.85f, 0.05f), Ring(x, y, 0.5f, 0.05f), Ring(x, y, 0.2f, 0.06f),
                Capsule(x, y, 0.04f, 0.9f), Capsule(y, x, 0.04f, 0.9f), Capsule((x + y) * 0.7071f, (y - x) * 0.7071f, 0.04f, 0.9f), Capsule((x - y) * 0.7071f, (x + y) * 0.7071f, 0.04f, 0.9f)));

            // ---- Enemies ----
            set.Drone = Save("drone", 80, (x, y) => Union(RegularPolygon(x, y, 6, 0.9f), Circle(x, y, 0.35f) * -1f + 0.15f));
            set.Interceptor = Save("interceptor", 80, (x, y) => Union(
                Polygon(x, y, P(0f, -1f), P(0.35f, 0.4f), P(0f, 0.8f), P(-0.35f, 0.4f)),
                Polygon(x, y, P(0.2f, 0.2f), P(0.95f, 0.7f), P(0.75f, 0.85f), P(0.1f, 0.55f)),
                Polygon(x, y, P(-0.2f, 0.2f), P(-0.95f, 0.7f), P(-0.75f, 0.85f), P(-0.1f, 0.55f))));
            set.Bomber = Save("bomber", 96, (x, y) => Union(
                Polygon(x, y, P(-0.95f, 0.1f), P(-0.5f, 0.6f), P(0.5f, 0.6f), P(0.95f, 0.1f), P(0.6f, -0.6f), P(-0.6f, -0.6f)),
                Capsule(x, y + 0.1f, 0.22f, 0.55f) - 0.02f));
            set.Kamikaze = Save("kamikaze", 80, (x, y) => Union(Star(x, y, 4, 0.95f, 0.35f), Circle(x, y, 0.3f)));
            set.ShieldDrone = Save("shielddrone", 80, (x, y) => Union(RoundedBox(x, y, 0.62f, 0.62f, 0.15f), Polygon(x, y, P(0f, 0.95f), P(0.25f, 0.5f), P(-0.25f, 0.5f))));
            set.Asteroid = Save("asteroid", 96, (x, y) => Polygon(x, y,
                P(-0.9f, 0.1f), P(-0.6f, 0.7f), P(-0.1f, 0.9f), P(0.5f, 0.75f), P(0.95f, 0.2f), P(0.8f, -0.5f), P(0.3f, -0.9f), P(-0.4f, -0.8f), P(-0.85f, -0.4f)));
            set.Turret = Save("turret", 96, (x, y) => Union(RegularPolygon(x, y, 8, 0.85f), Capsule(x, y - 0.55f, 0.16f, 0.45f), Capsule(x + 0.45f, y - 0.5f, 0.1f, 0.35f), Capsule(x - 0.45f, y - 0.5f, 0.1f, 0.35f)));

            // ---- Mini-bosses ----
            set.SentinelX = Save("sentinelx", 256, (x, y) => Union(
                RegularPolygon(x, y, 8, 0.75f),
                Polygon(x, y, P(-1f, -0.15f), P(-0.7f, 0f), P(-1f, 0.15f)),
                Polygon(x, y, P(1f, -0.15f), P(0.7f, 0f), P(1f, 0.15f)),
                Polygon(x, y, P(-0.15f, 1f), P(0f, 0.7f), P(0.15f, 1f)),
                Polygon(x, y, P(-0.15f, -1f), P(0f, -0.7f), P(0.15f, -1f)),
                Ring(x, y, 0.55f, 0.06f) * -1f + 0.1f));
            set.Widow = Save("widow", 256, (x, y) => Union(
                Circle(x, y + 0.1f, 0.42f), Circle(x, y - 0.35f, 0.28f),
                Leg(x, y, 0.3f, 0.2f, 0.95f, 0.55f), Leg(x, y, -0.3f, 0.2f, -0.95f, 0.55f),
                Leg(x, y, 0.35f, 0f, 0.98f, -0.05f), Leg(x, y, -0.35f, 0f, -0.98f, -0.05f),
                Leg(x, y, 0.3f, -0.2f, 0.85f, -0.6f), Leg(x, y, -0.3f, -0.2f, -0.85f, -0.6f),
                Leg(x, y, 0.2f, -0.35f, 0.55f, -0.95f), Leg(x, y, -0.2f, -0.35f, -0.55f, -0.95f)));
            set.ReaperWing = Save("reaperwing", 224, (x, y) => Union(
                Polygon(x, y, P(0f, -1f), P(0.22f, 0.2f), P(0f, 0.7f), P(-0.22f, 0.2f)),
                Polygon(x, y, P(0.15f, 0f), P(1f, 0.55f), P(0.95f, 0.85f), P(0.35f, 0.75f), P(0.1f, 0.45f)),
                Polygon(x, y, P(-0.15f, 0f), P(-1f, 0.55f), P(-0.95f, 0.85f), P(-0.35f, 0.75f), P(-0.1f, 0.45f))));

            // ---- Bosses ----
            set.Destroyer = Save("destroyer", 384, (x, y) => Union(
                Polygon(x, y, P(-0.35f, 0.95f), P(0.35f, 0.95f), P(0.5f, 0.2f), P(0.25f, -0.95f), P(-0.25f, -0.95f), P(-0.5f, 0.2f)),
                Polygon(x, y, P(-0.45f, 0.5f), P(-1f, 0.35f), P(-1f, -0.2f), P(-0.45f, -0.4f)),
                Polygon(x, y, P(0.45f, 0.5f), P(1f, 0.35f), P(1f, -0.2f), P(0.45f, -0.4f)),
                Capsule(x + 0.78f, y - 0.35f, 0.09f, 0.35f),
                Capsule(x - 0.78f, y - 0.35f, 0.09f, 0.35f)));
            set.LeviathanHead = Save("leviathan_head", 192, (x, y) => Union(
                Polygon(x, y, P(0f, -1f), P(0.6f, -0.2f), P(0.7f, 0.6f), P(0.3f, 0.95f), P(-0.3f, 0.95f), P(-0.7f, 0.6f), P(-0.6f, -0.2f)),
                Polygon(x, y, P(0.55f, 0.5f), P(0.95f, 0.95f), P(0.75f, 0.2f)), Polygon(x, y, P(-0.55f, 0.5f), P(-0.95f, 0.95f), P(-0.75f, 0.2f))));
            set.LeviathanSegment = Save("leviathan_segment", 96, (x, y) => Union(RoundedBox(x, y, 0.75f, 0.6f, 0.3f), Polygon(x, y, P(-0.9f, 0.7f), P(-0.5f, 0.2f), P(-0.85f, -0.2f)), Polygon(x, y, P(0.9f, 0.7f), P(0.5f, 0.2f), P(0.85f, -0.2f))));
            set.HiveQueen = Save("hivequeen", 384, (x, y) => Union(
                Circle(x, y + 0.35f, 0.5f), Capsule(x, y - 0.35f, 0.4f, 0.55f),
                RegularPolygon(x + 0.7f, y + 0.5f, 6, 0.28f), RegularPolygon(x - 0.7f, y + 0.5f, 6, 0.28f),
                RegularPolygon(x + 0.75f, y - 0.2f, 6, 0.25f), RegularPolygon(x - 0.75f, y - 0.2f, 6, 0.25f),
                Polygon(x, y, P(0.15f, 0.8f), P(0.45f, 1f), P(0.3f, 0.6f)), Polygon(x, y, P(-0.15f, 0.8f), P(-0.45f, 1f), P(-0.3f, 0.6f))));
            set.OmegaCore = Save("omegacore", 384, (x, y) => Union(RegularPolygon(x, y, 8, 0.9f), Ring(x, y, 0.6f, 0.05f) * -1f + 0.08f, Circle(x, y, 0.3f) * -1f + 0.12f));
            set.OmegaCoreForm2 = Save("omegacore2", 384, (x, y) => Union(Star(x, y, 8, 1f, 0.55f), Circle(x, y, 0.35f) * -1f + 0.1f));
            set.OmegaCoreForm3 = Save("omegacore3", 384, (x, y) => Union(Star(x, y, 6, 1f, 0.3f), Ring(x, y, 0.5f, 0.1f), Circle(x, y, 0.2f)));

            // ---- Bosses (phase E) ----
            set.IronWarden = Save("ironwarden", 320, (x, y) => Union(
                RoundedBox(x, y, 0.55f, 0.5f, 0.15f),
                Polygon(x, y, P(-0.55f, 0.3f), P(-1f, 0.1f), P(-1f, -0.5f), P(-0.6f, -0.35f)), Polygon(x, y, P(0.55f, 0.3f), P(1f, 0.1f), P(1f, -0.5f), P(0.6f, -0.35f)),
                Capsule(x - 0.85f, y - 0.7f, 0.12f, 0.3f), Capsule(x + 0.85f, y - 0.7f, 0.12f, 0.3f),
                Polygon(x, y, P(-0.25f, -0.5f), P(0.25f, -0.5f), P(0f, -1f))));
            set.Bastion = Save("bastion", 384, (x, y) => Union(
                Box(x, y + 0.3f, 1f, 0.35f), RoundedBox(x, y - 0.2f, 0.45f, 0.35f, 0.1f),
                Box(x - 0.75f, y - 0.25f, 0.15f, 0.3f), Box(x + 0.75f, y - 0.25f, 0.15f, 0.3f),
                Capsule(x, y - 0.75f, 0.14f, 0.3f)), fullRect: false);
            set.CorsairQueen = Save("corsairqueen", 320, (x, y) => Union(
                Polygon(x, y, P(0f, 1f), P(0.4f, 0.2f), P(0.3f, -0.8f), P(-0.3f, -0.8f), P(-0.4f, 0.2f)),
                Polygon(x, y, P(0.3f, 0.4f), P(1f, 0.9f), P(0.95f, -0.2f), P(0.35f, -0.4f)), Polygon(x, y, P(-0.3f, 0.4f), P(-1f, 0.9f), P(-0.95f, -0.2f), P(-0.35f, -0.4f)),
                Star(x, y - 0.3f, 4, 0.25f, 0.1f)));
            set.Assembler = Save("assembler", 384, (x, y) => Union(
                Box(x, y, 0.9f, 0.5f), Box(x - 0.6f, y + 0.7f, 0.2f, 0.3f), Box(x + 0.6f, y + 0.7f, 0.2f, 0.3f), Box(x, y + 0.7f, 0.2f, 0.3f),
                Ring(x - 0.5f, y - 0.1f, 0.22f, 0.05f) * -1f + 0.08f, Ring(x + 0.5f, y - 0.1f, 0.22f, 0.05f) * -1f + 0.08f,
                Capsule(x, y - 0.75f, 0.2f, 0.3f)));
            set.AetherGuardian = Save("aetherguardian", 320, (x, y) => Union(
                Star(x, y, 3, 1f, 0.45f), Ring(x, y, 0.55f, 0.06f), Circle(x, y, 0.25f)));
            set.RiftWalker = Save("riftwalker", 320, (x, y) => Union(
                Capsule(x, y, 0.3f, 0.8f), Leg(x, y, 0.2f, 0.4f, 0.95f, 0.9f, 0.08f), Leg(x, y, -0.2f, 0.4f, -0.95f, 0.9f, 0.08f),
                Leg(x, y, 0.2f, -0.4f, 0.9f, -0.9f, 0.08f), Leg(x, y, -0.2f, -0.4f, -0.9f, -0.9f, 0.08f), Ring(x, y, 0.5f, 0.04f) * -1f + 0.06f));
            set.Crystal = Save("crystal", 64, (x, y) => Polygon(x, y, P(0f, 1f), P(0.55f, 0.2f), P(0.3f, -0.9f), P(-0.3f, -0.9f), P(-0.55f, 0.2f)));
            set.Fabricator = Save("fabricator", 64, (x, y) => Union(RoundedBox(x, y, 0.7f, 0.5f, 0.15f), Ring(x, y, 0.3f, 0.05f) * -1f + 0.08f));
            set.TurretPart = Save("turretpart", 64, (x, y) => Union(RegularPolygon(x, y, 8, 0.7f), Capsule(x, y - 0.5f, 0.15f, 0.45f)));

            // ---- Environment ----
            set.Satellite = Save("satellite", 96, (x, y) => Union(
                RoundedBox(x, y, 0.25f, 0.35f, 0.05f),
                Box(x + 0.62f, y, 0.35f, 0.18f), Box(x - 0.62f, y, 0.35f, 0.18f),
                Capsule(x, y + 0.55f, 0.04f, 0.25f)));
            set.Planet = Save("planet", 256, (x, y) => SoftCircle(x, y, 0.95f), soft: true);
            set.FortressWall = Save("fortresswall", 256, (x, y) => Union(
                Box(x, y, 0.95f, 0.25f), Box(x - 0.6f, y + 0.45f, 0.15f, 0.25f), Box(x, y + 0.5f, 0.15f, 0.3f), Box(x + 0.6f, y + 0.45f, 0.15f, 0.25f),
                Box(x - 0.6f, y - 0.5f, 0.15f, 0.3f), Box(x + 0.6f, y - 0.5f, 0.15f, 0.3f), Capsule(x, y - 0.6f, 0.08f, 0.3f)), fullRect: true);
            set.HiveWall = Save("hivewall", 256, (x, y) => Union(
                RegularPolygon(x, y, 6, 0.32f), RegularPolygon(x + 0.58f, y + 0.33f, 6, 0.32f), RegularPolygon(x - 0.58f, y + 0.33f, 6, 0.32f),
                RegularPolygon(x + 0.58f, y - 0.33f, 6, 0.32f), RegularPolygon(x - 0.58f, y - 0.33f, 6, 0.32f), RegularPolygon(x, y + 0.66f, 6, 0.32f), RegularPolygon(x, y - 0.66f, 6, 0.32f)), fullRect: true);
            set.Colony = Save("colony", 256, (x, y) => Union(SoftCircle(x - 0.5f, y - 0.4f, 0.45f), SoftCircle(x + 0.35f, y - 0.5f, 0.35f), SoftCircle(x + 0.1f, y + 0.2f, 0.3f), Box(x, y - 0.85f, 0.95f, 0.1f)), soft: true, fullRect: true);
            set.Ruins = Save("ruins", 256, (x, y) => Union(Box(x - 0.7f, y, 0.1f, 0.9f), Box(x - 0.25f, y - 0.2f, 0.1f, 0.7f), Box(x + 0.25f, y + 0.1f, 0.1f, 0.8f), Box(x + 0.7f, y - 0.3f, 0.1f, 0.6f), Box(x, y + 0.9f, 0.95f, 0.08f)), fullRect: true);
            set.Rift = Save("rift", 256, (x, y) => Union(Ring(x, y, 0.85f, 0.05f), Ring(x, y, 0.6f, 0.04f), Ring(x, y, 0.35f, 0.04f), SoftCircle(x, y, 0.25f)), soft: true);
            set.StarCore = Save("starcore", 256, (x, y) => SoftCircle(x, y, 0.7f), soft: true);
            set.Logo = Save("logo", 256, (x, y) => Union(Star(x, y, 5, 0.9f, 0.4f), Ring(x, y, 0.95f, 0.04f)));

            AssetDatabase.SaveAssets();
            return set;
        }

        // ---- Rendering ------------------------------------------------------------------------------

        private static Sprite Save(string name, int size, Func<float, float, float> sdf, bool soft = false, int ppu = 100, bool fullRect = false)
        {
            // Final art (docs/ASSET_SPEC.md) wins over the procedural placeholder when the file exists.
            if (FinalAssets.TryLoadSprite(name, size, fullRect, out var finalSprite)) return finalSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            float aa = 2.5f / size;
            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f;
                    float y = (py + 0.5f) / size * 2f - 1f;
                    float d = sdf(x, y);
                    float alpha = soft ? Mathf.Clamp01(-d * 2.2f) : Mathf.Clamp01(0.5f - d / aa);
                    byte a = (byte)Mathf.RoundToInt(alpha * 255f);
                    pixels[py * size + px] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return WritePng(name, tex, ppu, fullRect);
        }

        private static Sprite SaveGradient(string name, int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                byte a = (byte)Mathf.RoundToInt(Mathf.SmoothStep(0f, 1f, t) * 255f);
                for (int x = 0; x < width; x++) pixels[y * width + x] = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return WritePng(name, tex, 100, true);
        }

        private static Sprite WritePng(string name, Texture2D tex, int ppu, bool fullRect)
        {
            string path = $"{Folder}/{name}.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.wrapMode = TextureWrapMode.Clamp;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = fullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
                settings.spriteGenerateFallbackPhysicsShape = false;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) Debug.LogError($"[Starfall] Failed to import placeholder sprite {path}");
            return sprite;
        }

        // ---- Signed distance primitives (negative = inside) ----------------------------------------------

        private static Vector2 P(float x, float y) => new Vector2(x, y);

        private static float Union(params float[] distances)
        {
            float d = float.MaxValue;
            for (int i = 0; i < distances.Length; i++) d = Mathf.Min(d, distances[i]);
            return d;
        }

        private static float Circle(float x, float y, float r) => Mathf.Sqrt(x * x + y * y) - r;

        private static float SoftCircle(float x, float y, float r) => (Mathf.Sqrt(x * x + y * y) - r) / r;

        private static float Ring(float x, float y, float r, float thickness) => Mathf.Abs(Mathf.Sqrt(x * x + y * y) - r) - thickness;

        private static float Box(float x, float y, float hw, float hh)
        {
            float dx = Mathf.Abs(x) - hw;
            float dy = Mathf.Abs(y) - hh;
            float outside = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));
            return outside + Mathf.Min(Mathf.Max(dx, dy), 0f);
        }

        private static float RoundedBox(float x, float y, float hw, float hh, float r) => Box(x, y, hw - r, hh - r) - r;

        private static float Capsule(float x, float y, float r, float halfLength)
        {
            float cy = Mathf.Clamp(y, -halfLength, halfLength);
            return Mathf.Sqrt(x * x + (y - cy) * (y - cy)) - r;
        }

        /// <summary>Line segment with thickness (spider legs).</summary>
        private static float Leg(float x, float y, float ax, float ay, float bx, float by, float r = 0.06f)
        {
            float px = x - ax, py = y - ay, ex = bx - ax, ey = by - ay;
            float h = Mathf.Clamp01((px * ex + py * ey) / (ex * ex + ey * ey));
            float dx = px - ex * h, dy = py - ey * h;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        private static float RegularPolygon(float x, float y, int sides, float r)
        {
            var pts = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float a = (i / (float)sides) * Mathf.PI * 2f + Mathf.PI / sides;
                pts[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }
            return Polygon(x, y, pts);
        }

        private static float Star(float x, float y, int points, float outer, float inner)
        {
            var pts = new Vector2[points * 2];
            for (int i = 0; i < points * 2; i++)
            {
                float a = (i / (float)(points * 2)) * Mathf.PI * 2f + Mathf.PI / 2f;
                float r = i % 2 == 0 ? outer : inner;
                pts[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }
            return Polygon(x, y, pts);
        }

        private static float Polygon(float x, float y, params Vector2[] v)
        {
            var p = new Vector2(x, y);
            int n = v.Length;
            float d = Vector2.Dot(p - v[0], p - v[0]);
            float s = 1f;
            for (int i = 0, j = n - 1; i < n; j = i, i++)
            {
                Vector2 e = v[j] - v[i];
                Vector2 w = p - v[i];
                float t = Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                Vector2 b = w - e * t;
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s *= -1f;
            }
            return s * Mathf.Sqrt(d);
        }
    }
}
