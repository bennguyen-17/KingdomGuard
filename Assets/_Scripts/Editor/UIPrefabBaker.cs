using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
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
        string marker = "Temp/UIPrefabBaker_v10.done";
        if (!File.Exists(marker))
        {
            BakeHUDPrefab();
            SetupMainMenuGuide();
            SetupMainMenuEnvironment();
            try { File.WriteAllText(marker, "v10"); } catch { }
        }
    }

    [MenuItem("Kingdom Guard/Tạo Prefab UI Chuẩn (Bake HUD)")]
    public static void BakeHUDPrefab()
    {
        string prefabDir = "Assets/Prefabs";
        if (!Directory.Exists(prefabDir))
            Directory.CreateDirectory(prefabDir);

        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Icon_Coin.png");
        Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Icon_Heart.png");
        Sprite waveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Icon_Wave.png");
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 1. Root Canvas
        GameObject canvasRoot = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        UIManager uiManager = canvasRoot.AddComponent<UIManager>();

        // EventSystem
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

        // 2.4 Pause Button (Top Rightmost)
        GameObject pauseBtnObj = new GameObject("PauseButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        pauseBtnObj.transform.SetParent(hudRoot.transform, false);
        SetRect(pauseBtnObj.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(56f, 56f), new Vector2(-24f, -20f), new Vector2(1f, 1f));
        pauseBtnObj.GetComponent<Image>().color = new Color32(23, 32, 42, 240);
        Outline pauseOutline = pauseBtnObj.GetComponent<Outline>();
        pauseOutline.effectColor = new Color32(231, 190, 99, 220);
        pauseOutline.effectDistance = new Vector2(2f, -2f);
        Button pauseBtn = pauseBtnObj.GetComponent<Button>();

        GameObject pauseTxtObj = new GameObject("Icon", typeof(RectTransform), typeof(Text));
        pauseTxtObj.transform.SetParent(pauseBtnObj.transform, false);
        Stretch(pauseTxtObj.GetComponent<RectTransform>());
        Text pauseTxt = pauseTxtObj.GetComponent<Text>();
        pauseTxt.font = font;
        pauseTxt.text = "❚❚";
        pauseTxt.fontSize = 20;
        pauseTxt.fontStyle = FontStyle.Bold;
        pauseTxt.color = new Color32(231, 190, 99, 255);
        pauseTxt.alignment = TextAnchor.MiddleCenter;

        // 2.5 Wave Action Button (Right next to Pause Button)
        GameObject actionBtnObj = new GameObject("WaveActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        actionBtnObj.transform.SetParent(hudRoot.transform, false);
        SetRect(actionBtnObj.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(280f, 56f), new Vector2(-90f, -20f), new Vector2(1f, 1f));
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

        // 4. Pause Panel
        GameObject pausePanel = BuildPausePanel(canvasRoot.transform, font, out Button resumeBtn, out Button pReplayBtn, out Button pMenuBtn);
        pausePanel.SetActive(false);

        // 5. Win Panel
        GameObject winPanel = BuildResultPanel(canvasRoot.transform, font, true, out Text[] winStats, out Button[] winBtns, out Text[] winRowLabels);
        winPanel.SetActive(false);

        // 6. Lose Panel
        GameObject losePanel = BuildResultPanel(canvasRoot.transform, font, false, out Text[] loseStats, out Button[] loseBtns, out Text[] loseRowLabels);
        losePanel.SetActive(false);

        // 7. Transition Curtain
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
        uiSo.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        uiSo.FindProperty("darkOverlay").objectReferenceValue = overlayImg;
        uiSo.FindProperty("transitionCurtain").objectReferenceValue = curtainImg;

        uiSo.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
        uiSo.FindProperty("resumeButton").objectReferenceValue = resumeBtn;
        uiSo.FindProperty("pauseReplayButton").objectReferenceValue = pReplayBtn;
        uiSo.FindProperty("pauseMenuButton").objectReferenceValue = pMenuBtn;

        Text pauseTitleTxt = pausePanel.transform.Find("TitleText")?.GetComponent<Text>();
        Text resumeBtnTxt = resumeBtn.GetComponentInChildren<Text>();
        Text pReplayBtnTxt = pReplayBtn.GetComponentInChildren<Text>();
        Text pMenuBtnTxt = pMenuBtn.GetComponentInChildren<Text>();

        Text winTitleTxt = winPanel.transform.Find("Banner/TitleText")?.GetComponent<Text>();
        Text winSubTxt = winPanel.transform.Find("Banner/SubtitleText")?.GetComponent<Text>();
        Text winNextBtnTxt = winBtns[0]?.GetComponentInChildren<Text>();
        Text winReplayBtnTxt = winBtns[1]?.GetComponentInChildren<Text>();
        Text winMenuBtnTxt = winBtns[2]?.GetComponentInChildren<Text>();

        Text loseTitleTxt = losePanel.transform.Find("Banner/TitleText")?.GetComponent<Text>();
        Text loseSubTxt = losePanel.transform.Find("Banner/SubtitleText")?.GetComponent<Text>();
        Text loseReplayBtnTxt = loseBtns[0]?.GetComponentInChildren<Text>();
        Text loseMenuBtnTxt = loseBtns[1]?.GetComponentInChildren<Text>();

        uiSo.FindProperty("pauseTitleText").objectReferenceValue = pauseTitleTxt;
        uiSo.FindProperty("resumeButtonText").objectReferenceValue = resumeBtnTxt;
        uiSo.FindProperty("pauseReplayButtonText").objectReferenceValue = pReplayBtnTxt;
        uiSo.FindProperty("pauseMenuButtonText").objectReferenceValue = pMenuBtnTxt;

        uiSo.FindProperty("winTitleText").objectReferenceValue = winTitleTxt;
        uiSo.FindProperty("winSubtitleText").objectReferenceValue = winSubTxt;
        uiSo.FindProperty("loseTitleText").objectReferenceValue = loseTitleTxt;
        uiSo.FindProperty("loseSubtitleText").objectReferenceValue = loseSubTxt;

        uiSo.FindProperty("winNextButtonText").objectReferenceValue = winNextBtnTxt;
        uiSo.FindProperty("winReplayButtonText").objectReferenceValue = winReplayBtnTxt;
        uiSo.FindProperty("winMenuButtonText").objectReferenceValue = winMenuBtnTxt;
        uiSo.FindProperty("loseReplayButtonText").objectReferenceValue = loseReplayBtnTxt;
        uiSo.FindProperty("loseMenuButtonText").objectReferenceValue = loseMenuBtnTxt;

        SerializedProperty winLabelsProp = uiSo.FindProperty("winRowLabels");
        winLabelsProp.arraySize = winRowLabels.Length;
        for (int i = 0; i < winRowLabels.Length; i++)
            winLabelsProp.GetArrayElementAtIndex(i).objectReferenceValue = winRowLabels[i];

        SerializedProperty loseLabelsProp = uiSo.FindProperty("loseRowLabels");
        loseLabelsProp.arraySize = loseRowLabels.Length;
        for (int i = 0; i < loseRowLabels.Length; i++)
            loseLabelsProp.GetArrayElementAtIndex(i).objectReferenceValue = loseRowLabels[i];

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
        string hudPath = "Assets/Prefabs/HUD.prefab";
        string hudResPath = "Assets/Resources/HUD.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasRoot, hudPath);
        PrefabUtility.SaveAsPrefabAsset(canvasRoot, hudResPath);
        Object.DestroyImmediate(canvasRoot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[UIPrefabBaker] Đã tạo thành công Prefab UI trực quan tại: {hudPath}");

        // Now put instance into current active scene if Level scene
        InsertPrefabIntoActiveScene(hudPath);
    }

    private static GameObject BuildPausePanel(Transform parent, Font font, out Button resumeBtn, out Button replayBtn, out Button menuBtn)
    {
        GameObject panel = new GameObject("PausePanel", typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(500f, 420f), Vector2.zero, new Vector2(0.5f, 0.5f));

        // Border & Body
        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(panel.transform, false);
        Stretch(borderObj.GetComponent<RectTransform>());
        borderObj.GetComponent<Image>().color = new Color32(231, 190, 99, 230);

        GameObject bodyObj = new GameObject("Body", typeof(RectTransform), typeof(Image));
        bodyObj.transform.SetParent(panel.transform, false);
        Stretch(bodyObj.GetComponent<RectTransform>());
        bodyObj.GetComponent<RectTransform>().offsetMin = new Vector2(4f, 4f);
        bodyObj.GetComponent<RectTransform>().offsetMax = new Vector2(-4f, -4f);
        bodyObj.GetComponent<Image>().color = Navy;

        // Title
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text), typeof(Shadow));
        titleObj.transform.SetParent(panel.transform, false);
        SetRect(titleObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(400f, 60f), new Vector2(0f, -40f), new Vector2(0.5f, 1f));
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = font;
        titleTxt.text = "TẠM DỪNG";
        titleTxt.fontSize = 36;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color = Gold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        Shadow titleSh = titleObj.GetComponent<Shadow>();
        titleSh.effectColor = new Color(0f, 0f, 0f, 0.8f);
        titleSh.effectDistance = new Vector2(0f, -3f);

        // Buttons
        resumeBtn = CreateButton(panel.transform, font, "TIẾP TỤC", new Color32(46, 125, 50, 255), new Vector2(0f, 45f), 360f);
        replayBtn = CreateButton(panel.transform, font, "CHƠI LẠI", new Color32(190, 120, 25, 255), new Vector2(0f, -30f), 360f);
        menuBtn = CreateButton(panel.transform, font, "VỀ MENU", new Color32(140, 50, 40, 255), new Vector2(0f, -105f), 360f);

        return panel;
    }

    private static GameObject BuildGuidePanel(Transform parent, Font font, out Button closeBtn)
    {
        GameObject panel = new GameObject("GuidePanel", typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1160f, 760f), Vector2.zero, new Vector2(0.5f, 0.5f));

        // Border & Body
        GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(Image));
        borderObj.transform.SetParent(panel.transform, false);
        Stretch(borderObj.GetComponent<RectTransform>());
        borderObj.GetComponent<Image>().color = new Color32(231, 190, 99, 230);

        GameObject bodyObj = new GameObject("Body", typeof(RectTransform), typeof(Image));
        bodyObj.transform.SetParent(panel.transform, false);
        Stretch(bodyObj.GetComponent<RectTransform>());
        bodyObj.GetComponent<RectTransform>().offsetMin = new Vector2(4f, 4f);
        bodyObj.GetComponent<RectTransform>().offsetMax = new Vector2(-4f, -4f);
        bodyObj.GetComponent<Image>().color = Navy;

        // Title Area (Không dùng background banner box)
        GameObject bannerObj = new GameObject("Banner", typeof(RectTransform));
        bannerObj.transform.SetParent(panel.transform, false);
        SetRect(bannerObj.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1100f, 75f), new Vector2(0f, -30f), new Vector2(0.5f, 1f));

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text), typeof(Shadow));
        titleObj.transform.SetParent(bannerObj.transform, false);
        Stretch(titleObj.GetComponent<RectTransform>());
        Text titleTxt = titleObj.GetComponent<Text>();
        titleTxt.font = font;
        titleTxt.text = LocalizationManager.GuideTitle;
        titleTxt.fontSize = 32;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.color = Gold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        Shadow titleSh = titleObj.GetComponent<Shadow>();
        titleSh.effectColor = new Color(0f, 0f, 0f, 0.8f);
        titleSh.effectDistance = new Vector2(0f, -2f);

        // Content Area
        GameObject contentObj = new GameObject("ContentText", typeof(RectTransform), typeof(Text));
        contentObj.transform.SetParent(panel.transform, false);
        SetRect(contentObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1060f, 530f), new Vector2(0f, 15f), new Vector2(0.5f, 0.5f));
        Text cTxt = contentObj.GetComponent<Text>();
        cTxt.font = font;
        cTxt.fontSize = 21;
        cTxt.lineSpacing = 1.3f;
        cTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
        cTxt.verticalOverflow = VerticalWrapMode.Overflow;
        cTxt.color = new Color32(235, 240, 245, 255);
        cTxt.alignment = TextAnchor.UpperLeft;
        cTxt.text = LocalizationManager.GuideContent;

        closeBtn = CreateButton(panel.transform, font, LocalizationManager.GuideClose, new Color32(46, 125, 50, 255), new Vector2(0f, -320f), 300f);

        return panel;
    }

    private static void InsertPrefabIntoActiveScene(string prefabPath)
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        if (!activeScene.name.StartsWith("Level_")) return;

        var existing = GameObject.Find("HUD");
        if (existing != null)
            Object.DestroyImmediate(existing);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "HUD";
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log($"[UIPrefabBaker] Đã tự động chèn HUD vào Scene: {activeScene.name}");
        }
    }

    [MenuItem("Kingdom Guard/Cập nhật Hướng Dẫn vào Main Menu")]
    public static void SetupMainMenuGuide()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!File.Exists(scenePath)) return;

        var currentScene = EditorSceneManager.GetActiveScene();
        bool wasDifferentScene = currentScene.path != scenePath;
        var scene = wasDifferentScene ? EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single) : currentScene;

        GameObject canvasObj = GameObject.Find("MainMenuCanvas") ?? GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("[UIPrefabBaker] Không tìm thấy MainMenuCanvas trong MainMenu.unity!");
            return;
        }

        MainMenuController mmc = Object.FindAnyObjectByType<MainMenuController>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 1. Reposition Play & Quit buttons to accommodate Guide button nicely
        Transform playBtn = canvasObj.transform.Find("Play Button");
        Transform quitBtn = canvasObj.transform.Find("Quit Button");

        if (playBtn != null)
        {
            playBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -80f);
        }

        if (quitBtn != null)
        {
            quitBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -240f);
        }

        // 2. Add or update Guide Button
        Transform existingGuideBtn = canvasObj.transform.Find("Guide Button");
        GameObject guideBtnObj;
        if (existingGuideBtn == null)
        {
            guideBtnObj = new GameObject("Guide Button", typeof(RectTransform), typeof(Image), typeof(Button));
            guideBtnObj.transform.SetParent(canvasObj.transform, false);
        }
        else
        {
            guideBtnObj = existingGuideBtn.gameObject;
        }

        SetRect(guideBtnObj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(360f, 68f), new Vector2(0f, -160f), new Vector2(0.5f, 0.5f));
        guideBtnObj.GetComponent<Image>().color = new Color32(36, 52, 65, 255);

        // Guide Button Label
        Transform labelTrans = guideBtnObj.transform.Find("Button Label") ?? guideBtnObj.transform.Find("Text");
        GameObject labelObj;
        if (labelTrans == null)
        {
            labelObj = new GameObject("Button Label", typeof(RectTransform), typeof(Text), typeof(Shadow));
            labelObj.transform.SetParent(guideBtnObj.transform, false);
            Stretch(labelObj.GetComponent<RectTransform>());
        }
        else
        {
            labelObj = labelTrans.gameObject;
        }

        Text labelTxt = labelObj.GetComponent<Text>();
        labelTxt.font = font;
        labelTxt.text = LocalizationManager.GuideButton;
        labelTxt.fontSize = 24;
        labelTxt.fontStyle = FontStyle.Bold;
        labelTxt.color = Gold;
        labelTxt.alignment = TextAnchor.MiddleCenter;

        Button guideBtn = guideBtnObj.GetComponent<Button>();
        if (mmc != null)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(guideBtn.onClick, mmc.OpenGuide);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(guideBtn.onClick, mmc.OpenGuide);
        }

        // 3. Build or update GuidePanel
        Transform existingGuidePanel = canvasObj.transform.Find("GuidePanel");
        if (existingGuidePanel != null)
        {
            Object.DestroyImmediate(existingGuidePanel.gameObject);
        }

        GameObject guidePanel = BuildGuidePanel(canvasObj.transform, font, out Button closeBtn);
        guidePanel.SetActive(false);

        if (mmc != null)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(closeBtn.onClick, mmc.CloseGuide);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.onClick, mmc.CloseGuide);
        }

        // 4. Create or update Language Button at Top-Right
        Transform existingLangBtn = canvasObj.transform.Find("Language Button");
        GameObject langBtnObj;
        if (existingLangBtn == null)
        {
            langBtnObj = new GameObject("Language Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            langBtnObj.transform.SetParent(canvasObj.transform, false);
        }
        else
        {
            langBtnObj = existingLangBtn.gameObject;
        }

        SetRect(langBtnObj.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(170f, 48f), new Vector2(-25f, -25f), new Vector2(1f, 1f));
        langBtnObj.GetComponent<Image>().color = new Color32(23, 34, 46, 240);
        Outline langOutline = langBtnObj.GetComponent<Outline>() ?? langBtnObj.AddComponent<Outline>();
        langOutline.effectColor = new Color32(231, 190, 99, 230);
        langOutline.effectDistance = new Vector2(1.5f, -1.5f);

        Transform langLabelTrans = langBtnObj.transform.Find("Button Label") ?? langBtnObj.transform.Find("Text");
        GameObject langLabelObj;
        if (langLabelTrans == null)
        {
            langLabelObj = new GameObject("Button Label", typeof(RectTransform), typeof(Text), typeof(Shadow));
            langLabelObj.transform.SetParent(langBtnObj.transform, false);
            Stretch(langLabelObj.GetComponent<RectTransform>());
        }
        else
        {
            langLabelObj = langLabelTrans.gameObject;
        }

        Text langLabelTxt = langLabelObj.GetComponent<Text>();
        langLabelTxt.font = font;
        langLabelTxt.text = LocalizationManager.LanguageButton;
        langLabelTxt.fontSize = 16;
        langLabelTxt.fontStyle = FontStyle.Bold;
        langLabelTxt.color = Color.white;
        langLabelTxt.alignment = TextAnchor.MiddleCenter;

        Button langBtn = langBtnObj.GetComponent<Button>();
        if (mmc != null)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(langBtn.onClick, mmc.ToggleLanguage);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(langBtn.onClick, mmc.ToggleLanguage);

            Text playBtnTxt = playBtn?.GetComponentInChildren<Text>();
            Text quitBtnTxt = quitBtn?.GetComponentInChildren<Text>();
            Text footerTxt = canvasObj.transform.Find("Menu Footer")?.GetComponentInChildren<Text>();

            Text gTitleTxt = guidePanel.transform.Find("Banner/TitleText")?.GetComponent<Text>();
            Text gContentTxt = guidePanel.transform.Find("ContentText")?.GetComponent<Text>();
            Text gCloseTxt = closeBtn?.GetComponentInChildren<Text>();

            SerializedObject mmcSo = new SerializedObject(mmc);
            mmcSo.FindProperty("guidePanel").objectReferenceValue = guidePanel;
            mmcSo.FindProperty("playButtonText").objectReferenceValue = playBtnTxt;
            mmcSo.FindProperty("guideButtonText").objectReferenceValue = labelTxt;
            mmcSo.FindProperty("quitButtonText").objectReferenceValue = quitBtnTxt;
            mmcSo.FindProperty("languageButtonText").objectReferenceValue = langLabelTxt;
            mmcSo.FindProperty("footerText").objectReferenceValue = footerTxt;
            mmcSo.FindProperty("guideTitleText").objectReferenceValue = gTitleTxt;
            mmcSo.FindProperty("guideContentText").objectReferenceValue = gContentTxt;
            mmcSo.FindProperty("guideCloseButtonText").objectReferenceValue = gCloseTxt;
            mmcSo.ApplyModifiedProperties();

            mmc.UpdateLanguageUI();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[UIPrefabBaker] Đã cập nhật thành công Nút Ngôn Ngữ, Hướng Dẫn vào MainMenu.unity!");

        if (wasDifferentScene && !string.IsNullOrEmpty(currentScene.path))
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }
    }

    private static GameObject BuildResultPanel(Transform parent, Font font, bool isWin, out Text[] statTexts, out Button[] buttons, out Text[] rowLabels)
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
        rowLabels = new Text[labels.Length];
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
            rowLabels[i] = lblTxt;

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

    private static Sprite GetFirstSprite(string path)
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var o in all)
        {
            if (o is Sprite s) return s;
        }
        return null;
    }

    private static int ExtractTrailingNumber(string s)
    {
        int idx = s.LastIndexOf('_');
        if (idx >= 0 && int.TryParse(s.Substring(idx + 1), out int n))
            return n;
        return -1;
    }

    private static Sprite[] GetAllSprites(string path)
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> list = new List<Sprite>();
        foreach (var o in all)
        {
            if (o is Sprite s) list.Add(s);
        }
        list.Sort((a, b) =>
        {
            int na = ExtractTrailingNumber(a.name);
            int nb = ExtractTrailingNumber(b.name);
            if (na >= 0 && nb >= 0) return na.CompareTo(nb);
            return string.Compare(a.name, b.name, System.StringComparison.Ordinal);
        });
        return list.ToArray();
    }

    private static Sprite[] EnsureSprites(string path, int frameWidth, int frameHeight, int count, Vector2 pivot)
    {
        Sprite[] sprites = GetAllSprites(path);
        if (sprites != null && sprites.Length >= count)
            return sprites;

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null)
        {
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;

            SpriteMetaData[] metas = new SpriteMetaData[count];
            string baseName = Path.GetFileNameWithoutExtension(path);
            for (int i = 0; i < count; i++)
            {
                metas[i] = new SpriteMetaData
                {
                    name = baseName + "_" + i,
                    rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = pivot
                };
            }
            ti.spritesheet = metas;
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            sprites = GetAllSprites(path);
        }
        return sprites;
    }

    private static void CreateCloud(Transform parent, Sprite sprite, Vector2 pos, Vector2 size, float speed, float alpha)
    {
        GameObject obj = new GameObject("Cloud", typeof(RectTransform), typeof(Image), typeof(UICloudDrifter));
        obj.transform.SetParent(parent, false);
        SetRect(obj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, pos, new Vector2(0.5f, 0.5f));
        Image img = obj.GetComponent<Image>();
        img.sprite = sprite;
        img.color = new Color(1f, 1f, 1f, alpha);
        img.raycastTarget = false;
        UICloudDrifter drifter = obj.GetComponent<UICloudDrifter>();
        drifter.Setup(speed, pos.y);
    }

    private static void CreateAnimatedUI(Transform parent, string name, Sprite[] frames, float fps, Vector2 pos, Vector2 size, int startOffset = 0, float alpha = 1f)
    {
        if (frames == null || frames.Length == 0) return;

        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UIFrameAnimator));
        obj.transform.SetParent(parent, false);
        SetRect(obj.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, pos, new Vector2(0.5f, 0.5f));
        Image img = obj.GetComponent<Image>();
        img.raycastTarget = false;
        img.preserveAspect = true;
        if (alpha < 1f)
        {
            img.color = new Color(1f, 1f, 1f, alpha);
        }
        int initialIdx = Mathf.Clamp(startOffset, 0, frames.Length - 1);
        img.sprite = frames[initialIdx];

        UIFrameAnimator anim = obj.GetComponent<UIFrameAnimator>();
        anim.Setup(frames, fps, true, startOffset);
    }

    [MenuItem("Kingdom Guard/Tạo hiệu ứng Môi trường Động vào Main Menu")]
    public static void SetupMainMenuEnvironment()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        var currentScene = EditorSceneManager.GetActiveScene();
        bool wasDifferentScene = currentScene.path != scenePath;
        Scene scene = wasDifferentScene ? EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single) : currentScene;

        GameObject canvasObj = GameObject.Find("MainMenuCanvas");
        if (canvasObj == null)
        {
            Debug.LogError("[UIPrefabBaker] Không tìm thấy MainMenuCanvas!");
            return;
        }

        AssetDatabase.ImportAsset("Assets/Art/UI/MainMenu_BG.jpg", ImportAssetOptions.ForceUpdate);

        // 1. Xóa container cũ nếu đã tồn tại để tránh trùng lặp
        Transform bgTrans = canvasObj.transform.Find("Main Menu Background");
        if (bgTrans != null)
        {
            Image bgImg = bgTrans.GetComponent<Image>();
            if (bgImg != null)
            {
                Sprite cleanSprite = GetFirstSprite("Assets/Art/UI/MainMenu_BG.jpg");
                if (cleanSprite != null) bgImg.sprite = cleanSprite;
            }
        }

        Transform oldEnv = canvasObj.transform.Find("Environment_Decorations");
        if (oldEnv != null)
        {
            Object.DestroyImmediate(oldEnv.gameObject);
        }

        GameObject envObj = new GameObject("Environment_Decorations", typeof(RectTransform));
        envObj.transform.SetParent(canvasObj.transform, false);
        Stretch(envObj.GetComponent<RectTransform>());

        // Đặt ngay sau Pixel World Tint (nằm phía sau Logo và các nút bấm)
        Transform tintTrans = canvasObj.transform.Find("Pixel World Tint");
        if (tintTrans != null)
        {
            envObj.transform.SetSiblingIndex(tintTrans.GetSiblingIndex() + 1);
        }
        else
        {
            envObj.transform.SetSiblingIndex(1);
        }

        // --- A. BỌT SÓNG BIỂN VỖ BỜ (Water Foam Animations) ---
        Sprite[] foamFrames = EnsureSprites("Assets/Art/UI/Animated/Water_Foam.png", 192, 192, 16, new Vector2(0.5f, 0.5f));
        if (foamFrames != null && foamFrames.Length > 0)
        {
            CreateAnimatedUI(envObj.transform, "Foam_WestCliff", foamFrames, 11f, new Vector2(-684f, -112f), new Vector2(220f, 150f), 0, 0.85f);
            CreateAnimatedUI(envObj.transform, "Foam_BottomLeft", foamFrames, 10f, new Vector2(-480f, -380f), new Vector2(240f, 160f), 3, 0.85f);
            CreateAnimatedUI(envObj.transform, "Foam_CenterLagoon", foamFrames, 11f, new Vector2(-195f, -195f), new Vector2(200f, 140f), 6, 0.85f);
            CreateAnimatedUI(envObj.transform, "Foam_CenterBay", foamFrames, 10.5f, new Vector2(-102f, -347f), new Vector2(220f, 150f), 9, 0.85f);
            CreateAnimatedUI(envObj.transform, "Foam_VillageCoast", foamFrames, 11f, new Vector2(479f, -347f), new Vector2(260f, 160f), 12, 0.85f);
            CreateAnimatedUI(envObj.transform, "Foam_EastCliff", foamFrames, 10f, new Vector2(619f, -77f), new Vector2(220f, 150f), 14, 0.85f);
        }

        // --- B. RẶNG CÂY ĐUNG ĐƯA THEO GIÓ (Swaying Trees) ---
        Sprite[] pineFrames = EnsureSprites("Assets/Art/UI/Animated/Tree1_Sway.png", 192, 256, 8, new Vector2(0.5f, 0.2f));
        Sprite[] birchFrames = EnsureSprites("Assets/Art/UI/Animated/Tree3_Sway.png", 192, 192, 8, new Vector2(0.5f, 0.2f));

        Vector2 pineSize = new Vector2(223f, 298f);
        Vector2 birchSize = new Vector2(223f, 223f);

        if (pineFrames != null && pineFrames.Length > 0)
        {
            // Cây thông đơn độc mỏm đá trung tâm
            CreateAnimatedUI(envObj.transform, "Tree_Pine_CenterCliff", pineFrames, 7f, new Vector2(-33f, 239f), pineSize, 0);

            // Rừng thông trên cao bên phải (trên vách đá)
            CreateAnimatedUI(envObj.transform, "Tree_Pine_Forest1", pineFrames, 6.5f, new Vector2(72f, 321f), pineSize, 1);
            CreateAnimatedUI(envObj.transform, "Tree_Pine_Forest2", pineFrames, 7.2f, new Vector2(165f, 333f), pineSize, 3);
            CreateAnimatedUI(envObj.transform, "Tree_Pine_Forest3", pineFrames, 6.8f, new Vector2(246f, 321f), pineSize, 5);
            CreateAnimatedUI(envObj.transform, "Tree_Pine_Forest4", pineFrames, 7.0f, new Vector2(328f, 345f), pineSize, 2);
            CreateAnimatedUI(envObj.transform, "Tree_Pine_Forest5", pineFrames, 6.6f, new Vector2(421f, 333f), pineSize, 4);
            CreateAnimatedUI(envObj.transform, "Tree_Pine_Forest6", pineFrames, 6.9f, new Vector2(537f, 356f), pineSize, 1);
            CreateAnimatedUI(envObj.transform, "Tree_Pine_Forest7", pineFrames, 7.1f, new Vector2(595f, 333f), pineSize, 3);
        }

        if (birchFrames != null && birchFrames.Length > 0)
        {
            // Cụm cây phong vàng bên lâu đài
            CreateAnimatedUI(envObj.transform, "Tree_Birch_Castle1", birchFrames, 6.8f, new Vector2(-649f, 122f), birchSize, 1);
            CreateAnimatedUI(envObj.transform, "Tree_Birch_Castle2", birchFrames, 7.0f, new Vector2(-614f, 28f), birchSize, 4);
            CreateAnimatedUI(envObj.transform, "Tree_Birch_Castle3", birchFrames, 6.5f, new Vector2(-474f, -42f), birchSize, 2);
            CreateAnimatedUI(envObj.transform, "Tree_Birch_Castle4", birchFrames, 7.2f, new Vector2(-428f, -7f), birchSize, 5);

            // Cụm cây phong vàng phía nam làng
            CreateAnimatedUI(envObj.transform, "Tree_Birch_Village1", birchFrames, 6.6f, new Vector2(328f, -300f), birchSize, 0);
            CreateAnimatedUI(envObj.transform, "Tree_Birch_Village2", birchFrames, 7.0f, new Vector2(398f, -277f), birchSize, 3);
        }

        // --- C. LÍNH GÁC & CUNG THỦ (Units Idle) ---
        Sprite[] warriorFrames = GetAllSprites("Assets/Art/TinySwords_Pack/Units/Blue Units/Warrior/Warrior_Idle.png");
        if (warriorFrames != null && warriorFrames.Length > 0)
        {
            // Hiệp sĩ canh gác trước lối lên lâu đài
            CreateAnimatedUI(envObj.transform, "Knight_Guard", warriorFrames, 8f, new Vector2(-370f, 168f), new Vector2(110f, 110f), 0);
        }

        Sprite[] archerFrames = GetAllSprites("Assets/Art/TinySwords_Pack/Units/Blue Units/Archer/Archer_Idle.png");
        if (archerFrames != null && archerFrames.Length > 0)
        {
            // Cung thủ đứng chuẩn xác trên sàn gỗ tháp canh góc dưới trái
            CreateAnimatedUI(envObj.transform, "Watchtower_Archer", archerFrames, 7f, new Vector2(-488f, -25f), new Vector2(90f, 90f), 0);
        }

        // --- D. ĐÀN CỪU GẶM CỎ (Grazing Sheep) ---
        Sprite[] sheepFrames = GetAllSprites("Assets/Art/Map_Decorations/Source_Images/Resources/Meat/Sheep/Sheep_Grass.png");
        if (sheepFrames != null && sheepFrames.Length > 0)
        {
            Vector2 sheepSize = new Vector2(64f, 64f);
            CreateAnimatedUI(envObj.transform, "Sheep_Pasture1", sheepFrames, 6f, new Vector2(165f, -329f), sheepSize, 0);
            CreateAnimatedUI(envObj.transform, "Sheep_Pasture2", sheepFrames, 5.5f, new Vector2(299f, -329f), sheepSize, 3);
            CreateAnimatedUI(envObj.transform, "Sheep_Pasture3", sheepFrames, 6.2f, new Vector2(531f, -118f), sheepSize, 5);
            CreateAnimatedUI(envObj.transform, "Sheep_Pasture4", sheepFrames, 5.8f, new Vector2(293f, -101f), sheepSize, 1);
            CreateAnimatedUI(envObj.transform, "Sheep_Pasture5", sheepFrames, 6.4f, new Vector2(241f, -77f), sheepSize, 4);
        }

        // --- E. CLOUDS (Mây bay lững lờ trên tầng cao) ---
        Sprite cloud1 = GetFirstSprite("Assets/Art/Map_Decorations/Source_Images/Decorations/Clouds/Clouds_01.png");
        Sprite cloud2 = GetFirstSprite("Assets/Art/Map_Decorations/Source_Images/Decorations/Clouds/Clouds_02.png");
        Sprite cloud3 = GetFirstSprite("Assets/Art/Map_Decorations/Source_Images/Decorations/Clouds/Clouds_03.png");

        if (cloud1 != null || cloud2 != null)
        {
            // Đám mây 1: Tầng cao, to, trôi êm dịu (22px/s)
            CreateCloud(envObj.transform, cloud1 ?? cloud2, new Vector2(-600f, 380f), new Vector2(480f, 210f), 22f, 0.75f);
            // Đám mây 2: Tầng giữa, trôi vừa (32px/s)
            CreateCloud(envObj.transform, cloud2 ?? cloud1, new Vector2(100f, 320f), new Vector2(420f, 180f), 32f, 0.6f);
            // Đám mây 3: Tầng trên cùng, mờ ảo (18px/s)
            CreateCloud(envObj.transform, cloud3 ?? cloud1, new Vector2(-200f, 430f), new Vector2(520f, 230f), 18f, 0.65f);
            // Đám mây 4: Tầng phải, nhỏ nhắn (38px/s)
            CreateCloud(envObj.transform, cloud1 ?? cloud3, new Vector2(650f, 360f), new Vector2(360f, 160f), 38f, 0.55f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[UIPrefabBaker] Đã hoàn thiện toàn bộ Main Menu Động chuẩn phong cách Tiny Swords!");

        if (wasDifferentScene && !string.IsNullOrEmpty(currentScene.path))
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }
    }
}
