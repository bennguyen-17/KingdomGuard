using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public struct ResultData
    {
        public int wavesCompleted;
        public int currentWave;
        public int totalWaves;
        public int enemiesDefeated;
        public int baseHealthPercent;
        public int goldEarned;
        public int score;
    }

    [Header("Panels & Overlays")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Image darkOverlay;
    [SerializeField] private Image transitionCurtain;

    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseReplayButton;
    [SerializeField] private Button pauseMenuButton;

    [Header("Win Stats Text References")]
    [SerializeField] private Text winWavesText;
    [SerializeField] private Text winEnemiesText;
    [SerializeField] private Text winHealthText;
    [SerializeField] private Text winGoldText;
    [SerializeField] private Text winScoreText;

    [Header("Lose Stats Text References")]
    [SerializeField] private Text loseWavesText;
    [SerializeField] private Text loseEnemiesText;
    [SerializeField] private Text loseHealthText;
    [SerializeField] private Text loseScoreText;

    [Header("Win Buttons")]
    [SerializeField] private Button winNextButton;
    [SerializeField] private Button winReplayButton;
    [SerializeField] private Button winMenuButton;

    [Header("Lose Buttons")]
    [SerializeField] private Button loseReplayButton;
    [SerializeField] private Button loseMenuButton;

    [Header("Pause Localized Labels")]
    [SerializeField] private Text pauseTitleText;
    [SerializeField] private Text resumeButtonText;
    [SerializeField] private Text pauseReplayButtonText;
    [SerializeField] private Text pauseMenuButtonText;

    [Header("Win/Lose Localized Labels")]
    [SerializeField] private Text winTitleText;
    [SerializeField] private Text winSubtitleText;
    [SerializeField] private Text loseTitleText;
    [SerializeField] private Text loseSubtitleText;
    [SerializeField] private Text winNextButtonText;
    [SerializeField] private Text winReplayButtonText;
    [SerializeField] private Text winMenuButtonText;
    [SerializeField] private Text loseReplayButtonText;
    [SerializeField] private Text loseMenuButtonText;
    [SerializeField] private Text[] winRowLabels;
    [SerializeField] private Text[] loseRowLabels;

    private Coroutine animationRoutine;
    private Coroutine curtainRoutine;
    private bool screenShown;
    private bool isPaused;
    private float previousTimeScale = 1f;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        // Setup Win/Lose Button Listeners
        string currentScene = SceneManager.GetActiveScene().name;
        string nextSceneName = currentScene == "Level_1" ? "Level_2" : (currentScene == "Level_2" ? "Level_3" : "");

        if (winNextButton != null)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                winNextButton.gameObject.SetActive(true);
                winNextButton.onClick.AddListener(() => TransitionTo(() => SceneManager.LoadScene(nextSceneName)));
            }
            else
            {
                winNextButton.gameObject.SetActive(false);
            }
        }

        if (winReplayButton != null)
            winReplayButton.onClick.AddListener(() => GameManager.Instance?.ReplayLevel());

        if (winMenuButton != null)
            winMenuButton.onClick.AddListener(ShowMainMenu);

        if (loseReplayButton != null)
            loseReplayButton.onClick.AddListener(() => GameManager.Instance?.ReplayLevel());

        if (loseMenuButton != null)
            loseMenuButton.onClick.AddListener(ShowMainMenu);

        // Setup Pause Listeners
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (pauseReplayButton != null)
            pauseReplayButton.onClick.AddListener(() => { ResumeGame(); GameManager.Instance?.ReplayLevel(); });

        if (pauseMenuButton != null)
            pauseMenuButton.onClick.AddListener(() => { ResumeGame(); ShowMainMenu(); });

        // Initial States
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);
        if (transitionCurtain != null) transitionCurtain.gameObject.SetActive(false);

        UpdateLocalizedUI();
    }

    private void Start()
    {
        UpdateLocalizedUI();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateLocalizedUI;
        UpdateLocalizedUI();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateLocalizedUI;
    }

    public void UpdateLocalizedUI()
    {
        if (pauseTitleText != null) pauseTitleText.text = LocalizationManager.PauseTitle;
        if (resumeButtonText != null) resumeButtonText.text = LocalizationManager.ResumeBtn;
        if (pauseReplayButtonText != null) pauseReplayButtonText.text = LocalizationManager.ReplayBtn;
        if (pauseMenuButtonText != null) pauseMenuButtonText.text = LocalizationManager.MenuBtn;

        if (winTitleText != null) winTitleText.text = LocalizationManager.WinTitle;
        if (winSubtitleText != null) winSubtitleText.text = LocalizationManager.WinSubtitle;
        if (loseTitleText != null) loseTitleText.text = LocalizationManager.LoseTitle;
        if (loseSubtitleText != null) loseSubtitleText.text = LocalizationManager.LoseSubtitle;

        if (winNextButtonText != null) winNextButtonText.text = LocalizationManager.NextLevel;
        if (winReplayButtonText != null) winReplayButtonText.text = LocalizationManager.ReplayBtn;
        if (winMenuButtonText != null) winMenuButtonText.text = LocalizationManager.MenuBtn;
        if (loseReplayButtonText != null) loseReplayButtonText.text = LocalizationManager.ReplayBtn;
        if (loseMenuButtonText != null) loseMenuButtonText.text = LocalizationManager.MenuBtn;

        if (winRowLabels != null)
        {
            string[] winKeys = new[] { LocalizationManager.WavesCompleted, LocalizationManager.EnemiesDefeated, LocalizationManager.BaseHp, LocalizationManager.GoldEarned, LocalizationManager.Score };
            for (int i = 0; i < winRowLabels.Length && i < winKeys.Length; i++)
                if (winRowLabels[i] != null) winRowLabels[i].text = winKeys[i];
        }

        if (loseRowLabels != null)
        {
            string[] loseKeys = new[] { LocalizationManager.WaveReached, LocalizationManager.EnemiesDefeated, LocalizationManager.BaseHp, LocalizationManager.Score };
            for (int i = 0; i < loseRowLabels.Length && i < loseKeys.Length; i++)
                if (loseRowLabels[i] != null) loseRowLabels[i].text = loseKeys[i];
        }
    }

    private void Update()
    {
        // Keyboard Shortcuts for Pause (Esc or P)
        if (Keyboard.current != null && !screenShown)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        if (screenShown) return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (screenShown || isPaused) return;

        UpdateLocalizedUI();
        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.color = new Color(0f, 0f, 0f, 0.65f);
        }

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (darkOverlay != null)
            darkOverlay.gameObject.SetActive(false);
    }

    public void ShowWinScreen(ResultData data)
    {
        if (screenShown) return;
        screenShown = true;

        UpdateLocalizedUI();
        if (isPaused) ResumeGame();
        if (hudRoot != null) hudRoot.SetActive(false);

        if (winWavesText != null) winWavesText.text = $"{data.wavesCompleted} / {data.totalWaves}";
        if (winEnemiesText != null) winEnemiesText.text = data.enemiesDefeated.ToString();
        if (winHealthText != null) winHealthText.text = $"{data.baseHealthPercent}%";
        if (winGoldText != null) winGoldText.text = data.goldEarned.ToString();
        if (winScoreText != null) winScoreText.text = data.score.ToString("N0");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            AnimatePanel(winPanel);
        }
    }

    public void ShowLoseScreen(ResultData data)
    {
        if (screenShown) return;
        screenShown = true;

        UpdateLocalizedUI();
        if (isPaused) ResumeGame();
        if (hudRoot != null) hudRoot.SetActive(false);

        if (loseWavesText != null) loseWavesText.text = $"{data.currentWave} / {data.totalWaves}";
        if (loseEnemiesText != null) loseEnemiesText.text = data.enemiesDefeated.ToString();
        if (loseHealthText != null) loseHealthText.text = $"{data.baseHealthPercent}%";
        if (loseScoreText != null) loseScoreText.text = data.score.ToString("N0");

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            AnimatePanel(losePanel);
        }
    }

    private void AnimatePanel(GameObject panel)
    {
        if (darkOverlay != null) darkOverlay.gameObject.SetActive(true);
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(AnimateInRoutine(panel));
    }

    private IEnumerator AnimateInRoutine(GameObject panel)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null) group = panel.AddComponent<CanvasGroup>();

        group.alpha = 0f;
        panel.transform.localScale = Vector3.one * 0.85f;

        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.25f));
            if (darkOverlay != null) darkOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.65f, t));
            group.alpha = t;
            panel.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.85f, Vector3.one, t);
            yield return null;
        }

        group.alpha = 1f;
        panel.transform.localScale = Vector3.one;
        animationRoutine = null;
    }

    public void TransitionTo(System.Action action)
    {
        if (curtainRoutine != null) StopCoroutine(curtainRoutine);
        curtainRoutine = StartCoroutine(FadeCurtain(action));
    }

    private IEnumerator FadeCurtain(System.Action action)
    {
        if (transitionCurtain == null)
        {
            action?.Invoke();
            yield break;
        }

        transitionCurtain.gameObject.SetActive(true);
        Color color = transitionCurtain.color;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Clamp01(elapsed / 0.2f);
            transitionCurtain.color = color;
            yield return null;
        }
        action?.Invoke();
        elapsed = 0f;
        while (elapsed < 0.28f)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = 1f - Mathf.Clamp01(elapsed / 0.28f);
            transitionCurtain.color = color;
            yield return null;
        }
        color.a = 0f;
        transitionCurtain.color = color;
        transitionCurtain.gameObject.SetActive(false);
        curtainRoutine = null;
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void HideMenus()
    {
    }
}
