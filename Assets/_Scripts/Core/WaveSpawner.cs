using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave configuration")]
    public GameObject enemyPrefab;
    public string mainMenuScene = "MainMenu";
    [Min(1)] public int totalWaves = 10;
    [Min(1)] public int enemiesPerWave = 5;
    [Min(0.1f)] public float spawnInterval = 1.5f;
    [Min(0f)] public float timeBetweenWaves = 3f;

    [Header("Path configuration (Để trống sẽ tự động tìm tất cả đường trong Scene)")]
    public Waypoints[] paths;

    private int waveIndex;
    private int spawnedThisWave;
    private int aliveEnemies;
    private float timer;
    private bool waitingForNextWave;

    private void Start()
    {
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();

        // Tự động tìm tất cả các con đường nếu chưa kéo thả trong Inspector
        if (paths == null || paths.Length == 0)
        {
            paths = FindObjectsByType<Waypoints>(FindObjectsSortMode.None);
        }

        totalWaves = Mathf.Max(1, totalWaves);
        enemiesPerWave = Mathf.Max(1, enemiesPerWave);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ConfigureWaves(totalWaves);
            GameManager.Instance.ConfigureMainMenu(mainMenuScene);
        }
        StartWave();
    }

    private void Update()
    {
        if (GameManager.GameIsOver) return;
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

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("WaveSpawner needs an Enemy Prefab assigned.", this);
            enabled = false;
            return;
        }

        // Đảm bảo luôn có danh sách đường đi
        if (paths == null || paths.Length == 0)
        {
            paths = FindObjectsByType<Waypoints>(FindObjectsSortMode.None);
        }

        if (paths == null || paths.Length == 0)
        {
            Debug.LogError("WaveSpawner cannot spawn enemies because no Waypoints are configured or found in the scene.", this);
            enabled = false;
            return;
        }

        // Luân phiên chia đều quái ra các con đường trong Scene
        Waypoints chosenPath = paths[spawnedThisWave % paths.Length];
        if (chosenPath == null || chosenPath.PointCount == 0)
        {
            Debug.LogError("The chosen path has no waypoints configured.", this);
            return;
        }

        Vector3 spawnPosition = chosenPath.GetPoint(0).position;
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
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
        enemyComponent.Initialize(this, chosenPath);
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
