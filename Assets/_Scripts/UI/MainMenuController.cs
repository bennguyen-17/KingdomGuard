using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject guidePanel;

    [Header("Localized Text References")]
    [SerializeField] private Text playButtonText;
    [SerializeField] private Text guideButtonText;
    [SerializeField] private Text quitButtonText;
    [SerializeField] private Text languageButtonText;
    [SerializeField] private Text footerText;
    [SerializeField] private Text guideTitleText;
    [SerializeField] private Text guideContentText;
    [SerializeField] private Text guideCloseButtonText;

    private void Awake()
    {
        // Khi Play game, tự động ẩn GuidePanel nếu trước đó đang bật trong Scene
        if (guidePanel != null && Application.isPlaying)
        {
            guidePanel.SetActive(false);
        }
    }

    private void Start()
    {
        UpdateLanguageUI();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += UpdateLanguageUI;
        UpdateLanguageUI();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateLanguageUI;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Tự động đồng bộ chữ từ LocalizationManager lên Canvas ngay trong Edit Mode khi compile xong code
        if (!Application.isPlaying)
        {
            UpdateLanguageUI();
        }
    }

    [ContextMenu("Đồng bộ chữ từ Code lên Canvas")]
    public void ForceSyncTextInEditor()
    {
        UpdateLanguageUI();
        UnityEditor.EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        Debug.Log("[MainMenuController] Đã đồng bộ chữ từ LocalizationManager lên Canvas thành công!");
    }
#endif

    private void Update()
    {
        // Support Esc key to close guide popup if open
        if (guidePanel != null && guidePanel.activeSelf)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
            {
                CloseGuide();
            }
        }
    }

    public void ToggleLanguage()
    {
        LocalizationManager.ToggleLanguage();
    }

    public void UpdateLanguageUI()
    {
        if (playButtonText != null) playButtonText.text = LocalizationManager.PlayButton;
        if (guideButtonText != null) guideButtonText.text = LocalizationManager.GuideButton;
        if (quitButtonText != null) quitButtonText.text = LocalizationManager.QuitButton;
        if (languageButtonText != null) languageButtonText.text = LocalizationManager.LanguageButton;
        if (footerText != null) footerText.text = LocalizationManager.FooterText;
        if (guideTitleText != null) guideTitleText.text = LocalizationManager.GuideTitle;
        if (guideContentText != null) guideContentText.text = LocalizationManager.GuideContent;
        if (guideCloseButtonText != null) guideCloseButtonText.text = LocalizationManager.GuideClose;
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_1");
    }

    public void OpenGuide()
    {
        if (guidePanel != null)
            guidePanel.SetActive(true);
    }

    public void CloseGuide()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Thoát trò chơi");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
