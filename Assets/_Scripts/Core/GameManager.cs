using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static int Money;
    public int startMoney = 100;
    public static int Lives;
    public int startLives = 20;
    public static bool GameIsOver { get; private set; }
    public static GameManager Instance { get; private set; }
    public bool IsGameplayActive { get; private set; }

    private const string PendingLevelKey = "KingdomGuard.PendingLevel";
    private const string HighestUnlockedLevelKey = "KingdomGuard.HighestUnlockedLevel";
    private const string CompletedPrefix = "KingdomGuard.Completed.";

    private int enemiesDefeated;
    private int goldEarned;
    private int wavesCompleted;
    private int currentWave;
    private int totalWaves;
    private UIManager resultUI;

    private void Awake()
    {
        Instance = this;
        GameIsOver = false;
        Time.timeScale = 1f;
        resultUI = GetComponent<UIManager>();
        if (resultUI == null) resultUI = gameObject.AddComponent<UIManager>();
    }

    private void Start()
    {
        WaveSpawner spawner = FindAnyObjectByType<WaveSpawner>();
        if (spawner != null) ConfigureWaves(spawner.totalWaves);
        ResetRunStats();

        string pendingScene = PlayerPrefs.GetString(PendingLevelKey, string.Empty);
        if (pendingScene == SceneManager.GetActiveScene().path)
        {
            PlayerPrefs.DeleteKey(PendingLevelKey);
            BeginGameplay();
        }
        else
        {
            resultUI.ShowMainMenu();
        }
    }

    private void Update()
    {
        if (!GameIsOver && Lives <= 0) EndGame();
    }

    public void ConfigureWaves(int count) => totalWaves = Mathf.Max(1, count);
    public void SetCurrentWave(int wave) => currentWave = wave;
    public void CompleteWave(int wave)
    {
        wavesCompleted = Mathf.Max(wavesCompleted, wave);
    }

    public void RegisterEnemyDefeated(int reward)
    {
        if (GameIsOver) return;
        enemiesDefeated++;
        reward = Mathf.Max(0, reward);
        Money += reward;
        goldEarned += reward;
    }

    public void EndGame()
    {
        if (GameIsOver) return;
        GameIsOver = true;
        Time.timeScale = 0f;
        resultUI.ShowLoseScreen(CreateResults());
    }

    public void WinGame()
    {
        if (GameIsOver) return;
        GameIsOver = true;
        Time.timeScale = 0f;
        SaveLevelCompletion();
        resultUI.ShowWinScreen(CreateResults());
    }

    public void StartSelectedLevel(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath)) return;
        string activePath = SceneManager.GetActiveScene().path;
        ResetRunStats();

        if (scenePath == activePath)
        {
            FindAnyObjectByType<WaveSpawner>()?.ResetForMenu();
            BeginGameplay();
            return;
        }

        PlayerPrefs.SetString(PendingLevelKey, scenePath);
        PlayerPrefs.Save();
        resultUI.TransitionTo(() => SceneManager.LoadScene(scenePath));
    }

    public void OpenMainMenu()
    {
        Time.timeScale = 1f;
        GameIsOver = false;
        IsGameplayActive = false;
        MainMenuUI.Instance?.SetBackgroundVisible(true);
        FindAnyObjectByType<WaveSpawner>()?.ResetForMenu();
        ResetRunStats();
        resultUI.ShowMainMenu();
    }

    private void BeginGameplay()
    {
        Time.timeScale = 1f;
        GameIsOver = false;
        IsGameplayActive = true;
        MainMenuUI.Instance?.SetBackgroundVisible(false);
        resultUI.HideMenus();
        FindAnyObjectByType<WaveSpawner>()?.BeginWaveSequence();
    }

    private void ResetRunStats()
    {
        Money = startMoney;
        Lives = startLives;
        enemiesDefeated = 0;
        goldEarned = 0;
        wavesCompleted = 0;
        currentWave = 0;
        GameIsOver = false;
        IsGameplayActive = false;
    }

    private void SaveLevelCompletion()
    {
        string path = SceneManager.GetActiveScene().path;
        PlayerPrefs.SetInt(CompletedPrefix + path, 1);
        int levelOrdinal = GetPlayableSceneOrdinal(path);
        int highestUnlocked = PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1);
        PlayerPrefs.SetInt(HighestUnlockedLevelKey, Mathf.Max(highestUnlocked, levelOrdinal + 1));
        PlayerPrefs.Save();
    }

    public static bool IsLevelUnlocked(int ordinal)
    {
        return ordinal <= PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1);
    }

    public static bool IsLevelCompleted(string scenePath)
    {
        return PlayerPrefs.GetInt(CompletedPrefix + scenePath, 0) != 0;
    }

    public static bool IsLaunchingScene(string scenePath)
    {
        return PlayerPrefs.GetString(PendingLevelKey, string.Empty) == scenePath;
    }

    private static int GetPlayableSceneOrdinal(string scenePath)
    {
        int ordinal = 1;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (IsMenuScene(path)) continue;
            if (path == scenePath) return ordinal;
            ordinal++;
        }
        return Mathf.Max(0, ordinal - 1);
    }

    public static bool IsMenuScene(string scenePath)
    {
        return System.IO.Path.GetFileNameWithoutExtension(scenePath).Equals("MainMenu", System.StringComparison.OrdinalIgnoreCase);
    }

    private UIManager.ResultData CreateResults()
    {
        int basePercent = startLives <= 0 ? 0 : Mathf.Clamp(Mathf.RoundToInt(100f * Lives / startLives), 0, 100);
        int score = enemiesDefeated * 250 + wavesCompleted * 500 + basePercent * 20;
        return new UIManager.ResultData
        {
            wavesCompleted = wavesCompleted,
            currentWave = currentWave,
            totalWaves = totalWaves,
            enemiesDefeated = enemiesDefeated,
            baseHealthPercent = basePercent,
            goldEarned = goldEarned,
            score = score
        };
    }

    public void ReplayLevel()
    {
        string path = SceneManager.GetActiveScene().path;
        PlayerPrefs.SetString(PendingLevelKey, path);
        PlayerPrefs.Save();
        LoadScene(path);
    }
    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        while (nextIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(nextIndex);
            if (!IsMenuScene(path))
            {
                StartSelectedLevel(path);
                return;
            }
            nextIndex++;
        }
        Debug.LogWarning("No next level is configured in Build Settings.");
    }

    public void LoadMainMenu()
    {
        OpenMainMenu();
    }

    private static void LoadScene(string scenePath)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(scenePath);
    }
}
