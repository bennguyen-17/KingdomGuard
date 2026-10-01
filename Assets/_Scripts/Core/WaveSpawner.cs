using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave configuration")]
    public GameObject enemyPrefab;
    [Min(1)] public int totalWaves = 10;
    [Min(1)] public int enemiesPerWave = 5;
    [Min(0.1f)] public float spawnInterval = 1.5f;
    [Min(0f)] public float timeBetweenWaves = 3f;

    private int waveIndex;
    private int spawnedThisWave;
    private int aliveEnemies;
    private float timer;
    private bool waitingForNextWave;
    private bool started;

    private void Start()
    {
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();

        totalWaves = Mathf.Max(1, totalWaves);
        enemiesPerWave = Mathf.Max(1, enemiesPerWave);
        if (GameManager.Instance != null)
            GameManager.Instance.ConfigureWaves(totalWaves);
    }

    private void Update()
    {
        if (GameManager.GameIsOver) return;
        if (!started) return;
        if (GameManager.Lives <= 0)
        {
            GameManager.Instance?.EndGame();
            return;
        }

        if (waitingForNextWave)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) StartWave();
            return;
        }

        if (spawnedThisWave < enemiesPerWave)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f) SpawnEnemy();
        }
        else if (aliveEnemies == 0)
        {
            GameManager.Instance?.CompleteWave(waveIndex);
            if (waveIndex >= totalWaves) GameManager.Instance?.WinGame();
            else
            {
                waitingForNextWave = true;
                timer = timeBetweenWaves;
            }
        }
    }

    private void StartWave()
    {
        waitingForNextWave = false;
        waveIndex++;
        spawnedThisWave = 0;
        timer = 0f;
        GameManager.Instance?.SetCurrentWave(waveIndex);
    }

    public void BeginWaveSequence()
    {
        ResetForMenu();
        started = true;
        StartWave();
    }

    public void ResetForMenu()
    {
        foreach (Enemy enemy in FindObjectsByType<Enemy>())
            Destroy(enemy.gameObject);
        started = false;
        waveIndex = 0;
        spawnedThisWave = 0;
        aliveEnemies = 0;
        timer = 0f;
        waitingForNextWave = false;
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("WaveSpawner needs an Enemy Prefab assigned.", this);
            enabled = false;
            return;
        }

        if (Waypoints.points == null || Waypoints.points.Length == 0)
        {
            Debug.LogError("WaveSpawner cannot spawn enemies because no Waypoints are configured.", this);
            enabled = false;
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError("The assigned enemy prefab needs an Enemy component.", enemy);
            Destroy(enemy);
            enabled = false;
            return;
        }

        aliveEnemies++;
        spawnedThisWave++;
        enemyComponent.Initialize(this);
        timer = spawnInterval;
    }

    public void NotifyEnemyRemoved(bool defeated)
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        if (defeated) GameManager.Instance?.RegisterEnemyDefeated(25);
    }

    [ContextMenu("Debug/Show Victory Result")]
    private void DebugShowVictory()
    {
        if (GameManager.Instance != null) GameManager.Instance.WinGame();
    }

    [ContextMenu("Debug/Show Defeat Result")]
    private void DebugShowDefeat()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Lives = 0;
            GameManager.Instance.EndGame();
        }
    }
}
