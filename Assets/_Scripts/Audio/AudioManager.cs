using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    public AudioClip bgmBattle;

    [Header("Sound Effects (SFX)")]
    public AudioClip sfxArrowShoot;
    public AudioClip sfxCannonShoot;
    public AudioClip sfxHit;
    public AudioClip sfxEnemyDeath;
    public AudioClip sfxTowerPlace;
    public AudioClip sfxUIClick;
    public AudioClip sfxVictory;
    public AudioClip sfxDefeat;

    private const string BGM_VOL_KEY = "KingdomGuard.BGMVolume";
    private const string SFX_VOL_KEY = "KingdomGuard.SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        LoadVolumeSettings();
        AutoLoadClipsIfNull();
    }

    private void Start()
    {
        if (bgmBattle != null && (bgmSource != null && !bgmSource.isPlaying))
        {
            PlayBGM(bgmBattle);
        }
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void AutoLoadClipsIfNull()
    {
        // Tự động load clips từ thư mục Audio nếu chưa gắn trên Inspector
        if (bgmBattle == null) bgmBattle = Resources.Load<AudioClip>("Audio/BGM/BGM_Battle");
        if (sfxArrowShoot == null) sfxArrowShoot = Resources.Load<AudioClip>("Audio/SFX/SFX_Arrow_Shoot");
        if (sfxCannonShoot == null) sfxCannonShoot = Resources.Load<AudioClip>("Audio/SFX/SFX_Cannon_Shoot");
        if (sfxHit == null) sfxHit = Resources.Load<AudioClip>("Audio/SFX/SFX_Hit");
        if (sfxEnemyDeath == null) sfxEnemyDeath = Resources.Load<AudioClip>("Audio/SFX/SFX_Enemy_Death");
        if (sfxTowerPlace == null) sfxTowerPlace = Resources.Load<AudioClip>("Audio/SFX/SFX_Tower_Place");
        if (sfxUIClick == null) sfxUIClick = Resources.Load<AudioClip>("Audio/SFX/SFX_UI_Click");
        if (sfxVictory == null) sfxVictory = Resources.Load<AudioClip>("Audio/SFX/SFX_Victory");
        if (sfxDefeat == null) sfxDefeat = Resources.Load<AudioClip>("Audio/SFX/SFX_Defeat");
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volumeScale);
    }

    // Tiện ích phát SFX nhanh
    public void PlayArrowShoot() => PlaySFX(sfxArrowShoot);
    public void PlayCannonShoot() => PlaySFX(sfxCannonShoot);
    public void PlayHit() => PlaySFX(sfxHit);
    public void PlayEnemyDeath() => PlaySFX(sfxEnemyDeath);
    public void PlayTowerPlace() => PlaySFX(sfxTowerPlace);
    public void PlayUIClick() => PlaySFX(sfxUIClick);
    public void PlayVictory() => PlaySFX(sfxVictory);
    public void PlayDefeat() => PlaySFX(sfxDefeat);

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = volume;
        PlayerPrefs.SetFloat(BGM_VOL_KEY, volume);
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_VOL_KEY, volume);
    }

    public float GetBGMVolume() => bgmSource != null ? bgmSource.volume : 1f;
    public float GetSFXVolume() => sfxSource != null ? sfxSource.volume : 1f;

    private void LoadVolumeSettings()
    {
        float bgmVol = PlayerPrefs.GetFloat(BGM_VOL_KEY, 0.7f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1.0f);
        if (bgmSource != null) bgmSource.volume = bgmVol;
        if (sfxSource != null) sfxSource.volume = sfxVol;
    }
}
