using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Opt-in build smoke check: KingdomGuard.exe --kg-audio-smoke-test.</summary>
public class AudioPlayerSmokeTest : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        if (Application.isEditor || Array.IndexOf(Environment.GetCommandLineArgs(), "--kg-audio-smoke-test") < 0) return;
        GameObject runner = new GameObject("Audio Build Smoke Test");
        DontDestroyOnLoad(runner);
        runner.AddComponent<AudioPlayerSmokeTest>();
    }
    private IEnumerator Start()
    {
        for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
        {
            if (SceneManager.GetActiveScene().buildIndex != index)
                yield return SceneManager.LoadSceneAsync(index);
            yield return null;
            yield return null;
            bool valid = AudioManager.Instance != null && GameManager.Instance != null
                && FindAnyObjectByType<AudioSettingsUI>() != null
                && Resources.LoadAll<AudioClip>("Audio").Length == 12
                && FindObjectsByType<AudioManager>().Length == 1;
            if (!valid) { Debug.LogError("KG_AUDIO_SMOKE_FAIL scene=" + index); Application.Quit(1); yield break; }
            Debug.Log("KG_AUDIO_SMOKE_PASS scene=" + index);
        }
        Debug.Log("KG_AUDIO_SMOKE_COMPLETE");
        Application.Quit(0);
    }
}
