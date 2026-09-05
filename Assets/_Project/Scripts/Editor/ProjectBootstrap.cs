using System;
using System.IO;
using Starfall.Audio;
using Starfall.Bosses;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Logic;
using Starfall.Player;
using Starfall.Pooling;
using Starfall.PowerUps;
using Starfall.VFX;
using Starfall.Waves;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Starfall.EditorTools
{
    /// <summary>
    /// One-shot project generator. Creates settings, placeholder art, data assets, prefabs and scenes so the
    /// project is playable right after cloning. Idempotent: re-running updates existing assets in place.
    ///
    /// Batch usage:
    ///   Unity.exe -batchmode -projectPath . -executeMethod Starfall.EditorTools.ProjectBootstrap.PrepareProject -quit
    ///   Unity.exe -batchmode -projectPath . -executeMethod Starfall.EditorTools.ProjectBootstrap.GenerateAll -quit
    /// </summary>
    public static class ProjectBootstrap
    {
        public const string DataRoot = "Assets/_Project/ScriptableObjects";
        public const string PrefabRoot = "Assets/_Project/Prefabs";
        public const string SceneRoot = "Assets/_Project/Scenes";

        public sealed class PrefabSet
        {
            public PooledObject PlayerProjectile, EnemyProjectile, Enemy, SentinelX, Destroyer, Pickup, Explosion, FloatingText;
            public GameObject Player;
        }

        public sealed class DataSet
        {
            public GameConfig Config;
            public AudioLibrary Audio;
            public ShipDefinition Vanguard;
            public WeaponDefinition Laser;
            public EnemyDefinition Drone, DroneSupply, Interceptor, Bomber, Kamikaze, ShieldDrone, Asteroid;
            public BossDefinition SentinelX, Destroyer;
            public PowerUpDefinition[] PowerUps;
            public DropTable DefaultDrops, RichDrops;
            public StageDefinition[] Stages;
            public StageDefinition MenuLook;
        }

        // ---- Entry points ------------------------------------------------------------------------------

        [MenuItem("Starfall/1. Prepare Project (settings + TextMeshPro)")]
        public static void PrepareProject()
        {
            Debug.Log("[Starfall] PrepareProject: start");
            ConfigureLayersAndPhysics();
            ConfigureInputHandler();
            ConfigurePlayerSettings();
            EnsureTmpResources();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Starfall] PrepareProject: done");
        }

        [MenuItem("Starfall/2. Generate Assets, Prefabs and Scenes")]
        public static void GenerateAll()
        {
            Debug.Log("[Starfall] GenerateAll: start");
            ConfigureLayersAndPhysics();
            EnsureFolders();
            var art = PlaceholderArt.GenerateAll();
            UiBuilder.PanelSprite = art.Panel;
            var prefabs = CreatePooledPrefabs(art);
            var data = CreateData(art, prefabs);
            prefabs.Player = CreatePlayerPrefab(art, data);
            SceneBuilder.BuildAll(art, data, prefabs);
            ConfigureBuildSettings(data.Config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Starfall] GenerateAll: done");
        }

        [MenuItem("Starfall/Run Full Bootstrap")]
        public static void Full()
        {
            PrepareProject();
            GenerateAll();
        }

        // ---- Project settings ------------------------------------------------------------------------------

        private static void ConfigureLayersAndPhysics()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets != null && assets.Length > 0)
            {
                var tagManager = new SerializedObject(assets[0]);
                var layers = tagManager.FindProperty("layers");
                SetLayer(layers, GameLayers.Player, "Player");
                SetLayer(layers, GameLayers.PlayerProjectile, "PlayerProjectile");
                SetLayer(layers, GameLayers.Enemy, "Enemy");
                SetLayer(layers, GameLayers.EnemyProjectile, "EnemyProjectile");
                SetLayer(layers, GameLayers.PowerUp, "PowerUp");
                SetLayer(layers, GameLayers.Obstacle, "Obstacle");
                tagManager.ApplyModifiedPropertiesWithoutUndo();
            }

            // Collision matrix: disable everything between our layers, then enable the meaningful pairs.
            int[] ours = { GameLayers.Player, GameLayers.PlayerProjectile, GameLayers.Enemy, GameLayers.EnemyProjectile, GameLayers.PowerUp, GameLayers.Obstacle };
            foreach (var a in ours)
                for (int b = 0; b < 32; b++)
                    Physics2D.IgnoreLayerCollision(a, b, true);

            Allow(GameLayers.PlayerProjectile, GameLayers.Enemy);
            Allow(GameLayers.PlayerProjectile, GameLayers.Obstacle);
            Allow(GameLayers.EnemyProjectile, GameLayers.Player);
            Allow(GameLayers.Enemy, GameLayers.Player);
            Allow(GameLayers.Obstacle, GameLayers.Player);
            Allow(GameLayers.PowerUp, GameLayers.Player);
            Physics2D.queriesHitTriggers = true;
            Physics2D.gravity = Vector2.zero;
        }

        private static void Allow(int a, int b) => Physics2D.IgnoreLayerCollision(a, b, false);

        private static void SetLayer(SerializedProperty layers, int index, string name)
        {
            if (layers == null || index >= layers.arraySize) return;
            layers.GetArrayElementAtIndex(index).stringValue = name;
        }

        private static void ConfigureInputHandler()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var prop = so.FindProperty("activeInputHandler");
            if (prop != null)
            {
                prop.intValue = 2; // Both (Input System for gameplay/UI, legacy kept for third-party tools)
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void ConfigurePlayerSettings()
        {
            try
            {
                PlayerSettings.companyName = "Starfall Team";
                PlayerSettings.productName = "Starfall Defense";
                PlayerSettings.bundleVersion = "0.1.0";
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                PlayerSettings.allowedAutorotateToPortrait = true;
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
                PlayerSettings.allowedAutorotateToLandscapeLeft = false;
                PlayerSettings.allowedAutorotateToLandscapeRight = false;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.starfallteam.starfalldefense");
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.starfallteam.starfalldefense");
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
                PlayerSettings.Android.bundleVersionCode = 1;
                PlayerSettings.iOS.targetOSVersionString = "13.0";
                PlayerSettings.iOS.buildNumber = "1";
                PlayerSettings.runInBackground = false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Starfall] Some player settings could not be applied: {e.Message}");
            }
        }

        private static void EnsureTmpResources()
        {
            if (UiBuilder.DefaultFont != null)
            {
                Debug.Log("[Starfall] TextMeshPro resources already present.");
                return;
            }
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui");
            string root = info != null ? info.resolvedPath : null;
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                Debug.LogWarning("[Starfall] Could not resolve com.unity.ugui package path to import TMP resources.");
                return;
            }
            var files = Directory.GetFiles(root, "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                Debug.LogWarning("[Starfall] TMP Essential Resources.unitypackage not found in com.unity.ugui.");
                return;
            }
            Debug.Log($"[Starfall] Importing TMP essentials from {files[0]}");
            AssetDatabase.ImportPackage(files[0], false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ConfigureBuildSettings(GameConfig config)
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{SceneRoot}/{config.BootScene}.unity", true),
                new EditorBuildSettingsScene($"{SceneRoot}/{config.MainMenuScene}.unity", true),
                new EditorBuildSettingsScene($"{SceneRoot}/{config.GameplayScene}.unity", true),
            };
        }

        private static void EnsureFolders()
        {
            foreach (var folder in new[]
                     {
                         DataRoot, $"{DataRoot}/Ships", $"{DataRoot}/Weapons", $"{DataRoot}/Enemies", $"{DataRoot}/Bosses",
                         $"{DataRoot}/Waves", $"{DataRoot}/Stages", $"{DataRoot}/PowerUps", $"{DataRoot}/Audio", $"{DataRoot}/Config",
                         PrefabRoot, $"{PrefabRoot}/Player", $"{PrefabRoot}/Enemies", $"{PrefabRoot}/Bosses", $"{PrefabRoot}/Projectiles",
                         $"{PrefabRoot}/PowerUps", $"{PrefabRoot}/UI", $"{PrefabRoot}/VFX", SceneRoot, PlaceholderArt.Folder,
                     })
            {
                Directory.CreateDirectory(folder);
            }
            AssetDatabase.Refresh();
        }

        // ---- Assets helpers ----------------------------------------------------------------------------------

        public static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static T SavePrefab<T>(GameObject go, string path) where T : Component
        {
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return saved.GetComponent<T>();
        }

        private static SpriteRenderer AddSprite(GameObject parent, string name, Sprite sprite, Color color, int order, float scale = 1f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        private static Rigidbody2D AddKinematicBody(GameObject go)
        {
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.useFullKinematicContacts = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.interpolation = RigidbodyInterpolation2D.None;
            return rb;
        }

        // ---- Prefabs ---------------------------------------------------------------------------------------

        private static PrefabSet CreatePooledPrefabs(PlaceholderArt.Set art)
        {
            var set = new PrefabSet();
            set.PlayerProjectile = CreateProjectilePrefab("PlayerProjectile", art.Projectile, GameLayers.PlayerProjectile, new Vector2(0.18f, 0.5f));
            set.EnemyProjectile = CreateProjectilePrefab("EnemyProjectile", art.Bullet, GameLayers.EnemyProjectile, new Vector2(0.28f, 0.28f));
            set.Enemy = CreateEnemyPrefab("Enemy", art, false);
            set.SentinelX = CreateEnemyPrefab("SentinelX", art, true);
            set.Destroyer = CreateEnemyPrefab("Destroyer", art, true, withLaser: true);
            set.Pickup = CreatePickupPrefab(art);
            set.Explosion = CreateExplosionPrefab(art);
            set.FloatingText = CreateFloatingTextPrefab();
            return set;
        }

        private static PooledObject CreateProjectilePrefab(string name, Sprite sprite, int layer, Vector2 colliderSize)
        {
            var go = new GameObject(name) { layer = layer };
            go.AddComponent<PooledObject>();
            AddKinematicBody(go);
            var col = go.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;
            col.size = colliderSize;
            col.direction = CapsuleDirection2D.Vertical;
            var projectile = go.AddComponent<Projectile>();
            projectile.body = AddSprite(go, "Body", sprite, Color.white, SortingOrders.Projectile);
            projectile.hitCollider = col;
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/Projectiles/{name}.prefab");
        }

        private static PooledObject CreateEnemyPrefab(string name, PlaceholderArt.Set art, bool boss, bool withLaser = false)
        {
            var go = new GameObject(name) { layer = GameLayers.Enemy };
            go.AddComponent<PooledObject>();
            var health = go.AddComponent<Health>();
            health.faction = Faction.Enemy;
            AddKinematicBody(go);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = boss ? 1.1f : 0.4f;

            Enemy enemy = boss ? go.AddComponent<BossController>() : go.AddComponent<Enemy>();
            enemy.body = AddSprite(go, "Body", boss ? (withLaser ? art.Destroyer : art.SentinelX) : art.Drone, Color.white, SortingOrders.Enemy);
            enemy.shieldVisual = AddSprite(go, "Shield", art.Ring, new Color(0.4f, 0.9f, 1f, 0.6f), SortingOrders.EnemyShield, boss ? 2.4f : 1.5f);
            enemy.shieldVisual.enabled = false;
            enemy.hitbox = col;

            if (withLaser && enemy is BossController bossController)
            {
                var laserGo = new GameObject("FrontLaser") { layer = GameLayers.EnemyProjectile };
                laserGo.transform.SetParent(go.transform, false);
                laserGo.transform.localPosition = new Vector3(0f, -0.9f, 0f);
                AddKinematicBody(laserGo);
                var beamCol = laserGo.AddComponent<BoxCollider2D>();
                beamCol.isTrigger = true;
                beamCol.enabled = false;
                var laser = laserGo.AddComponent<LaserBeam>();
                laser.beamCollider = beamCol;
                laser.beamSprite = AddSprite(laserGo, "Beam", art.Pixel, new Color(1f, 0.35f, 0.55f, 0.9f), SortingOrders.Laser);
                laser.beamSprite.enabled = false;
                bossController.frontLaser = laser;
            }

            string folder = boss ? "Bosses" : "Enemies";
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/{folder}/{name}.prefab");
        }

        private static PooledObject CreatePickupPrefab(PlaceholderArt.Set art)
        {
            var go = new GameObject("PowerUpPickup") { layer = GameLayers.PowerUp };
            go.AddComponent<PooledObject>();
            AddKinematicBody(go);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            var pickup = go.AddComponent<PowerUpPickup>();
            pickup.body = AddSprite(go, "Body", art.Circle, Color.white, SortingOrders.PowerUp, 0.5f);
            pickup.ring = AddSprite(go, "Ring", art.Ring, Color.white, SortingOrders.PowerUp + 1, 0.62f);
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/PowerUps/PowerUpPickup.prefab");
        }

        private static PooledObject CreateExplosionPrefab(PlaceholderArt.Set art)
        {
            var go = new GameObject("Explosion");
            go.AddComponent<PooledObject>();
            var fx = go.AddComponent<ExplosionEffect>();
            fx.sprite = AddSprite(go, "Glow", art.Dot, Color.white, SortingOrders.Vfx);
            fx.ring = AddSprite(go, "Ring", art.Ring, Color.white, SortingOrders.Vfx + 1);
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/VFX/Explosion.prefab");
        }

        private static PooledObject CreateFloatingTextPrefab()
        {
            var go = new GameObject("FloatingText");
            go.AddComponent<PooledObject>();
            var ft = go.AddComponent<FloatingText>();
            var text = go.AddComponent<TextMeshPro>();
            text.text = "POWER";
            text.fontSize = 4f;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.rectTransform.sizeDelta = new Vector2(6f, 1.2f);
            text.sortingOrder = SortingOrders.FloatingText;
            var font = UiBuilder.DefaultFont;
            if (font != null) text.font = font;
            ft.label = text;
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/VFX/FloatingText.prefab");
        }

        private static GameObject CreatePlayerPrefab(PlaceholderArt.Set art, DataSet data)
        {
            var go = new GameObject("Player") { layer = GameLayers.Player };
            var health = go.AddComponent<Health>();
            health.faction = Faction.Player;
            health.maxHull = data.Vanguard.MaxHull;
            health.maxShield = data.Vanguard.MaxShield;
            AddKinematicBody(go);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = data.Vanguard.HitboxRadius;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var body = AddSprite(visual, "Body", art.Ship, data.Vanguard.Tint, SortingOrders.Player);
            var flame = AddSprite(visual, "Thruster", art.Flame, new Color(0.4f, 0.8f, 1f, 0.9f), SortingOrders.Player - 1, 0.45f);
            flame.transform.localPosition = new Vector3(0f, -0.62f, 0f);
            var shield = AddSprite(go, "Shield", art.Ring, new Color(0.4f, 0.8f, 1f, 0.5f), SortingOrders.PlayerShield, 1.35f);
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.55f, 0f);

            var movement = go.AddComponent<PlayerMovement>();
            movement.bankTarget = visual.transform;
            var weapon = go.AddComponent<WeaponController>();
            weapon.muzzle = muzzle.transform;
            var effects = go.AddComponent<PlayerStatusEffects>();
            var ultimate = go.AddComponent<UltimateController>();
            var thruster = visual.AddComponent<ThrusterFlicker>();
            thruster.flame = flame;
            thruster.baseLength = 0.45f;

            var ship = go.AddComponent<PlayerShip>();
            ship.definition = data.Vanguard;
            ship.body = body;
            ship.shieldVisual = shield;
            ship.hitbox = col;
            ship.movement = movement;
            ship.weapon = weapon;
            ship.effects = effects;
            ship.ultimate = ultimate;
            ship.thruster = thruster;

            var saved = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabRoot}/Player/Player.prefab");
            UnityEngine.Object.DestroyImmediate(go);
            return saved;
        }

        // ---- Data --------------------------------------------------------------------------------------------

        private static DataSet CreateData(PlaceholderArt.Set art, PrefabSet prefabs)
        {
            var d = new DataSet();

            d.Laser = CreateOrLoad<WeaponDefinition>($"{DataRoot}/Weapons/Laser.asset");
            d.Laser.DisplayName = "Laser";
            d.Laser.ProjectilePrefab = prefabs.PlayerProjectile;
            d.Laser.FireInterval = 0.14f;
            d.Laser.Damage = 4f;
            d.Laser.ProjectileSpeed = 18f;
            d.Laser.ProjectileLifetime = 2.5f;
            d.Laser.ProjectileScale = 1f;
            d.Laser.ProjectileColor = new Color(0.35f, 0.9f, 1f);
            d.Laser.ParallelSpacing = 0.22f;
            d.Laser.SideShotAngle = 9f;
            d.Laser.MaxLevel = 5;
            d.Laser.DamagePerLevel = 0.15f;

            d.Vanguard = CreateOrLoad<ShipDefinition>($"{DataRoot}/Ships/Vanguard.asset");
            d.Vanguard.DisplayName = "SF-01 Vanguard";
            d.Vanguard.MaxHull = 100f;
            d.Vanguard.MaxShield = 50f;
            d.Vanguard.MoveSpeed = 9f;
            d.Vanguard.Acceleration = 60f;
            d.Vanguard.Deceleration = 80f;
            d.Vanguard.BankAngle = 18f;
            d.Vanguard.Weapon = d.Laser;
            d.Vanguard.Sprite = art.Ship;
            d.Vanguard.Tint = new Color(0.85f, 0.95f, 1f);
            d.Vanguard.HitboxRadius = 0.28f;

            d.PowerUps = CreatePowerUps(prefabs);
            d.DefaultDrops = CreateDropTable("DefaultDrops", 0.14f, d.PowerUps);
            d.RichDrops = CreateDropTable("RichDrops", 0.75f, d.PowerUps);

            d.Drone = CreateEnemy("Drone", art.Drone, new Color(0.55f, 0.9f, 1f), e =>
            {
                e.MaxHull = 12f; e.ContactDamage = 15f; e.ScoreValue = 100; e.EnergyOnKill = 6f; e.DropTable = d.DefaultDrops;
                e.Movement = MovementKind.StraightDown; e.MovementSettings.Speed = 2.6f;
                e.Attack = AttackKind.Forward; e.AttackSettings.Interval = 2.6f; e.AttackSettings.InitialDelay = 1.2f;
                e.AttackSettings.Damage = 8f; e.AttackSettings.ProjectileSpeed = 5f; e.AttackSettings.Color = new Color(0.6f, 0.9f, 1f);
                e.ExplosionColor = new Color(0.6f, 0.9f, 1f);
            });
            d.DroneSupply = CreateEnemy("DroneSupply", art.Drone, new Color(1f, 0.85f, 0.4f), e =>
            {
                e.DisplayName = "Supply Drone";
                e.MaxHull = 14f; e.ContactDamage = 15f; e.ScoreValue = 100; e.EnergyOnKill = 6f; e.DropTable = d.RichDrops;
                e.Movement = MovementKind.StraightDown; e.MovementSettings.Speed = 2.2f;
                e.Attack = AttackKind.None;
                e.ExplosionColor = new Color(1f, 0.85f, 0.4f);
            });
            d.Interceptor = CreateEnemy("Interceptor", art.Interceptor, new Color(1f, 0.6f, 0.25f), e =>
            {
                e.MaxHull = 16f; e.ContactDamage = 18f; e.ScoreValue = 100; e.EnergyOnKill = 7f; e.DropTable = d.DefaultDrops;
                e.Movement = MovementKind.Weave; e.MovementSettings.Speed = 4f; e.MovementSettings.Amplitude = 1.8f; e.MovementSettings.Frequency = 3f;
                e.Attack = AttackKind.Aimed; e.AttackSettings.Interval = 1.5f; e.AttackSettings.InitialDelay = 0.7f; e.AttackSettings.BurstCount = 2;
                e.AttackSettings.BurstInterval = 0.15f; e.AttackSettings.Damage = 8f; e.AttackSettings.ProjectileSpeed = 7f;
                e.AttackSettings.Color = new Color(1f, 0.6f, 0.25f);
                e.ExplosionColor = new Color(1f, 0.6f, 0.25f);
            });
            d.Bomber = CreateEnemy("Bomber", art.Bomber, new Color(0.8f, 0.45f, 0.95f), e =>
            {
                e.MaxHull = 45f; e.ContactDamage = 25f; e.ScoreValue = 100; e.EnergyOnKill = 10f; e.DropTable = d.DefaultDrops; e.Scale = 1.25f; e.ColliderRadius = 0.45f;
                e.Movement = MovementKind.StraightDown; e.MovementSettings.Speed = 1.4f;
                e.Attack = AttackKind.Forward; e.AttackSettings.Interval = 2.8f; e.AttackSettings.InitialDelay = 1.5f; e.AttackSettings.Count = 3;
                e.AttackSettings.SpreadAngle = 40f; e.AttackSettings.Damage = 18f; e.AttackSettings.ProjectileSpeed = 4f; e.AttackSettings.ProjectileScale = 1.7f;
                e.AttackSettings.Color = new Color(0.9f, 0.5f, 1f);
                e.ExplosionScale = 1.5f; e.ExplosionColor = new Color(0.9f, 0.5f, 1f);
            });
            d.Kamikaze = CreateEnemy("Kamikaze", art.Kamikaze, new Color(1f, 0.3f, 0.3f), e =>
            {
                e.MaxHull = 10f; e.ContactDamage = 25f; e.SelfDestructOnContact = true; e.ScoreValue = 100; e.EnergyOnKill = 6f; e.DropTable = d.DefaultDrops;
                e.Movement = MovementKind.Chase; e.MovementSettings.Speed = 5f; e.MovementSettings.TurnRate = 120f;
                e.Attack = AttackKind.None; e.MinLifetime = 1f;
                e.ExplosionColor = new Color(1f, 0.3f, 0.3f);
            });
            d.ShieldDrone = CreateEnemy("ShieldDrone", art.ShieldDrone, new Color(0.45f, 1f, 0.6f), e =>
            {
                e.DisplayName = "Shield";
                e.MaxHull = 20f; e.MaxShield = 30f; e.ShieldColor = new Color(0.3f, 0.85f, 1f, 0.6f); e.ContactDamage = 18f; e.ScoreValue = 100; e.EnergyOnKill = 8f; e.DropTable = d.DefaultDrops;
                e.Movement = MovementKind.HoverStrafe; e.MovementSettings.Speed = 3f; e.MovementSettings.HoldHeight = 0.28f; e.MovementSettings.HoldDuration = 7f; e.MovementSettings.StrafeSpeed = 2.2f;
                e.Attack = AttackKind.Aimed; e.AttackSettings.Interval = 2f; e.AttackSettings.InitialDelay = 1.5f; e.AttackSettings.Damage = 10f; e.AttackSettings.ProjectileSpeed = 6f;
                e.AttackSettings.Color = new Color(0.45f, 1f, 0.6f);
                e.ExplosionColor = new Color(0.45f, 1f, 0.6f);
            });
            d.Asteroid = CreateEnemy("Asteroid", art.Asteroid, new Color(0.55f, 0.5f, 0.45f), e =>
            {
                e.MaxHull = 40f; e.ContactDamage = 20f; e.ScoreValue = 25; e.EnergyOnKill = 0f; e.DropTable = null; e.IsObstacle = true; e.ImmuneToUltimate = true;
                e.Scale = 1.1f; e.ColliderRadius = 0.42f;
                e.Movement = MovementKind.StraightDown; e.MovementSettings.Speed = 1.8f; e.Attack = AttackKind.None; e.MinLifetime = 2f;
                e.ExplosionColor = new Color(0.7f, 0.6f, 0.5f);
            });

            d.SentinelX = CreateSentinelX(art, prefabs);
            d.Destroyer = CreateDestroyer(art, prefabs);

            d.Stages = CreateStages(art, d);
            d.MenuLook = CreateOrLoad<StageDefinition>($"{DataRoot}/Stages/MenuLook.asset");
            d.MenuLook.DisplayName = "Menu";
            d.MenuLook.BackgroundTop = new Color(0.05f, 0.05f, 0.18f);
            d.MenuLook.BackgroundBottom = new Color(0.01f, 0.0f, 0.05f);
            d.MenuLook.StarDensity = 1f;
            d.MenuLook.Music = MusicId.Menu;
            d.MenuLook.Events = new StageEvent[0];

            d.Audio = CreateOrLoad<AudioLibrary>($"{DataRoot}/Audio/AudioLibrary.asset");
            d.Audio.UseSynthesizedPlaceholders = true;
            if (d.Audio.Sfx == null || d.Audio.Sfx.Length == 0)
            {
                var ids = (SfxId[])Enum.GetValues(typeof(SfxId));
                d.Audio.Sfx = new AudioLibrary.SfxEntry[ids.Length];
                for (int i = 0; i < ids.Length; i++) d.Audio.Sfx[i] = new AudioLibrary.SfxEntry { Id = ids[i], Clips = new AudioClip[0], Volume = 1f };
            }
            if (d.Audio.Music == null || d.Audio.Music.Length == 0)
            {
                var ids = (MusicId[])Enum.GetValues(typeof(MusicId));
                d.Audio.Music = new AudioLibrary.MusicEntry[ids.Length];
                for (int i = 0; i < ids.Length; i++) d.Audio.Music[i] = new AudioLibrary.MusicEntry { Id = ids[i], Clip = null, Volume = 1f };
            }

            d.Config = CreateOrLoad<GameConfig>($"{DataRoot}/Config/GameConfig.asset");
            d.Config.Stages = d.Stages;

            AssetDatabase.SaveAssets();
            return d;
        }

        private static EnemyDefinition CreateEnemy(string name, Sprite sprite, Color tint, Action<EnemyDefinition> init)
        {
            var e = CreateOrLoad<EnemyDefinition>($"{DataRoot}/Enemies/{name}.asset");
            e.DisplayName = name;
            e.Sprite = sprite;
            e.Tint = tint;
            e.Scale = 1f;
            e.ColliderRadius = 0.36f;
            e.MovementSettings = MovementParams.Default;
            e.AttackSettings = AttackParams.Default;
            e.MinLifetime = 1.5f;
            e.ExplosionScale = 1f;
            e.IsElite = false;
            e.IsObstacle = false;
            e.ImmuneToUltimate = false;
            e.SelfDestructOnContact = false;
            e.MaxShield = 0f;
            init(e);
            EditorUtility.SetDirty(e);
            return e;
        }

        private static PowerUpDefinition[] CreatePowerUps(PrefabSet prefabs)
        {
            PowerUpDefinition Make(string name, PowerUpKind kind, string display, string label, Color color, float duration, float magnitude, float weight)
            {
                var p = CreateOrLoad<PowerUpDefinition>($"{DataRoot}/PowerUps/{name}.asset");
                p.Kind = kind;
                p.DisplayName = display;
                p.Label = label;
                p.Color = color;
                p.Duration = duration;
                p.Magnitude = magnitude;
                p.Weight = weight;
                p.PickupPrefab = prefabs.Pickup;
                p.FallSpeed = 1.6f;
                p.SwayAmplitude = 0.4f;
                EditorUtility.SetDirty(p);
                return p;
            }

            return new[]
            {
                Make("Blue_LaserLevel", PowerUpKind.LaserLevel, "Laser Upgrade", "LASER UP", new Color(0.3f, 0.6f, 1f), 0f, 1f, 3f),
                Make("Green_Shield", PowerUpKind.ShieldRestore, "Shield Restore", "SHIELD +50", new Color(0.3f, 1f, 0.45f), 0f, 50f, 3f),
                Make("Red_Damage", PowerUpKind.DamageBoost, "Damage Boost", "DAMAGE x2", new Color(1f, 0.25f, 0.25f), 8f, 2f, 2f),
                Make("Yellow_Speed", PowerUpKind.SpeedBoost, "Speed Boost", "SPEED UP", new Color(1f, 0.9f, 0.2f), 8f, 1.4f, 2f),
                Make("Purple_Energy", PowerUpKind.Energy, "Energy Cell", "ENERGY +40", new Color(0.75f, 0.35f, 1f), 0f, 40f, 2f),
                Make("White_Invincible", PowerUpKind.Invincibility, "Invincibility", "INVINCIBLE", Color.white, 5f, 1f, 1f),
            };
        }

        private static DropTable CreateDropTable(string name, float chance, PowerUpDefinition[] powerUps)
        {
            var table = CreateOrLoad<DropTable>($"{DataRoot}/PowerUps/{name}.asset");
            table.DropChance = chance;
            table.Entries = new DropTable.Entry[powerUps.Length];
            for (int i = 0; i < powerUps.Length; i++) table.Entries[i] = new DropTable.Entry { PowerUp = powerUps[i], Weight = powerUps[i].Weight };
            EditorUtility.SetDirty(table);
            return table;
        }

        private static AttackParams Attack(float interval, float damage, float speed, int count = 1, float spread = 0f, int burst = 1, float burstInterval = 0.12f, float scale = 1f, float initialDelay = 0f, Color? color = null, float lifetime = 6f)
        {
            var a = AttackParams.Default;
            a.Interval = interval;
            a.Damage = damage;
            a.ProjectileSpeed = speed;
            a.Count = count;
            a.SpreadAngle = spread;
            a.BurstCount = burst;
            a.BurstInterval = burstInterval;
            a.ProjectileScale = scale;
            a.InitialDelay = initialDelay;
            a.Lifetime = lifetime;
            a.Color = color ?? new Color(1f, 0.4f, 0.5f);
            a.OnlyWhenOnScreen = true;
            return a;
        }

        private static MovementParams Patrol(float amplitude, float frequency)
        {
            var m = MovementParams.Default;
            m.Amplitude = amplitude;
            m.Frequency = frequency;
            m.Speed = 0f;
            return m;
        }

        private static BossDefinition CreateSentinelX(PlaceholderArt.Set art, PrefabSet prefabs)
        {
            var b = CreateOrLoad<BossDefinition>($"{DataRoot}/Bosses/SentinelX.asset");
            b.DisplayName = "Sentinel-X";
            b.Title = "SENTINEL-X";
            b.Prefab = prefabs.SentinelX;
            b.Sprite = art.SentinelX;
            b.Tint = new Color(0.9f, 0.55f, 1f);
            b.Scale = 1f;
            b.ColliderRadius = 1.05f;
            b.MaxHull = 900f;
            b.MaxShield = 0f;
            b.ContactDamage = 30f;
            b.ScoreValue = 2500;
            b.EnergyOnKill = 30f;
            b.IsElite = true;
            b.DropTable = null;
            b.EntranceDuration = 2.5f;
            b.EntranceHeight = 0.22f;
            b.MinLifetime = 999f;
            b.ExplosionScale = 2.5f;
            b.ExplosionColor = new Color(0.9f, 0.55f, 1f);
            b.DeathExplosions = 8;
            b.DeathSequenceSeconds = 1.6f;
            b.Movement = MovementKind.LateralPatrol;
            b.MovementSettings = Patrol(3f, 0.8f);
            b.Attack = AttackKind.None;
            b.Phases = new[]
            {
                new BossPhase
                {
                    Name = "", HealthThreshold = 1f, Movement = MovementKind.LateralPatrol, MovementSettings = Patrol(3f, 0.8f),
                    Attacks = new[]
                    {
                        new BossAttack { Kind = BossAttackKind.Ring, Settings = Attack(1.8f, 12f, 4.5f, count: 10, scale: 1.1f, initialDelay: 0.8f, color: new Color(1f, 0.5f, 0.9f)) },
                    },
                },
                new BossPhase
                {
                    Name = "SENTINEL OVERDRIVE", HealthThreshold = 0.5f, Movement = MovementKind.LateralPatrol, MovementSettings = Patrol(3.5f, 1.4f),
                    Attacks = new[]
                    {
                        new BossAttack { Kind = BossAttackKind.Ring, Settings = Attack(1.4f, 12f, 5f, count: 14, scale: 1.1f, color: new Color(1f, 0.5f, 0.9f)) },
                        new BossAttack { Kind = BossAttackKind.Aimed, Settings = Attack(2.2f, 12f, 6.5f, count: 3, spread: 30f, burst: 2, burstInterval: 0.2f, color: new Color(1f, 0.8f, 0.3f)) },
                    },
                },
            };
            EditorUtility.SetDirty(b);
            return b;
        }

        private static BossDefinition CreateDestroyer(PlaceholderArt.Set art, PrefabSet prefabs)
        {
            var b = CreateOrLoad<BossDefinition>($"{DataRoot}/Bosses/Destroyer.asset");
            b.DisplayName = "The Destroyer";
            b.Title = "THE DESTROYER";
            b.Prefab = prefabs.Destroyer;
            b.Sprite = art.Destroyer;
            b.Tint = new Color(1f, 0.45f, 0.55f);
            b.Scale = 1f;
            b.ColliderRadius = 1.4f;
            b.MaxHull = 2800f;
            b.MaxShield = 0f;
            b.ContactDamage = 40f;
            b.ScoreValue = 10000;
            b.EnergyOnKill = 50f;
            b.IsElite = true;
            b.DropTable = null;
            b.EntranceDuration = 3f;
            b.EntranceHeight = 0.2f;
            b.MinLifetime = 999f;
            b.ExplosionScale = 3f;
            b.ExplosionColor = new Color(1f, 0.5f, 0.4f);
            b.DeathExplosions = 14;
            b.DeathSequenceSeconds = 2.4f;
            b.Movement = MovementKind.LateralPatrol;
            b.MovementSettings = Patrol(2.5f, 0.6f);
            b.Attack = AttackKind.None;
            var cannonColor = new Color(1f, 0.45f, 0.35f);
            var missileColor = new Color(1f, 0.8f, 0.3f);
            b.Phases = new[]
            {
                new BossPhase
                {
                    Name = "", HealthThreshold = 1f, Movement = MovementKind.LateralPatrol, MovementSettings = Patrol(2.5f, 0.6f),
                    Attacks = new[]
                    {
                        new BossAttack { Kind = BossAttackKind.SideCannons, MuzzleOffset = 1.6f, Settings = Attack(0.9f, 12f, 7f, initialDelay: 0.6f, color: cannonColor) },
                    },
                },
                new BossPhase
                {
                    Name = "MISSILE BAYS OPEN", HealthThreshold = 0.7f, Movement = MovementKind.LateralPatrol, MovementSettings = Patrol(3f, 0.9f),
                    Attacks = new[]
                    {
                        new BossAttack { Kind = BossAttackKind.SideCannons, MuzzleOffset = 1.6f, Settings = Attack(0.8f, 12f, 7f, count: 2, spread: 12f, color: cannonColor) },
                        new BossAttack { Kind = BossAttackKind.Missiles, MuzzleOffset = 1.2f, HomingTurnRate = 60f, Settings = Attack(3.5f, 18f, 4f, scale: 1.4f, initialDelay: 1f, color: missileColor, lifetime: 6f) },
                    },
                },
                new BossPhase
                {
                    Name = "CORE EXPOSED", HealthThreshold = 0.4f, Movement = MovementKind.LateralPatrol, MovementSettings = Patrol(3.5f, 1.2f),
                    Attacks = new[]
                    {
                        new BossAttack { Kind = BossAttackKind.SideCannons, MuzzleOffset = 1.6f, Settings = Attack(0.7f, 12f, 7.5f, count: 3, spread: 20f, color: cannonColor) },
                        new BossAttack { Kind = BossAttackKind.Missiles, MuzzleOffset = 1.2f, HomingTurnRate = 70f, Settings = Attack(4f, 18f, 4.2f, scale: 1.4f, color: missileColor, lifetime: 6f) },
                        new BossAttack { Kind = BossAttackKind.FrontLaser, TelegraphSeconds = 1.2f, BeamSeconds = 1.6f, BeamWidth = 1.2f, BeamDamagePerSecond = 45f, Settings = Attack(6f, 0f, 0f, initialDelay: 2f) },
                    },
                },
            };
            EditorUtility.SetDirty(b);
            return b;
        }

        // ---- Waves & stages ----------------------------------------------------------------------------------

        private static SpawnEntry Entry(EnemyDefinition enemy, int count, float interval, SpawnPattern pattern, float delayBefore = 0f, float value = 0.5f)
        {
            return new SpawnEntry { Enemy = enemy, Count = count, Interval = interval, Pattern = pattern, DelayBefore = delayBefore, PatternValue = value };
        }

        private static WaveDefinition Wave(string name, bool waitForClear, float maxDuration, params SpawnEntry[] entries)
        {
            var w = CreateOrLoad<WaveDefinition>($"{DataRoot}/Waves/{name}.asset");
            w.Label = name;
            w.Entries = entries;
            w.WaitForClear = waitForClear;
            w.MaxDuration = maxDuration;
            EditorUtility.SetDirty(w);
            return w;
        }

        private static StageEvent WaveEvent(WaveDefinition wave) => new StageEvent { Type = StageEventType.Wave, Wave = wave };
        private static StageEvent MessageEvent(string text, float seconds) => new StageEvent { Type = StageEventType.Message, Message = text, Seconds = seconds };
        private static StageEvent DelayEvent(float seconds) => new StageEvent { Type = StageEventType.Delay, Seconds = seconds };
        private static StageEvent AsteroidEvent(bool on) => new StageEvent { Type = StageEventType.AsteroidField, Flag = on };
        private static StageEvent BossEvent(BossDefinition boss, bool mini) => new StageEvent { Type = mini ? StageEventType.MiniBoss : StageEventType.Boss, Boss = boss };

        private static StageDefinition[] CreateStages(PlaceholderArt.Set art, DataSet d)
        {
            // ---- Stage 1: Orbital Sector ----
            var s1 = CreateOrLoad<StageDefinition>($"{DataRoot}/Stages/Stage1_OrbitalSector.asset");
            s1.DisplayName = "Orbital Sector";
            s1.Subtitle = "Earth orbit, 2237";
            s1.Briefing = "The Swarm vanguard has breached the orbital defense grid.\nClear the debris field and stop the drones before they reach the surface.\n\nDrag to move. The Vanguard fires automatically.\nTap the energy bar (or press E) to unleash the Ultimate when it is full.";
            s1.Music = MusicId.Stage1;
            s1.CompletionBonus = 1000;
            s1.BackgroundTop = new Color(0.02f, 0.06f, 0.18f);
            s1.BackgroundBottom = new Color(0f, 0.01f, 0.06f);
            s1.Fog = new Color(0f, 0f, 0f, 0f);
            s1.StarDensity = 0.8f;
            s1.StarTint = Color.white;
            s1.DebrisSprites = new[] { art.Satellite };
            s1.DebrisInterval = 4f;
            s1.AsteroidDefinition = null;
            s1.Events = new[]
            {
                MessageEvent("ORBITAL SECTOR", 2.2f),
                DelayEvent(1.2f),
                MessageEvent("DRAG TO MOVE  -  AUTO FIRE ON", 3f),
                WaveEvent(Wave("S1_W1_DroneLine", true, 30f, Entry(d.Drone, 4, 0.5f, SpawnPattern.TopLine))),
                WaveEvent(Wave("S1_W2_DroneVee", true, 30f, Entry(d.Drone, 5, 0.35f, SpawnPattern.TopVee, 0.8f))),
                MessageEvent("SUPPLY DRONES CARRY POWER-UPS", 2.5f),
                WaveEvent(Wave("S1_W3_Supply", true, 30f, Entry(d.DroneSupply, 2, 1.2f, SpawnPattern.TopAlternate), Entry(d.Drone, 6, 0.45f, SpawnPattern.TopRandom, 0.5f))),
                MessageEvent("INTERCEPTORS INBOUND", 2f),
                WaveEvent(Wave("S1_W4_Interceptors", true, 35f, Entry(d.Interceptor, 3, 0.9f, SpawnPattern.TopRandom))),
                WaveEvent(Wave("S1_W5_Mixed", true, 40f, Entry(d.Drone, 6, 0.4f, SpawnPattern.TopLine), Entry(d.Interceptor, 3, 0.8f, SpawnPattern.TopAlternate, 2f))),
                WaveEvent(Wave("S1_W6_Mixed2", true, 40f, Entry(d.Interceptor, 5, 0.6f, SpawnPattern.TopAlternate), Entry(d.Drone, 5, 0.35f, SpawnPattern.TopVee, 1.5f), Entry(d.DroneSupply, 1, 0f, SpawnPattern.TopCenter, 1f))),
                MessageEvent("FINAL WAVE", 2f),
                WaveEvent(Wave("S1_W7_Final", true, 45f, Entry(d.Drone, 8, 0.3f, SpawnPattern.TopLine), Entry(d.Interceptor, 4, 0.7f, SpawnPattern.TopRandom, 1.5f), Entry(d.DroneSupply, 2, 1f, SpawnPattern.TopAlternate, 2f))),
            };
            EditorUtility.SetDirty(s1);

            // ---- Stage 2: Asteroid Field ----
            var s2 = CreateOrLoad<StageDefinition>($"{DataRoot}/Stages/Stage2_AsteroidField.asset");
            s2.DisplayName = "Asteroid Field";
            s2.Subtitle = "Between Mars and Jupiter";
            s2.Briefing = "The Swarm is using the asteroid belt as cover.\nAvoid the rocks, break the shielded units and take down the Sentinel-X outpost.";
            s2.Music = MusicId.Stage2;
            s2.CompletionBonus = 2000;
            s2.BackgroundTop = new Color(0.1f, 0.06f, 0.12f);
            s2.BackgroundBottom = new Color(0.03f, 0.02f, 0.05f);
            s2.Fog = new Color(0f, 0f, 0f, 0f);
            s2.StarDensity = 0.5f;
            s2.StarTint = new Color(1f, 0.95f, 0.85f);
            s2.DebrisSprites = new Sprite[0];
            s2.DebrisInterval = 0f;
            s2.AsteroidDefinition = d.Asteroid;
            s2.AsteroidInterval = 1.4f;
            s2.Events = new[]
            {
                MessageEvent("ASTEROID FIELD", 2.2f),
                AsteroidEvent(true),
                DelayEvent(2f),
                WaveEvent(Wave("S2_W1_Drones", true, 30f, Entry(d.Drone, 6, 0.4f, SpawnPattern.TopAlternate))),
                MessageEvent("BOMBERS DETECTED", 2f),
                WaveEvent(Wave("S2_W2_Bombers", true, 40f, Entry(d.Bomber, 2, 1.5f, SpawnPattern.TopAlternate), Entry(d.Drone, 4, 0.4f, SpawnPattern.TopLine, 1f))),
                MessageEvent("KAMIKAZE UNITS!", 2f),
                WaveEvent(Wave("S2_W3_Kamikaze", true, 35f, Entry(d.Kamikaze, 4, 0.8f, SpawnPattern.TopRandom), Entry(d.DroneSupply, 1, 0f, SpawnPattern.TopCenter, 1f))),
                MessageEvent("SHIELDED UNITS - BREAK THE SHIELD FIRST", 2.5f),
                WaveEvent(Wave("S2_W4_Shields", true, 40f, Entry(d.ShieldDrone, 2, 1.2f, SpawnPattern.TopAlternate), Entry(d.Interceptor, 3, 0.7f, SpawnPattern.TopRandom, 2f))),
                WaveEvent(Wave("S2_W5_Mixed", true, 45f, Entry(d.Bomber, 2, 1.2f, SpawnPattern.TopAlternate), Entry(d.Kamikaze, 3, 0.9f, SpawnPattern.TopRandom, 1.5f), Entry(d.ShieldDrone, 1, 0f, SpawnPattern.TopCenter, 2f), Entry(d.DroneSupply, 1, 0f, SpawnPattern.TopRandom, 1f))),
                WaveEvent(Wave("S2_W6_Mixed2", true, 45f, Entry(d.ShieldDrone, 2, 1f, SpawnPattern.TopAlternate), Entry(d.Bomber, 2, 1.2f, SpawnPattern.TopLine, 1f), Entry(d.Kamikaze, 4, 0.7f, SpawnPattern.TopRandom, 2f), Entry(d.Drone, 5, 0.35f, SpawnPattern.TopVee, 1f))),
                AsteroidEvent(false),
                BossEvent(d.SentinelX, true),
            };
            EditorUtility.SetDirty(s2);

            // ---- Stage 3: Violet Nebula ----
            var s3 = CreateOrLoad<StageDefinition>($"{DataRoot}/Stages/Stage3_VioletNebula.asset");
            s3.DisplayName = "Violet Nebula";
            s3.Subtitle = "Swarm staging ground";
            s3.Briefing = "Sensors are degraded inside the nebula.\nThe Destroyer, flagship of The Swarm, is here. End the invasion.";
            s3.Music = MusicId.Stage3;
            s3.CompletionBonus = 5000;
            s3.BackgroundTop = new Color(0.2f, 0.05f, 0.3f);
            s3.BackgroundBottom = new Color(0.08f, 0.01f, 0.14f);
            s3.Fog = new Color(0.55f, 0.2f, 0.85f, 0.22f);
            s3.StarDensity = 0.35f;
            s3.StarTint = new Color(0.9f, 0.75f, 1f);
            s3.DebrisSprites = new Sprite[0];
            s3.DebrisInterval = 0f;
            s3.AsteroidDefinition = null;
            s3.Events = new[]
            {
                MessageEvent("VIOLET NEBULA", 2.2f),
                DelayEvent(1.5f),
                WaveEvent(Wave("S3_W1_Rush", true, 40f, Entry(d.Interceptor, 5, 0.5f, SpawnPattern.TopRandom), Entry(d.Drone, 6, 0.3f, SpawnPattern.TopLine, 1f))),
                WaveEvent(Wave("S3_W2_Heavy", true, 45f, Entry(d.Bomber, 3, 1f, SpawnPattern.TopLine), Entry(d.ShieldDrone, 2, 1f, SpawnPattern.TopAlternate, 1.5f), Entry(d.DroneSupply, 1, 0f, SpawnPattern.TopCenter, 1f))),
                WaveEvent(Wave("S3_W3_Swarm", true, 45f, Entry(d.Kamikaze, 5, 0.6f, SpawnPattern.TopRandom), Entry(d.Interceptor, 4, 0.6f, SpawnPattern.TopAlternate, 1f), Entry(d.Drone, 6, 0.3f, SpawnPattern.TopVee, 1f))),
                WaveEvent(Wave("S3_W4_All", true, 50f, Entry(d.ShieldDrone, 3, 0.9f, SpawnPattern.TopLine), Entry(d.Bomber, 2, 1.2f, SpawnPattern.TopAlternate, 1f), Entry(d.Kamikaze, 4, 0.7f, SpawnPattern.TopRandom, 1f), Entry(d.Interceptor, 4, 0.6f, SpawnPattern.TopRandom, 1f), Entry(d.Drone, 6, 0.3f, SpawnPattern.TopLine, 1f), Entry(d.DroneSupply, 2, 1f, SpawnPattern.TopAlternate, 1f))),
                MessageEvent("FLAGSHIP SIGNATURE DETECTED", 2.5f),
                BossEvent(d.Destroyer, false),
            };
            EditorUtility.SetDirty(s3);

            return new[] { s1, s2, s3 };
        }
    }
}
