using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class WaveDefinition
    {
        [Tooltip("Số lượng Goblin")]
        public int goblinCount = 6;
        [Tooltip("Số lượng Orc (quái trâu máu)")]
        public int orcCount = 0;
        [Tooltip("Khoảng cách giữa các lần sinh quái (giây)")]
        public float spawnInterval = 1.5f;
    }

    private struct QueuedEnemy
    {
        public string enemyName;
        public float baseHp;
        public float speed;
        public int gold;
        public int castleDmg;
        public string animName;
    }

    [Header("Prefabs cho từng loại quái")]
    [Tooltip("Prefab riêng cho Goblin (nếu để trống sẽ dùng Enemy Prefab)")]
    public GameObject goblinPrefab;
    [Tooltip("Prefab riêng cho Orc (nếu để trống sẽ dùng Enemy Prefab)")]
    public GameObject orcPrefab;
    [Tooltip("Prefab quái mặc định / dự phòng")]
    public GameObject enemyPrefab;

    [Header("Wave Configuration (GDD)")]
    [Tooltip("Tổng số wave cho bản MVP (Mặc định: 6 wave)")]
    public int totalWaves = 6;
    [Tooltip("Thời gian nghỉ giữa các wave theo GDD (12 giây)")]
    public float timeBetweenWaves = 12f;

    [Header("Cấu hình chi tiết 6 Wave chuẩn GDD")]
    public WaveDefinition[] waveConfigs = new WaveDefinition[]
    {
        new WaveDefinition { goblinCount = 5,  orcCount = 0, spawnInterval = 1.5f }, // Wave 1: 5 Goblin
        new WaveDefinition { goblinCount = 8,  orcCount = 0, spawnInterval = 1.5f }, // Wave 2: 8 Goblin
        new WaveDefinition { goblinCount = 12, orcCount = 0, spawnInterval = 1.2f }, // Wave 3: 12 Goblin
        new WaveDefinition { goblinCount = 6,  orcCount = 3, spawnInterval = 1.2f }, // Wave 4: 6 Goblin + 3 Orc
        new WaveDefinition { goblinCount = 8,  orcCount = 4, spawnInterval = 1.0f }, // Wave 5: 8 Goblin + 4 Orc
        new WaveDefinition { goblinCount = 10, orcCount = 6, spawnInterval = 0.8f }  // Wave 6: 10 Goblin + 6 Orc
    };

    [Header("ScriptableObject Quái (Tùy chọn nếu có)")]
    public EnemyData goblinData;
    public EnemyData orcData;

    [Header("Path Configuration")]
    public Waypoints[] paths;

    private int waveIndex = 0;
    private int aliveEnemies = 0;
    private float timer = 0f;
    private bool isPreparingWave1 = true; // Cho phép người chơi đặt tháp trước khi bắt đầu!
    private bool waitingForNextWave = false;
    private bool started = false;

    private readonly Queue<QueuedEnemy> enemyQueue = new Queue<QueuedEnemy>();
    private float currentSpawnInterval = 1.5f;

    public int CurrentWave => waveIndex;
    public float WaveCountdown => timer;
    public bool IsWaitingForNextWave => waitingForNextWave;
    public bool IsPreparingWave1 => isPreparingWave1;

    private void Start()
    {
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();

        if (paths == null || paths.Length == 0)
        {
            paths = FindObjectsByType<Waypoints>();
        }

        totalWaves = Mathf.Max(1, totalWaves);
        if (GameManager.Instance != null)
            GameManager.Instance.ConfigureWaves(totalWaves);
    }

    private void Update()
    {
        if (GameManager.GameIsOver) return;
        if (!started) return;

        bool spacePressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

        // Giai đoạn chuẩn bị: Chờ người chơi đặt tháp xong rồi bấm Bắt đầu Wave 1
        if (isPreparingWave1)
        {
            if (spacePressed)
            {
                StartFirstWave();
            }
            return;
        }

        if (GameManager.Lives <= 0)
        {
            GameManager.Instance?.EndGame();
            return;
        }

        // Khoảng nghỉ giữa các Wave (12 giây theo GDD)
        if (waitingForNextWave)
        {
            if (spacePressed)
            {
                StartWaveEarly();
                return;
            }

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartWave();
            }
            return;
        }

        if (enemyQueue.Count > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SpawnNextEnemy();
            }
        }
        else if (aliveEnemies == 0)
        {
            GameManager.Instance?.CompleteWave(waveIndex);
            if (waveIndex >= totalWaves)
            {
                GameManager.Instance?.WinGame();
            }
            else
            {
                waitingForNextWave = true;
                timer = timeBetweenWaves;
            }
        }
    }

    public void StartFirstWave()
    {
        if (isPreparingWave1)
        {
            isPreparingWave1 = false;
            StartWave();
        }
    }

    private void StartWave()
    {
        waitingForNextWave = false;
        waveIndex++;
        timer = 0f;
        enemyQueue.Clear();

        GameManager.Instance?.SetCurrentWave(waveIndex);

        int configIdx = Mathf.Clamp(waveIndex - 1, 0, waveConfigs.Length - 1);
        WaveDefinition config = waveConfigs[configIdx];
        currentSpawnInterval = config.spawnInterval;

        int gCount = config.goblinCount;
        int oCount = config.orcCount;

        while (gCount > 0 || oCount > 0)
        {
            if (gCount > 0)
            {
                enemyQueue.Enqueue(new QueuedEnemy
                {
                    enemyName = "Goblin",
                    baseHp = 50f,
                    speed = 2.0f,
                    gold = 5,
                    castleDmg = 1,
                    animName = "Goblin_Walk"
                });
                gCount--;
            }

            if (oCount > 0)
            {
                enemyQueue.Enqueue(new QueuedEnemy
                {
                    enemyName = "Orc",
                    baseHp = 150f,
                    speed = 1.2f,
                    gold = 10,
                    castleDmg = 2,
                    animName = "Orc_Walk"
                });
                oCount--;
            }
        }
    }

    private void SpawnNextEnemy()
    {
        if (enemyQueue.Count == 0) return;

        if (paths == null || paths.Length == 0)
        {
            paths = FindObjectsByType<Waypoints>();
        }

        if (paths == null || paths.Length == 0)
        {
            Debug.LogError("WaveSpawner: Không tìm thấy Waypoints nào trong Scene.");
            enabled = false;
            return;
        }

        QueuedEnemy next = enemyQueue.Dequeue();
        Waypoints chosenPath = paths[Random.Range(0, paths.Length)];
        if (chosenPath == null || chosenPath.PointCount == 0) return;

        GameObject prefabToSpawn = enemyPrefab;
        if (next.enemyName == "Goblin" && goblinPrefab != null) prefabToSpawn = goblinPrefab;
        else if (next.enemyName == "Orc" && orcPrefab != null) prefabToSpawn = orcPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError("WaveSpawner: Chưa gán Enemy Prefab trong Inspector!");
            return;
        }

        Vector3 spawnPos = chosenPath.GetPoint(0).position;
        GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        Enemy enemy = obj.GetComponent<Enemy>();

        if (enemy != null)
        {
            aliveEnemies++;
            enemy.Initialize(this, chosenPath);

            float hpMult = 1f + 0.10f * (waveIndex - 1);

            if (next.enemyName == "Goblin" && goblinData != null)
                enemy.Configure(goblinData, hpMult);
            else if (next.enemyName == "Orc" && orcData != null)
                enemy.Configure(orcData, hpMult);
            else
                enemy.ConfigureDirect(next.enemyName, next.baseHp, next.speed, next.gold, next.castleDmg, hpMult, next.animName);
        }

        timer = currentSpawnInterval;
    }

    /// <summary>
    /// Cho phép người chơi nhấn "Bắt đầu sớm" (Early Start) để bỏ qua đếm ngược 12s giữa wave
    /// </summary>
    public void StartWaveEarly()
    {
        if (waitingForNextWave)
        {
            timer = 0f;
            StartWave();
        }
    }

    public void BeginWaveSequence()
    {
        ResetForMenu();
        started = true;
        isPreparingWave1 = true; // Luôn chờ người chơi đặt tháp xong mới chạy Wave 1!
    }

    public void ResetForMenu()
    {
        foreach (Enemy enemy in FindObjectsByType<Enemy>())
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }

        started = false;
        waveIndex = 0;
        aliveEnemies = 0;
        timer = 0f;
        isPreparingWave1 = true;
        waitingForNextWave = false;
        enemyQueue.Clear();
    }

    public void NotifyEnemyRemoved(bool defeated, int reward = 0)
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        if (defeated)
        {
            int finalReward = reward > 0 ? reward : 5;
            GameManager.Instance?.RegisterEnemyDefeated(finalReward);
        }
    }

    public int CurrentWaveIndex => waveIndex;
    public int TotalWaves => totalWaves;
    public float NextWaveTimer => timer;
    public int AliveEnemiesCount => aliveEnemies + (enemyQueue != null ? enemyQueue.Count : 0);
    public bool HasGameStarted => started;
}
