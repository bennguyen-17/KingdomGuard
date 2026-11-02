using UnityEngine;

// Kept independent of Unity population lookup so hold times can be verified deterministically.
public sealed class BattleMusicPolicy
{
    public enum Intensity { Calm, Tense, Intense }
    public Intensity Current { get; private set; }
    private Intensity pending;
    private float pendingSeconds, dwell;
    public void Reset() { Current = pending = Intensity.Calm; pendingSeconds = dwell = 0f; }

    public Intensity Advance(int living, bool boss, float seconds)
    {
        dwell += Mathf.Max(0f, seconds);
        Intensity desired;
        if (boss || living >= 10) desired = Intensity.Intense;
        else if (Current == Intensity.Intense && living >= 8) desired = Intensity.Intense;
        else if (living >= 5 || (Current != Intensity.Calm && living >= 4)) desired = Intensity.Tense;
        else desired = Intensity.Calm;
        if (desired != pending) { pending = desired; pendingSeconds = 0f; }
        pendingSeconds += Mathf.Max(0f, seconds);
        if (desired != Current && (boss || (dwell >= 5f && pendingSeconds >= (desired > Current ? 2f : 4f))))
        {
            Current = desired; dwell = pendingSeconds = 0f;
        }
        return Current;
    }
}

public sealed class BattleMusicDirector : MonoBehaviour
{
    public int LiveEnemyCount { get; private set; }
    public bool HasLivingBoss { get; private set; }
    private readonly BattleMusicPolicy policy = new BattleMusicPolicy();
    private AudioManager audioManager;
    private float untilScan;
    private void Awake() => audioManager = GetComponent<AudioManager>();
    public void ResetForBattle()
    {
        policy.Reset(); untilScan = 0f; LiveEnemyCount = 0; HasLivingBoss = false;
    }
    public void RefreshPopulation()
    {
        LiveEnemyCount = 0; HasLivingBoss = false;
        // The WaveSpawner count includes queued enemies. Count only living scene objects.
        foreach (Enemy enemy in FindObjectsByType<Enemy>())
        {
            if (!enemy.isActiveAndEnabled || enemy.CurrentHealth <= 0f) continue;
            LiveEnemyCount++;
            if (string.Equals(enemy.EnemyKey, "Boss", System.StringComparison.OrdinalIgnoreCase)) HasLivingBoss = true;
        }
    }
    private void Update()
    {
        if (audioManager.CurrentMusicState != AudioManager.MusicState.Battle || Time.timeScale == 0f) return;
        untilScan -= Time.unscaledDeltaTime;
        if (untilScan <= 0f) { RefreshPopulation(); untilScan = .25f; }
        var intensity = policy.Advance(LiveEnemyCount, HasLivingBoss, Time.unscaledDeltaTime);
        if (intensity != audioManager.BattleIntensity) audioManager.SetBattleIntensity(intensity);
    }
}
