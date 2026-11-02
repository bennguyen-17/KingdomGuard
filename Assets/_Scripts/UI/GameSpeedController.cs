using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Thanh chọn tốc độ game: 1x / 2x / 3x (phím tắt 1, 2, 3).
// - Được HUDController tự gắn vào object "InGameHUD", tự dựng nút bằng code.
// - Nằm dưới cụm Gold / Lives ở góc trên trái.
public class GameSpeedController : MonoBehaviour
{
    public static readonly float[] Speeds = { 1f, 2f, 3f };

    private static readonly Color NormalColor = new Color32(38, 50, 66, 230);
    private static readonly Color SelectedColor = new Color32(39, 174, 96, 255); // Emerald Green
    private static readonly Color SelectedOutline = new Color32(241, 196, 15, 255); // Golden outline

    private const float ButtonWidth = 64f;
    private const float ButtonHeight = 42f;
    private const float Gap = 6f;

    private readonly Image[] faces = new Image[Speeds.Length];
    private readonly Outline[] outlines = new Outline[Speeds.Length];
    private int index;
    private UIManager ui;

    private void Start()
    {
        BuildBar();
        Refresh();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) SetSpeed(0);
        else if (kb.digit2Key.wasPressedThisFrame) SetSpeed(1);
        else if (kb.digit3Key.wasPressedThisFrame) SetSpeed(2);
    }

    private void SetSpeed(int i)
    {
        if (i < 0 || i >= Speeds.Length) return;
        if (GameManager.GameIsOver) return;

        if (ui == null) ui = FindAnyObjectByType<UIManager>();
        if (ui != null && ui.IsPaused) return;

        index = i;
        Time.timeScale = Speeds[index];
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < faces.Length; i++)
        {
            if (faces[i] != null)
            {
                bool active = (i == index);
                faces[i].color = active ? SelectedColor : NormalColor;
                if (outlines[i] != null)
                {
                    outlines[i].effectColor = active ? (Color)SelectedOutline : new Color(0.1f, 0.15f, 0.22f, 0.8f);
                    outlines[i].effectDistance = active ? new Vector2(2f, -2f) : new Vector2(1f, -1f);
                }
            }
        }
    }

    private void BuildBar()
    {
        Text sample = GetComponentInChildren<Text>(true);
        Font font = sample != null ? sample.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var barGo = new GameObject("SpeedBar", typeof(RectTransform));
        barGo.transform.SetParent(transform, false);
        var barRect = (RectTransform)barGo.transform;
        barRect.anchorMin = barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 1f);
        barRect.anchoredPosition = new Vector2(24f, -86f);
        barRect.sizeDelta = new Vector2(Speeds.Length * ButtonWidth + (Speeds.Length - 1) * Gap, ButtonHeight);

        for (int i = 0; i < Speeds.Length; i++)
        {
            int captured = i;

            var go = new GameObject("Speed_x" + Speeds[i].ToString("0"),
                                    typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            go.transform.SetParent(barGo.transform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(i * (ButtonWidth + Gap), 0f);
            rt.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            faces[i] = go.GetComponent<Image>();
            faces[i].color = NormalColor;

            outlines[i] = go.GetComponent<Outline>();
            outlines[i].effectColor = new Color(0.1f, 0.15f, 0.22f, 0.8f);
            outlines[i].effectDistance = new Vector2(1f, -1f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = faces[i];
            button.onClick.AddListener(() => SetSpeed(captured));

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Shadow));
            labelGo.transform.SetParent(go.transform, false);
            var lr = (RectTransform)labelGo.transform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = lr.offsetMax = Vector2.zero;

            var shadow = labelGo.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);

            var label = labelGo.GetComponent<Text>();
            label.font = font;
            label.fontSize = 20;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = Speeds[i].ToString("0") + "x";
            label.raycastTarget = false;
        }
    }
}
