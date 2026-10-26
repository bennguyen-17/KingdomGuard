using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum MusicState { Menu, Preparation, Battle, Result }
    public static AudioManager Instance { get; private set; }
    public MusicState CurrentMusicState { get; private set; }
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    private AudioSource importantSource;
    private AudioSource uiSource;
    public AudioClip bgmPreparation, bgmBattle;
    public AudioClip sfxArrowShoot, sfxCannonShoot, sfxHit, sfxEnemyDeath;
    public AudioClip sfxTowerPlace, sfxUIClick, sfxVictory, sfxDefeat;
    public AudioClip sfxBuildFailed, sfxBaseDamage;
    private const string BGMKey = "KingdomGuard.BGMVolume";
    private const string SFXKey = "KingdomGuard.SFXVolume";
    private readonly Dictionary<AudioClip, float> lastPlayed = new Dictionary<AudioClip, float>();
    private float nextCombatSound;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() => Instance = null;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() => EnsureInstance();

    public static AudioManager EnsureInstance()
    {
        if (Instance == null)
        {
            AudioManager existing = FindAnyObjectByType<AudioManager>();
            if (existing != null) return existing;
            new GameObject("AudioManager").AddComponent<AudioManager>();
        }
        return Instance;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        bgmSource = ConfigureSource(bgmSource, true);
        sfxSource = ConfigureSource(sfxSource, false);
        importantSource = ConfigureSource(null, false);
        uiSource = ConfigureSource(null, false);
        SetBGMVolume(PlayerPrefs.GetFloat(BGMKey, 0.7f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXKey, 0.8f));
        Load(ref bgmPreparation, "BGM/BGM_Preparation");
        Load(ref bgmBattle, "BGM/BGM_Battle");
        Load(ref sfxArrowShoot, "SFX/SFX_Arrow_Shoot");
        Load(ref sfxCannonShoot, "SFX/SFX_Cannon_Shoot");
        Load(ref sfxHit, "SFX/SFX_Hit");
        Load(ref sfxEnemyDeath, "SFX/SFX_Enemy_Death");
        Load(ref sfxTowerPlace, "SFX/SFX_Tower_Place");
        Load(ref sfxUIClick, "SFX/SFX_UI_Click");
        Load(ref sfxVictory, "SFX/SFX_Victory");
        Load(ref sfxDefeat, "SFX/SFX_Defeat");
        Load(ref sfxBuildFailed, "SFX/SFX_Build_Failed");
        Load(ref sfxBaseDamage, "SFX/SFX_Base_Damage");
        SetMusicState(MusicState.Menu);
    }
    private AudioSource ConfigureSource(AudioSource source, bool loop)
    {
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        return source;
    }
    private static void Load(ref AudioClip clip, string path)
    {
        if (clip == null) clip = Resources.Load<AudioClip>("Audio/" + path);
        if (clip == null) Debug.LogWarning("Missing audio clip: Audio/" + path);
    }
    public void SetMusicState(MusicState state)
    {
        CurrentMusicState = state;
        if (state == MusicState.Result) { StopBGM(); sfxSource.Stop(); lastPlayed.Clear(); return; }
        if (state == MusicState.Menu) { sfxSource.Stop(); importantSource.Stop(); }
        PlayBGM(state == MusicState.Battle ? bgmBattle : bgmPreparation);
    }
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }
    public void StopBGM() { if (bgmSource != null) bgmSource.Stop(); }
    // Frequent combat events cannot mask UI, warnings or results.
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null || CurrentMusicState == MusicState.Result) return;
        float now = Time.unscaledTime;
        if (now < nextCombatSound || (lastPlayed.TryGetValue(clip, out float last) && now - last < 0.1f)) return;
        nextCombatSound = now + 0.04f;
        lastPlayed[clip] = now;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }
    private void PlayImportant(AudioClip clip, AudioSource source = null)
    {
        if (clip == null) return;
        if (lastPlayed.TryGetValue(clip, out float last) && Time.unscaledTime - last < 0.1f) return;
        lastPlayed[clip] = Time.unscaledTime;
        (source != null ? source : importantSource).PlayOneShot(clip);
    }
    public void PlayArrowShoot() => PlaySFX(sfxArrowShoot, 0.45f);
    public void PlayCannonShoot() => PlaySFX(sfxCannonShoot, 0.65f);
    public void PlayHit() => PlaySFX(sfxHit, 0.35f);
    public void PlayEnemyDeath() => PlaySFX(sfxEnemyDeath, 0.5f);
    public void PlayTowerPlace() => PlayImportant(sfxTowerPlace);
    public void PlayBuildFailed() => PlayImportant(sfxBuildFailed);
    public void PlayBaseDamage() => PlayImportant(sfxBaseDamage);
    public void PlayUIClick() => PlayImportant(sfxUIClick, uiSource);
    public void PlayVictory() { SetMusicState(MusicState.Result); importantSource.Stop(); PlayImportant(sfxVictory); }
    public void PlayDefeat() { SetMusicState(MusicState.Result); importantSource.Stop(); PlayImportant(sfxDefeat); }
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BGMKey, bgmSource.volume);
    }
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = importantSource.volume = uiSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFXKey, sfxSource.volume);
    }
    public float GetBGMVolume() => bgmSource.volume;
    public float GetSFXVolume() => sfxSource.volume;
    public void SaveVolumeSettings() => PlayerPrefs.Save();
    private void OnApplicationPause(bool paused) { if (paused) SaveVolumeSettings(); }
    private void OnApplicationQuit() => SaveVolumeSettings();
    private void OnDestroy() { if (Instance == this) { SaveVolumeSettings(); Instance = null; } }
}
