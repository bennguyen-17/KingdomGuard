using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public struct ResultData
    {
        public int currentWave;
        public int totalWaves;
        public int wavesCompleted;
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

        ApplyResultScreenStyling();
        UpdateLocalizedUI();
    }

    private void Start()
    {
        ApplyResultScreenStyling();
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

        if (winNextButtonText != null) winNextButtonText.text = "▶  " + LocalizationManager.NextLevel;
        if (winReplayButtonText != null) winReplayButtonText.text = "↻  " + LocalizationManager.ReplayBtn;
        if (winMenuButtonText != null) winMenuButtonText.text = "◆  " + LocalizationManager.MenuBtn;
        if (loseReplayButtonText != null) loseReplayButtonText.text = "↻  " + LocalizationManager.ReplayBtn;
        if (loseMenuButtonText != null) loseMenuButtonText.text = "◆  " + LocalizationManager.MenuBtn;

        if (winRowLabels != null)
        {
            string[] winKeys = new[]
            {
                "★  " + LocalizationManager.WavesCompleted,
                "⚔  " + LocalizationManager.EnemiesDefeated,
                "♥  " + LocalizationManager.BaseHp,
                "◆  " + LocalizationManager.GoldEarned,
                "★  " + LocalizationManager.Score
            };
            for (int i = 0; i < winRowLabels.Length && i < winKeys.Length; i++)
                if (winRowLabels[i] != null) winRowLabels[i].text = winKeys[i];
        }

        if (loseRowLabels != null)
        {
            string[] loseKeys = new[]
            {
                "★  " + LocalizationManager.WaveReached,
                "⚔  " + LocalizationManager.EnemiesDefeated,
                "♥  " + LocalizationManager.BaseHp,
                "★  " + LocalizationManager.Score
            };
            for (int i = 0; i < loseRowLabels.Length && i < loseKeys.Length; i++)
                if (loseRowLabels[i] != null) loseRowLabels[i].text = loseKeys[i];
        }
    }

    private void ApplyResultScreenStyling()
    {
        // 1. Style WinPanel
        if (winPanel != null)
        {
            Transform body = winPanel.transform.Find("Body");
            if (body != null)
            {
                var img = body.GetComponent<Image>();
                if (img != null) img.color = new Color(0.08f, 0.11f, 0.16f, 0.96f);
                EnsureOutline(body.gameObject, new Color(0.85f, 0.65f, 0.13f, 0.95f), new Vector2(3f, -3f));
            }

            Transform border = winPanel.transform.Find("Border");
            if (border != null)
            {
                var img = border.GetComponent<Image>();
                if (img != null) img.color = new Color(0.95f, 0.78f, 0.25f, 0.35f);
            }

            Transform banner = winPanel.transform.Find("Banner");
            if (banner != null)
            {
                var img = banner.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white;
                    if (img.sprite == null)
                        img.sprite = Resources.Load<Sprite>("UI/Ribbon_Gold");
                }
            }

            if (winTitleText != null)
            {
                winTitleText.fontSize = 40;
                winTitleText.fontStyle = FontStyle.Bold;
                winTitleText.color = new Color(1f, 0.95f, 0.70f);
                EnsureOutline(winTitleText.gameObject, new Color(0.28f, 0.16f, 0.02f, 1f), new Vector2(2f, -2f));
                EnsureShadow(winTitleText.gameObject, new Color(0f, 0f, 0f, 0.9f), new Vector2(3f, -3f));
            }

            if (winSubtitleText != null)
            {
                winSubtitleText.fontSize = 17;
                winSubtitleText.fontStyle = FontStyle.Bold;
                winSubtitleText.color = new Color(0.98f, 0.98f, 0.94f);
                EnsureShadow(winSubtitleText.gameObject, new Color(0f, 0f, 0f, 0.8f), new Vector2(1.5f, -1.5f));
            }

            StyleButton(winNextButton, new Color(0.16f, 0.65f, 0.35f), new Color(0.22f, 0.80f, 0.42f), new Color(0.10f, 0.45f, 0.22f), new Color(0.08f, 0.35f, 0.18f));
            StyleButton(winReplayButton, new Color(0.80f, 0.32f, 0.20f), new Color(0.95f, 0.42f, 0.28f), new Color(0.60f, 0.22f, 0.12f), new Color(0.45f, 0.15f, 0.08f));
            StyleButton(winMenuButton, new Color(0.22f, 0.30f, 0.40f), new Color(0.32f, 0.44f, 0.58f), new Color(0.15f, 0.20f, 0.28f), new Color(0.12f, 0.16f, 0.24f));
        }

        // 2. Style LosePanel
        if (losePanel != null)
        {
            Transform body = losePanel.transform.Find("Body");
            if (body != null)
            {
                var img = body.GetComponent<Image>();
                if (img != null) img.color = new Color(0.10f, 0.08f, 0.10f, 0.96f);
                EnsureOutline(body.gameObject, new Color(0.75f, 0.15f, 0.15f, 0.95f), new Vector2(3f, -3f));
            }

            Transform border = losePanel.transform.Find("Border");
            if (border != null)
            {
                var img = border.GetComponent<Image>();
                if (img != null) img.color = new Color(0.85f, 0.25f, 0.25f, 0.35f);
            }

            Transform banner = losePanel.transform.Find("Banner");
            if (banner != null)
            {
                var img = banner.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white;
                    if (img.sprite == null)
                        img.sprite = Resources.Load<Sprite>("UI/Ribbon_Red");
                }
            }

            if (loseTitleText != null)
            {
                loseTitleText.fontSize = 40;
                loseTitleText.fontStyle = FontStyle.Bold;
                loseTitleText.color = Color.white;
                EnsureOutline(loseTitleText.gameObject, new Color(0.35f, 0.05f, 0.05f, 1f), new Vector2(2f, -2f));
                EnsureShadow(loseTitleText.gameObject, new Color(0f, 0f, 0f, 0.9f), new Vector2(3f, -3f));
            }

            if (loseSubtitleText != null)
            {
                loseSubtitleText.fontSize = 17;
                loseSubtitleText.fontStyle = FontStyle.Bold;
                loseSubtitleText.color = new Color(0.95f, 0.85f, 0.85f);
                EnsureShadow(loseSubtitleText.gameObject, new Color(0f, 0f, 0f, 0.8f), new Vector2(1.5f, -1.5f));
            }

            StyleButton(loseReplayButton, new Color(0.80f, 0.32f, 0.20f), new Color(0.95f, 0.42f, 0.28f), new Color(0.60f, 0.22f, 0.12f), new Color(0.45f, 0.15f, 0.08f));
            StyleButton(loseMenuButton, new Color(0.22f, 0.30f, 0.40f), new Color(0.32f, 0.44f, 0.58f), new Color(0.15f, 0.20f, 0.28f), new Color(0.12f, 0.16f, 0.24f));
        }

        // 3. Style StatsCard Rows
        StyleStatsCard(winPanel);
        StyleStatsCard(losePanel);
    }

    private void StyleStatsCard(GameObject panel)
    {
        if (panel == null) return;
        Transform card = panel.transform.Find("StatsCard");
        if (card == null) return;

        for (int i = 0; i < card.childCount; i++)
        {
            Transform row = card.GetChild(i);
            var img = row.GetComponent<Image>();
            if (img != null) img.color = new Color(0.12f, 0.16f, 0.22f, 0.82f);
            EnsureOutline(row.gameObject, new Color(0.25f, 0.32f, 0.44f, 0.5f), new Vector2(1f, -1f));

            Transform label = row.Find("Label");
            if (label != null)
            {
                var txt = label.GetComponent<Text>();
                if (txt != null)
                {
                    txt.fontSize = 18;
                    txt.fontStyle = FontStyle.Bold;
                    txt.color = new Color(0.88f, 0.91f, 0.95f);
                    EnsureShadow(label.gameObject, new Color(0f, 0f, 0f, 0.6f), new Vector2(1f, -1f));
                }
            }

            Transform val = row.Find("Value");
            if (val != null)
            {
                var txt = val.GetComponent<Text>();
                if (txt != null)
                {
                    txt.fontSize = 22;
                    txt.fontStyle = FontStyle.Bold;
                    EnsureShadow(val.gameObject, new Color(0f, 0f, 0f, 0.8f), new Vector2(1.5f, -1.5f));
                }
            }
        }
    }

    private void StyleButton(Button btn, Color normal, Color hover, Color pressed, Color outlineCol)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = normal;

        var colors = btn.colors;
        colors.normalColor = normal;
        colors.highlightedColor = hover;
        colors.pressedColor = pressed;
        colors.selectedColor = normal;
        btn.colors = colors;

        EnsureOutline(btn.gameObject, outlineCol, new Vector2(2f, -2f));
        EnsureShadow(btn.gameObject, new Color(0f, 0f, 0f, 0.55f), new Vector2(0f, -3f));

        var txt = btn.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            EnsureShadow(txt.gameObject, new Color(0f, 0f, 0f, 0.75f), new Vector2(1.5f, -1.5f));
        }
    }

    private static void EnsureOutline(GameObject go, Color color, Vector2 dist)
    {
        if (go == null) return;
        var ol = go.GetComponent<Outline>();
        if (ol == null) ol = go.AddComponent<Outline>();
        ol.effectColor = color;
        ol.effectDistance = dist;
        ol.enabled = true;
    }

    private static void EnsureShadow(GameObject go, Color color, Vector2 dist)
    {
        if (go == null) return;
        var sh = go.GetComponent<Shadow>();
        if (sh == null) sh = go.AddComponent<Shadow>();
        sh.effectColor = color;
        sh.effectDistance = dist;
        sh.enabled = true;
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

        if (winWavesText != null) { winWavesText.text = $"{data.wavesCompleted} / {data.totalWaves}"; winWavesText.color = new Color(0.35f, 0.78f, 1f); }
        if (winEnemiesText != null) { winEnemiesText.text = data.enemiesDefeated.ToString(); winEnemiesText.color = new Color(1f, 0.45f, 0.25f); }
        if (winHealthText != null) { winHealthText.text = $"{data.baseHealthPercent}%"; winHealthText.color = new Color(0.25f, 0.88f, 0.45f); }
        if (winGoldText != null) { winGoldText.text = data.goldEarned.ToString(); winGoldText.color = new Color(1f, 0.75f, 0.15f); }
        if (winScoreText != null) { winScoreText.text = data.score.ToString("N0"); winScoreText.color = new Color(1f, 0.84f, 0f); }

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

        if (loseWavesText != null) { loseWavesText.text = $"{data.currentWave} / {data.totalWaves}"; loseWavesText.color = new Color(0.35f, 0.78f, 1f); }
        if (loseEnemiesText != null) { loseEnemiesText.text = data.enemiesDefeated.ToString(); loseEnemiesText.color = new Color(1f, 0.45f, 0.25f); }
        if (loseHealthText != null) { loseHealthText.text = $"{data.baseHealthPercent}%"; loseHealthText.color = new Color(0.95f, 0.35f, 0.35f); }
        if (loseScoreText != null) { loseScoreText.text = data.score.ToString("N0"); loseScoreText.color = new Color(1f, 0.84f, 0f); }

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
            if (darkOverlay != null) darkOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.75f, t));
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
        if (curtainRoutine != null) return;
        curtainRoutine = StartCoroutine(CurtainRoutine(action));
    }

    private IEnumerator CurtainRoutine(System.Action action)
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
