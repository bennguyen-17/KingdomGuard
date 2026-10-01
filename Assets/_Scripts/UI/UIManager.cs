using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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
    private Coroutine animationRoutine;
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

    private void BuildUI()
    {
        if (canvas != null) return;
        GameObject canvasObject = new GameObject("Result Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (FindObjectOfType<EventSystem>() == null)
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
