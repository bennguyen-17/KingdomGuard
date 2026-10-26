using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[InitializeOnLoad]
public static class UIPrefabBaker
{
    private static readonly Color32 Navy = new Color32(19, 27, 34, 250);
    private static readonly Color32 Gold = new Color32(231, 190, 99, 255);
    private static readonly Color32 Muted = new Color32(184, 193, 194, 255);

    static UIPrefabBaker()
    {
        EditorApplication.delayCall += CheckAndBake;
    }

    private static void CheckAndBake()
    {
        string prefabPath = "Assets/Prefabs/GameCanvas.prefab";
        if (!File.Exists(prefabPath))
        {
            BakeGameCanvasPrefab();
        }
    }

    [MenuItem("Kingdom Guard/Tạo Prefab UI Chuẩn (Bake GameCanvas)")]
    public static void BakeGameCanvasPrefab()
    {
        string prefabDir = "Assets/Prefabs";
        if (!Directory.Exists(prefabDir))
            Directory.CreateDirectory(prefabDir);

        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Icon_Coin.png");
        Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Icon_Heart.png");
        Sprite waveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Icon_Wave.png");
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 1. Root Canvas
        GameObject canvasRoot = new GameObject("GameCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        UIManager uiManager = canvasRoot.AddComponent<UIManager>();

        // EventSystem if not present
        GameObject esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        esObj.transform.SetParent(canvasRoot.transform, false);

        // 2. InGameHUD Root
        GameObject hudRoot = new GameObject("InGameHUD", typeof(RectTransform));
        hudRoot.transform.SetParent(canvasRoot.transform, false);
        Stretch(hudRoot.GetComponent<RectTransform>());
        HUDController hudCtrl = hudRoot.AddComponent<HUDController>();

        // 2.1 Gold Badge
        GameObject goldBadge = new GameObject("GoldBadge", typeof(RectTransform), typeof(Image), typeof(Outline));
        goldBadge.transform.SetParent(hudRoot.transform, false);
        SetRect(goldBadge.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(210f, 56f), new Vector2(24f, -20f), new Vector2(0f, 1f));
        goldBadge.GetComponent<Image>().color = new Color32(23, 32, 42, 240);
        Outline goldOutline = goldBadge.GetComponent<Outline>();
        goldOutline.effectColor = new Color32(231, 190, 99, 220);
        goldOutline.effectDistance = new Vector2(2f, -2f);

        GameObject coinObj = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
        coinObj.transform.SetParent(goldBadge.transform, false);
        SetRect(coinObj.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44f, 44f), new Vector2(10f, 0f), new Vector2(0f, 0.5f));
        Image coinImg = coinObj.GetComponent<Image>();
        coinImg.sprite = coinSprite;
        coinImg.preserveAspect = true;

        GameObject goldTextObj = new GameObject("GoldText", typeof(RectTransform), typeof(Text), typeof(Shadow));
        goldTextObj.transform.SetParent(goldBadge.transform, false);
        RectTransform gtRect = goldTextObj.GetComponent<RectTransform>();
        gtRect.anchorMin = new Vector2(0f, 0f);
        gtRect.anchorMax = new Vector2(1f, 1f);
        gtRect.offsetMin = new Vector2(62f, 0f);
        gtRect.offsetMax = new Vector2(-8f, 0f);
        Text goldTxt = goldTextObj.GetComponent<Text>();
        goldTxt.font = font;
        goldTxt.text = "200";
        goldTxt.fontSize = 28;
        goldTxt.fontStyle = FontStyle.Bold;
        goldTxt.color = new Color32(255, 215, 0, 255);
        goldTxt.alignment = TextAnchor.MiddleLeft;
        Shadow goldShadow = goldTextObj.GetComponent<Shadow>();
        goldShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        goldShadow.effectDistance = new Vector2(2f, -2f);

        // 2.2 Lives Badge
        GameObject livesBadge = new GameObject("LivesBadge", typeof(RectTransform), typeof(Image), typeof(Outline));
        livesBadge.transform.SetParent(hudRoot.transform, false);
        SetRect(livesBadge.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(210f, 56f), new Vector2(248f, -20f), new Vector2(0f, 1f));
        livesBadge.GetComponent<Image>().color = new Color32(23, 32, 42, 240);
        Outline livesOutline = livesBadge.GetComponent<Outline>();
        livesOutline.effectColor = new Color32(217, 83, 79, 220);
        livesOutline.effectDistance = new Vector2(2f, -2f);

        GameObject heartObj = new GameObject("HeartIcon", typeof(RectTransform), typeof(Image));
        heartObj.transform.SetParent(livesBadge.transform, false);
        SetRect(heartObj.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44f, 44f), new Vector2(10f, 0f), new Vector2(0f, 0.5f));
        Image heartImg = heartObj.GetComponent<Image>();
        heartImg.sprite = heartSprite;
        heartImg.preserveAspect = true;

