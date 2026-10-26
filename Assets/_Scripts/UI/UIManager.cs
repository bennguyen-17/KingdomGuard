using System.Collections;
using UnityEngine;
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

    private Coroutine animationRoutine;
    private Coroutine curtainRoutine;
    private bool screenShown;

    private void Awake()
    {
        // Setup Button Listeners
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

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);
        if (transitionCurtain != null) transitionCurtain.gameObject.SetActive(false);
    }

    public void ShowWinScreen(ResultData data)
    {
        if (screenShown) return;
        screenShown = true;

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
