using Starfall.Audio;
using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Input;
using Starfall.Player;
using Starfall.Pooling;
using Starfall.Scoring;
using Starfall.UI;
using Starfall.VFX;
using Starfall.Waves;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Starfall.EditorTools
{
    /// <summary>Builds the Boot, MainMenu and Gameplay scenes from code.</summary>
    public static class SceneBuilder
    {
        public static void BuildAll(PlaceholderArt.Set art, ProjectBootstrap.DataSet data, ProjectBootstrap.PrefabSet prefabs)
        {
            BuildBoot(data);
            BuildMainMenu(art, data);
            BuildGameplay(art, data, prefabs);
        }

        // ---- Shared -------------------------------------------------------------------------------------------

        private static Scene NewScene()
        {
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void SaveScene(Scene scene, string name)
        {
            string path = $"{ProjectBootstrap.SceneRoot}/{name}.unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[Starfall] Saved scene {path}");
        }

        private static Camera CreateCamera(Color background)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.nearClipPlane = -10f;
            cam.farClipPlane = 100f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            go.transform.position = new Vector3(0f, 0f, -10f);
            go.AddComponent<AudioListener>();
            return cam;
        }

        private static PlayArea CreatePlayArea(Camera cam, GameConfig config)
        {
            var go = new GameObject("PlayArea");
            var area = go.AddComponent<PlayArea>();
            area.targetCamera = cam;
            area.targetWidth = config != null ? config.PlayAreaWidth : 10f;
            area.despawnMargin = config != null ? config.DespawnMargin : 2.5f;
            return area;
        }

        private static StageBackground CreateBackground(PlaceholderArt.Set art, PlayArea area, Camera cam, StageDefinition autoApply)
        {
            var go = new GameObject("Background");
            var bg = go.AddComponent<StageBackground>();
            bg.area = area;
            bg.dotSprite = art.Dot;
            bg.gradientSprite = art.Gradient;
            bg.targetCamera = cam;
            bg.starCount = 90;
            bg.autoApply = autoApply;
            return bg;
        }

        private static SettingsPanel CreateSettingsPanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "SettingsPanel", out var column);
            var panel = root.gameObject.AddComponent<SettingsPanel>();
            UiBuilder.AddColumnText(column, "Title", "SETTINGS", 64f, UiBuilder.Accent, 90f, FontStyles.Bold);
            panel.masterSlider = UiBuilder.CreateSlider(column, "Master", "MASTER VOLUME", 0f, 1f, out _);
            panel.musicSlider = UiBuilder.CreateSlider(column, "Music", "MUSIC", 0f, 1f, out _);
            panel.sfxSlider = UiBuilder.CreateSlider(column, "Sfx", "EFFECTS", 0f, 1f, out _);
            panel.sensitivitySlider = UiBuilder.CreateSlider(column, "Sensitivity", "TOUCH SENSITIVITY", 0.5f, 3f, out _);
            panel.autoFireToggle = UiBuilder.CreateToggle(column, "AutoFire", "AUTO FIRE");
            panel.resetProgressButton = UiBuilder.CreateButton(column, "ResetProgress", "RESET PROGRESS", new Vector2(560f, 88f), 34f, new Color(0.35f, 0.1f, 0.15f, 0.95f));
            panel.resetFeedbackText = UiBuilder.AddColumnText(column, "ResetFeedback", "", 28f, UiBuilder.Accent2, 40f);
            panel.backButton = UiBuilder.CreateButton(column, "Back", "BACK", new Vector2(560f, 96f));
            panel.firstSelected = panel.masterSlider.gameObject;
            return panel;
        }

        // ---- Boot -----------------------------------------------------------------------------------------------

        private static void BuildBoot(ProjectBootstrap.DataSet data)
        {
            var scene = NewScene();
            CreateCamera(Color.black);
            var bootGo = new GameObject("Boot");
            var boot = bootGo.AddComponent<BootLoader>();
            boot.config = data.Config;
            boot.audioLibrary = data.Audio;
            boot.minimumSplashSeconds = 0.8f;

            var canvas = UiBuilder.CreateCanvas("Canvas", 0);
            var title = UiBuilder.CreateText(canvas.transform, "Title", "STARFALL\nDEFENSE", 110f, UiBuilder.Accent, TextAlignmentOptions.Center, FontStyles.Bold);
            UiBuilder.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 400f));
            var sub = UiBuilder.CreateText(canvas.transform, "Subtitle", "MVP BUILD  -  PLACEHOLDER ASSETS", 30f, new Color(0.6f, 0.7f, 0.8f));
            UiBuilder.Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -260f), new Vector2(900f, 60f));
            SaveScene(scene, data.Config.BootScene);
        }

        // ---- Main menu -------------------------------------------------------------------------------------------

        private static void BuildMainMenu(PlaceholderArt.Set art, ProjectBootstrap.DataSet data)
        {
            var scene = NewScene();
            var cam = CreateCamera(data.MenuLook.BackgroundBottom);
            var area = CreatePlayArea(cam, data.Config);
            CreateBackground(art, area, cam, data.MenuLook);
            UiBuilder.CreateEventSystem();

            var canvas = UiBuilder.CreateCanvas("Canvas", 0);
            var controllerGo = new GameObject("MainMenu");
            var controller = controllerGo.AddComponent<MainMenuController>();
            controller.config = data.Config;
            controller.audioLibrary = data.Audio;

            // Main panel
            var mainRoot = UiBuilder.CreatePanelRoot(canvas.transform, "MainPanel", out var column);
            mainRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            var mainPanel = mainRoot.gameObject.AddComponent<UiPanel>();
            UiBuilder.AddColumnText(column, "Title", "STARFALL", 120f, UiBuilder.Accent, 130f, FontStyles.Bold);
            UiBuilder.AddColumnText(column, "Title2", "DEFENSE", 72f, UiBuilder.Accent2, 90f, FontStyles.Bold);
            controller.highScoreText = UiBuilder.AddColumnText(column, "HighScore", "HIGH SCORE  0", 32f, UiBuilder.TextColor, 60f);
            controller.playButton = UiBuilder.CreateButton(column, "Play", "PLAY", new Vector2(600f, 110f), 44f);
            controller.continueButton = UiBuilder.CreateButton(column, "Continue", "CONTINUE", new Vector2(600f, 110f), 44f);
            controller.continueLabel = controller.continueButton.GetComponentInChildren<TextMeshProUGUI>();
            controller.settingsButton = UiBuilder.CreateButton(column, "Settings", "SETTINGS", new Vector2(600f, 96f), 38f);
            controller.creditsButton = UiBuilder.CreateButton(column, "Credits", "CREDITS", new Vector2(600f, 96f), 38f);
            controller.quitButton = UiBuilder.CreateButton(column, "Quit", "QUIT", new Vector2(600f, 96f), 38f);
            controller.versionText = UiBuilder.AddColumnText(column, "Version", "v0.1.0", 24f, new Color(0.5f, 0.6f, 0.7f), 40f);
            mainPanel.firstSelected = controller.playButton.gameObject;
            controller.mainPanel = mainPanel;

            // Settings
            controller.settingsPanel = CreateSettingsPanel(canvas.transform);

            // Credits
            var creditsRoot = UiBuilder.CreatePanelRoot(canvas.transform, "CreditsPanel", out var creditsColumn);
            var creditsPanel = creditsRoot.gameObject.AddComponent<UiPanel>();
            UiBuilder.AddColumnText(creditsColumn, "Title", "CREDITS", 64f, UiBuilder.Accent, 90f, FontStyles.Bold);
            UiBuilder.AddColumnText(creditsColumn, "Body",
                "STARFALL DEFENSE - MVP 1.0\n\nDesign, code and placeholder art: Starfall Team\nBuilt with Unity 6\n\nPlaceholder audio is synthesized at runtime.\nAll shapes are original procedural placeholders.",
                30f, UiBuilder.TextColor, 320f);
            controller.creditsBackButton = UiBuilder.CreateButton(creditsColumn, "Back", "BACK", new Vector2(560f, 96f));
            creditsPanel.firstSelected = controller.creditsBackButton.gameObject;
            controller.creditsPanel = creditsPanel;

            SaveScene(scene, data.Config.MainMenuScene);
        }

        // ---- Gameplay -----------------------------------------------------------------------------------------------

        private static void BuildGameplay(PlaceholderArt.Set art, ProjectBootstrap.DataSet data, ProjectBootstrap.PrefabSet prefabs)
        {
            var scene = NewScene();
            var config = data.Config;
            var cam = CreateCamera(data.Stages[0].BackgroundBottom);
            var shake = cam.gameObject.AddComponent<CameraShake>();
            var area = CreatePlayArea(cam, config);
            var background = CreateBackground(art, area, cam, null);
            UiBuilder.CreateEventSystem();

            // Services
            var services = new GameObject("Services");
            var ctx = services.AddComponent<GameplayContext>();
            var pools = services.AddComponent<PoolService>();
            var registry = services.AddComponent<EnemyRegistry>();
            var spawner = services.AddComponent<EnemySpawner>();
            var score = services.AddComponent<ScoreService>();
            var director = services.AddComponent<StageDirector>();
            var vfx = services.AddComponent<VfxSpawner>();
            var input = services.AddComponent<GameInputReader>();
            var flow = services.AddComponent<GameFlowController>();

            spawner.defaultEnemyPrefab = prefabs.Enemy;
            spawner.enemyProjectilePrefab = prefabs.EnemyProjectile;
            spawner.defaultPickupPrefab = prefabs.Pickup;
            score.config = config;

            var flashGo = new GameObject("ScreenFlash");
            flashGo.transform.SetParent(services.transform, false);
            var flash = flashGo.AddComponent<SpriteRenderer>();
            flash.sprite = art.Pixel;
            flash.color = new Color(1f, 1f, 1f, 0f);
            flash.enabled = false;
            flash.sortingOrder = SortingOrders.Vfx + 5;
            vfx.pools = pools;
            vfx.explosionPrefab = prefabs.Explosion;
            vfx.floatingTextPrefab = prefabs.FloatingText;
            vfx.screenFlash = flash;

            // Player
            var playerGo = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Player);
            playerGo.name = "Player";
            playerGo.transform.position = new Vector3(0f, -5f, 0f);
            var player = playerGo.GetComponent<PlayerShip>();

            ctx.config = config;
            ctx.audioLibrary = data.Audio;
            ctx.playArea = area;
            ctx.pools = pools;
            ctx.enemies = registry;
            ctx.spawner = spawner;
            ctx.player = player;
            ctx.input = input;
            ctx.score = score;
            ctx.stageDirector = director;
            ctx.vfx = vfx;
            ctx.cameraShake = shake;

            // UI
            var canvas = UiBuilder.CreateCanvas("Canvas", 0);

            var touchPad = UiBuilder.CreateImage(canvas.transform, "TouchPad", new Color(0f, 0f, 0f, 0f), null, true);
            UiBuilder.Stretch(touchPad.rectTransform);
            var pad = touchPad.gameObject.AddComponent<TouchPad>();
            pad.input = input;

            var hudRoot = UiBuilder.CreateRect(canvas.transform, "HUD");
            UiBuilder.Stretch(hudRoot);
            hudRoot.gameObject.AddComponent<SafeAreaFitter>();
            var hudView = hudRoot.gameObject.AddComponent<HudView>();
            var hudPresenter = hudRoot.gameObject.AddComponent<HudPresenter>();
            hudPresenter.view = hudView;
            BuildHud(hudRoot, hudView, art);

            var briefing = BuildBriefingPanel(canvas.transform);
            var pause = BuildPausePanel(canvas.transform);
            var victory = BuildVictoryPanel(canvas.transform);
            var gameOver = BuildGameOverPanel(canvas.transform);
            var settings = CreateSettingsPanel(canvas.transform);

            flow.ctx = ctx;
            flow.hud = hudPresenter;
            flow.briefingPanel = briefing;
            flow.pausePanel = pause;
            flow.victoryPanel = victory;
            flow.gameOverPanel = gameOver;
            flow.settingsPanel = settings;
            flow.background = background;
            flow.touchControls = touchPad.gameObject;

            SaveScene(scene, config.GameplayScene);
        }

        private static void BuildHud(RectTransform root, HudView view, PlaceholderArt.Set art)
        {
            var textColor = UiBuilder.TextColor;
            const float margin = 28f;

            // Top-left: lives, hull, shield
            var topLeft = UiBuilder.CreateRect(root, "TopLeft");
            UiBuilder.Place(topLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(margin, -margin), new Vector2(420f, 170f));
            var livesLabel = UiBuilder.CreateText(topLeft, "LivesLabel", "SHIPS", 26f, new Color(0.6f, 0.75f, 0.9f), TextAlignmentOptions.Left);
            UiBuilder.Place(livesLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(120f, 36f));
            view.livesText = UiBuilder.CreateText(topLeft, "Lives", "x3", 40f, textColor, TextAlignmentOptions.Left, FontStyles.Bold);
            UiBuilder.Place(view.livesText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(110f, 4f), new Vector2(200f, 48f));
            var hull = UiBuilder.CreateBar(topLeft, "HullBar", new Vector2(400f, 30f), new Color(0.1f, 0.1f, 0.14f, 0.85f), new Color(1f, 0.45f, 0.3f), out view.hullFill);
            UiBuilder.Place(hull.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -54f), new Vector2(400f, 30f));
            var hullLabel = UiBuilder.CreateText(hull.transform, "Label", "HULL", 20f, Color.white, TextAlignmentOptions.Left, FontStyles.Bold);
            UiBuilder.Stretch(hullLabel.rectTransform, 10f, 0f, 0f, 0f);
            var shield = UiBuilder.CreateBar(topLeft, "ShieldBar", new Vector2(400f, 30f), new Color(0.1f, 0.1f, 0.14f, 0.85f), new Color(0.35f, 0.8f, 1f), out view.shieldFill);
            UiBuilder.Place(shield.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -92f), new Vector2(400f, 30f));
            var shieldLabel = UiBuilder.CreateText(shield.transform, "Label", "SHIELD", 20f, Color.white, TextAlignmentOptions.Left, FontStyles.Bold);
            UiBuilder.Stretch(shieldLabel.rectTransform, 10f, 0f, 0f, 0f);

            // Top-right: score + multiplier
            var topRight = UiBuilder.CreateRect(root, "TopRight");
            UiBuilder.Place(topRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-margin, -margin), new Vector2(420f, 140f));
            var scoreLabel = UiBuilder.CreateText(topRight, "ScoreLabel", "SCORE", 26f, new Color(0.6f, 0.75f, 0.9f), TextAlignmentOptions.Right);
            UiBuilder.Place(scoreLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(420f, 36f));
            view.scoreText = UiBuilder.CreateText(topRight, "Score", "0", 48f, textColor, TextAlignmentOptions.Right, FontStyles.Bold);
            UiBuilder.Place(view.scoreText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(420f, 56f));
            view.multiplierText = UiBuilder.CreateText(topRight, "Multiplier", "x1", 40f, textColor, TextAlignmentOptions.Right, FontStyles.Bold);
            UiBuilder.Place(view.multiplierText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, -92f), new Vector2(420f, 48f));

            // Top-center: pause
            view.pauseButton = UiBuilder.CreateButton(root, "PauseButton", "II", new Vector2(96f, 72f), 36f, new Color(0.08f, 0.12f, 0.22f, 0.8f));
            UiBuilder.Place(view.pauseButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -margin), new Vector2(96f, 72f));

            // Center: message + boss bar
            view.messageText = UiBuilder.CreateText(root, "Message", "", 54f, UiBuilder.Accent, TextAlignmentOptions.Center, FontStyles.Bold);
            UiBuilder.Place(view.messageText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 260f), new Vector2(1000f, 140f));
            view.messageText.enabled = false;

            var bossGroup = UiBuilder.CreateRect(root, "BossGroup");
            UiBuilder.Place(bossGroup, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(700f, 70f));
            view.bossNameText = UiBuilder.CreateText(bossGroup, "BossName", "BOSS", 30f, UiBuilder.Accent2, TextAlignmentOptions.Center, FontStyles.Bold);
            UiBuilder.Place(view.bossNameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(700f, 36f));
            var bossBar = UiBuilder.CreateBar(bossGroup, "BossBar", new Vector2(700f, 24f), new Color(0.1f, 0.05f, 0.1f, 0.9f), UiBuilder.Accent2, out view.bossFill);
            UiBuilder.Place(bossBar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(700f, 24f));
            view.bossGroup = bossGroup.gameObject;
            bossGroup.gameObject.SetActive(false);

            // Bottom-center: energy bar (tap to activate Ultimate)
            view.ultimateButton = UiBuilder.CreateButton(root, "UltimateButton", "", new Vector2(520f, 84f), 30f, new Color(0.08f, 0.1f, 0.18f, 0.9f));
            var ultRt = view.ultimateButton.GetComponent<RectTransform>();
            UiBuilder.Place(ultRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, margin), new Vector2(520f, 84f));
            Object.DestroyImmediate(view.ultimateButton.GetComponent<LayoutElement>());
            var glow = UiBuilder.CreateImage(ultRt, "Glow", new Color(0.75f, 0.4f, 1f, 0.55f), art.Panel);
            UiBuilder.Stretch(glow.rectTransform, -8f, -8f, -8f, -8f);
            glow.transform.SetAsFirstSibling();
            view.ultimateGlow = glow;
            glow.enabled = false;
            view.energyFill = UiBuilder.CreateImage(ultRt, "Fill", new Color(0.75f, 0.4f, 1f, 0.9f), art.Panel);
            UiBuilder.Stretch(view.energyFill.rectTransform, 6f, 6f, 6f, 6f);
            view.energyFill.type = Image.Type.Filled;
            view.energyFill.fillMethod = Image.FillMethod.Horizontal;
            view.energyFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            view.energyFill.fillAmount = 0f;
            view.energyFill.transform.SetSiblingIndex(1);
            view.energyLabel = view.ultimateButton.GetComponentInChildren<TextMeshProUGUI>();
            view.energyLabel.text = "ENERGY 0%";
            view.energyLabel.transform.SetAsLastSibling();

            // Bottom-left: weapon
            view.weaponText = UiBuilder.CreateText(root, "Weapon", "LASER LV.1", 30f, textColor, TextAlignmentOptions.Left, FontStyles.Bold);
            UiBuilder.Place(view.weaponText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(margin, margin + 8f), new Vector2(260f, 60f));

            // Bottom-right: special indicator
            view.specialText = UiBuilder.CreateText(root, "Special", "", 30f, new Color(1f, 0.9f, 0.4f), TextAlignmentOptions.Right, FontStyles.Bold);
            UiBuilder.Place(view.specialText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margin, margin + 8f), new Vector2(260f, 60f));
        }

        private static BriefingPanel BuildBriefingPanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "BriefingPanel", out var column, 900f);
            var panel = root.gameObject.AddComponent<BriefingPanel>();
            panel.titleText = UiBuilder.AddColumnText(column, "Title", "STAGE 1", 60f, UiBuilder.Accent, 80f, FontStyles.Bold);
            panel.subtitleText = UiBuilder.AddColumnText(column, "Subtitle", "", 30f, UiBuilder.Accent2, 44f);
            panel.bodyText = UiBuilder.AddColumnText(column, "Body", "", 30f, UiBuilder.TextColor, 360f);
            panel.launchButton = UiBuilder.CreateButton(column, "Launch", "LAUNCH", new Vector2(560f, 110f), 44f);
            panel.hintText = UiBuilder.AddColumnText(column, "Hint", "TAP TO LAUNCH", 26f, new Color(0.6f, 0.7f, 0.85f), 40f);
            panel.firstSelected = panel.launchButton.gameObject;
            return panel;
        }

        private static PausePanel BuildPausePanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "PausePanel", out var column);
            var panel = root.gameObject.AddComponent<PausePanel>();
            UiBuilder.AddColumnText(column, "Title", "PAUSED", 72f, UiBuilder.Accent, 100f, FontStyles.Bold);
            panel.continueButton = UiBuilder.CreateButton(column, "Continue", "CONTINUE", new Vector2(560f, 104f), 42f);
            panel.settingsButton = UiBuilder.CreateButton(column, "Settings", "SETTINGS", new Vector2(560f, 96f), 38f);
            panel.restartButton = UiBuilder.CreateButton(column, "Restart", "RESTART STAGE", new Vector2(560f, 96f), 38f);
            panel.menuButton = UiBuilder.CreateButton(column, "Menu", "MAIN MENU", new Vector2(560f, 96f), 38f);
            panel.firstSelected = panel.continueButton.gameObject;
            return panel;
        }

        private static VictoryPanel BuildVictoryPanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "VictoryPanel", out var column);
            var panel = root.gameObject.AddComponent<VictoryPanel>();
            panel.titleText = UiBuilder.AddColumnText(column, "Title", "SECTOR CLEARED", 60f, UiBuilder.Accent, 90f, FontStyles.Bold);
            panel.scoreText = UiBuilder.AddColumnText(column, "Score", "SCORE 0", 40f, UiBuilder.TextColor, 56f, FontStyles.Bold);
            panel.multiplierText = UiBuilder.AddColumnText(column, "Multiplier", "", 30f, UiBuilder.TextColor, 44f);
            panel.enemiesText = UiBuilder.AddColumnText(column, "Enemies", "", 30f, UiBuilder.TextColor, 44f);
            panel.damageText = UiBuilder.AddColumnText(column, "Damage", "", 30f, UiBuilder.TextColor, 44f);
            panel.livesText = UiBuilder.AddColumnText(column, "Lives", "", 30f, UiBuilder.TextColor, 44f);
            panel.recordText = UiBuilder.AddColumnText(column, "Record", "NEW HIGH SCORE!", 34f, new Color(1f, 0.85f, 0.2f), 48f, FontStyles.Bold);
            panel.nextButton = UiBuilder.CreateButton(column, "Next", "NEXT STAGE", new Vector2(560f, 104f), 42f);
            panel.menuButton = UiBuilder.CreateButton(column, "Menu", "MAIN MENU", new Vector2(560f, 96f), 38f);
            panel.firstSelected = panel.nextButton.gameObject;
            return panel;
        }

        private static GameOverPanel BuildGameOverPanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "GameOverPanel", out var column);
            var panel = root.gameObject.AddComponent<GameOverPanel>();
            UiBuilder.AddColumnText(column, "Title", "GAME OVER", 72f, UiBuilder.Accent2, 100f, FontStyles.Bold);
            panel.scoreText = UiBuilder.AddColumnText(column, "Score", "FINAL SCORE 0", 40f, UiBuilder.TextColor, 56f, FontStyles.Bold);
            panel.recordText = UiBuilder.AddColumnText(column, "Record", "NEW HIGH SCORE!", 34f, new Color(1f, 0.85f, 0.2f), 48f, FontStyles.Bold);
            panel.restartButton = UiBuilder.CreateButton(column, "Restart", "RETRY STAGE", new Vector2(560f, 104f), 42f);
            panel.menuButton = UiBuilder.CreateButton(column, "Menu", "MAIN MENU", new Vector2(560f, 96f), 38f);
            panel.firstSelected = panel.restartButton.gameObject;
            return panel;
        }
    }
}