        GameObject livesTextObj = new GameObject("LivesText", typeof(RectTransform), typeof(Text), typeof(Shadow));
        livesTextObj.transform.SetParent(livesBadge.transform, false);
        RectTransform ltRect = livesTextObj.GetComponent<RectTransform>();
        ltRect.anchorMin = new Vector2(0f, 0f);
        ltRect.anchorMax = new Vector2(1f, 1f);
        ltRect.offsetMin = new Vector2(60f, 0f);
        ltRect.offsetMax = new Vector2(-8f, 0f);
        Text livesTxt = livesTextObj.GetComponent<Text>();
        livesTxt.font = font;
        livesTxt.text = "20 / 20";
        livesTxt.fontSize = 24;
        livesTxt.fontStyle = FontStyle.Bold;
        livesTxt.color = Color.white;
        livesTxt.alignment = TextAnchor.MiddleLeft;
        Shadow livesShadow = livesTextObj.GetComponent<Shadow>();
        livesShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        livesShadow.effectDistance = new Vector2(2f, -2f);

        // 2.3 Wave Badge
        GameObject waveBadge = new GameObject("WaveBadge", typeof(RectTransform), typeof(Image), typeof(Outline));
        waveBadge.transform.SetParent(hudRoot.transform, false);
        SetRect(waveBadge.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(270f, 58f), new Vector2(0f, -20f), new Vector2(0.5f, 1f));
        waveBadge.GetComponent<Image>().color = new Color32(20, 27, 34, 245);
        Outline waveOutline = waveBadge.GetComponent<Outline>();
        waveOutline.effectColor = new Color32(231, 190, 99, 230);
        waveOutline.effectDistance = new Vector2(2f, -2f);

