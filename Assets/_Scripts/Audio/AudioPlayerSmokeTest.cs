using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Opt-in verification commands for the actual Windows player.</summary>
public class AudioPlayerSmokeTest : MonoBehaviour
{
    [Serializable] private class PreferencesBackup { public bool musicExists, effectsExists; public float music, effects; }
    private static PreferencesBackup startup;
    private const string MusicKey = "KingdomGuard.BGMVolume", EffectsKey = "KingdomGuard.SFXVolume";
    private static string BackupPath => Path.Combine(Application.persistentDataPath, "SoundQaPrefsBackup.json");
    private static bool Requested(string flag) => Array.IndexOf(Environment.GetCommandLineArgs(), flag) >= 0;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void CapturePreferences()
    {
        if (Application.isEditor || !Requested("--kg-audio-save-settings")) return;
        startup = new PreferencesBackup { musicExists = PlayerPrefs.HasKey(MusicKey), effectsExists = PlayerPrefs.HasKey(EffectsKey), music = PlayerPrefs.GetFloat(MusicKey, .7f), effects = PlayerPrefs.GetFloat(EffectsKey, .8f) };
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        if (Application.isEditor || !(Requested("--kg-audio-smoke-test") || Requested("--kg-audio-save-settings") || Requested("--kg-audio-check-settings") || Requested("--kg-audio-restore-settings"))) return;
        GameObject runner = new GameObject("Audio Build Verification");
        DontDestroyOnLoad(runner); runner.AddComponent<AudioPlayerSmokeTest>();
    }
    private static void RestorePreferences()
    {
        if (!File.Exists(BackupPath)) return;
        PreferencesBackup previous = JsonUtility.FromJson<PreferencesBackup>(File.ReadAllText(BackupPath));
        if (previous.musicExists) PlayerPrefs.SetFloat(MusicKey, previous.music); else PlayerPrefs.DeleteKey(MusicKey);
        if (previous.effectsExists) PlayerPrefs.SetFloat(EffectsKey, previous.effects); else PlayerPrefs.DeleteKey(EffectsKey);
        PlayerPrefs.Save(); File.Delete(BackupPath);
        Debug.Log("KG_AUDIO_PREFS_RESTORED");
    }
    private IEnumerator Start()
    {
        yield return null; yield return null;
        if (Requested("--kg-audio-restore-settings")) { RestorePreferences(); Application.Quit(0); yield break; }
        if (Requested("--kg-audio-save-settings"))
        {
            if (File.Exists(BackupPath)) { Debug.LogError("Stale audio QA preference backup; restore before starting a new test"); Application.Quit(1); yield break; }
            File.WriteAllText(BackupPath, JsonUtility.ToJson(startup));
            AudioManager.Instance.SetBGMVolume(.23f); AudioManager.Instance.SetSFXVolume(.41f); AudioManager.Instance.SaveVolumeSettings();
            Debug.Log("KG_AUDIO_SETTINGS_SAVED music=0.23 effects=0.41");
            Application.Quit(0); yield break;
        }
        if (Requested("--kg-audio-check-settings"))
        {
            bool restored = Mathf.Approximately(AudioManager.Instance.GetBGMVolume(), .23f) && Mathf.Approximately(AudioManager.Instance.GetSFXVolume(), .41f);
            Debug.Log(restored ? "KG_AUDIO_SETTINGS_REOPEN_PASS" : "KG_AUDIO_SETTINGS_REOPEN_FAIL");
            // Restore on the quitting callback, after AudioManager has saved its values.
            Application.quitting += RestorePreferences;
            Application.Quit(restored ? 0 : 1); yield break;
        }
        for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
        {
            if (SceneManager.GetActiveScene().buildIndex != index) yield return SceneManager.LoadSceneAsync(index);
            yield return null; yield return null; yield return new WaitForSecondsRealtime(.2f);
            bool menu = GameManager.IsMenuScene(SceneManager.GetActiveScene().path);
            bool valid = AudioManager.Instance != null && (menu || GameManager.Instance != null)
                && FindAnyObjectByType<AudioSettingsUI>() != null
                && Resources.LoadAll<AudioClip>("Audio").Length == AudioManager.ExpectedRuntimeClipCount
                && FindObjectsByType<AudioManager>().Length == 1
                && AudioManager.Instance.CurrentMusicState == (menu ? AudioManager.MusicState.Menu : AudioManager.MusicState.Preparation);
            if (!valid) { Debug.LogError("KG_AUDIO_SMOKE_FAIL scene=" + index); Application.Quit(1); yield break; }
            Debug.Log("KG_AUDIO_SMOKE_PASS scene=" + index);
        }
        Debug.Log("KG_AUDIO_SMOKE_COMPLETE"); Application.Quit(0);
    }
}
