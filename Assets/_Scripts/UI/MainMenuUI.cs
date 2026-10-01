using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EnsureMainMenuBuilt();
    }

    private void Start()
    {
        Time.timeScale = 1f;

    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded("Game"))
        {
            SceneManager.LoadScene("Game");
        }
        else if (SceneManager.sceneCountInBuildSettings > 1)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenuUI] Quitting game application...");
        Application.Quit();
    }

    public void SetBackgroundVisible(bool visible)
    {
        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(visible);
    }

    #region Auto-Builder for MainMenu UI
    [ContextMenu("Build Main Menu UI")]
    public void BuildMainMenuInEditor()
    {
        EnsureMainMenuBuilt(forceRebuild: true);
    }

    private void EnsureMainMenuBuilt(bool forceRebuild = false)
    {
        if (!forceRebuild && backgroundImage != null)
            return;

        // 1. Canvas setup
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("MainMenuCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        menuCanvas = canvas;

        // Remove the old menu content while keeping the background and canvas.
        RemoveMenuObject(canvas.transform, "LogoImage");
        RemoveMenuObject(canvas.transform, "TitleText");
        RemoveMenuObject(canvas.transform, "PlayButton");
        RemoveMenuObject(canvas.transform, "QuitButton");
        titleText = null;
        playButton = null;
        quitButton = null;

        // 2. Background Image
        Transform bgTrans = canvas.transform.Find("Background");
        GameObject bgGO;
        if (bgTrans == null)
        {
            bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvas.transform, false);
            bgGO.transform.SetAsFirstSibling();
        }
        else
        {
            bgGO = bgTrans.gameObject;
        }

        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        if (bgRT == null) bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        backgroundImage = bgGO.GetComponent<Image>();
        if (backgroundImage == null) backgroundImage = bgGO.AddComponent<Image>();
        backgroundImage.color = Color.white;

#if UNITY_EDITOR
        string bgPath = "Assets/Art/UI/MainMenu_BG.jpg";
        Sprite bgSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
        if (bgSprite != null) backgroundImage.sprite = bgSprite;

        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }

    private static void RemoveMenuObject(Transform parent, string objectName)
    {
        Transform menuObject = parent.Find(objectName);
        if (menuObject == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying) DestroyImmediate(menuObject.gameObject);
        else Destroy(menuObject.gameObject);
#else
        Destroy(menuObject.gameObject);
#endif
    }
    #endregion
}
