using Starfall.Combat;
using Starfall.Core;
using Starfall.Enemies;
using Starfall.Input;
using Starfall.Logic;
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
        public static void BuildAll(PlaceholderArt.Set art, ContentFactory.DataSet data, ProjectBootstrap.PrefabSet prefabs)
        {
            BuildBoot(art, data);
            BuildMainMenu(art, data);
            BuildGameplay(art, data, prefabs);
        }

        // ---- Shared -------------------------------------------------------------------------------------------

        private static Scene NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

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
            panel.shakeSlider = UiBuilder.CreateSlider(column, "Shake", "SCREEN SHAKE", 0f, 1f, out _);
            panel.autoFireToggle = UiBuilder.CreateToggle(column, "AutoFire", "AUTO FIRE");
            panel.reducedEffectsToggle = UiBuilder.CreateToggle(column, "ReducedEffects", "REDUCED EFFECTS");
            panel.grazeFeedbackToggle = UiBuilder.CreateToggle(column, "GrazeFeedback", "GRAZE FEEDBACK");
            panel.showHitboxToggle = UiBuilder.CreateToggle(column, "ShowHitbox", "SHOW HITBOX");
            panel.resetProgressButton = UiBuilder.CreateButton(column, "ResetProgress", "RESET PROGRESS", new Vector2(560f, 88f), 34f, new Color(0.35f, 0.1f, 0.15f, 0.95f));
            panel.resetFeedbackText = UiBuilder.AddColumnText(column, "ResetFeedback", "", 28f, UiBuilder.Accent2, 40f);
            panel.backButton = UiBuilder.CreateButton(column, "Back", "BACK", new Vector2(560f, 96f));
            panel.firstSelected = panel.masterSlider.gameObject;
            return panel;
        }

        private static AchievementToast CreateToast(Transform canvas)
        {
            var host = UiBuilder.CreateRect(canvas, "AchievementToast");
            UiBuilder.Place(host, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(760f, 120f));
            var toast = host.gameObject.AddComponent<AchievementToast>();
            var panel = UiBuilder.CreateImage(host, "Panel", new Color(0.05f, 0.12f, 0.2f, 0.95f), UiBuilder.PanelSprite);
            panel.type = Image.Type.Sliced;
            UiBuilder.Stretch(panel.rectTransform);
            var title = UiBuilder.CreateText(panel.transform, "Title", "ACHIEVEMENT", 30f, new Color(1f, 0.85f, 0.3f), TextAlignmentOptions.Center, FontStyles.Bold);
            UiBuilder.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(740f, 44f));
            var body = UiBuilder.CreateText(panel.transform, "Body", "", 24f, UiBuilder.TextColor);
            UiBuilder.Place(body.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(740f, 50f));
            toast.panel = panel.rectTransform;
            toast.titleText = title;
            toast.bodyText = body;
            panel.gameObject.SetActive(false);
            return toast;
        }

        // ---- Boot / splash ----------------------------------------------------------------------------------

        private static void BuildBoot(PlaceholderArt.Set art, ContentFactory.DataSet data)
        {
            var scene = NewScene();
            CreateCamera(Color.black);
            var bootGo = new GameObject("Boot");
            var boot = bootGo.AddComponent<BootLoader>();
            boot.config = data.Config;
            boot.audioLibrary = data.Audio;
            boot.cardSeconds = 1.4f;

            var canvas = UiBuilder.CreateCanvas("Canvas", 0);

            // Studio card
            var studio = UiBuilder.CreateRect(canvas.transform, "StudioCard");
            UiBuilder.Stretch(studio);
            var studioGroup = studio.gameObject.AddComponent<CanvasGroup>();
            studioGroup.alpha = 0f;
            var logo = UiBuilder.CreateImage(studio, "Logo", UiBuilder.Accent, art.Logo);
            UiBuilder.Place(logo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(260f, 260f));
            if (UiBuilder.HologramMaterial != null) logo.material = UiBuilder.HologramMaterial;
            var studioName = UiBuilder.CreateText(studio, "Name", "STARFALL TEAM", 60f, UiBuilder.TextColor, TextAlignmentOptions.Center, FontStyles.Bold);
            UiBuilder.Place(studioName.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -100f), new Vector2(900f, 90f));
            var presents = UiBuilder.CreateText(studio, "Presents", "presents", 30f, new Color(0.6f, 0.7f, 0.85f));
            UiBuilder.Place(presents.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(900f, 50f));
            studio.gameObject.SetActive(false);

            // Title card
            var titleCard = UiBuilder.CreateRect(canvas.transform, "TitleCard");
            UiBuilder.Stretch(titleCard);
            var titleGroup = titleCard.gameObject.AddComponent<CanvasGroup>();
            titleGroup.alpha = 0f;
            var title = UiBuilder.CreateText(titleCard, "Title", "STARFALL\nDEFENSE", 110f, UiBuilder.Accent, TextAlignmentOptions.Center, FontStyles.Bold);
            UiBuilder.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 400f));
            var sub = UiBuilder.CreateText(titleCard, "Subtitle", "MADE WITH UNITY  -  PLACEHOLDER ASSETS", 28f, new Color(0.6f, 0.7f, 0.8f));
            UiBuilder.Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -260f), new Vector2(900f, 60f));
            titleCard.gameObject.SetActive(false);

            boot.studioCard = studioGroup;
            boot.titleCard = titleGroup;
            SaveScene(scene, data.Config.BootScene);
        }

        // ---- Main menu -------------------------------------------------------------------------------------------

        private static void BuildMainMenu(PlaceholderArt.Set art, ContentFactory.DataSet data)
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
            controller.highScoreText = UiBuilder.AddColumnText(column, "HighScore", "HIGH SCORE  0", 30f, UiBuilder.TextColor, 44f);
            controller.pilotText = UiBuilder.AddColumnText(column, "Pilot", "PILOT LV.1", 26f, new Color(0.7f, 0.85f, 1f), 40f);
            controller.playButton = UiBuilder.CreateButton(column, "Play", "PLAY", new Vector2(600f, 104f), 44f);
            controller.hangarButton = UiBuilder.CreateButton(column, "Hangar", "HANGAR", new Vector2(600f, 90f), 36f);
            controller.upgradesButton = UiBuilder.CreateButton(column, "Upgrades", "UPGRADES", new Vector2(600f, 90f), 36f);
            controller.rankingButton = UiBuilder.CreateButton(column, "Ranking", "RANKING", new Vector2(600f, 90f), 36f);
            controller.settingsButton = UiBuilder.CreateButton(column, "Settings", "SETTINGS", new Vector2(600f, 90f), 36f);
            controller.creditsButton = UiBuilder.CreateButton(column, "Credits", "CREDITS", new Vector2(600f, 90f), 36f);
            controller.quitButton = UiBuilder.CreateButton(column, "Quit", "QUIT", new Vector2(600f, 90f), 36f);
            controller.versionText = UiBuilder.AddColumnText(column, "Version", "v0.2.0", 24f, new Color(0.5f, 0.6f, 0.7f), 36f);
            mainPanel.firstSelected = controller.playButton.gameObject;
            controller.mainPanel = mainPanel;

            controller.missionPanel = BuildMissionPanel(canvas.transform, data);
            controller.hangarPanel = BuildHangarPanel(canvas.transform, art);
            controller.upgradesPanel = BuildUpgradesPanel(canvas.transform);
            controller.rankingPanel = BuildRankingPanel(canvas.transform);
            controller.settingsPanel = CreateSettingsPanel(canvas.transform);

            // Credits
            var creditsRoot = UiBuilder.CreatePanelRoot(canvas.transform, "CreditsPanel", out var creditsColumn);
            var creditsPanel = creditsRoot.gameObject.AddComponent<UiPanel>();
            UiBuilder.AddColumnText(creditsColumn, "Title", "CREDITS", 64f, UiBuilder.Accent, 90f, FontStyles.Bold);
            UiBuilder.AddColumnText(creditsColumn, "Body",
                "STARFALL DEFENSE\n\nTEAM\nDesign, code and placeholder art: Starfall Team\n\nTOOLS\nUnity 6, Input System, TextMeshPro\nProcedural SDF sprites and synthesized audio\n\nAll shapes and sounds are original placeholders.\nNo third-party assets are included.",
                28f, UiBuilder.TextColor, 420f);
            controller.creditsBackButton = UiBuilder.CreateButton(creditsColumn, "Back", "BACK", new Vector2(560f, 96f));
            creditsPanel.firstSelected = controller.creditsBackButton.gameObject;
            controller.creditsPanel = creditsPanel;

            CreateToast(canvas.transform);
            SaveScene(scene, data.Config.MainMenuScene);
        }

        private static MissionPanel BuildMissionPanel(Transform canvas, ContentFactory.DataSet data)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "MissionPanel", out var column, 900f);
            var panel = root.gameObject.AddComponent<MissionPanel>();
            UiBuilder.AddColumnText(column, "Title", "SELECT MISSION", 60f, UiBuilder.Accent, 84f, FontStyles.Bold);
            UiBuilder.AddColumnText(column, "CampaignLabel", "CAMPAIGN", 30f, UiBuilder.Accent2, 40f, FontStyles.Bold);
            int count = data.Stages.Count;
            panel.stageRows = new ListRow[count];
            for (int i = 0; i < count; i++)
                panel.stageRows[i] = UiBuilder.CreateListRow(column, "Stage" + i, new Vector2(860f, 84f), false);
            UiBuilder.AddColumnText(column, "ModesLabel", "EXTRA MODES", 30f, UiBuilder.Accent2, 40f, FontStyles.Bold);
            var modes = UiBuilder.CreateRect(column, "Modes");
            var modesLayout = modes.gameObject.AddComponent<HorizontalLayoutGroup>();
            modesLayout.spacing = 14f;
            modesLayout.childAlignment = TextAnchor.MiddleCenter;
            modesLayout.childControlWidth = false;
            modesLayout.childControlHeight = false;
            var modesLe = modes.gameObject.AddComponent<LayoutElement>();
            modesLe.preferredHeight = 96f;
            modesLe.preferredWidth = 860f;
            panel.survivalButton = UiBuilder.CreateButton(modes, "Survival", "SURVIVAL", new Vector2(276f, 90f), 30f);
            panel.bossRushButton = UiBuilder.CreateButton(modes, "BossRush", "BOSS RUSH", new Vector2(276f, 90f), 30f);
            panel.dailyButton = UiBuilder.CreateButton(modes, "Daily", "DAILY", new Vector2(276f, 90f), 30f);
            panel.modeInfoText = UiBuilder.AddColumnText(column, "ModeInfo", "", 22f, new Color(0.7f, 0.8f, 0.95f), 80f);
            panel.backButton = UiBuilder.CreateButton(column, "Back", "BACK", new Vector2(560f, 90f));
            panel.firstSelected = panel.stageRows.Length > 0 ? panel.stageRows[0].gameObject : panel.backButton.gameObject;
            return panel;
        }

        private static HangarPanel BuildHangarPanel(Transform canvas, PlaceholderArt.Set art)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "HangarPanel", out var column, 960f);
            var panel = root.gameObject.AddComponent<HangarPanel>();
            var layout = column.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            UiBuilder.AddColumnText(column, "Title", "HANGAR", 54f, UiBuilder.Accent, 66f, FontStyles.Bold);

            // Preview block: holographic ship + description + stats
            var previewBlock = UiBuilder.CreateRect(column, "Preview");
            var pbLe = previewBlock.gameObject.AddComponent<LayoutElement>();
            pbLe.preferredHeight = 330f;
            pbLe.preferredWidth = 920f;
            var glow = UiBuilder.CreateImage(previewBlock, "Glow", new Color(0.35f, 0.9f, 1f, 0.3f), art.Dot);
            UiBuilder.Place(glow.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150f, 0f), new Vector2(300f, 300f));
            var previewImage = UiBuilder.CreateImage(previewBlock, "Ship", Color.white, art.Ship);
            UiBuilder.Place(previewImage.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150f, 0f), new Vector2(220f, 220f));
            previewImage.preserveAspect = true;
            if (UiBuilder.HologramMaterial != null) previewImage.material = UiBuilder.HologramMaterial;
            var holo = previewBlock.gameObject.AddComponent<HologramPreview>();
            holo.image = previewImage;
            holo.glow = glow;
            panel.previewImage = previewImage;
            panel.preview = holo;
            panel.previewName = UiBuilder.CreateText(previewBlock, "Name", "SF-01 VANGUARD", 36f, UiBuilder.TextColor, TextAlignmentOptions.Left, FontStyles.Bold);
            UiBuilder.Place(panel.previewName.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(310f, 0f), new Vector2(600f, 48f));
            panel.previewDescription = UiBuilder.CreateText(previewBlock, "Description", "", 24f, new Color(0.8f, 0.88f, 1f), TextAlignmentOptions.TopLeft);
            UiBuilder.Place(panel.previewDescription.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(310f, -52f), new Vector2(600f, 110f));
            panel.statsText = UiBuilder.CreateText(previewBlock, "Stats", "", 22f, UiBuilder.Accent, TextAlignmentOptions.TopLeft);
            UiBuilder.Place(panel.statsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(310f, -170f), new Vector2(600f, 120f));

            panel.walletText = UiBuilder.AddColumnText(column, "Wallet", "0 CREDITS", 26f, new Color(1f, 0.85f, 0.3f), 36f, FontStyles.Bold);
            panel.actionButton = UiBuilder.CreateButton(column, "Action", "EQUIP", new Vector2(560f, 84f), 34f, new Color(0.1f, 0.3f, 0.25f, 0.95f));
            panel.actionLabel = panel.actionButton.GetComponentInChildren<TextMeshProUGUI>();

            // Two columns: ships (left) and weapons (right)
            var lists = UiBuilder.CreateRect(column, "Lists");
            var listsLe = lists.gameObject.AddComponent<LayoutElement>();
            listsLe.preferredHeight = 7 * 62f + 40f;
            listsLe.preferredWidth = 920f;
            var shipsCol = UiBuilder.CreateRect(lists, "Ships");
            UiBuilder.Place(shipsCol, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(450f, 480f));
            var shipsLayout = shipsCol.gameObject.AddComponent<VerticalLayoutGroup>();
            shipsLayout.spacing = 6f;
            shipsLayout.childControlWidth = false;
            shipsLayout.childControlHeight = false;
            shipsLayout.childForceExpandHeight = false;
            var weaponsCol = UiBuilder.CreateRect(lists, "Weapons");
            UiBuilder.Place(weaponsCol, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(450f, 480f));
            var weaponsLayout = weaponsCol.gameObject.AddComponent<VerticalLayoutGroup>();
            weaponsLayout.spacing = 6f;
            weaponsLayout.childControlWidth = false;
            weaponsLayout.childControlHeight = false;
            weaponsLayout.childForceExpandHeight = false;

            var shipsLabel = UiBuilder.CreateText(shipsCol, "Label", "SHIPS", 24f, UiBuilder.Accent2, TextAlignmentOptions.Left, FontStyles.Bold);
            shipsLabel.rectTransform.sizeDelta = new Vector2(450f, 30f);
            panel.shipRows = new ListRow[ProgressionRules.ShipCount];
            for (int i = 0; i < panel.shipRows.Length; i++)
                panel.shipRows[i] = UiBuilder.CreateListRow(shipsCol, "Ship" + i, new Vector2(450f, 56f), true);
            var weaponsLabel = UiBuilder.CreateText(weaponsCol, "Label", "WEAPONS", 24f, UiBuilder.Accent2, TextAlignmentOptions.Left, FontStyles.Bold);
            weaponsLabel.rectTransform.sizeDelta = new Vector2(450f, 30f);
            panel.weaponRows = new ListRow[ProgressionRules.WeaponCount];
            for (int i = 0; i < panel.weaponRows.Length; i++)
                panel.weaponRows[i] = UiBuilder.CreateListRow(weaponsCol, "Weapon" + i, new Vector2(450f, 56f), true);

            panel.backButton = UiBuilder.CreateButton(column, "Back", "BACK", new Vector2(560f, 84f));
            panel.firstSelected = panel.shipRows[0].gameObject;
            return panel;
        }

        private static UpgradesPanel BuildUpgradesPanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "UpgradesPanel", out var column, 960f);
            var panel = root.gameObject.AddComponent<UpgradesPanel>();
            column.GetComponent<VerticalLayoutGroup>().spacing = 10f;
            UiBuilder.AddColumnText(column, "Title", "UPGRADES", 54f, UiBuilder.Accent, 66f, FontStyles.Bold);
            panel.walletText = UiBuilder.AddColumnText(column, "Wallet", "0 CREDITS", 26f, new Color(1f, 0.85f, 0.3f), 36f, FontStyles.Bold);
            panel.rows = new ListRow[UpgradeCatalog.NodeCount];
            for (int i = 0; i < panel.rows.Length; i++)
                panel.rows[i] = UiBuilder.CreateListRow(column, "Node" + i, new Vector2(920f, 96f), false);
            panel.backButton = UiBuilder.CreateButton(column, "Back", "BACK", new Vector2(560f, 84f));
            panel.firstSelected = panel.rows[0].gameObject;
            return panel;
        }

        private static RankingPanel BuildRankingPanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "RankingPanel", out var column, 960f);
            var panel = root.gameObject.AddComponent<RankingPanel>();
            column.GetComponent<VerticalLayoutGroup>().spacing = 8f;
            UiBuilder.AddColumnText(column, "Title", "RANKING", 54f, UiBuilder.Accent, 66f, FontStyles.Bold);
            var tabs = UiBuilder.CreateRect(column, "Tabs");
            var tabsLayout = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 8f;
            tabsLayout.childAlignment = TextAnchor.MiddleCenter;
            tabsLayout.childControlWidth = false;
            tabsLayout.childControlHeight = false;
            var tabsLe = tabs.gameObject.AddComponent<LayoutElement>();
            tabsLe.preferredHeight = 70f;
            tabsLe.preferredWidth = 920f;
            string[] names = { "CAMPAIGN", "SURVIVAL", "BOSS RUSH", "DAILY" };
            panel.modeTabs = new Button[names.Length];
            for (int i = 0; i < names.Length; i++)
                panel.modeTabs[i] = UiBuilder.CreateButton(tabs, "Tab" + i, names[i], new Vector2(222f, 64f), 24f);
            panel.rows = new ListRow[SaveData.LeaderboardSize];
            for (int i = 0; i < panel.rows.Length; i++)
                panel.rows[i] = UiBuilder.CreateListRow(column, "Row" + i, new Vector2(920f, 62f), false);
            panel.emptyText = UiBuilder.AddColumnText(column, "Empty", "No runs recorded yet.", 26f, new Color(0.7f, 0.8f, 0.95f), 40f);
            panel.achievementsText = UiBuilder.AddColumnText(column, "Achievements", "", 22f, UiBuilder.TextColor, 200f);
            panel.achievementsText.alignment = TextAlignmentOptions.TopLeft;
            panel.backButton = UiBuilder.CreateButton(column, "Back", "BACK", new Vector2(560f, 84f));
            panel.firstSelected = panel.modeTabs[0].gameObject;
            return panel;
        }

        // ---- Gameplay -----------------------------------------------------------------------------------------------

        private static void BuildGameplay(PlaceholderArt.Set art, ContentFactory.DataSet data, ProjectBootstrap.PrefabSet prefabs)
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
            var tracker = services.AddComponent<RunTracker>();
            var flow = services.AddComponent<GameFlowController>();

            spawner.defaultEnemyPrefab = prefabs.Enemy;
            spawner.enemyProjectilePrefab = prefabs.EnemyProjectile;
            spawner.defaultPickupPrefab = prefabs.Pickup;
            spawner.webSprite = art.Web;
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
            vfx.sparksPrefab = prefabs.Sparks;
            vfx.screenFlash = flash;

            // Risk Zone vignette (plan §5.1): edge tint that follows the risk state.
            var vignetteGo = new GameObject("RiskVignette");
            vignetteGo.transform.SetParent(services.transform, false);
            var vignetteRing = vignetteGo.AddComponent<SpriteRenderer>();
            vignetteRing.sprite = art.Ring;
            vignetteRing.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(ProjectBootstrap.MaterialRoot + "/Additive.mat");
            vignetteRing.color = new Color(0f, 0f, 0f, 0f);
            vignetteRing.enabled = false;
            var vignette = vignetteGo.AddComponent<RiskVignette>();
            vignette.ring = vignetteRing;

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
            ctx.runTracker = tracker;

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
            CreateToast(canvas.transform);

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

            // Top-left: lives, hull, shield, critical warning
            var topLeft = UiBuilder.CreateRect(root, "TopLeft");
            UiBuilder.Place(topLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(margin, -margin), new Vector2(420f, 200f));
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
            view.criticalText = UiBuilder.CreateText(topLeft, "Critical", "! HULL CRITICAL !", 26f, new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Left, FontStyles.Bold);
            UiBuilder.Place(view.criticalText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -130f), new Vector2(400f, 36f));
            view.criticalText.enabled = false;

            // Top-right: score, multiplier, wave
            var topRight = UiBuilder.CreateRect(root, "TopRight");
            UiBuilder.Place(topRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-margin, -margin), new Vector2(420f, 180f));
            var scoreLabel = UiBuilder.CreateText(topRight, "ScoreLabel", "SCORE", 26f, new Color(0.6f, 0.75f, 0.9f), TextAlignmentOptions.Right);
            UiBuilder.Place(scoreLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(420f, 36f));
            view.scoreText = UiBuilder.CreateText(topRight, "Score", "0", 48f, textColor, TextAlignmentOptions.Right, FontStyles.Bold);
            UiBuilder.Place(view.scoreText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(420f, 56f));
            view.multiplierText = UiBuilder.CreateText(topRight, "Multiplier", "x1", 40f, textColor, TextAlignmentOptions.Right, FontStyles.Bold);
            UiBuilder.Place(view.multiplierText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, -92f), new Vector2(420f, 48f));
            view.waveText = UiBuilder.CreateText(topRight, "Wave", "", 22f, new Color(0.7f, 0.85f, 1f), TextAlignmentOptions.Right);
            UiBuilder.Place(view.waveText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, -140f), new Vector2(420f, 32f));

            // Risk Zone + Overdrive (plan §5): above the energy bar so the eye stays near the ship.
            var riskGroup = UiBuilder.CreateRect(root, "RiskGroup");
            UiBuilder.Place(riskGroup, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, margin + 100f), new Vector2(560f, 92f));
            view.riskText = UiBuilder.CreateText(riskGroup, "RiskText", "RISK SAFE x1", 30f, HudView.AllyColor, TextAlignmentOptions.Center, FontStyles.Bold);
            UiBuilder.Place(view.riskText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(560f, 40f));
            var riskBar = UiBuilder.CreateBar(riskGroup, "RiskBar", new Vector2(520f, 12f), new Color(0.06f, 0.16f, 0.24f, 0.85f), HudView.AllyColor, out view.riskFill);
            UiBuilder.Place(riskBar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(520f, 12f));
            view.riskFill.fillAmount = 0f;
            var odBar = UiBuilder.CreateBar(riskGroup, "OverdriveBar", new Vector2(520f, 16f), new Color(0.06f, 0.16f, 0.24f, 0.85f), HudView.AllyColor, out view.overdriveFill);
            UiBuilder.Place(odBar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(520f, 16f));
            view.overdriveFill.fillAmount = 0f;
            view.overdriveLabel = UiBuilder.CreateText(odBar.transform, "Label", "OVERDRIVE", 14f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UiBuilder.Stretch(view.overdriveLabel.rectTransform);
            view.grazeText = UiBuilder.CreateText(riskGroup, "Graze", "", 22f, HudView.AllyColor, TextAlignmentOptions.Right, FontStyles.Bold);
            UiBuilder.Place(view.grazeText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(200f, 40f));

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

            // Bottom-left: weapon + charge bar
            view.weaponText = UiBuilder.CreateText(root, "Weapon", "LASER LV.1", 30f, textColor, TextAlignmentOptions.Left, FontStyles.Bold);
            UiBuilder.Place(view.weaponText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(margin, margin + 8f), new Vector2(260f, 60f));
            var charge = UiBuilder.CreateBar(root, "ChargeBar", new Vector2(240f, 14f), new Color(0.1f, 0.1f, 0.14f, 0.85f), new Color(0.4f, 1f, 0.9f), out view.chargeFill);
            UiBuilder.Place(charge.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(margin, margin + 70f), new Vector2(240f, 14f));
            view.chargeFill.fillAmount = 0f;
            view.chargeFill.enabled = false;

            // Bottom-right: special indicator
            view.specialText = UiBuilder.CreateText(root, "Special", "", 30f, new Color(1f, 0.9f, 0.4f), TextAlignmentOptions.Right, FontStyles.Bold);
            UiBuilder.Place(view.specialText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-margin, margin + 8f), new Vector2(300f, 60f));
        }

        private static BriefingPanel BuildBriefingPanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "BriefingPanel", out var column, 900f);
            var panel = root.gameObject.AddComponent<BriefingPanel>();
            panel.titleText = UiBuilder.AddColumnText(column, "Title", "STAGE 1", 56f, UiBuilder.Accent, 76f, FontStyles.Bold);
            panel.subtitleText = UiBuilder.AddColumnText(column, "Subtitle", "", 28f, UiBuilder.Accent2, 40f);
            panel.bodyText = UiBuilder.AddColumnText(column, "Body", "", 28f, UiBuilder.TextColor, 300f);
            panel.objectivesText = UiBuilder.AddColumnText(column, "Objectives", "", 26f, new Color(0.7f, 0.95f, 1f), 130f);
            panel.objectivesText.alignment = TextAlignmentOptions.TopLeft;
            panel.loadoutText = UiBuilder.AddColumnText(column, "Loadout", "", 26f, new Color(1f, 0.85f, 0.3f), 40f, FontStyles.Bold);
            panel.launchButton = UiBuilder.CreateButton(column, "Launch", "LAUNCH", new Vector2(560f, 104f), 44f);
            panel.hintText = UiBuilder.AddColumnText(column, "Hint", "TAP TO LAUNCH", 24f, new Color(0.6f, 0.7f, 0.85f), 36f);
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
            panel.restartButton = UiBuilder.CreateButton(column, "Restart", "RESTART", new Vector2(560f, 96f), 38f);
            panel.menuButton = UiBuilder.CreateButton(column, "Menu", "MAIN MENU", new Vector2(560f, 96f), 38f);
            panel.firstSelected = panel.continueButton.gameObject;
            return panel;
        }

        private static VictoryPanel BuildVictoryPanel(Transform canvas)
        {
            var root = UiBuilder.CreatePanelRoot(canvas, "VictoryPanel", out var column);
            var panel = root.gameObject.AddComponent<VictoryPanel>();
            panel.titleText = UiBuilder.AddColumnText(column, "Title", "SECTOR CLEARED", 56f, UiBuilder.Accent, 84f, FontStyles.Bold);
            panel.rankText = UiBuilder.AddColumnText(column, "Rank", "RANK S", 96f, UiBuilder.Accent2, 110f, FontStyles.Bold);
            panel.scoreText = UiBuilder.AddColumnText(column, "Score", "SCORE 0", 40f, UiBuilder.TextColor, 56f, FontStyles.Bold);
            panel.breakdownText = UiBuilder.AddColumnText(column, "Breakdown", "", 22f, new Color(0.7f, 0.85f, 1f), 60f);
            panel.multiplierText = UiBuilder.AddColumnText(column, "Multiplier", "", 30f, UiBuilder.TextColor, 44f);
            panel.enemiesText = UiBuilder.AddColumnText(column, "Enemies", "", 30f, UiBuilder.TextColor, 44f);
            panel.damageText = UiBuilder.AddColumnText(column, "Damage", "", 30f, UiBuilder.TextColor, 44f);
            panel.livesText = UiBuilder.AddColumnText(column, "Lives", "", 30f, UiBuilder.TextColor, 44f);
            panel.rewardsText = UiBuilder.AddColumnText(column, "Rewards", "", 28f, new Color(1f, 0.85f, 0.3f), 44f, FontStyles.Bold);
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
            panel.detailText = UiBuilder.AddColumnText(column, "Detail", "", 28f, UiBuilder.TextColor, 44f);
            panel.rewardsText = UiBuilder.AddColumnText(column, "Rewards", "", 28f, new Color(1f, 0.85f, 0.3f), 44f, FontStyles.Bold);
            panel.recordText = UiBuilder.AddColumnText(column, "Record", "NEW HIGH SCORE!", 34f, new Color(1f, 0.85f, 0.2f), 48f, FontStyles.Bold);
            panel.restartButton = UiBuilder.CreateButton(column, "Restart", "RETRY STAGE", new Vector2(560f, 104f), 42f);
            panel.menuButton = UiBuilder.CreateButton(column, "Menu", "MAIN MENU", new Vector2(560f, 96f), 38f);
            panel.firstSelected = panel.restartButton.gameObject;
            return panel;
        }
    }
}
