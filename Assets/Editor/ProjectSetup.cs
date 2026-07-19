using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Cinemachine;
using TMPro;
using EndlessRunner.Core;
using EndlessRunner.Player;
using EndlessRunner.World;
using EndlessRunner.PowerUps;
using EndlessRunner.Obstacles;
using EndlessRunner.Enemy;
using EndlessRunner.Save;
using EndlessRunner.Audio;
using EndlessRunner.Camera;
using EndlessRunner.UI;

namespace EndlessRunner.Editor
{
    public class ProjectSetup : EditorWindow
    {
        [MenuItem("Tools/Setup Endless Runner Project")]
        public static void RunSetup()
        {
            // 1. Create directory structures
            CreateDirectories();

            // 2. Generate standard URP materials
            Material playerMat = CreateMaterial("PlayerMat", Color.blue);
            Material chaserMat = CreateMaterial("ChaserMat", new Color(0.3f, 0f, 0.5f));
            Material coinMat = CreateMaterial("CoinMat", Color.yellow);
            Material gemMat = CreateMaterial("GemMat", Color.green);
            Material obstacleMat = CreateMaterial("ObstacleMat", Color.red);
            Material trackMat = CreateMaterial("TrackMat", Color.grey);

            // 3. Create configurable Power-Up configurations
            PowerUpConfig magnetConfig = CreatePowerUpConfig("MagnetConfig", PowerUpType.CoinMagnet, 8f, 1f);
            PowerUpConfig boostConfig = CreatePowerUpConfig("BoostConfig", PowerUpType.SpeedBoost, 5f, 12f);
            PowerUpConfig shieldConfig = CreatePowerUpConfig("ShieldConfig", PowerUpType.Shield, 10f, 1f);

            // 4. Create collectible prefabs
            GameObject coinPrefab = CreateCoinPrefab(coinMat);
            GameObject gemPrefab = CreateGemPrefab(gemMat);
            GameObject powerUpPrefab = CreatePowerUpPrefab(shieldConfig, playerMat);

            // 5. Create obstacle prefabs
            GameObject standardObstacle = CreateStandardObstaclePrefab(obstacleMat);
            GameObject lethalObstacle = CreateLethalObstaclePrefab(obstacleMat);

            // 6. Create track tile prefab
            GameObject trackPrefab = CreateTrackSegmentPrefab(trackMat, standardObstacle, coinPrefab, powerUpPrefab);

            // 7. Create BiomeData ScriptableObject
            BiomeData jungleBiome = CreateBiomeData("JungleBiome", trackPrefab, standardObstacle, coinPrefab, powerUpPrefab);

            // 8. Construct the active Unity Scene
            CreateActiveScene(jungleBiome, magnetConfig, boostConfig, shieldConfig, playerMat, chaserMat);

            EditorUtility.DisplayDialog("Setup Successful", 
                "The Endless Runner project has been fully generated!\n\n" +
                "1. Open 'Assets/Scenes/MainGame.unity'.\n" +
                "2. Press Play to test controls using WASD or touch swiping!\n" +
                "3. Use 'Tools/Setup Endless Runner Project' to regenerate at any time.", 
                "Awesome!");
        }

        private static void CreateDirectories()
        {
            string[] dirs = {
                "Assets/Prefabs",
                "Assets/ScriptableObjects",
                "Assets/Materials",
                "Assets/Scenes"
            };

            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            AssetDatabase.Refresh();
        }

        private static Material CreateMaterial(string name, Color color)
        {
            string path = $"Assets/Materials/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                // Create a standard shader material
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static PowerUpConfig CreatePowerUpConfig(string name, PowerUpType type, float duration, float value)
        {
            string path = $"Assets/ScriptableObjects/{name}.asset";
            PowerUpConfig config = AssetDatabase.LoadAssetAtPath<PowerUpConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PowerUpConfig>();
                config.type = type;
                config.baseDuration = duration;
                config.modifierValue = value;
                AssetDatabase.CreateAsset(config, path);
            }
            return config;
        }