        GameObject waveIconObj = new GameObject("WaveIcon", typeof(RectTransform), typeof(Image));
        waveIconObj.transform.SetParent(waveBadge.transform, false);
        SetRect(waveIconObj.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 40f), new Vector2(12f, 0f), new Vector2(0f, 0.5f));
        Image waveImg = waveIconObj.GetComponent<Image>();
        waveImg.sprite = waveSprite;
        waveImg.preserveAspect = true;

        GameObject waveTitleObj = new GameObject("WaveTitleText", typeof(RectTransform), typeof(Text));
        waveTitleObj.transform.SetParent(waveBadge.transform, false);
        RectTransform wtRect = waveTitleObj.GetComponent<RectTransform>();
        wtRect.anchorMin = new Vector2(0f, 0.45f);
        wtRect.anchorMax = new Vector2(1f, 1f);
        wtRect.offsetMin = new Vector2(62f, 0f);
        wtRect.offsetMax = new Vector2(-10f, -3f);
        Text waveTitleTxt = waveTitleObj.GetComponent<Text>();
        waveTitleTxt.font = font;
        waveTitleTxt.text = "WAVE 1 / 6";
        waveTitleTxt.fontSize = 21;
        waveTitleTxt.fontStyle = FontStyle.Bold;
        waveTitleTxt.color = new Color32(255, 235, 167, 255);
        waveTitleTxt.alignment = TextAnchor.UpperLeft;

        GameObject waveSubObj = new GameObject("WaveSubText", typeof(RectTransform), typeof(Text));
        waveSubObj.transform.SetParent(waveBadge.transform, false);
        RectTransform wsRect = waveSubObj.GetComponent<RectTransform>();
        wsRect.anchorMin = new Vector2(0f, 0f);
        wsRect.anchorMax = new Vector2(1f, 0.55f);
        wsRect.offsetMin = new Vector2(62f, 3f);
        wsRect.offsetMax = new Vector2(-10f, 0f);
        Text waveSubTxt = waveSubObj.GetComponent<Text>();
        waveSubTxt.font = font;
        waveSubTxt.text = "Giai đoạn chuẩn bị";
        waveSubTxt.fontSize = 14;
        waveSubTxt.color = new Color32(189, 195, 199, 255);
        waveSubTxt.alignment = TextAnchor.LowerLeft;

        // 2.4 Wave Action Button
        GameObject actionBtnObj = new GameObject("WaveActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        actionBtnObj.transform.SetParent(hudRoot.transform, false);
        SetRect(actionBtnObj.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(310f, 56f), new Vector2(-24f, -20f), new Vector2(1f, 1f));
        Image btnBorder = actionBtnObj.GetComponent<Image>();
        btnBorder.color = new Color32(180, 140, 50, 255);
        Button actionBtn = actionBtnObj.GetComponent<Button>();

        GameObject faceObj = new GameObject("Face", typeof(RectTransform), typeof(Image));
        faceObj.transform.SetParent(actionBtnObj.transform, false);
        Stretch(faceObj.GetComponent<RectTransform>());
        faceObj.GetComponent<RectTransform>().offsetMin = new Vector2(3f, 3f);
        faceObj.GetComponent<RectTransform>().offsetMax = new Vector2(-3f, -3f);
        Image btnFace = faceObj.GetComponent<Image>();
        btnFace.color = new Color32(46, 125, 50, 255);
        btnFace.raycastTarget = false;

        GameObject btnTxtObj = new GameObject("BtnText", typeof(RectTransform), typeof(Text), typeof(Shadow));
        btnTxtObj.transform.SetParent(actionBtnObj.transform, false);
        Stretch(btnTxtObj.GetComponent<RectTransform>());
        Text actionBtnTxt = btnTxtObj.GetComponent<Text>();
        actionBtnTxt.font = font;
        actionBtnTxt.text = "▶ BẮT ĐẦU WAVE 1 (Space)";
        actionBtnTxt.fontSize = 17;
        actionBtnTxt.fontStyle = FontStyle.Bold;
        actionBtnTxt.color = Color.white;
        actionBtnTxt.alignment = TextAnchor.MiddleCenter;
        Shadow btnShadow = btnTxtObj.GetComponent<Shadow>();
        btnShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        btnShadow.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject tipObj = new GameObject("TipText", typeof(RectTransform), typeof(Text), typeof(Shadow));
        tipObj.transform.SetParent(hudRoot.transform, false);
        SetRect(tipObj.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(450f, 24f), new Vector2(-24f, -82f), new Vector2(1f, 1f));
        Text tipTxt = tipObj.GetComponent<Text>();
        tipTxt.font = font;
        tipTxt.text = "💡 Hãy đặt tháp phòng thủ trước khi bấm Bắt đầu!";
        tipTxt.fontSize = 14;
        tipTxt.fontStyle = FontStyle.Bold;
        tipTxt.color = new Color32(255, 225, 120, 255);
        tipTxt.alignment = TextAnchor.MiddleRight;
        Shadow tipShadow = tipObj.GetComponent<Shadow>();
        tipShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        tipShadow.effectDistance = new Vector2(1f, -1f);

        // Assign fields to HUDController via SerializedObject
        SerializedObject hudSo = new SerializedObject(hudCtrl);
        hudSo.FindProperty("goldText").objectReferenceValue = goldTxt;
        hudSo.FindProperty("livesText").objectReferenceValue = livesTxt;
        hudSo.FindProperty("waveTitleText").objectReferenceValue = waveTitleTxt;
        hudSo.FindProperty("waveSubText").objectReferenceValue = waveSubTxt;
        hudSo.FindProperty("waveActionButton").objectReferenceValue = actionBtn;
        hudSo.FindProperty("waveActionButtonText").objectReferenceValue = actionBtnTxt;
        hudSo.FindProperty("waveActionButtonFace").objectReferenceValue = btnFace;
        hudSo.FindProperty("tipText").objectReferenceValue = tipTxt;
        hudSo.ApplyModifiedProperties();

        // 3. Dark Overlay
        GameObject overlayObj = new GameObject("DarkOverlay", typeof(RectTransform), typeof(Image));
        overlayObj.transform.SetParent(canvasRoot.transform, false);
        Stretch(overlayObj.GetComponent<RectTransform>());
        Image overlayImg = overlayObj.GetComponent<Image>();
        overlayImg.color = new Color(0.02f, 0.03f, 0.04f, 0.65f);
        overlayObj.SetActive(false);

        // 4. Win Panel
        GameObject winPanel = BuildResultPanel(canvasRoot.transform, font, true, out Text[] winStats, out Button[] winBtns);
        winPanel.SetActive(false);

        // 5. Lose Panel
        GameObject losePanel = BuildResultPanel(canvasRoot.transform, font, false, out Text[] loseStats, out Button[] loseBtns);
        losePanel.SetActive(false);

        // 6. Transition Curtain
        GameObject curtainObj = new GameObject("TransitionCurtain", typeof(RectTransform), typeof(Image));
        curtainObj.transform.SetParent(canvasRoot.transform, false);
        Stretch(curtainObj.GetComponent<RectTransform>());
        Image curtainImg = curtainObj.GetComponent<Image>();
        curtainImg.color = new Color32(13, 22, 25, 255);
        curtainObj.SetActive(false);

        // Assign fields to UIManager via SerializedObject
        SerializedObject uiSo = new SerializedObject(uiManager);
        uiSo.FindProperty("hudRoot").objectReferenceValue = hudRoot;
        uiSo.FindProperty("winPanel").objectReferenceValue = winPanel;
        uiSo.FindProperty("losePanel").objectReferenceValue = losePanel;
        uiSo.FindProperty("darkOverlay").objectReferenceValue = overlayImg;
        uiSo.FindProperty("transitionCurtain").objectReferenceValue = curtainImg;

        uiSo.FindProperty("winWavesText").objectReferenceValue = winStats[0];
        uiSo.FindProperty("winEnemiesText").objectReferenceValue = winStats[1];
        uiSo.FindProperty("winHealthText").objectReferenceValue = winStats[2];
        uiSo.FindProperty("winGoldText").objectReferenceValue = winStats[3];
        uiSo.FindProperty("winScoreText").objectReferenceValue = winStats[4];

        uiSo.FindProperty("loseWavesText").objectReferenceValue = loseStats[0];
        uiSo.FindProperty("loseEnemiesText").objectReferenceValue = loseStats[1];
        uiSo.FindProperty("loseHealthText").objectReferenceValue = loseStats[2];
        uiSo.FindProperty("loseScoreText").objectReferenceValue = loseStats[3];

        uiSo.FindProperty("winNextButton").objectReferenceValue = winBtns[0];
        uiSo.FindProperty("winReplayButton").objectReferenceValue = winBtns[1];
        uiSo.FindProperty("winMenuButton").objectReferenceValue = winBtns[2];

        uiSo.FindProperty("loseReplayButton").objectReferenceValue = loseBtns[0];
        uiSo.FindProperty("loseMenuButton").objectReferenceValue = loseBtns[1];
        uiSo.ApplyModifiedProperties();

        // Save as Prefab!
        string savePath = "Assets/Prefabs/GameCanvas.prefab";
        string resPath = "Assets/Resources/GameCanvas.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasRoot, savePath);
        PrefabUtility.SaveAsPrefabAsset(canvasRoot, resPath);
        Object.DestroyImmediate(canvasRoot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[UIPrefabBaker] Đã tạo thành công Prefab UI trực quan tại: {savePath} và {resPath}");

        // Now put instance into current active scene if Level scene
        InsertPrefabIntoActiveScene(savePath);
    }

    private static void InsertPrefabIntoActiveScene(string prefabPath)
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        if (!activeScene.name.StartsWith("Level_")) return;

        // Check if GameCanvas already in scene
        var existing = GameObject.Find("GameCanvas");
        if (existing != null)
            Object.DestroyImmediate(existing);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "GameCanvas";
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log($"[UIPrefabBaker] Đã tự động chèn GameCanvas vào Scene: {activeScene.name}");
        }
    }

    private static GameObject BuildResultPanel(Transform parent, Font font, bool isWin, out Text[] statTexts, out Button[] buttons)
    {
        string name = isWin ? "WinPanel" : "LosePanel";
        Color32 borderCol = isWin ? new Color32(114, 98, 61, 255) : new Color32(105, 57, 49, 255);
        Color32 bannerCol = isWin ? new Color32(43, 76, 44, 255) : new Color32(84, 38, 32, 255);
        Color32 accentCol = isWin ? new Color32(128, 190, 116, 255) : new Color32(208, 99, 72, 255);

        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 620f), Vector2.zero, new Vector2(0.5f, 0.5f));

        // Border & Body
        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(panel.transform, false);
        Stretch(borderObj.GetComponent<RectTransform>());
        borderObj.GetComponent<Image>().color = borderCol;

        GameObject bodyObj = new GameObject("Body", typeof(RectTransform), typeof(Image));
        bodyObj.transform.SetParent(panel.transform, false);
        Stretch(bodyObj.GetComponent<RectTransform>());
        bodyObj.GetComponent<RectTransform>().offsetMin = new Vector2(4f, 4f);
        bodyObj.GetComponent<RectTransform>().offsetMax = new Vector2(-4f, -4f);
        bodyObj.GetComponent<Image>().color = Navy;

        // Title Banner
        GameObject bannerObj = new GameObject("Banner", typeof(RectTransform), typeof(Image));
        bannerObj.transform.SetParent(panel.transform, false);
        SetRect(bannerObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(690f, 105f), new Vector2(0f, -70f), new Vector2(0.5f, 1f));
        bannerObj.GetComponent<Image>().color = bannerCol;

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text), typeof(Shadow));
        titleObj.transform.SetParent(bannerObj.transform, false);
        Stretch(titleObj.GetComponent<RectTransform>());
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = font;
        titleTxt.text = isWin ? "CHIẾN THẮNG!" : "THẤT BẠI!";
        titleTxt.fontSize = 42;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color = Color.white;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        Shadow titleSh = titleObj.GetComponent<Shadow>();
        titleSh.effectColor = new Color(0f, 0f, 0f, 0.7f);
        titleSh.effectDistance = new Vector2(0f, -3f);

        GameObject subObj = new GameObject("SubtitleText", typeof(RectTransform), typeof(Text));
        subObj.transform.SetParent(bannerObj.transform, false);
        SetRect(subObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(600f, 22f), new Vector2(0f, 12f), new Vector2(0.5f, 0f));
        Text subTxt = subObj.GetComponent<Text>();
        subTxt.font = font;
        subTxt.text = isWin ? "VƯƠNG QUỐC ĐÃ ĐƯỢC BẢO VỆ" : "LÂU ĐÀI ĐÃ THẤT THỦ";
        subTxt.fontSize = 14;
        subTxt.fontStyle = FontStyle.Bold;
        subTxt.color = accentCol;
        subTxt.alignment = TextAnchor.MiddleCenter;

        // Stats rows
        GameObject statsContainer = new GameObject("StatsContainer", typeof(RectTransform));
        statsContainer.transform.SetParent(panel.transform, false);
        SetRect(statsContainer.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(670f, 240f), new Vector2(0f, -30f), new Vector2(0.5f, 0.5f));

        string[] labels = isWin
            ? new[] { "WAVES COMPLETED", "ENEMIES DEFEATED", "BASE HP", "GOLD EARNED", "SCORE" }
            : new[] { "WAVE REACHED", "ENEMIES DEFEATED", "BASE HP", "SCORE" };

        statTexts = new Text[labels.Length];
        float rowHeight = 44f;
        float startY = (labels.Length - 1) * 0.5f * rowHeight;

        for (int i = 0; i < labels.Length; i++)
        {
            float y = startY - i * rowHeight;
            GameObject rowObj = new GameObject("Row_" + i, typeof(RectTransform), typeof(Image));
            rowObj.transform.SetParent(statsContainer.transform, false);
            SetRect(rowObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(670f, 38f), new Vector2(0f, y), new Vector2(0.5f, 0.5f));
            rowObj.GetComponent<Image>().color = i % 2 == 0 ? new Color(1f, 1f, 1f, 0.05f) : new Color(0f, 0f, 0f, 0.12f);

            GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(Text));
            lblObj.transform.SetParent(rowObj.transform, false);
            SetRect(lblObj.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300f, 32f), new Vector2(160f, 0f), new Vector2(0.5f, 0.5f));
            Text lblTxt = lblObj.GetComponent<Text>();
            lblTxt.font = font;
            lblTxt.text = labels[i];
            lblTxt.fontSize = 16;
            lblTxt.fontStyle = FontStyle.Bold;
            lblTxt.color = Muted;
            lblTxt.alignment = TextAnchor.MiddleLeft;

            GameObject valObj = new GameObject("Value", typeof(RectTransform), typeof(Text));
            valObj.transform.SetParent(rowObj.transform, false);
            SetRect(valObj.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(300f, 32f), new Vector2(-160f, 0f), new Vector2(0.5f, 0.5f));
            Text valTxt = valObj.GetComponent<Text>();
            valTxt.font = font;
            valTxt.text = "-";
            valTxt.fontSize = 20;
            valTxt.fontStyle = FontStyle.Bold;
            valTxt.color = Gold;
            valTxt.alignment = TextAnchor.MiddleRight;
            statTexts[i] = valTxt;
        }

        // Buttons container
        GameObject btnContainer = new GameObject("ButtonsContainer", typeof(RectTransform));
        btnContainer.transform.SetParent(panel.transform, false);
        SetRect(btnContainer.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(670f, 90f), new Vector2(0f, 55f), new Vector2(0.5f, 0f));

        if (isWin)
        {
            buttons = new Button[3];
            buttons[0] = CreateButton(btnContainer.transform, font, "NEXT LEVEL", new Color32(46, 125, 50, 255), new Vector2(-220f, 0f), 200f);
            buttons[1] = CreateButton(btnContainer.transform, font, "REPLAY", new Color32(40, 56, 68, 255), new Vector2(0f, 0f), 200f);
            buttons[2] = CreateButton(btnContainer.transform, font, "MAIN MENU", new Color32(40, 56, 68, 255), new Vector2(220f, 0f), 200f);
        }
        else
        {
            buttons = new Button[2];
            buttons[0] = CreateButton(btnContainer.transform, font, "REPLAY", new Color32(188, 86, 70, 255), new Vector2(-150f, 0f), 260f);
            buttons[1] = CreateButton(btnContainer.transform, font, "MAIN MENU", new Color32(40, 56, 68, 255), new Vector2(150f, 0f), 260f);
        }

        return panel;
    }

    private static Button CreateButton(Transform parent, Font font, string text, Color32 color, Vector2 pos, float width)
    {
        GameObject btnObj = new GameObject(text + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        SetRect(btnObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(width, 52f), pos, new Vector2(0.5f, 0.5f));
        btnObj.GetComponent<Image>().color = color;

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Shadow));
        txtObj.transform.SetParent(btnObj.transform, false);
        Stretch(txtObj.GetComponent<RectTransform>());
        Text txt = txtObj.GetComponent<Text>();
        txt.font = font;
        txt.text = text;
        txt.fontSize = 17;
        txt.fontStyle = FontStyle.Bold;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;

        Shadow sh = txtObj.GetComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.65f);
        sh.effectDistance = new Vector2(1.5f, -1.5f);

        return btnObj.GetComponent<Button>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 aMin, Vector2 aMax, Vector2 size, Vector2 pos, Vector2 pivot)
    {
        rect.anchorMin = aMin;
        rect.anchorMax = aMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;
    }
}
