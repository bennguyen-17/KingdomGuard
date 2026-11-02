using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum MusicState { Menu, Preparation, Battle, Result }
    public static AudioManager Instance { get; private set; }
    public const int ExpectedRuntimeClipCount = 15;
    public const int MaxCombatVoices = 4;
    public const float EffectsHeadroom = 0.65f;
    public MusicState CurrentMusicState { get; private set; }
    public event System.Action<AudioClip> SoundPlayed;
    public AudioClip RequestedMusicClip { get; private set; }
    public bool IsMusicPlaying => (bgmSource != null && bgmSource.isPlaying) || (secondaryMusic != null && secondaryMusic.isPlaying);
    public BattleMusicPolicy.Intensity BattleIntensity { get; private set; }
    public int ActiveCombatVoices
    {
        get { int count = 0; foreach (AudioSource voice in combatVoices) if (voice.isPlaying) count++; return count; }
    }
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    private AudioSource importantSource, uiSource, secondaryMusic;
    private BattleMusicDirector battleDirector;
    private readonly List<AudioSource> combatVoices = new List<AudioSource>();
    public AudioClip bgmPreparation, bgmBattle, bgmBattleMedium, bgmBattleHigh;
    public AudioClip sfxArrowShoot, sfxCannonShoot, sfxMagicShoot, sfxHit, sfxEnemyDeath;
    public AudioClip sfxTowerPlace, sfxUIClick, sfxVictory, sfxDefeat;
    public AudioClip sfxBuildFailed, sfxBaseDamage;
    private const string BGMKey = "KingdomGuard.BGMVolume";
    private const string SFXKey = "KingdomGuard.SFXVolume";
    private readonly Dictionary<AudioClip, float> lastPlayed = new Dictionary<AudioClip, float>();
    private float nextCombatSound, bgmVolume, sfxVolume, musicEnvelope = 1f, secondaryEnvelope;
    private Coroutine musicFade;

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
        bgmSource = ConfigureSource(bgmSource, true, 100);
        secondaryMusic = ConfigureSource(null, true, 100);
        sfxSource = ConfigureSource(sfxSource, false, 180);
        combatVoices.Add(sfxSource);
        for (int i = 1; i < MaxCombatVoices; i++) combatVoices.Add(ConfigureSource(null, false, 180));
        importantSource = ConfigureSource(null, false, 40);
        uiSource = ConfigureSource(null, false, 30);
        SetBGMVolume(PlayerPrefs.GetFloat(BGMKey, 0.7f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXKey, 0.8f));
        Load(ref bgmPreparation, "BGM/BGM_Preparation");
        Load(ref bgmBattle, "BGM/BGM_Battle");
        Load(ref bgmBattleMedium, "BGM/BGM_Battle_Medium");
        Load(ref bgmBattleHigh, "BGM/BGM_Battle_High");
        Load(ref sfxArrowShoot, "SFX/SFX_Arrow_Shoot");
        Load(ref sfxCannonShoot, "SFX/SFX_Cannon_Shoot");
        Load(ref sfxMagicShoot, "SFX/SFX_Magic_Shoot");
        Load(ref sfxHit, "SFX/SFX_Hit");
        Load(ref sfxEnemyDeath, "SFX/SFX_Enemy_Death");
        Load(ref sfxTowerPlace, "SFX/SFX_Tower_Place");
        Load(ref sfxUIClick, "SFX/SFX_UI_Click");
        Load(ref sfxVictory, "SFX/SFX_Victory");
        Load(ref sfxDefeat, "SFX/SFX_Defeat");
        Load(ref sfxBuildFailed, "SFX/SFX_Build_Failed");
        Load(ref sfxBaseDamage, "SFX/SFX_Base_Damage");
        battleDirector = gameObject.AddComponent<BattleMusicDirector>();
        SetMusicState(MusicState.Menu);
    }
    private AudioSource ConfigureSource(AudioSource source, bool loop, int priority)
    {
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        source.priority = priority;
        return source;
    }
    private static void Load(ref AudioClip clip, string path)
    {
        if (clip == null) clip = Resources.Load<AudioClip>("Audio/" + path);
        if (clip == null) Debug.LogWarning("Missing audio clip: Audio/" + path);
    }
    private void StopCombat()
    {
        foreach (AudioSource voice in combatVoices) voice.Stop();
        nextCombatSound = 0f;
    }
    public void SetMusicState(MusicState state)
    {
        MusicState previous = CurrentMusicState;
        CurrentMusicState = state;
        if (state == MusicState.Result)
        {
            StopBGM(); StopCombat(); lastPlayed.Clear(); return;
        }
        if (state != MusicState.Battle || previous == MusicState.Result)
        {
            StopCombat(); importantSource.Stop();
        }
        if (state == MusicState.Battle)
        {
            if (previous != MusicState.Battle)
            {
                battleDirector.ResetForBattle();
                SetBattleIntensity(BattleMusicPolicy.Intensity.Calm);
            }
        }
        else PlayBGM(bgmPreparation);
    }
    public void SetBattleIntensity(BattleMusicPolicy.Intensity intensity)
    {
        if (CurrentMusicState != MusicState.Battle) return;
        BattleIntensity = intensity;
        PlayBGM(intensity == BattleMusicPolicy.Intensity.Intense ? bgmBattleHigh :
                intensity == BattleMusicPolicy.Intensity.Tense ? bgmBattleMedium : bgmBattle);
    }
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (RequestedMusicClip == clip && (IsMusicPlaying || musicFade != null)) return;
        RequestedMusicClip = clip;
        // Finish the current crossfade before handling a newer request. This avoids
        // cutting either audible source when a wave changes during a transition.
        if (musicFade == null) musicFade = StartCoroutine(ChangeMusic());
    }
    private IEnumerator ChangeMusic()
    {
        while (RequestedMusicClip != null && (bgmSource.clip != RequestedMusicClip || !bgmSource.isPlaying))
        {
            AudioClip target = RequestedMusicClip;
            float initial = bgmSource.isPlaying ? musicEnvelope : 0f;
            secondaryMusic.clip = target;
            secondaryEnvelope = 0f;
            ApplyMusicVolume();
            secondaryMusic.Play();
            for (float elapsed = 0; elapsed < 1f; elapsed += Time.unscaledDeltaTime)
            {
                float progress = Mathf.Clamp01(elapsed);
                musicEnvelope = initial * (1f - progress);
                secondaryEnvelope = progress;
                ApplyMusicVolume();
                yield return null;
            }
            bgmSource.Stop();
            AudioSource old = bgmSource; bgmSource = secondaryMusic; secondaryMusic = old;
            musicEnvelope = 1f; secondaryEnvelope = 0f;
            ApplyMusicVolume();
        }
        musicFade = null;
    }
    private void ApplyMusicVolume()
    {
        bgmSource.volume = bgmVolume * musicEnvelope;
        secondaryMusic.volume = bgmVolume * secondaryEnvelope;
    }
    public void StopBGM()
    {
        if (musicFade != null) StopCoroutine(musicFade);
        musicFade = null;
        RequestedMusicClip = null;
        musicEnvelope = 1f;
        secondaryEnvelope = 0f;
        if (bgmSource != null) bgmSource.Stop();
        if (secondaryMusic != null) secondaryMusic.Stop();
        ApplyMusicVolume();
    }
    // One sound per free voice; never accumulate unbounded PlayOneShot voices.
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || CurrentMusicState != MusicState.Battle) return;
        float now = Time.unscaledTime;
        if (now < nextCombatSound || (lastPlayed.TryGetValue(clip, out float last) && now - last < .1f)) return;
        AudioSource free = combatVoices.Find(voice => !voice.isPlaying);
        if (free == null) return;
        nextCombatSound = now + .04f;
        lastPlayed[clip] = now;
        free.PlayOneShot(clip, SanitizeVolume(volumeScale));
        SoundPlayed?.Invoke(clip);
    }
    private void PlayImportant(AudioClip clip, AudioSource source = null, float cooldown = .1f)
    {
        if (clip == null) return;
        if (lastPlayed.TryGetValue(clip, out float last) && Time.unscaledTime - last < cooldown) return;
        lastPlayed[clip] = Time.unscaledTime;
        AudioSource output = source != null ? source : importantSource;
        output.Stop();
        output.PlayOneShot(clip);
        SoundPlayed?.Invoke(clip);
    }
    public void PlayArrowShoot() => PlaySFX(sfxArrowShoot, .45f);
    public void PlayCannonShoot() => PlaySFX(sfxCannonShoot, .65f);
    public void PlayMagicShoot() => PlaySFX(sfxMagicShoot, .5f);
    public void PlayHit() => PlaySFX(sfxHit, .35f);
    public void PlayEnemyDeath() => PlaySFX(sfxEnemyDeath, .5f);
    public void PlayTowerPlace() => PlayImportant(sfxTowerPlace);
    public void PlayBuildFailed() => PlayImportant(sfxBuildFailed, cooldown: .3f);
    public void PlayBaseDamage() => PlayImportant(sfxBaseDamage);
    public void PlayUIClick() => PlayImportant(sfxUIClick, uiSource);
    public void PlayVictory() { SetMusicState(MusicState.Result); importantSource.Stop(); PlayImportant(sfxVictory); }
    public void PlayDefeat() { SetMusicState(MusicState.Result); importantSource.Stop(); PlayImportant(sfxDefeat); }
    private static float SanitizeVolume(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Clamp01(value);
    public void SetBGMVolume(float volume)
    {
        bgmVolume = SanitizeVolume(volume);
        ApplyMusicVolume();
        PlayerPrefs.SetFloat(BGMKey, bgmVolume);
    }
    public void SetSFXVolume(float volume)
    {
        sfxVolume = SanitizeVolume(volume);
        foreach (AudioSource voice in combatVoices) voice.volume = sfxVolume * EffectsHeadroom;
        importantSource.volume = uiSource.volume = sfxVolume * EffectsHeadroom;
        PlayerPrefs.SetFloat(SFXKey, sfxVolume);
    }
    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;
    public void SaveVolumeSettings() => PlayerPrefs.Save();
    private void OnApplicationPause(bool paused) { if (paused) SaveVolumeSettings(); }
    private void OnApplicationQuit() => SaveVolumeSettings();
    private void OnDestroy() { if (Instance == this) { SaveVolumeSettings(); Instance = null; } }
}