        private static GameObject CreateCoinPrefab(Material mat)
        {
            string path = "Assets/Prefabs/Coin.prefab";
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Coin";
            go.tag = "Coin";
            
            // Adjust proportions to resemble a floating gold coin
            go.transform.localScale = new Vector3(0.6f, 0.08f, 0.6f);
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            go.GetComponent<Renderer>().material = mat;

            // Add Collectible script
            var colScript = go.AddComponent<Collectible>();
            var serObj = new SerializedObject(colScript);
            serObj.FindProperty("type").enumValueIndex = 0; // Coin
            serObj.FindProperty("rotationSpeed").floatValue = 180f;
            serObj.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateGemPrefab(Material mat)
        {
            string path = "Assets/Prefabs/Gem.prefab";
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Gem";
            go.tag = "Gem";
            go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            go.GetComponent<Renderer>().material = mat;

            var colScript = go.AddComponent<Collectible>();
            var serObj = new SerializedObject(colScript);
            serObj.FindProperty("type").enumValueIndex = 1; // Gem
            serObj.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePowerUpPrefab(PowerUpConfig cfg, Material mat)
        {
            string path = "Assets/Prefabs/PowerUp_Boost.prefab";
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "PowerUp_Boost";
            go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            go.GetComponent<Renderer>().material = mat;

            var colScript = go.AddComponent<Collectible>();
            var serObj = new SerializedObject(colScript);
            serObj.FindProperty("type").enumValueIndex = 2; // PowerUp
            serObj.FindProperty("powerUpConfig").objectReferenceValue = cfg;
            serObj.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateStandardObstaclePrefab(Material mat)
        {
            string path = "Assets/Prefabs/Obstacle_Standard.prefab";
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Obstacle_Standard";
            go.tag = "Obstacle";
            go.transform.localScale = new Vector3(2.5f, 1.2f, 1f); // Narrow block blocking a lane

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.material = mat;

            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            go.AddComponent<Obstacle>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateLethalObstaclePrefab(Material mat)
        {
            string path = "Assets/Prefabs/Obstacle_Lethal.prefab";
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Obstacle_Lethal";
            go.tag = "Obstacle";
            go.transform.localScale = new Vector3(8f, 0.1f, 2f); // Wide lava pit/bar

            go.GetComponent<Renderer>().material = mat;
            var col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var obs = go.AddComponent<Obstacle>();
            var serObj = new SerializedObject(obs);
            serObj.FindProperty("isLethal").boolValue = true;
            serObj.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateTrackSegmentPrefab(Material mat, GameObject obstacle, GameObject coin, GameObject pUp)
        {
            string path = "Assets/Prefabs/TrackSegment.prefab";
            GameObject go = new GameObject("TrackSegment");
            var segment = go.AddComponent<TrackSegment>();

            // Visual track mesh
            GameObject meshGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshGo.name = "TrackVisual";
            meshGo.transform.SetParent(go.transform);
            meshGo.transform.localScale = new Vector3(10f, 0.2f, 30f);
            meshGo.transform.localPosition = new Vector3(0f, -0.1f, 15f); // Pivoted at origin
            meshGo.GetComponent<Renderer>().material = mat;

            // Spawn points (Left, Middle, Right)
            List<Transform> obstaclePoints = new List<Transform>();
            List<Transform> coinPoints = new List<Transform>();
            List<Transform> powerPoints = new List<Transform>();

            float[] lanes = { -2.5f, 0f, 2.5f };
            for (int l = 0; l < lanes.Length; l++)
            {
                float x = lanes[l];

                // Coins at Z = 5, 10, 15
                for (int zOffset = 5; zOffset <= 25; zOffset += 10)
                {
                    GameObject pt = new GameObject($"CoinSpawn_{l}_{zOffset}");
                    pt.transform.SetParent(go.transform);
                    pt.transform.localPosition = new Vector3(x, 0.6f, zOffset);
                    coinPoints.Add(pt.transform);
                }

                // Obstacle at Z = 15
                if (l != 1) // Keep center lane open on default tiles
                {
                    GameObject pt = new GameObject($"ObstacleSpawn_{l}");
                    pt.transform.SetParent(go.transform);
                    pt.transform.localPosition = new Vector3(x, 0.7f, 15f);
                    obstaclePoints.Add(pt.transform);
                }

                // Power up at Z = 25
                if (l == 1)
                {
                    GameObject pt = new GameObject($"PowerUpSpawn");
                    pt.transform.SetParent(go.transform);
                    pt.transform.localPosition = new Vector3(x, 0.8f, 25f);
                    powerPoints.Add(pt.transform);
                }
            }

            var serObj = new SerializedObject(segment);
            serObj.FindProperty("length").floatValue = 30f;
            
            // Set spawn points array values
            SetSerializedArray(serObj.FindProperty("obstacleSpawnPoints"), obstaclePoints);
            SetSerializedArray(serObj.FindProperty("coinSpawnPoints"), coinPoints);
            SetSerializedArray(serObj.FindProperty("powerUpSpawnPoints"), powerPoints);

            serObj.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            return prefab;
        }

        private static BiomeData CreateBiomeData(string name, GameObject track, GameObject obstacle, GameObject coin, GameObject pUp)
        {
            string path = $"Assets/ScriptableObjects/{name}.asset";
            BiomeData biome = AssetDatabase.LoadAssetAtPath<BiomeData>(path);
            if (biome == null)
            {
                biome = ScriptableObject.CreateInstance<BiomeData>();
                biome.biomeName = "Jungle Prototype";
                biome.trackPrefabs = new GameObject[] { track };
                biome.obstaclePrefabs = new GameObject[] { obstacle };
                biome.coinPrefabs = new GameObject[] { coin };
                biome.powerUpPrefabs = new GameObject[] { pUp };
                biome.fogColor = new Color(0.1f, 0.25f, 0.15f); // Greenish jungle fog
                biome.fogDensity = 0.02f;
                AssetDatabase.CreateAsset(biome, path);
            }
            return biome;
        }

        private static void CreateActiveScene(BiomeData biome, PowerUpConfig mag, PowerUpConfig bst, PowerUpConfig shld, Material pMat, Material cMat)
        {
            // Clear current scene / Create new
            var scene = EditorSceneManager.NewScene(EditorSceneManager.NewSceneSetup.DefaultGameObjects, EditorSceneManager.NewSceneMode.Single);
            
            // Add Directional Light configurations
            var lightGo = GameObject.Find("Directional Light");
            if (lightGo != null)
            {
                var light = lightGo.GetComponent<Light>();
                light.color = new Color(1f, 0.95f, 0.8f);
                light.intensity = 1.3f;
            }

            // Create Managers
            GameObject managersObj = new GameObject("_Managers");
            var saveManager = managersObj.AddComponent<SaveManager>();
            var audioManager = managersObj.AddComponent<AudioManager>();
            var objectPooler = managersObj.AddComponent<ObjectPooler>();

            // Setup temporary audio sources
            var musicSrc = managersObj.AddComponent<AudioSource>();
            var sfxSrc = managersObj.AddComponent<AudioSource>();
            var serAudio = new SerializedObject(audioManager);
            serAudio.FindProperty("musicSource").objectReferenceValue = musicSrc;
            serAudio.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
            serAudio.ApplyModifiedProperties();

            // Create Player
            GameObject playerGo = new GameObject("Player");
            playerGo.tag = "Player";
            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var playerController = playerGo.AddComponent<PlayerController>();
            var pm = playerGo.AddComponent<PowerUpManager>();

            // Visual Model for player
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "VisualModel";
            body.transform.SetParent(playerGo.transform);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.GetComponent<Renderer>().material = pMat;
            DestroyImmediate(body.GetComponent<Collider>()); // Let CharacterController handle collisions

            // Create Enemy Chaser
            GameObject chaserGo = new GameObject("Enemy_Chaser");
            var chaserScript = chaserGo.AddComponent<EnemyChaser>();
            var chaserBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chaserBody.name = "ChaserVisual";
            chaserBody.transform.SetParent(chaserGo.transform);
            chaserBody.transform.localPosition = new Vector3(0f, 1f, 0f);
            chaserBody.transform.localScale = new Vector3(1.5f, 2f, 1.5f);
            chaserBody.GetComponent<Renderer>().material = cMat;
            DestroyImmediate(chaserBody.GetComponent<Collider>());
            
            var serChaser = new SerializedObject(chaserScript);
            serChaser.FindProperty("player").objectReferenceValue = playerController;
            serChaser.ApplyModifiedProperties();

            // Create Cinemachine Camera config
            GameObject vcamGo = new GameObject("Cinemachine_Camera");
            var vcam = vcamGo.AddComponent<CinemachineCamera>();
            vcam.Follow = playerGo.transform;
            vcam.LookAt = playerGo.transform;

            var noise = vcamGo.AddComponent<CinemachineBasicMultiChannelPerlin>();
            // Load noise profile if standard exists (optional, otherwise sets gains directly)
            
            var camConfig = vcamGo.AddComponent<CinemachineCameraConfig>();
            var serCamConfig = new SerializedObject(camConfig);
            serCamConfig.FindProperty("player").objectReferenceValue = playerController;
            serCamConfig.FindProperty("cinemachineCamera").objectReferenceValue = vcam;
            serCamConfig.ApplyModifiedProperties();

            // Create Gameplay
            GameObject gameplayObj = new GameObject("_Gameplay");
            var gameManager = gameplayObj.AddComponent<GameManager>();
            var levelGenerator = gameplayObj.AddComponent<LevelGenerator>();

            // Setup level generator fields
            var serLevelGen = new SerializedObject(levelGenerator);
            serLevelGen.FindProperty("player").objectReferenceValue = playerController;
            var biomesProp = serLevelGen.FindProperty("biomes");
            biomesProp.arraySize = 1;
            biomesProp.GetArrayElementAtIndex(0).objectReferenceValue = biome;
            serLevelGen.ApplyModifiedProperties();

            // Setup PlayerController fields to link scripts
            var serPlayer = new SerializedObject(playerController);
            // Link to manager events if needed, but they are resolved at runtime anyway
            serPlayer.ApplyModifiedProperties();

            // Setup GameManager fields
            var serGM = new SerializedObject(gameManager);
            serGM.FindProperty("player").objectReferenceValue = playerController;
            serGM.ApplyModifiedProperties();

            // Create Canvas & View Hierarchies
            CreateUIElements(playerController, gameManager, pm);

            // Save the Scene
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainGame.unity");
        }

        private static void CreateUIElements(PlayerController pc, GameManager gm, PowerUpManager pm)
        {
            GameObject canvasGo = new GameObject("UICanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<CanvasGroup>();

            // Setup Event System
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create View Panels
            HUDView hud = CreatePanel<HUDView>(canvasGo, "HUD_Panel");
            MainMenuView menu = CreatePanel<MainMenuView>(canvasGo, "MainMenu_Panel");
            ShopView shop = CreatePanel<ShopView>(canvasGo, "Shop_Panel");
            SettingsView settings = CreatePanel<SettingsView>(canvasGo, "Settings_Panel");
            GameOverView gameOver = CreatePanel<GameOverView>(canvasGo, "GameOver_Panel");

            // Build mock text templates inside views so the layout works
            TMP_Text dummyScore = CreateTextMesh(hud.gameObject, "ScoreText", "0000000", new Vector2(0f, 400f));
            TMP_Text dummyCoins = CreateTextMesh(hud.gameObject, "CoinsText", "0", new Vector2(-400f, 400f));
            TMP_Text dummyGems = CreateTextMesh(hud.gameObject, "GemsText", "0", new Vector2(-400f, 350f));
            
            GameObject comboObj = new GameObject("ComboPanel");
            comboObj.transform.SetParent(hud.transform);
            comboObj.AddComponent<RectTransform>();
            TMP_Text dummyCombo = CreateTextMesh(comboObj, "ComboText", "2x", new Vector2(0f, 250f));
            Image dummyComboRing = CreateImage(comboObj, "ComboProgressRing", Color.cyan);

            GameObject mRing = new GameObject("MagnetRing");
            mRing.transform.SetParent(hud.transform);
            Image mRingFill = CreateImage(mRing, "MagnetRingFill", Color.magenta);

            GameObject bRing = new GameObject("BoostRing");
            bRing.transform.SetParent(hud.transform);
            Image bRingFill = CreateImage(bRing, "BoostRingFill", Color.blue);

            GameObject sRing = new GameObject("ShieldRing");
            sRing.transform.SetParent(hud.transform);
            Image sRingFill = CreateImage(sRing, "ShieldRingFill", Color.green);

            Button hudPauseBtn = CreateButton(hud.gameObject, "PauseButton", "PAUSE", new Vector2(400f, 400f));

            // Main Menu Displays
            TMP_Text menuHighScore = CreateTextMesh(menu.gameObject, "HighScoreText", "HIGH SCORE: 0", new Vector2(0f, 150f));
            TMP_Text menuLevel = CreateTextMesh(menu.gameObject, "LevelText", "LEVEL: 1", new Vector2(0f, 200f));
            Button menuPlayBtn = CreateButton(menu.gameObject, "PlayButton", "RUN GAME", new Vector2(0f, 0f));
            Button menuShopBtn = CreateButton(menu.gameObject, "ShopButton", "UPGRADES SHOP", new Vector2(0f, -80f));
            Button menuSettingsBtn = CreateButton(menu.gameObject, "SettingsButton", "SETTINGS", new Vector2(0f, -160f));

            // Shop Displays
            TMP_Text shopBalance = CreateTextMesh(shop.gameObject, "BalanceText", "COINS: 0", new Vector2(0f, 250f));
            TMP_Text magnetLvlText = CreateTextMesh(shop.gameObject, "MagnetLevelText", "LEVEL: 0/5", new Vector2(-200f, 50f));
            TMP_Text magnetCostText = CreateTextMesh(shop.gameObject, "MagnetCostText", "1000 COINS", new Vector2(-200f, 0f));
            Button upgradeMagnetBtn = CreateButton(shop.gameObject, "UpgradeMagnetBtn", "UPGRADE MAGNET", new Vector2(-200f, -80f));

            TMP_Text boostLvlText = CreateTextMesh(shop.gameObject, "BoostLevelText", "LEVEL: 0/5", new Vector2(200f, 50f));
            TMP_Text boostCostText = CreateTextMesh(shop.gameObject, "BoostCostText", "1000 COINS", new Vector2(200f, 0f));
            Button upgradeBoostBtn = CreateButton(shop.gameObject, "UpgradeBoostBtn", "UPGRADE BOOST", new Vector2(200f, -80f));
            Button shopCloseBtn = CreateButton(shop.gameObject, "CloseButton", "BACK", new Vector2(0f, -220f));

            // Settings Displays
            Slider musicSlider = CreateSlider(settings.gameObject, "MusicVolumeSlider", new Vector2(0f, 50f));
            Slider sfxSlider = CreateSlider(settings.gameObject, "SFXVolumeSlider", new Vector2(0f, -50f));
            Button settingsCloseBtn = CreateButton(settings.gameObject, "CloseButton", "CLOSE", new Vector2(0f, -150f));

            // GameOver Displays
            TMP_Text goScoreText = CreateTextMesh(gameOver.gameObject, "FinalScoreText", "SCORE: 0", new Vector2(0f, 200f));
            TMP_Text goCoinsText = CreateTextMesh(gameOver.gameObject, "CoinsText", "+0 COINS", new Vector2(0f, 150f));
            TMP_Text goGemsText = CreateTextMesh(gameOver.gameObject, "GemsText", "+0 GEMS", new Vector2(0f, 100f));
            TMP_Text goHighScoreText = CreateTextMesh(gameOver.gameObject, "HighScoreText", "HIGH SCORE: 0", new Vector2(0f, 50f));
            
            Button goReviveBtn = CreateButton(gameOver.gameObject, "ReviveButton", "REVIVE", new Vector2(0f, -50f));
            TMP_Text goGemCostText = CreateTextMesh(gameOver.gameObject, "GemCostInfoText", "REVIVE (COSTS 1)", new Vector2(0f, -100f));
            Button goRestartBtn = CreateButton(gameOver.gameObject, "RestartButton", "RUN AGAIN", new Vector2(0f, -180f));
            Button goMainMenuBtn = CreateButton(gameOver.gameObject, "MainMenuButton", "MAIN MENU", new Vector2(0f, -250f));

            // Wire HUD References
            var serHUD = new SerializedObject(hud);
            serHUD.FindProperty("player").objectReferenceValue = pc;
            serHUD.FindProperty("scoreText").objectReferenceValue = dummyScore;
            serHUD.FindProperty("coinsText").objectReferenceValue = dummyCoins;
            serHUD.FindProperty("gemsText").objectReferenceValue = dummyGems;
            serHUD.FindProperty("comboPanel").objectReferenceValue = comboObj;
            serHUD.FindProperty("comboText").objectReferenceValue = dummyCombo;
            serHUD.FindProperty("comboProgressRing").objectReferenceValue = dummyComboRing;
            serHUD.FindProperty("magnetRingObj").objectReferenceValue = mRing;
            serHUD.FindProperty("magnetRingFill").objectReferenceValue = mRingFill;
            serHUD.FindProperty("boostRingObj").objectReferenceValue = bRing;
            serHUD.FindProperty("boostRingFill").objectReferenceValue = bRingFill;
            serHUD.FindProperty("shieldRingObj").objectReferenceValue = sRing;
            serHUD.FindProperty("shieldRingFill").objectReferenceValue = sRingFill;
            serHUD.FindProperty("pauseButton").objectReferenceValue = hudPauseBtn;
            serHUD.FindProperty("pausePanel").objectReferenceValue = settings; // settings acting as pause panel
            serHUD.ApplyModifiedProperties();

            // Wire Menu References
            var serMenu = new SerializedObject(menu);
            serMenu.FindProperty("highScoreText").objectReferenceValue = menuHighScore;
            serMenu.FindProperty("playerLevelText").objectReferenceValue = menuLevel;
            serMenu.FindProperty("playButton").objectReferenceValue = menuPlayBtn;
            serMenu.FindProperty("shopButton").objectReferenceValue = menuShopBtn;
            serMenu.FindProperty("settingsButton").objectReferenceValue = menuSettingsBtn;
            serMenu.FindProperty("shopView").objectReferenceValue = shop;
            serMenu.FindProperty("settingsView").objectReferenceValue = settings;
            serMenu.ApplyModifiedProperties();

            // Wire Shop References
            var serShop = new SerializedObject(shop);
            serShop.FindProperty("balanceText").objectReferenceValue = shopBalance;
            serShop.FindProperty("magnetLevelText").objectReferenceValue = magnetLvlText;
            serShop.FindProperty("magnetCostText").objectReferenceValue = magnetCostText;
            serShop.FindProperty("upgradeMagnetButton").objectReferenceValue = upgradeMagnetBtn;
            serShop.FindProperty("boostLevelText").objectReferenceValue = boostLvlText;
            serShop.FindProperty("boostCostText").objectReferenceValue = boostCostText;
            serShop.FindProperty("upgradeBoostButton").objectReferenceValue = upgradeBoostBtn;
            serShop.FindProperty("closeButton").objectReferenceValue = shopCloseBtn;
            serShop.FindProperty("mainMenuView").objectReferenceValue = menu;
            serShop.ApplyModifiedProperties();

            // Wire Settings References
            var serSettings = new SerializedObject(settings);
            serSettings.FindProperty("musicVolumeSlider").objectReferenceValue = musicSlider;
            serSettings.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider;
            serSettings.FindProperty("closeButton").objectReferenceValue = settingsCloseBtn;
            serSettings.ApplyModifiedProperties();

            // Wire GameOver References
            var serGO = new SerializedObject(gameOver);
            serGO.FindProperty("finalScoreText").objectReferenceValue = goScoreText;
            serGO.FindProperty("coinsCollectedText").objectReferenceValue = goCoinsText;
            serGO.FindProperty("gemsCollectedText").objectReferenceValue = goGemsText;
            serGO.FindProperty("highScoreText").objectReferenceValue = goHighScoreText;
            serGO.FindProperty("reviveButton").objectReferenceValue = goReviveBtn;
            serGO.FindProperty("gemCostInfoText").objectReferenceValue = goGemCostText;
            serGO.FindProperty("restartButton").objectReferenceValue = goRestartBtn;
            serGO.FindProperty("mainMenuButton").objectReferenceValue = goMainMenuBtn;
            serGO.FindProperty("mainMenuView").objectReferenceValue = menu;
            serGO.ApplyModifiedProperties();

            // Turn off all panels initially, show only MainMenu
            hud.Show(); // HUD should be active when starting but we can toggle it in code
            shop.Hide();
            settings.Hide();
            gameOver.Hide();
            menu.Show();
        }

        private static T CreatePanel<T>(GameObject parent, string name) where T : View
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.AddComponent<CanvasGroup>();
            return go.AddComponent<T>();
        }

        private static TMP_Text CreateTextMesh(GameObject parent, string name, string defaultText, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24;

            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(400f, 50f);
            return tmp;
        }

        private static Image CreateImage(GameObject parent, string name, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static Button CreateButton(GameObject parent, string name, string labelText, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(250f, 60f);

            GameObject textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 20;
            tmp.color = Color.white;

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }

        private static Slider CreateSlider(GameObject parent, string name, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            var slider = go.AddComponent<Slider>();

            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(300f, 20f);

            // Add simple visuals
            GameObject bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = Color.gray;

            var bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            slider.targetGraphic = bgImg;
            return slider;
        }

        private static void SetSerializedArray(SerializedProperty prop, List<Transform> items)
        {
            prop.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }
    }
}
