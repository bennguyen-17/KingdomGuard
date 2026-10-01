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

    [Header("Result navigation")]
    [SerializeField] private string mainMenuScene = "MainMenu";

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
        Money = startMoney;
        Lives = startLives;
        WaveSpawner spawner = FindObjectOfType<WaveSpawner>();
        if (spawner != null) ConfigureWaves(spawner.totalWaves);
    }

    private void Update()
    {
        if (!GameIsOver && Lives <= 0) EndGame();
    }

    public void ConfigureWaves(int count) => totalWaves = Mathf.Max(1, count);
    public void ConfigureMainMenu(string sceneName) => mainMenuScene = sceneName;
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
        resultUI.ShowWinScreen(CreateResults());
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

    public void ReplayLevel() => LoadScene(SceneManager.GetActiveScene().name);
    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings) LoadScene(nextIndex);
        else Debug.LogWarning("No next level is configured in Build Settings.");
    }

    public void LoadMainMenu()
    {
        if (!string.IsNullOrWhiteSpace(mainMenuScene) && Application.CanStreamedLevelBeLoaded(mainMenuScene))
            LoadScene(mainMenuScene);
        else
            Debug.LogWarning("Main menu scene '" + mainMenuScene + "' is not configured in Build Settings.");
    }

    private static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private static void LoadScene(int buildIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }
}
