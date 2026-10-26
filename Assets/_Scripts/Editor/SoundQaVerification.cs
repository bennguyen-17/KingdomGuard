#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Run with -batchmode -executeMethod SoundQaVerification.Run. Uses real Play Mode.
[InitializeOnLoad]
public static class SoundQaVerification
{
    [Serializable] private class Results { public string unityVersion; public List<string> passed = new List<string>(); public string failure; public string build; public bool legacyAudioIncluded; }
    private static Results results;
    private static int step;
    private static double next;
    private const string Active = "KingdomGuard.QA.Active";
    private static string Root => Directory.GetParent(Application.dataPath).FullName;
    private static readonly string[] Keys = { "KingdomGuard.BGMVolume", "KingdomGuard.SFXVolume", "KingdomGuard.HighestUnlockedLevel", "KingdomGuard.Completed.Assets/Scenes/Level_1.unity", "KingdomGuard.PendingLevel" };

    static SoundQaVerification()
    {
        EditorApplication.playModeStateChanged += OnPlayMode;
        if (SessionState.GetBool(Active, false) && EditorApplication.isPlaying)
            EditorApplication.delayCall += BeginChecks;
    }
    public static void Run()
    {
        SessionState.SetBool(Active, true);
        SessionState.SetBool(Active + ".PlayOptions", EditorSettings.enterPlayModeOptionsEnabled);
        SessionState.SetInt(Active + ".PlayOptionsValue", (int)EditorSettings.enterPlayModeOptions);
        EditorSettings.enterPlayModeOptionsEnabled = false;
        foreach (string key in Keys)
        {
            SessionState.SetBool(key + ".Exists", PlayerPrefs.HasKey(key));
            if (key.Contains("Volume")) SessionState.SetFloat(key, PlayerPrefs.GetFloat(key));
            else if (key.EndsWith("PendingLevel")) SessionState.SetString(key, PlayerPrefs.GetString(key));
            else SessionState.SetInt(key, PlayerPrefs.GetInt(key));
        }
        PlayerPrefs.DeleteKey("KingdomGuard.PendingLevel");
        EditorSceneManager.OpenScene("Assets/Scenes/Level_1.unity");
        EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
    }
    private static void OnPlayMode(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(Active, false)) return;
        Debug.Log("KG_QA_STATE " + state);
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            BeginChecks();
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
            EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetBool(Active + ".PlayOptions", true);
            EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(Active + ".PlayOptionsValue", 1);
            SessionState.SetBool(Active, false);
            Finish();
        }
    }
    private static void BeginChecks()
    {
        if (results != null) return;
        results = new Results { unityVersion = Application.unityVersion };
        step = 0;
        next = EditorApplication.timeSinceStartup + 2;
        Application.logMessageReceived += OnLog;
        EditorApplication.update += Tick;
        Debug.Log("KG_QA_BEGIN");
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        results.passed.Add(message);
        Debug.Log("KG_QA_PASS " + message);
    }
    private static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error) results.failure = message + "\n" + stack;
    }
    private static void Tick()
    {
        if (EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + 0.6;
        try
        {
            if (!string.IsNullOrEmpty(results.failure)) throw new Exception(results.failure);
            AudioManager audio = AudioManager.Instance;
            GameManager gm = GameManager.Instance;
            switch (step++)
            {
                case 0:
                    Check(audio != null && gm != null, "Runtime bootstrap creates AudioManager and GameManager");
                    Check(Resources.LoadAll<AudioClip>("Audio").Length == 12, "All 12 runtime clips import as AudioClip");
                    Check(audio.CurrentMusicState == AudioManager.MusicState.Menu, "Menu uses preparation music state");
                    Check(UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>() != null, "Audio settings are available on runtime canvas");
                    UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>().SetOpen(true);
                    CaptureAudioPanel();
                    Slider[] sliders = UnityEngine.Object.FindObjectsByType<Slider>();
                    Check(sliders.Length == 2, "Settings panel exposes two sliders");
                    Slider musicSlider = sliders.First(s => s.name == "Music Slider");
                    Canvas.ForceUpdateCanvases();
                    RectTransform sliderRect = musicSlider.GetComponent<RectTransform>();
                    Vector3 hit = sliderRect.TransformPoint(new Vector3(sliderRect.rect.xMin + sliderRect.rect.width * .75f, sliderRect.rect.center.y));
                    PointerEventData pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, hit), button = PointerEventData.InputButton.Left };
                    ExecuteEvents.Execute(musicSlider.gameObject, pointer, ExecuteEvents.pointerDownHandler);
                    Check(Mathf.Abs(audio.GetBGMVolume() - .75f) < .03f, "Pointer events on slider map to expected music volume");
                    ExecuteEvents.Execute(musicSlider.gameObject, pointer, ExecuteEvents.pointerUpHandler);
                    float beforeKeyboard = musicSlider.value;
                    ExecuteEvents.Execute(musicSlider.gameObject, new AxisEventData(EventSystem.current) { moveDir = MoveDirection.Right, moveVector = Vector2.right }, ExecuteEvents.moveHandler);
                    Check(musicSlider.value > beforeKeyboard, "Keyboard navigation increases selected slider value");
                    sliders.First(s => s.name == "Music Slider").value = 0.23f;
                    sliders.First(s => s.name == "Effects Slider").value = 0.41f;
                    Check(Mathf.Approximately(audio.GetBGMVolume(), .23f) && Mathf.Approximately(audio.GetSFXVolume(), .41f), "Sliders update separate music/effects volumes");
                    audio.SetBGMVolume(-1); audio.SetSFXVolume(2); audio.SaveVolumeSettings();
                    Check(audio.GetBGMVolume() == 0 && audio.GetSFXVolume() == 1, "Volume values clamp to zero and one");
                    Check(PlayerPrefs.GetFloat("KingdomGuard.SFXVolume") == 1, "Volume changes are persisted");
                    UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>().SetOpen(false);
                    new GameObject("DuplicateAudioForQA").AddComponent<AudioManager>();
                    break;
                case 1:
                    Check(UnityEngine.Object.FindObjectsByType<AudioManager>().Length == 1, "Duplicate manager is removed");
                    audio.SetMusicState(AudioManager.MusicState.Preparation);
                    Check(audio.CurrentMusicState == AudioManager.MusicState.Preparation, "Preparation state selects quiet track");
                    WaveSpawner spawner = UnityEngine.Object.FindAnyObjectByType<WaveSpawner>();
                    spawner.totalWaves = 1; spawner.enemiesPerWave = 1; spawner.spawnInterval = .1f;
                    gm.StartSelectedLevel(SceneManager.GetActiveScene().path);
                    Check(audio.CurrentMusicState == AudioManager.MusicState.Battle, "Starting a wave selects battle music");
                    break;
                case 2:
                    Enemy enemy = UnityEngine.Object.FindAnyObjectByType<Enemy>();
                    Check(enemy != null, "Wave spawns configured enemy on waypoint path");
                    enemy.TakeDamage(1000);
                    break;
                case 3:
                    Check(GameManager.GameIsOver && Time.timeScale == 0, "Clearing final wave triggers victory and freezes gameplay");
                    Check(audio.CurrentMusicState == AudioManager.MusicState.Result, "Victory stops background combat music");
                    gm.OpenMainMenu();
                    Check(!GameManager.GameIsOver && Time.timeScale == 1 && audio.CurrentMusicState == AudioManager.MusicState.Menu, "Returning to menu resets result and music state");
                    gm.StartSelectedLevel(SceneManager.GetActiveScene().path);
                    GameManager.Lives = 0;
                    break;
                case 4:
                    Check(GameManager.GameIsOver && audio.CurrentMusicState == AudioManager.MusicState.Result, "Zero lives triggers defeat with result audio state");
                    gm.OpenMainMenu();
                    SceneManager.LoadScene("Assets/Scenes/Level_2.unity");
                    break;
                case 5:
                    Check(AudioManager.Instance != null && UnityEngine.Object.FindObjectsByType<AudioManager>().Length == 1, "Audio singleton survives Level 2 transition");
                    Check(AudioManager.Instance.GetSFXVolume() == 1, "Volume survives scene transition");
                    SceneManager.LoadScene("Assets/Scenes/Level_3.unity");
                    break;
                case 6:
                    Check(GameManager.Instance != null && UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>() != null, "Level 3 initializes game and audio controls");
                    Check(UnityEngine.Object.FindObjectsByType<Waypoints>().Length == 4, "Level 3 loads all four waypoint paths");
                    Stop();
                    break;
            }
        }
        catch (Exception error) { results.failure = error.ToString(); Stop(); }
    }
    private static void Stop()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        File.WriteAllText(Path.Combine(Root, "Docs/QA/unity_verification.json"), JsonUtility.ToJson(results, true));
        EditorApplication.isPlaying = false;
    }
    private static void CaptureAudioPanel()
    {
        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<AudioSettingsUI>().GetComponent<Canvas>();
        Camera camera = Camera.main;
        RenderMode oldMode = canvas.renderMode;
        Camera oldCamera = canvas.worldCamera;
        float oldDistance = canvas.planeDistance;
        RenderTexture oldTarget = camera.targetTexture;
        string folder = Path.Combine(Root, "Docs/QA/Visual");
        Directory.CreateDirectory(folder);
        foreach (int width in new[] { 1280, 1600, 1920 })
        {
            int height = width * 9 / 16;
            RenderTexture target = RenderTexture.GetTemporary(width, height, 24);
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = camera.nearClipPlane + 0.1f;
            Canvas.ForceUpdateCanvases();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = target });
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(folder, "audio-" + width + "x" + height + ".png"), image.EncodeToPNG());
            UnityEngine.Object.Destroy(image);
            RenderTexture.active = previous;
            camera.targetTexture = oldTarget;
            RenderTexture.ReleaseTemporary(target);
        }
        canvas.renderMode = oldMode;
        canvas.worldCamera = oldCamera;
        canvas.planeDistance = oldDistance;
    }
    private static void Finish()
    {
        string file = Path.Combine(Root, "Docs/QA/unity_verification.json");
        results = JsonUtility.FromJson<Results>(File.ReadAllText(file));
        if (string.IsNullOrEmpty(results.failure))
        {
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            BuildReport report = BuildPipeline.BuildPlayer(scenes, "Builds/QA/KingdomGuard.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);
            results.build = report.summary.result + "; errors=" + report.summary.totalErrors + "; warnings=" + report.summary.totalWarnings;
            results.legacyAudioIncluded = report.packedAssets.SelectMany(asset => asset.contents).Any(asset => asset.sourceAssetPath.StartsWith("Assets/Audio/", StringComparison.Ordinal));
            if (results.legacyAudioIncluded) results.failure = "Build unexpectedly includes legacy audio with unknown sources";
            if (report.summary.result != BuildResult.Succeeded) results.failure = "Windows build failed";
        }
        File.WriteAllText(file, JsonUtility.ToJson(results, true));
        EditorApplication.Exit(string.IsNullOrEmpty(results.failure) ? 0 : 1);
    }
}
#endif
