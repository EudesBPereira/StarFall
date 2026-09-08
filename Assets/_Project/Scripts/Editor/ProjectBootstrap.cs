using System;
using System.IO;
using Starfall.Audio;
using Starfall.Bosses;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Player;
using Starfall.Pooling;
using Starfall.PowerUps;
using Starfall.VFX;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Starfall.EditorTools
{
    /// <summary>
    /// One-shot project generator. Creates settings, placeholder art, materials, data assets, prefabs and scenes so
    /// the project is playable right after cloning. Idempotent: re-running updates existing assets in place.
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
        public const string MaterialRoot = "Assets/_Project/Materials";

        public sealed class MaterialSet
        {
            public Material Hologram, Shield, Additive, Beam, SparkParticle, SmokeParticle;
        }

        public sealed class PrefabSet
        {
            public PooledObject PlayerProjectile, EnemyProjectile, Enemy, Pickup, Explosion, FloatingText, Sparks;
            public PooledObject SentinelX, Widow, ReaperWing, Destroyer, Leviathan, HiveQueen, OmegaCore;
            public PooledObject IronWarden, Bastion, CorsairQueen, Assembler, AetherGuardian, RiftWalker;
            public GameObject Player;
        }

        // ---- Entry points ------------------------------------------------------------------------------

        [MenuItem("Starfall/1. Prepare Project (settings + TextMeshPro)")]
        public static void PrepareProject()
        {
            Debug.Log("[Starfall] PrepareProject: start");
            ConfigureLayersAndPhysics();
            ConfigureInputHandler();
            ConfigurePlayerSettings();
            FinalAssets.TryApplyIcon();
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
            var materials = CreateMaterials(art);
            UiBuilder.HologramMaterial = materials.Hologram;
            var prefabs = CreatePooledPrefabs(art, materials);
            var data = ContentFactory.CreateData(art, prefabs);
            prefabs.Player = CreatePlayerPrefab(art, materials, data);
            SceneBuilder.BuildAll(art, data, prefabs);
            ConfigureBuildSettings(data.Config);
            FinalAssets.TryApplyIcon();
            Debug.Log($"[Starfall] Final art sprites in use: {FinalAssets.SpritesReplaced}");
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
                prop.intValue = 2;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void ConfigurePlayerSettings()
        {
            try
            {
                PlayerSettings.companyName = "Starfall Team";
                PlayerSettings.productName = "Starfall Defense";
                PlayerSettings.bundleVersion = "0.2.0";
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
                PlayerSettings.Android.bundleVersionCode = 2;
                PlayerSettings.iOS.targetOSVersionString = "13.0";
                PlayerSettings.iOS.buildNumber = "2";
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
                         $"{PrefabRoot}/PowerUps", $"{PrefabRoot}/UI", $"{PrefabRoot}/VFX", SceneRoot, PlaceholderArt.Folder, MaterialRoot,
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

        private static Material CreateMaterial(string name, string shaderName, Texture texture = null)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogWarning($"[Starfall] Shader '{shaderName}' not found; using Sprites/Default for {name}.");
                shader = Shader.Find("Sprites/Default");
            }
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else mat.shader = shader;
            if (texture != null) mat.mainTexture = texture;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static MaterialSet CreateMaterials(PlaceholderArt.Set art)
        {
            var set = new MaterialSet
            {
                Hologram = CreateMaterial("Hologram", "Starfall/HologramSprite"),
                Shield = CreateMaterial("Shield", "Starfall/ShieldSprite"),
                Additive = CreateMaterial("Additive", "Starfall/AdditiveSprite"),
                Beam = CreateMaterial("Beam", "Starfall/EnergyBeam"),
                SparkParticle = CreateMaterial("SparkParticle", "Starfall/AdditiveSprite", art.Spark != null ? art.Spark.texture : null),
                SmokeParticle = CreateMaterial("SmokeParticle", "Sprites/Default", art.Dot != null ? art.Dot.texture : null),
            };
            AssetDatabase.SaveAssets();
            return set;
        }

        private static T SavePrefab<T>(GameObject go, string path) where T : Component
        {
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return saved.GetComponent<T>();
        }

        public static SpriteRenderer AddSprite(GameObject parent, string name, Sprite sprite, Color color, int order, float scale = 1f, Material material = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            if (material != null) sr.sharedMaterial = material;
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

        private static ThrusterFlicker AddThruster(GameObject parent, Sprite flame, Color color, Vector3 localPos, float scale, Material additive, bool pointUp)
        {
            var flameSr = AddSprite(parent, "Thruster", flame, color, SortingOrders.Enemy - 1, scale, additive);
            flameSr.transform.localPosition = localPos;
            if (pointUp) flameSr.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            var thruster = flameSr.gameObject.AddComponent<ThrusterFlicker>();
            thruster.flame = flameSr;
            thruster.baseLength = 0.45f;
            return thruster;
        }

        // ---- Prefabs ---------------------------------------------------------------------------------------

        private static PrefabSet CreatePooledPrefabs(PlaceholderArt.Set art, MaterialSet materials)
        {
            var set = new PrefabSet();
            set.PlayerProjectile = CreateProjectilePrefab("PlayerProjectile", art.Projectile, art.Dot, GameLayers.PlayerProjectile, new Vector2(0.18f, 0.5f), materials);
            set.EnemyProjectile = CreateProjectilePrefab("EnemyProjectile", art.Bullet, art.Dot, GameLayers.EnemyProjectile, new Vector2(0.28f, 0.28f), materials);
            set.Enemy = CreateEnemyPrefab("Enemy", art, materials, null, 0.4f, false);
            set.SentinelX = CreateEnemyPrefab("SentinelX", art, materials, art.SentinelX, 1.05f, false);
            set.Widow = CreateEnemyPrefab("Widow", art, materials, art.Widow, 1.0f, false);
            set.ReaperWing = CreateEnemyPrefab("ReaperWing", art, materials, art.ReaperWing, 0.9f, false);
            set.Destroyer = CreateEnemyPrefab("Destroyer", art, materials, art.Destroyer, 1.4f, true);
            set.Leviathan = CreateEnemyPrefab("Leviathan", art, materials, art.LeviathanHead, 0.8f, false);
            set.HiveQueen = CreateEnemyPrefab("HiveQueen", art, materials, art.HiveQueen, 1.4f, false);
            set.OmegaCore = CreateEnemyPrefab("OmegaCore", art, materials, art.OmegaCore, 1.5f, true);
            set.IronWarden = CreateEnemyPrefab("IronWarden", art, materials, art.IronWarden, 1.3f, false);
            set.Bastion = CreateEnemyPrefab("Bastion", art, materials, art.Bastion, 1.6f, true);
            set.CorsairQueen = CreateEnemyPrefab("CorsairQueen", art, materials, art.CorsairQueen, 1.2f, false);
            set.Assembler = CreateEnemyPrefab("Assembler", art, materials, art.Assembler, 1.5f, false);
            set.AetherGuardian = CreateEnemyPrefab("AetherGuardian", art, materials, art.AetherGuardian, 1.2f, true);
            set.RiftWalker = CreateEnemyPrefab("RiftWalker", art, materials, art.RiftWalker, 1.2f, false);
            set.Pickup = CreatePickupPrefab(art, materials);
            set.Explosion = CreateExplosionPrefab(art, materials);
            set.FloatingText = CreateFloatingTextPrefab();
            set.Sparks = CreateSparksPrefab(materials);
            return set;
        }

        private static PooledObject CreateProjectilePrefab(string name, Sprite sprite, Sprite glowSprite, int layer, Vector2 colliderSize, MaterialSet materials)
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
            projectile.glow = AddSprite(go, "Glow", glowSprite, new Color(1f, 1f, 1f, 0.5f), SortingOrders.Projectile - 1, 1.6f, materials.Additive);
            projectile.hitCollider = col;
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/Projectiles/{name}.prefab");
        }

        private static PooledObject CreateEnemyPrefab(string name, PlaceholderArt.Set art, MaterialSet materials, Sprite bossSprite, float radius, bool withLaser)
        {
            bool boss = bossSprite != null;
            var go = new GameObject(name) { layer = GameLayers.Enemy };
            go.AddComponent<PooledObject>();
            var health = go.AddComponent<Health>();
            health.faction = Faction.Enemy;
            AddKinematicBody(go);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;

            Enemy enemy = boss ? go.AddComponent<BossController>() : go.AddComponent<Enemy>();
            enemy.body = AddSprite(go, "Body", boss ? bossSprite : art.Drone, Color.white, SortingOrders.Enemy);
            enemy.shieldVisual = AddSprite(go, "Shield", art.Ring, new Color(0.4f, 0.9f, 1f, 0.6f), SortingOrders.EnemyShield, boss ? 2.4f : 1.5f, materials.Shield);
            enemy.shieldVisual.enabled = false;
            enemy.hitbox = col;
            enemy.thruster = AddThruster(go, art.Flame, new Color(1f, 0.5f, 0.2f, 0.9f), new Vector3(0f, radius * 0.9f, 0f), boss ? 0.8f : 0.35f, materials.Additive, true);

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
                laser.beamSprite = AddSprite(laserGo, "Beam", art.Pixel, new Color(1f, 0.35f, 0.55f, 0.9f), SortingOrders.Laser, 1f, materials.Beam);
                laser.beamSprite.enabled = false;
                bossController.frontLaser = laser;
            }

            string folder = boss ? "Bosses" : "Enemies";
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/{folder}/{name}.prefab");
        }

        private static PooledObject CreatePickupPrefab(PlaceholderArt.Set art, MaterialSet materials)
        {
            var go = new GameObject("PowerUpPickup") { layer = GameLayers.PowerUp };
            go.AddComponent<PooledObject>();
            AddKinematicBody(go);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            var pickup = go.AddComponent<PowerUpPickup>();
            pickup.body = AddSprite(go, "Body", art.Circle, Color.white, SortingOrders.PowerUp, 0.5f);
            pickup.ring = AddSprite(go, "Ring", art.Ring, Color.white, SortingOrders.PowerUp + 1, 0.62f, materials.Additive);
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/PowerUps/PowerUpPickup.prefab");
        }

        private static PooledObject CreateExplosionPrefab(PlaceholderArt.Set art, MaterialSet materials)
        {
            var go = new GameObject("Explosion");
            go.AddComponent<PooledObject>();
            var fx = go.AddComponent<ExplosionEffect>();
            fx.sprite = AddSprite(go, "Glow", art.Dot, Color.white, SortingOrders.Vfx, 1f, materials.Additive);
            fx.ring = AddSprite(go, "Ring", art.Ring, Color.white, SortingOrders.Vfx + 1, 1f, materials.Additive);
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/VFX/Explosion.prefab");
        }

        private static PooledObject CreateSparksPrefab(MaterialSet materials)
        {
            var go = new GameObject("Sparks");
            go.AddComponent<PooledObject>();
            var ps = go.AddComponent<ParticleSystem>();
            ConfigureBurstParticles(ps, materials.SparkParticle, 0.25f, 0.6f, 3f, 8f, 0.06f, 0.16f, SortingOrders.Vfx + 2);
            go.AddComponent<ParticleBurst>();
            return SavePrefab<PooledObject>(go, $"{PrefabRoot}/VFX/Sparks.prefab");
        }

        /// <summary>Common setup for one-shot particle bursts (no emission by itself; scripts call Emit).</summary>
        private static void ConfigureBurstParticles(ParticleSystem ps, Material material, float lifeMin, float lifeMax, float speedMin, float speedMax, float sizeMin, float sizeMax, int sortingOrder)
        {
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startColor = Color.white;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 128;
            main.gravityModifier = 0f;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = gradient;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.5f;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
        }

        private static ParticleSystem AddSmoke(GameObject parent, MaterialSet materials)
        {
            var go = new GameObject("Smoke");
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = new Vector3(0f, -0.2f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startColor = new Color(0.35f, 0.35f, 0.4f, 0.7f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 64;
            var emission = ps.emission;
            emission.rateOverTime = 14f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.1f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.5f, 0.2f), 0f), new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 0.4f), new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = gradient;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.6f));
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = materials.SmokeParticle;
            renderer.sortingOrder = SortingOrders.Player + 1;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
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

        private static GameObject CreatePlayerPrefab(PlaceholderArt.Set art, MaterialSet materials, ContentFactory.DataSet data)
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
            var thruster = AddThruster(visual, art.Flame, new Color(0.4f, 0.8f, 1f, 0.9f), new Vector3(0f, -0.62f, 0f), 0.45f, materials.Additive, false);
            var shield = AddSprite(go, "Shield", art.Ring, new Color(0.4f, 0.8f, 1f, 0.5f), SortingOrders.PlayerShield, 1.35f, materials.Shield);
            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            var chargeGlow = AddSprite(muzzle, "ChargeGlow", art.Dot, new Color(0.6f, 0.9f, 1f, 0.6f), SortingOrders.Projectile, 0.3f, materials.Additive);
            chargeGlow.enabled = false;
            var smoke = AddSmoke(go, materials);
            var hitboxVisual = AddSprite(go, "HitboxVisual", art.Ring, new Color(1f, 1f, 1f, 0.85f), SortingOrders.Player + 2, 0.56f, materials.Additive);
            hitboxVisual.enabled = false;
            var riskSensor = go.AddComponent<RiskSensor>();
            riskSensor.hitbox = col;
            riskSensor.hitboxVisual = hitboxVisual;
            var droneGo = new GameObject("CompanionDrone");
            droneGo.transform.SetParent(go.transform, false);
            droneGo.transform.localPosition = new Vector3(0.9f, 0.35f, 0f);
            var droneSprite = AddSprite(droneGo, "Body", art.Drone2, new Color(0.14f, 0.84f, 1f), SortingOrders.Player + 1, 0.4f);
            droneSprite.enabled = false;
            var drone = droneGo.AddComponent<CompanionDrone>();
            drone.body = droneSprite;

            var movement = go.AddComponent<PlayerMovement>();
            movement.bankTarget = visual.transform;
            var weapon = go.AddComponent<WeaponController>();
            weapon.muzzle = muzzle.transform;
            weapon.chargeGlow = chargeGlow;
            var effects = go.AddComponent<PlayerStatusEffects>();
            var ultimate = go.AddComponent<UltimateController>();
            var engineGo = new GameObject("EngineAudio");
            engineGo.transform.SetParent(go.transform, false);
            engineGo.AddComponent<AudioSource>();
            var engine = engineGo.AddComponent<EngineAudio>();

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
            ship.smoke = smoke;
            ship.engineAudio = engine;
            ship.riskSensor = riskSensor;
            ship.companionDrone = drone;

            var saved = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabRoot}/Player/Player.prefab");
            UnityEngine.Object.DestroyImmediate(go);
            return saved;
        }
    }
}
