using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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

    private Canvas canvas;
    private Image overlay;
    private ResultPanel winPanel;
    private ResultPanel losePanel;
    private GameObject mainMenuRoot;
    private GameObject levelSelectRoot;
    private CanvasGroup mainMenuGroup;
    private CanvasGroup levelSelectGroup;
    private RectTransform levelGrid;
    private Image transitionCurtain;
    private Coroutine animationRoutine;
    private Coroutine menuTransitionRoutine;
    private Coroutine curtainRoutine;
    private bool screenShown;

    private static readonly Color Navy = new Color32(19, 27, 34, 250);
    private static readonly Color Gold = new Color32(231, 190, 99, 255);
    private static readonly Color Muted = new Color32(184, 193, 194, 255);
    private static Sprite roundedSprite;

    private void Awake()
    {
        BuildUI();
    }

    public void ShowWinScreen(ResultData data)
    {
        if (screenShown) return;
        screenShown = true;
        Show(winPanel, data, true);
    }

    public void ShowLoseScreen(ResultData data)
    {
        if (screenShown) return;
        screenShown = true;
        Show(losePanel, data, false);
    }

#if UNITY_EDITOR
    public void BuildUICanvasInEditor()
    {
        BuildUI();
    }
#endif

    private void BuildUI()
    {
        if (canvas != null) return;
        GameObject canvasObject = new GameObject("KingdomGuard Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventObject.transform.SetParent(canvasObject.transform, false);
        }

        overlay = MakeImage("Dark Overlay", canvasObject.transform, new Color(0.025f, 0.04f, 0.05f, 0f));
        Stretch(overlay.rectTransform);
        overlay.raycastTarget = true;
        overlay.gameObject.SetActive(false);
        winPanel = BuildPanel(true);
        losePanel = BuildPanel(false);
        winPanel.root.SetActive(false);
        losePanel.root.SetActive(false);
        BuildMenus(canvasObject.transform);
        transitionCurtain = MakeImage("Scene Transition", canvasObject.transform, new Color32(13, 22, 25, 255));
        Stretch(transitionCurtain.rectTransform);
        transitionCurtain.raycastTarget = true;
        transitionCurtain.gameObject.SetActive(false);

        if (GameManager.IsLaunchingScene(SceneManager.GetActiveScene().path))
        {
            mainMenuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void BuildMenus(Transform parent)
    {
        mainMenuRoot = CreateMenuRoot("MainMenuPanel", parent, out mainMenuGroup);
        Image atmosphere = MakeImage("Pixel World Tint", mainMenuRoot.transform, new Color32(12, 54, 55, 35));
        Stretch(atmosphere.rectTransform);
        atmosphere.raycastTarget = false;

        Image plaqueShadow = MakeImage("Logo Shadow", mainMenuRoot.transform, new Color32(9, 20, 22, 155));
        SetRect(plaqueShadow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(950f, 176f), new Vector2(0f, 201f));
        Image plaqueFrame = MakeImage("Logo Stone Frame", mainMenuRoot.transform, new Color32(47, 64, 62, 250));
        SetRect(plaqueFrame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(938f, 166f), new Vector2(0f, 207f));
        Image plaqueFace = MakeImage("Logo Parchment", mainMenuRoot.transform, new Color32(190, 168, 119, 248));
        SetRect(plaqueFace.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(916f, 144f), new Vector2(0f, 207f));

        Image topTrim = MakeImage("Pixel Gold Trim", mainMenuRoot.transform, new Color32(224, 187, 99, 255));
        SetRect(topTrim.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(690f, 5f), new Vector2(0f, 267f));
        AddPixelStud(mainMenuRoot.transform, new Vector2(-446f, 207f));
        AddPixelStud(mainMenuRoot.transform, new Vector2(446f, 207f));

        Text logo = MakeText("KingdomGuard Logo", mainMenuRoot.transform, "KINGDOMGUARD", 64, new Color32(247, 226, 170, 255), FontStyle.Bold);
        SetRect(logo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900f, 92f), new Vector2(0f, 213f));
        logo.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3f, -4f);
        Outline logoOutline = logo.gameObject.AddComponent<Outline>();
        logoOutline.effectColor = new Color32(57, 45, 32, 230);
        logoOutline.effectDistance = new Vector2(2f, -2f);

        Text tagline = MakeText("Menu Tagline", mainMenuRoot.transform, "DEFEND THE REALM  /  BUILD YOUR KINGDOM", 17, new Color32(242, 225, 180, 255), FontStyle.Bold);
        SetRect(tagline.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(710f, 38f), new Vector2(0f, 145f));
        tagline.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);

        Button play = MakePixelButton("PLAY", mainMenuRoot.transform, new Vector2(365f, 88f), new Vector2(0f, 25f),
            new Color32(115, 74, 43, 255), new Color32(220, 180, 92, 255), 25);
        play.name = "Play Button";
        play.onClick.AddListener(ShowLevelSelection);

        Text footer = MakeText("Menu Footer", mainMenuRoot.transform, "A PIXEL FANTASY TOWER DEFENSE", 14, new Color32(226, 226, 196, 235), FontStyle.Bold);
        SetRect(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(500f, 34f), new Vector2(0f, 44f));
        footer.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(1f, -2f);

        levelSelectRoot = CreateMenuRoot("LevelSelectionPanel", parent, out levelSelectGroup);
        Image dim = MakeImage("Selection Atmosphere", levelSelectRoot.transform, new Color32(10, 25, 27, 115));
        Stretch(dim.rectTransform);
        dim.raycastTarget = false;
        Image levelFrame = MakeImage("Level Board Frame", levelSelectRoot.transform, new Color32(42, 58, 56, 252));
        SetRect(levelFrame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1020f, 710f), Vector2.zero);
        levelFrame.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -8f);
        Image levelBoard = MakeImage("Level Board", levelSelectRoot.transform, new Color32(50, 66, 61, 248));
        SetRect(levelBoard.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(998f, 688f), Vector2.zero);
        Image boardInset = MakeImage("Parchment Inner Border", levelSelectRoot.transform, new Color32(157, 132, 83, 255));
        SetRect(boardInset.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(942f, 572f), new Vector2(0f, 10f));
        Image boardPaper = MakeImage("Parchment Level Field", levelSelectRoot.transform, new Color32(190, 173, 127, 255));
        SetRect(boardPaper.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(926f, 556f), new Vector2(0f, 10f));

        Text selectTitle = MakeText("Select Level Title", levelSelectRoot.transform, "SELECT LEVEL", 43, new Color32(248, 224, 167, 255), FontStyle.Bold);
        SetRect(selectTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(800f, 64f), new Vector2(0f, 292f));
        selectTitle.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3f, -3f);
        Outline selectOutline = selectTitle.gameObject.AddComponent<Outline>();
        selectOutline.effectColor = new Color32(29, 43, 42, 255);
        selectOutline.effectDistance = new Vector2(2f, -2f);

        GameObject viewportObject = new GameObject("Level Scroll View", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
        viewportObject.transform.SetParent(levelSelectRoot.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        SetRect(viewportRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(850f, 420f), new Vector2(0f, 8f));
        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = new Color32(190, 173, 127, 255);
        viewportImage.raycastTarget = true;
        viewportObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject("Level Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);
        levelGrid = contentObject.GetComponent<RectTransform>();
        levelGrid.anchorMin = new Vector2(0f, 1f);
        levelGrid.anchorMax = new Vector2(1f, 1f);
        levelGrid.pivot = new Vector2(0.5f, 1f);
        levelGrid.anchoredPosition = Vector2.zero;
        levelGrid.sizeDelta = Vector2.zero;
        GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(250f, 126f);
        grid.spacing = new Vector2(24f, 20f);
        grid.padding = new RectOffset(18, 18, 18, 18);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ScrollRect scroll = viewportObject.GetComponent<ScrollRect>();
        scroll.viewport = viewportRect;
        scroll.content = levelGrid;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 35f;

        Button back = MakePixelButton("BACK", levelSelectRoot.transform, new Vector2(250f, 66f), new Vector2(0f, -300f),
            new Color32(80, 75, 58, 255), new Color32(184, 156, 95, 255), 19);
        back.name = "Back Button";
        back.onClick.AddListener(ShowMainMenuFromLevels);

        BuildLevelCards();
        levelSelectRoot.SetActive(false);
    }

    private void BuildLevelCards()
    {
        for (int i = levelGrid.childCount - 1; i >= 0; i--)
            Destroy(levelGrid.GetChild(i).gameObject);

        List<string> scenes = new List<string>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (!string.IsNullOrEmpty(path) && !GameManager.IsMenuScene(path)) scenes.Add(path);
        }

        if (scenes.Count == 0)
        {
            Text empty = MakeText("No Levels Message", levelGrid, "NO LEVELS FOUND IN BUILD SETTINGS", 20, new Color32(72, 62, 45, 255), FontStyle.Bold);
            SetRect(empty.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(720f, 100f), new Vector2(0f, -120f));
            return;
        }

        for (int i = 0; i < scenes.Count; i++)
        {
            string scenePath = scenes[i];
            int levelNumber = i + 1;
            bool unlocked = GameManager.IsLevelUnlocked(levelNumber);
            bool completed = GameManager.IsLevelCompleted(scenePath);
            Color face = !unlocked ? new Color32(77, 78, 68, 255) : completed ? new Color32(91, 114, 71, 255) : new Color32(112, 78, 48, 255);
            Color edge = !unlocked ? new Color32(108, 105, 87, 255) : new Color32(225, 187, 105, 255);
            Button card = MakePixelButton("LEVEL " + levelNumber, levelGrid, new Vector2(250f, 126f), Vector2.zero, face, edge, 21);
            card.interactable = unlocked;
            card.GetComponent<RectTransform>().localScale = Vector3.one;
            Text cardTitle = card.GetComponentInChildren<Text>();
            SetRect(cardTitle.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(-8f, -22f), new Vector2(0f, 1f));
            cardTitle.text = levelNumber.ToString("00");
            cardTitle.fontSize = 31;
            Text cardStatus = MakeText("Level Status", card.transform, completed ? "CLEARED" : unlocked ? "ENTER REALM" : "LOCKED", 14,
                unlocked ? new Color32(248, 224, 167, 255) : new Color32(176, 168, 143, 255), FontStyle.Bold);
            SetRect(cardStatus.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.5f), new Vector2(-8f, -14f), new Vector2(0f, 0f));
            if (unlocked) card.onClick.AddListener(() => GameManager.Instance?.StartSelectedLevel(scenePath));
        }
    }

    private static GameObject CreateMenuRoot(string name, Transform parent, out CanvasGroup group)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());
        group = root.GetComponent<CanvasGroup>();
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
        return root;
    }

    private static void AddPixelStud(Transform parent, Vector2 position)
    {
        Image stud = MakeImage("Pixel Brass Stud", parent, new Color32(222, 186, 106, 255));
        SetRect(stud.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(13f, 13f), position);
    }

    private static Button MakePixelButton(string label, Transform parent, Vector2 size, Vector2 position, Color faceColor, Color borderColor, int fontSize)
    {
        Image frame = MakeImage(label + " Pixel Frame", parent, borderColor);
        SetRect(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position);
        Button button = frame.gameObject.AddComponent<Button>();
        button.targetGraphic = frame;
        ColorBlock colors = button.colors;
        colors.normalColor = borderColor;
        colors.highlightedColor = Color.Lerp(borderColor, Color.white, 0.28f);
        colors.pressedColor = Color.Lerp(borderColor, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        button.transition = Selectable.Transition.ColorTint;
        PixelButtonMotion motion = frame.gameObject.AddComponent<PixelButtonMotion>();
        motion.enabled = true;

        Image face = MakeImage("Wooden Face", frame.transform, faceColor);
        SetRect(face.rectTransform, Vector2.zero, Vector2.one, new Vector2(-8f, -8f), Vector2.zero);
        face.raycastTarget = false;
        Image highlight = MakeImage("Top Pixel Highlight", frame.transform, Color.Lerp(faceColor, Color.white, 0.19f));
        SetRect(highlight.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-12f, 4f), new Vector2(0f, -7f));
        highlight.raycastTarget = false;
        Text text = MakeText("Button Label", frame.transform, label, fontSize, new Color32(255, 239, 198, 255), FontStyle.Bold);
        Stretch(text.rectTransform);
        text.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color32(44, 34, 26, 225);
        outline.effectDistance = new Vector2(1f, -1f);
        return button;
    }

    public void ShowMainMenu()
    {
        if (mainMenuRoot == null) return;
        StopMenuTransition();
        winPanel.root.SetActive(false);
        losePanel.root.SetActive(false);
        overlay.gameObject.SetActive(false);
        transitionCurtain.gameObject.SetActive(false);
        mainMenuRoot.SetActive(true);
        levelSelectRoot.SetActive(false);
        mainMenuGroup.alpha = 1f;
        mainMenuGroup.interactable = true;
        mainMenuGroup.blocksRaycasts = true;
        screenShown = false;
    }

    public void HideMenus()
    {
        if (mainMenuRoot == null) return;
        StopMenuTransition();
        mainMenuRoot.SetActive(false);
        levelSelectRoot.SetActive(false);
    }

    private void ShowLevelSelection()
    {
        if (menuTransitionRoutine != null) StopCoroutine(menuTransitionRoutine);
        BuildLevelCards();
        menuTransitionRoutine = StartCoroutine(SwitchMenuPanels(mainMenuRoot, mainMenuGroup, levelSelectRoot, levelSelectGroup));
    }

    private void ShowMainMenuFromLevels()
    {
        if (menuTransitionRoutine != null) StopCoroutine(menuTransitionRoutine);
        menuTransitionRoutine = StartCoroutine(SwitchMenuPanels(levelSelectRoot, levelSelectGroup, mainMenuRoot, mainMenuGroup));
    }

    private IEnumerator SwitchMenuPanels(GameObject fromRoot, CanvasGroup from, GameObject toRoot, CanvasGroup to)
    {
        toRoot.SetActive(true);
        to.alpha = 0f;
        to.interactable = false;
        to.blocksRaycasts = false;
        float elapsed = 0f;
        while (elapsed < 0.22f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.22f);
            from.alpha = 1f - t;
            to.alpha = t;
            yield return null;
        }
        fromRoot.SetActive(false);
        from.alpha = 1f;
        from.interactable = true;
        from.blocksRaycasts = true;
        to.alpha = 1f;
        to.interactable = true;
        to.blocksRaycasts = true;
        menuTransitionRoutine = null;
    }

    public void TransitionTo(System.Action action)
    {
        if (curtainRoutine != null) StopCoroutine(curtainRoutine);
        curtainRoutine = StartCoroutine(FadeCurtain(action));
    }

    private IEnumerator FadeCurtain(System.Action action)
    {
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
        if (GameManager.Instance != null && GameManager.Instance.IsGameplayActive)
        {
            mainMenuRoot.SetActive(false);
            levelSelectRoot.SetActive(false);
        }
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

    private void StopMenuTransition()
    {
        if (menuTransitionRoutine != null)
        {
            StopCoroutine(menuTransitionRoutine);
            menuTransitionRoutine = null;
        }
    }

    private ResultPanel BuildPanel(bool won)
    {
        Color accent = won ? new Color32(128, 190, 116, 255) : new Color32(208, 99, 72, 255);
        Color mainButton = won ? new Color32(76, 122, 75, 255) : new Color32(153, 66, 53, 255);
        GameObject root = new GameObject(won ? "WinPanel" : "LosePanel", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(760f, 625f);

        Image shadow = MakeRoundedImage("Panel Shadow", root.transform, new Color(0f, 0f, 0f, 0.48f));
        Stretch(shadow.rectTransform);
        shadow.rectTransform.anchoredPosition = new Vector2(0f, -9f);
        shadow.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -5f);

        Image border = MakeRoundedImage("Decorative Border", root.transform, won ? new Color32(114, 98, 61, 255) : new Color32(105, 57, 49, 255));
        Stretch(border.rectTransform);
        Image panel = MakeRoundedImage("Panel", root.transform, Navy);
        SetRect(panel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(744f, 609f), new Vector2(0f, 0f));
        panel.rectTransform.anchorMin = panel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        panel.rectTransform.anchoredPosition = Vector2.zero;

        Image topAccent = MakeImage("Top Accent", root.transform, accent);
        SetRect(topAccent.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(240f, 5f), new Vector2(0f, -4f));

        Text title = MakeText("Result Title", root.transform, won ? "VICTORY!" : "DEFEAT", 54, won ? Gold : new Color32(235, 128, 95, 255), FontStyle.Bold);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700f, 74f), new Vector2(0f, -34f));

        Text subtitle = MakeText("Subtitle", root.transform, won ? "All enemies have been defeated!" : "The enemy has broken through your defenses.", 19, Muted, FontStyle.Normal);
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(690f, 36f), new Vector2(0f, -100f));

        Image divider = MakeImage("Header Divider", root.transform, new Color(accent.r, accent.g, accent.b, 0.55f));
        SetRect(divider.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(580f, 1.5f), new Vector2(0f, -148f));

        string[] labels = won
            ? new[] { "WAVES COMPLETED", "ENEMIES DEFEATED", "BASE HP", "GOLD EARNED", "SCORE" }
            : new[] { "WAVE REACHED", "ENEMIES DEFEATED", "BASE HP", "SCORE" };
        int rowCount = labels.Length;
        float firstY = won ? -184f : -196f;
        float spacing = won ? 55f : 62f;
        Text[] values = new Text[rowCount];
        CanvasGroup statsGroup = new GameObject("Statistics", typeof(RectTransform), typeof(CanvasGroup)).GetComponent<CanvasGroup>();
        statsGroup.transform.SetParent(root.transform, false);
        Stretch((RectTransform)statsGroup.transform);
        for (int i = 0; i < rowCount; i++)
        {
            float y = firstY - i * spacing;
            Text label = MakeText(labels[i], statsGroup.transform, labels[i], 16, Muted, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleLeft;
            SetRect(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(380f, 38f), new Vector2(-90f, y));
            Text value = MakeText("Value " + labels[i], statsGroup.transform, "—", 19, Color.white, FontStyle.Bold);
            value.alignment = TextAnchor.MiddleRight;
            SetRect(value.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(260f, 38f), new Vector2(165f, y));
            values[i] = value;
        }

        CanvasGroup buttonGroup = new GameObject("Actions", typeof(RectTransform), typeof(CanvasGroup)).GetComponent<CanvasGroup>();
        buttonGroup.transform.SetParent(root.transform, false);
        Stretch((RectTransform)buttonGroup.transform);
        string[] buttonLabels = won ? new[] { "NEXT LEVEL", "REPLAY", "MAIN MENU" } : new[] { "RETRY", "MAIN MENU" };
        float buttonY = -548f;
        float buttonWidth = won ? 204f : 254f;
        float buttonGap = 16f;
        float totalWidth = buttonLabels.Length * buttonWidth + (buttonLabels.Length - 1) * buttonGap;
        for (int i = 0; i < buttonLabels.Length; i++)
        {
            bool primary = i == 0;
            Color normal = primary ? mainButton : new Color32(50, 62, 67, 255);
            Button button = MakeButton(buttonLabels[i], buttonGroup.transform, normal, primary ? Gold : new Color32(109, 123, 125, 255), buttonWidth, 58f);
            float x = -totalWidth * 0.5f + buttonWidth * 0.5f + i * (buttonWidth + buttonGap);
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(buttonWidth, 58f), new Vector2(x, buttonY));
            if (won && i == 0) button.onClick.AddListener(() => GameManager.Instance?.LoadNextLevel());
            else if (won && i == 1) button.onClick.AddListener(() => GameManager.Instance?.ReplayLevel());
            else if (!won && i == 0) button.onClick.AddListener(() => GameManager.Instance?.ReplayLevel());
            else button.onClick.AddListener(() => GameManager.Instance?.LoadMainMenu());
        }

        Text[] orderedValues = values;
        return new ResultPanel(root, rootRect, title, statsGroup, buttonGroup, orderedValues, won);
    }

    private void Show(ResultPanel panel, ResultData data, bool won)
    {
        if (canvas == null) BuildUI();
        mainMenuRoot.SetActive(false);
        levelSelectRoot.SetActive(false);
        winPanel.root.SetActive(panel == winPanel);
        losePanel.root.SetActive(panel == losePanel);
        panel.SetValues(data);
        overlay.gameObject.SetActive(true);
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(AnimateIn(panel));
    }

    private IEnumerator AnimateIn(ResultPanel panel)
    {
        CanvasGroup group = panel.root.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        panel.rootRect.localScale = Vector3.one * 0.82f;
        panel.title.rectTransform.localScale = Vector3.one * 0.92f;
        panel.stats.alpha = 0f;
        panel.buttons.alpha = 0f;
        Color overlayColor = overlay.color;
        float elapsed = 0f;
        while (elapsed < 0.28f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.28f));
            overlay.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, Mathf.Lerp(0f, 0.68f, t));
            group.alpha = t;
            panel.rootRect.localScale = Vector3.LerpUnclamped(Vector3.one * 0.82f, Vector3.one, t);
            yield return null;
        }
        panel.rootRect.localScale = Vector3.one;
        yield return FadeGroup(panel.stats, 0.16f);
        panel.title.rectTransform.localScale = Vector3.one;
        yield return FadeGroup(panel.buttons, 0.16f);
        animationRoutine = null;
    }

    private static IEnumerator FadeGroup(CanvasGroup group, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        group.alpha = 1f;
    }

    private static Image MakeImage(string name, Transform parent, Color color)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(Image));
        item.transform.SetParent(parent, false);
        Image image = item.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Image MakeRoundedImage(string name, Transform parent, Color color)
    {
        if (roundedSprite == null)
        {
            const int size = 32;
            const float radius = 7f;
            Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            texture.name = "Runtime Rounded UI Sprite";
            texture.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = x < radius ? radius : (x >= size - radius ? size - radius - 1 : x);
                float cy = y < radius ? radius : (y >= size - radius ? size - radius - 1 : y);
                bool corner = (x < radius || x >= size - radius) && (y < radius || y >= size - radius);
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                texture.SetPixel(x, y, !corner || distance <= radius ? Color.white : new Color(1f, 1f, 1f, 0f));
            }
            texture.Apply();
            roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(9f, 9f, 9f, 9f));
            roundedSprite.name = "Runtime Rounded UI Sprite";
        }

        Image image = MakeImage(name, parent, color);
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        return image;
    }

    private static Text MakeText(string name, Transform parent, string content, int size, Color color, FontStyle style)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(Text));
        item.transform.SetParent(parent, false);
        Text text = item.GetComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static Button MakeButton(string label, Transform parent, Color normal, Color border, float width, float height)
    {
        Image frame = MakeRoundedImage(label + " Frame", parent, border);
        Button button = frame.gameObject.AddComponent<Button>();
        button.targetGraphic = frame;
        ColorBlock colors = button.colors;
        colors.normalColor = border;
        colors.highlightedColor = Color.Lerp(border, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(border, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        Shadow shadow = frame.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -3f);
        Image face = MakeRoundedImage("Face", frame.transform, normal);
        SetRect(face.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-4f, -4f), Vector2.zero);
        face.raycastTarget = false;
        Text text = MakeText("Label", frame.transform, label, 16, Color.white, FontStyle.Bold);
        Stretch(text.rectTransform);
        text.rectTransform.offsetMin = new Vector2(4f, 2f);
        text.rectTransform.offsetMax = new Vector2(-4f, -2f);
        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private sealed class ResultPanel
    {
        public readonly GameObject root;
        public readonly RectTransform rootRect;
        public readonly Text title;
        public readonly CanvasGroup stats;
        public readonly CanvasGroup buttons;
        private readonly Text[] values;
        private readonly bool isWin;

        public ResultPanel(GameObject root, RectTransform rootRect, Text title, CanvasGroup stats, CanvasGroup buttons, Text[] values, bool isWin)
        {
            this.root = root;
            this.rootRect = rootRect;
            this.title = title;
            this.stats = stats;
            this.buttons = buttons;
            this.values = values;
            this.isWin = isWin;
        }

        public void SetValues(ResultData data)
        {
            int row = 0;
            if (isWin)
            {
                values[row++].text = data.wavesCompleted + " / " + data.totalWaves;
                values[row++].text = data.enemiesDefeated.ToString();
                values[row++].text = data.baseHealthPercent + "%";
                values[row++].text = "+" + data.goldEarned;
                values[row].text = data.score.ToString("N0");
            }
            else
            {
                values[row++].text = data.currentWave + " / " + data.totalWaves;
                values[row++].text = data.enemiesDefeated.ToString();
                values[row++].text = data.baseHealthPercent + "%";
                values[row].text = data.score.ToString("N0");
            }
        }
    }
}

public class PixelButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 targetScale = Vector3.one;

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 18f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Button button = GetComponent<Button>();
        if (button != null && button.interactable) targetScale = Vector3.one * 1.035f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Button button = GetComponent<Button>();
        if (button != null && button.interactable) targetScale = Vector3.one * 0.97f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Button button = GetComponent<Button>();
        if (button != null && button.interactable) targetScale = Vector3.one * 1.035f;
    }
}
