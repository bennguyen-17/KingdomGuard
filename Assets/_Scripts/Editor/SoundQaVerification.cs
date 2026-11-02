#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class SoundQaVerification
{
    [Serializable] private class Results
    {
        public string date = "2026-11-02";
        public string unityVersion;
        public List<string> passed = new List<string>();
        public string failure;
        public string build;
        public bool legacyAudioIncluded;
        public int sceneCount;
    }
    private const string Active = "KingdomGuard.QA.Active";
    private static Results results;
    private static int step, retries;
    private static Vector2 protectedPoint;
    private static double next, deadline;
    private static readonly List<AudioClip> sounds = new List<AudioClip>();
    private static readonly List<Enemy> adaptiveEnemies = new List<Enemy>();
    private static string Root => Directory.GetParent(Application.dataPath).FullName;
    private static readonly string[] Keys = { "KingdomGuard.BGMVolume", "KingdomGuard.SFXVolume", "KingdomGuard.HighestUnlockedLevel", "KingdomGuard.PendingLevel", "KingdomGuard.Completed.Assets/Scenes/Level_1.unity" };
    static SoundQaVerification() { EditorApplication.playModeStateChanged += OnMode; }
    public static void Run()
    {
        SessionState.SetBool(Active, true);
        SessionState.SetBool(Active + ".Options", EditorSettings.enterPlayModeOptionsEnabled);
        SessionState.SetInt(Active + ".Value", (int)EditorSettings.enterPlayModeOptions);
        EditorSettings.enterPlayModeOptionsEnabled = false;
        foreach (string key in Keys)
        {
            SessionState.SetBool(key + ".Exists", PlayerPrefs.HasKey(key));
            if (key.Contains("Volume")) SessionState.SetFloat(key, PlayerPrefs.GetFloat(key));
            else if (key.EndsWith("PendingLevel")) SessionState.SetString(key, PlayerPrefs.GetString(key));
            else SessionState.SetInt(key, PlayerPrefs.GetInt(key));
        }
        PlayerPrefs.DeleteKey("KingdomGuard.PendingLevel");
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
    }
    private static void OnMode(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            results = new Results { unityVersion = Application.unityVersion, sceneCount = SceneManager.sceneCountInBuildSettings };
            step = retries = 0; next = EditorApplication.timeSinceStartup + 2; deadline = next + 120;
            AudioManager.Instance.SoundPlayed += Heard;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
        }
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            foreach (string key in Keys)
            {
                if (!SessionState.GetBool(key + ".Exists", false)) PlayerPrefs.DeleteKey(key);
                else if (key.Contains("Volume")) PlayerPrefs.SetFloat(key, SessionState.GetFloat(key, 0));
                else if (key.EndsWith("PendingLevel")) PlayerPrefs.SetString(key, SessionState.GetString(key, ""));
                else PlayerPrefs.SetInt(key, SessionState.GetInt(key, 0));
            }
            PlayerPrefs.Save();
            EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetBool(Active + ".Options", true);
            EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(Active + ".Value", 1);
            SessionState.SetBool(Active, false);
            Finish();
        }
    }
    private static void Heard(AudioClip clip) => sounds.Add(clip);
    private static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error) results.failure = message + "\n" + stack;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        results.passed.Add(message); Debug.Log("KG_SOUND_PASS " + message);
    }
    private static object Field(object obj, string field) => obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
    private static void Call(object obj, string method, params object[] args) => obj.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(obj, args);
    private static Node FirstNode(BuildManager build) => ((IDictionary)Field(build, "cellToNode")).Values.Cast<Node>().First();
    private static void Choose(BuildManager build, Node node, int index)
    {
        Call(build, "OpenMenu", node); Call(build, "Activate", index); Call(build, "Activate", index);
    }
    private static void RouteShot(int type, AudioClip expected)
    {
        AudioManager audio = AudioManager.Instance;
        audio.SetMusicState(AudioManager.MusicState.Battle);
        BuildManager build = UnityEngine.Object.FindAnyObjectByType<BuildManager>();
        GameObject target = new GameObject("QA Target", typeof(Enemy)); target.transform.position = new Vector3(1000, 1000);
        GameObject go = UnityEngine.Object.Instantiate(build.towerPrefab, new Vector3(-1000, -1000), Quaternion.identity);
        Tower tower = go.GetComponent<Tower>(); tower.Setup(build.towerTypes[type]); tower.enabled = false;
        typeof(Tower).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tower, target.transform);
        sounds.Clear(); Call(tower, "Shoot");
        Check(sounds.Count == 1 && sounds[0] == expected, "Tower " + build.towerTypes[type].towerName + " emits its own shot clip exactly once");
        foreach (Bullet bullet in UnityEngine.Object.FindObjectsByType<Bullet>()) UnityEngine.Object.Destroy(bullet.gameObject);
        UnityEngine.Object.Destroy(go); UnityEngine.Object.Destroy(target);
    }
    private static void VerifyAdaptivePolicy()
    {
        var policy = new BattleMusicPolicy(); policy.Reset();
        Check(policy.Advance(5, false, 4.9f) == BattleMusicPolicy.Intensity.Calm, "Adaptive music holds its initial tier for five seconds");
        Check(policy.Advance(5, false, .2f) == BattleMusicPolicy.Intensity.Tense, "Five living enemies select tense music after the hold");
        Check(policy.Advance(4, false, 6f) == BattleMusicPolicy.Intensity.Tense, "Four enemies retain tense music through hysteresis");
        Check(policy.Advance(3, false, 3.9f) == BattleMusicPolicy.Intensity.Tense, "Dropping to three enemies does not immediately lower intensity");
        Check(policy.Advance(3, false, .2f) == BattleMusicPolicy.Intensity.Calm, "Four stable seconds at three enemies return calm music");
        Check(policy.Advance(10, false, 5.1f) == BattleMusicPolicy.Intensity.Intense, "Ten living enemies select intense music");
        Check(policy.Advance(8, false, 6f) == BattleMusicPolicy.Intensity.Intense, "Eight enemies retain intense music through hysteresis");
        Check(policy.Advance(7, false, 4.1f) == BattleMusicPolicy.Intensity.Tense, "Seven stable enemies lower intense music to tense");
        policy.Reset();
        Check(policy.Advance(1, true, .01f) == BattleMusicPolicy.Intensity.Intense, "A living Boss bypasses the ordinary intensity hold");
        policy.Reset(); policy.Advance(5, false, 3f); policy.Advance(0, false, .1f);
        Check(policy.Advance(5, false, 1.9f) == BattleMusicPolicy.Intensity.Calm, "Interrupted population growth resets the promotion hold");
    }
    private static Enemy AddAdaptiveEnemy()
    {
        var enemy = new GameObject("QA Adaptive Enemy", typeof(Enemy)).GetComponent<Enemy>();
        enemy.transform.position = new Vector3(1000, 1000); adaptiveEnemies.Add(enemy); return enemy;
    }
    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + .6;
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Sound QA exceeded 120 seconds");
            if (!string.IsNullOrEmpty(results.failure)) throw new Exception(results.failure);
            AudioManager audio = AudioManager.Instance;
            GameManager gm = GameManager.Instance;
            BuildManager build = UnityEngine.Object.FindAnyObjectByType<BuildManager>();
            switch (step++)
            {
                case 0:
                    VerifyAdaptivePolicy();
                    Check(UnityEngine.Object.FindAnyObjectByType<MainMenuController>() != null, "MainMenu loads its controller");
                    Check(Resources.LoadAll<AudioClip>("Audio").Length == AudioManager.ExpectedRuntimeClipCount, "All 15 runtime clips import successfully");
                    Check(audio.CurrentMusicState == AudioManager.MusicState.Menu && audio.RequestedMusicClip == audio.bgmPreparation && audio.IsMusicPlaying, "MainMenu plays preparation track");
                    var settings = UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>(); settings.SetOpen(true);
                    Slider[] sliders = settings.GetComponentsInChildren<Slider>();
                    Check(sliders.Length == 2, "Sound settings expose separate Music and Effects controls");
                    sliders.First(s => s.name == "Music Slider").value = .23f;
                    sliders.First(s => s.name == "Effects Slider").value = .41f;
                    Check(Mathf.Approximately(audio.GetBGMVolume(), .23f) && Mathf.Approximately(audio.GetSFXVolume(), .41f), "Sliders control volumes independently");
                    settings.SetOpen(false);
                    Check(Mathf.Approximately(PlayerPrefs.GetFloat(Keys[0]), .23f) && Mathf.Approximately(PlayerPrefs.GetFloat(Keys[1]), .41f), "Closing settings persists both user volumes");
                    new GameObject("Duplicate Audio QA").AddComponent<AudioManager>();
                    SceneManager.LoadScene("Level_1"); break;
                case 1:
                    Check(UnityEngine.Object.FindObjectsByType<AudioManager>().Length == 1, "Scene load and duplicate bootstrap retain exactly one AudioManager");
                    Check(Mathf.Approximately(audio.GetBGMVolume(), .23f) && Mathf.Approximately(audio.GetSFXVolume(), .41f), "Level 1 preserves saved volume values");
                    Check(audio.CurrentMusicState == AudioManager.MusicState.Preparation, "Level preparation uses quiet music state");
                    Node node = FirstNode(build); int money = GameManager.Money;
                    Call(build, "OpenMenu", node); sounds.Clear(); Call(build, "Activate", 0);
                    Check(!sounds.Contains(audio.sfxTowerPlace) && node.Tower == null && GameManager.Money == money, "Selecting a tower preview neither confirms purchase nor plays success");
                    Call(build, "Activate", 0);
                    Check(sounds.Count(c => c == audio.sfxTowerPlace) == 1 && node.Tower != null && GameManager.Money == money - build.towerTypes[0].levels[0].cost, "Confirmed purchase deducts exact Gold and plays one success cue");
                    break;
                case 2:
                    Node upgradeNode = FirstNode(build); int beforeUpgrade = GameManager.Money; sounds.Clear();
                    Choose(build, upgradeNode, 0);
                    Check(upgradeNode.Tower.Level == 1 && GameManager.Money == beforeUpgrade - build.towerTypes[0].levels[1].cost && sounds.Count(c => c == audio.sfxTowerPlace) == 1, "Upgrade commits once and plays confirmation");
                    int savedGold = GameManager.Money; GameManager.Money = 0;
                    Call(build, "OpenMenu", upgradeNode); Call(build, "RefreshOptions"); sounds.Clear();
                    for (int i = 0; i < 100; i++) Call(build, "Activate", 0);
                    Check(GameManager.Money == 0 && upgradeNode.Tower.Level == 1, "Rejected upgrades leave Gold and tower level unchanged");
                    Check(sounds.Count(c => c == audio.sfxBuildFailed) == 1 && !sounds.Contains(audio.sfxTowerPlace), "One hundred rapid rejection clicks yield one error cue and no success cue");
                    GameManager.Money = savedGold; Call(build, "CloseMenu"); break;
                case 3:
                    Node sellNode = FirstNode(build); int beforeSell = GameManager.Money; int total = sellNode.Tower.TotalSpent; sounds.Clear();
                    Choose(build, sellNode, 1);
                    Check(sellNode.Tower == null && GameManager.Money == beforeSell + Mathf.RoundToInt(total * build.sellRefund) && sounds.Count(c => c == audio.sfxTowerPlace) == 1, "Sell refunds configured Gold and plays one confirmation");
                    break;
                case 4: RouteShot(0, audio.sfxArrowShoot); break;
                case 5: RouteShot(1, audio.sfxCannonShoot); break;
                case 6: RouteShot(2, audio.sfxMagicShoot); break;
                case 7:
                    sounds.Clear();
                    for (int i = 0; i < 1000; i++) { audio.PlayArrowShoot(); audio.PlayCannonShoot(); audio.PlayMagicShoot(); audio.PlayHit(); audio.PlayEnemyDeath(); }
                    Check(sounds.Count < 10 && audio.ActiveCombatVoices <= AudioManager.MaxCombatVoices, "Five thousand combat requests are throttled with at most four active voices");
                    audio.PlayBaseDamage(); audio.PlayUIClick();
                    Check(sounds.Contains(audio.sfxBaseDamage) && sounds.Contains(audio.sfxUIClick), "Warning and UI cues remain available under combat spam");
                    break;
                case 8:
                    var ui = UnityEngine.Object.FindAnyObjectByType<UIManager>(); ui.PauseGame(); sounds.Clear();
                    UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>().SetOpen(true);
                    RectTransform panelRect = GameObject.Find("Sound Settings").GetComponent<RectTransform>();
                    protectedPoint = RectTransformUtility.WorldToScreenPoint(null, panelRect.TransformPoint(panelRect.rect.center));
                    Check(AudioSettingsUI.ConsumesPointer(protectedPoint), "Sound panel consumes pointer positions instead of passing them to tower placement");
                    Check(Time.timeScale == 0 && sounds.Contains(audio.sfxUIClick), "Sound controls emit UI audio while gameplay is paused");
                    audio.SetMusicState(AudioManager.MusicState.Preparation); break;
                case 9:
                    Check(Time.timeScale == 0 && audio.IsMusicPlaying && audio.RequestedMusicClip == audio.bgmPreparation, "Music transition completes using unscaled time during pause");
                    audio.SetBGMVolume(0); audio.SetSFXVolume(0);
                    Check(audio.GetComponents<AudioSource>().All(source => source.volume == 0), "Muting Music and Effects silences all managed sources");
                    audio.SetBGMVolume(-1); audio.SetSFXVolume(2);
                    Check(audio.GetBGMVolume() == 0 && audio.GetSFXVolume() == 1, "Volume endpoints clamp to zero and one");
                    audio.SetBGMVolume(.23f); audio.SetSFXVolume(.41f);
                    UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>().SetOpen(false);
                    Check(!AudioSettingsUI.ConsumesPointer(protectedPoint), "Closed sound panel releases its previous pointer region");
                    UnityEngine.Object.FindAnyObjectByType<UIManager>().ResumeGame();
                    GameManager.Money = 200;
                    Choose(build, FirstNode(build), 0);
                    FirstNode(build).Tower.enabled = false; FirstNode(build).Tower.CancelInvoke();
                    var wave = UnityEngine.Object.FindAnyObjectByType<WaveSpawner>();
                    wave.totalWaves = 2; wave.timeBetweenWaves = 5;
                    wave.waveConfigs = new[] { new WaveSpawner.WaveDefinition { goblinCount = 1, spawnInterval = .1f }, new WaveSpawner.WaveDefinition { goblinCount = 1, bossCount = 1, spawnInterval = .1f } };
                    gm.ConfigureWaves(2); wave.BeginWaveSequence(); wave.StartFirstWave();
                    Check(audio.CurrentMusicState == AudioManager.MusicState.Battle && audio.RequestedMusicClip == audio.bgmBattle, "Starting real wave selects battle track"); break;
                case 10:
                    Enemy enemy = UnityEngine.Object.FindAnyObjectByType<Enemy>(); Check(enemy != null, "Configured wave actually spawns an enemy");
                    enemy.TakeDamage(10000); break;
                case 11:
                    Check(UnityEngine.Object.FindAnyObjectByType<WaveSpawner>().IsWaitingForNextWave && audio.CurrentMusicState == AudioManager.MusicState.Preparation && audio.RequestedMusicClip == audio.bgmPreparation, "Clearing wave returns music to preparation during intermission");
                    UnityEngine.Object.FindAnyObjectByType<WaveSpawner>().StartWaveEarly();
                    Check(audio.CurrentMusicState == AudioManager.MusicState.Battle, "Early start switches music back to battle");
                    sounds.Clear(); break;
                case 12:
                    foreach (Enemy remaining in UnityEngine.Object.FindObjectsByType<Enemy>()) remaining.TakeDamage(10000);
                    if (!GameManager.GameIsOver) { if (++retries > 12) throw new Exception("Final QA wave did not finish"); step = 12; }
                    break;
                case 13:
                    Check(GameManager.GameIsOver && Time.timeScale == 0 && audio.CurrentMusicState == AudioManager.MusicState.Result, "Final wave triggers victory result state");
                    Check(sounds.Count(c => c == audio.sfxVictory) == 1 && !audio.IsMusicPlaying && audio.ActiveCombatVoices == 0, "Victory plays once with BGM and combat voices stopped");
                    sounds.Clear(); for (int i = 0; i < 100; i++) audio.PlayArrowShoot();
                    Check(sounds.Count == 0, "Result state rejects late combat sounds");
                    gm.ReplayLevel(); break;
                case 14:
                    Check(gm != null && !GameManager.GameIsOver && audio.CurrentMusicState == AudioManager.MusicState.Preparation && audio.IsMusicPlaying, "Replay restores preparation music and clears result state");
                    Check(Mathf.Approximately(audio.GetBGMVolume(), .23f) && Mathf.Approximately(audio.GetSFXVolume(), .41f), "Replay preserves saved volume settings");
                    sounds.Clear(); GameManager.Lives = 0; break;
                case 15:
                    Check(GameManager.GameIsOver && audio.CurrentMusicState == AudioManager.MusicState.Result && !audio.IsMusicPlaying && sounds.Count(c => c == audio.sfxDefeat) == 1, "Defeat plays once and stops BGM");
                    gm.OpenMainMenu(); break;
                case 16:
                    Check(SceneManager.GetActiveScene().name == "MainMenu" && audio.CurrentMusicState == AudioManager.MusicState.Menu && audio.IsMusicPlaying, "Returning from defeat restores MainMenu music");
                    SceneManager.LoadScene("Level_2"); break;
                case 17:
                    Check(UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>() != null && UnityEngine.Object.FindObjectsByType<AudioManager>().Length == 1 && Mathf.Approximately(audio.GetSFXVolume(), .41f), "Level 2 retains single audio manager, controls and volume");
                    SceneManager.LoadScene("Level_3"); break;
                case 18:
                    Check(UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>() != null && UnityEngine.Object.FindObjectsByType<AudioManager>().Length == 1 && Mathf.Approximately(audio.GetBGMVolume(), .23f), "Level 3 retains single audio manager, controls and volume");
                    sounds.Clear();
                    GameObject.Find("Speed_x2").GetComponent<Button>().onClick.Invoke();
                    Check(Time.timeScale == 2 && sounds.Count(c => c == audio.sfxUIClick) == 1, "Runtime-created speed control emits a single UI click");
                    GameObject.Find("Speed_x1").GetComponent<Button>().onClick.Invoke();
                    UnityEngine.Object.FindAnyObjectByType<UIManager>().PauseGame();
                    UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>().SetOpen(true);
                    CaptureAudioPanel();
                    UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>().SetOpen(false);
                    UnityEngine.Object.FindAnyObjectByType<UIManager>().ResumeGame();
                    break;
                case 19:
                    var queuedWave = UnityEngine.Object.FindAnyObjectByType<WaveSpawner>();
                    queuedWave.waveConfigs = new[] { new WaveSpawner.WaveDefinition { goblinCount = 100, spawnInterval = 100f } };
                    Call(queuedWave, "StartWave"); Call(queuedWave, "SpawnNextEnemy");
                    Enemy first = UnityEngine.Object.FindAnyObjectByType<Enemy>(); first.speed = 0; adaptiveEnemies.Clear(); adaptiveEnemies.Add(first);
                    var director = audio.GetComponent<BattleMusicDirector>(); director.RefreshPopulation();
                    Check(queuedWave.AliveEnemiesCount == 100 && director.LiveEnemyCount == 1, "Adaptive population excludes 99 queued enemies from a real wave");
                    for (int i = 0; i < 4; i++) AddAdaptiveEnemy();
                    Enemy inactive = AddAdaptiveEnemy(); inactive.gameObject.SetActive(false);
                    Enemy dead = AddAdaptiveEnemy(); typeof(Enemy).GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(dead, 0f);
                    director.RefreshPopulation();
                    Check(director.LiveEnemyCount == 5 && !director.HasLivingBoss, "Population scan excludes inactive and zero-health enemies");
                    next = EditorApplication.timeSinceStartup + 6.5; break;
                case 20:
                    Check(audio.BattleIntensity == BattleMusicPolicy.Intensity.Tense && audio.RequestedMusicClip == audio.bgmBattleMedium, "Five real living enemies route to the imported medium loop");
                    for (int i = 0; i < 5; i++) AddAdaptiveEnemy();
                    next = EditorApplication.timeSinceStartup + 6.5; break;
                case 21:
                    Check(audio.BattleIntensity == BattleMusicPolicy.Intensity.Intense && audio.RequestedMusicClip == audio.bgmBattleHigh, "Ten real living enemies route to the imported high loop");
                    foreach (Enemy extra in adaptiveEnemies.Skip(1)) if (extra != null) UnityEngine.Object.Destroy(extra.gameObject);
                    Time.timeScale = 0;
                    next = EditorApplication.timeSinceStartup + 6; break;
                case 22:
                    Check(audio.BattleIntensity == BattleMusicPolicy.Intensity.Intense, "Pausing freezes adaptive population hold timers");
                    Time.timeScale = 1;
                    next = EditorApplication.timeSinceStartup + 6.5; break;
                case 23:
                    Check(audio.BattleIntensity == BattleMusicPolicy.Intensity.Calm && audio.RequestedMusicClip == audio.bgmBattle, "One remaining enemy returns music to calm after the stable hold");
                    adaptiveEnemies[0].ConfigureDirect("Boss", 50, 0, 0, 1);
                    next = EditorApplication.timeSinceStartup + .45; break;
                case 24:
                    Check(audio.GetComponent<BattleMusicDirector>().HasLivingBoss && audio.RequestedMusicClip == audio.bgmBattleHigh, "A real living Boss requests the high loop within one population scan");
                    var musicSources = audio.GetComponents<AudioSource>().Where(source => source.loop).ToArray();
                    Check(musicSources.Length == 2 && musicSources.All(source => source.isPlaying), "Adaptive crossfade overlaps two playing music sources");
                    Check(musicSources.Sum(source => source.volume) <= audio.GetBGMVolume() + .0001f, "Crossfade gains sum to at most the user's music volume");
                    audio.SetBGMVolume(0);
                    Check(musicSources.All(source => source.volume == 0), "Music mute applies to both sources during crossfade");
                    audio.SetBGMVolume(.23f); audio.SetMusicState(AudioManager.MusicState.Result);
                    Check(musicSources.All(source => !source.isPlaying) && !audio.IsMusicPlaying, "Result cancels an in-progress adaptive crossfade and stops both sources");
                    audio.SetMusicState(AudioManager.MusicState.Preparation);
                    next = EditorApplication.timeSinceStartup + 1.5; break;
                case 25:
                    Check(audio.IsMusicPlaying && audio.RequestedMusicClip == audio.bgmPreparation, "Preparation music restarts after cancelling an adaptive transition");
                    Stop(); break;
            }
        }
        catch (Exception error) { results.failure = error.ToString(); Stop(); }
    }
    private static void Stop()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        if (AudioManager.Instance != null) AudioManager.Instance.SoundPlayed -= Heard;
        Save(); EditorApplication.isPlaying = false;
    }
    private static void Save()
    {
        string json = JsonUtility.ToJson(results, true);
        File.WriteAllText(Path.Combine(Root, "Docs/QA/unity_verification.json"), json);
        File.WriteAllText(Path.Combine(Root, "Docs/QA/sound_completion_2026-11-02.json"), json);
    }
    private static void CaptureAudioPanel()
    {
        Camera camera = Camera.main;
        Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        string folder = Path.Combine(Root, "Docs/QA/Visual"); Directory.CreateDirectory(folder);
        foreach (int width in new[] { 1280, 1600, 1920 })
        {
            int height = width * 9 / 16;
            RenderTexture target = RenderTexture.GetTemporary(width, height, 24);
            camera.targetTexture = target;
            foreach (Canvas canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = camera.nearClipPlane + .1f; }
            Canvas.ForceUpdateCanvases();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = target });
            RenderTexture previous = RenderTexture.active; RenderTexture.active = target;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            File.WriteAllBytes(Path.Combine(folder, "audio-" + width + "x" + height + ".png"), image.EncodeToPNG());
            UnityEngine.Object.Destroy(image); RenderTexture.active = previous;
            camera.targetTexture = null; RenderTexture.ReleaseTemporary(target);
        }
        foreach (Canvas canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
    }
    private static void Finish()
    {
        results = JsonUtility.FromJson<Results>(File.ReadAllText(Path.Combine(Root, "Docs/QA/unity_verification.json")));
        if (string.IsNullOrEmpty(results.failure))
        {
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            BuildReport report = BuildPipeline.BuildPlayer(scenes, "Builds/QA/KingdomGuard.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);
            results.build = report.summary.result + "; errors=" + report.summary.totalErrors + "; warnings=" + report.summary.totalWarnings;
            results.legacyAudioIncluded = report.packedAssets.SelectMany(a => a.contents).Any(a => a.sourceAssetPath.StartsWith("Assets/Audio/", StringComparison.Ordinal));
            if (results.legacyAudioIncluded) results.failure = "Legacy audio unexpectedly packaged";
            if (report.summary.result != BuildResult.Succeeded) results.failure = "Windows build failed";
        }
        Save(); EditorApplication.Exit(string.IsNullOrEmpty(results.failure) ? 0 : 1);
    }
}
#endif
