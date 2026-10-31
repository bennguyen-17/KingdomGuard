using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Thanh chọn tốc độ game: x1 / x2 / x3 (phím tắt 1, 2, 3).
// - Được HUDController tự gắn vào object "InGameHUD", tự dựng nút bằng code, KHÔNG cần sửa prefab.
// - Nằm dưới cụm Gold / Lives ở góc trên trái. Vì là con của InGameHUD nên tự ẩn khi hiện màn Win/Lose.
// - Dùng Time.timeScale nên quái, đạn, đếm ngược wave... đều nhanh theo.
// - Pause (UIManager) tự nhớ và khôi phục đúng tốc độ đang chọn.
// - Mỗi màn mới bắt đầu lại ở x1 (GameManager đặt timeScale = 1 khi vào màn).
public class GameSpeedController : MonoBehaviour
{
    public static readonly float[] Speeds = { 1f, 2f, 3f };

    private static readonly Color NormalColor = new Color32(44, 62, 80, 230);
    private static readonly Color SelectedColor = new Color32(46, 125, 50, 255);

    private const float ButtonWidth = 64f;
    private const float ButtonHeight = 44f;
    private const float Gap = 6f;

    private readonly Image[] faces = new Image[Speeds.Length];
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
        if (ui != null && ui.IsPaused) return;   // đang pause thì không đổi

        index = i;
        Time.timeScale = Speeds[index];
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < faces.Length; i++)
            if (faces[i] != null) faces[i].color = (i == index) ? SelectedColor : NormalColor;
    }

    private void BuildBar()
    {
        // Lấy font của HUD hiện có cho đồng bộ
        Text sample = GetComponentInChildren<Text>(true);
        Font font = sample != null ? sample.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var barGo = new GameObject("SpeedBar", typeof(RectTransform));
        barGo.transform.SetParent(transform, false);
        var barRect = (RectTransform)barGo.transform;
        barRect.anchorMin = barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot = new Vector2(0f, 1f);
        barRect.anchoredPosition = new Vector2(24f, -86f);   // GoldBadge cao tới y=-76 nên nằm ngay dưới
        barRect.sizeDelta = new Vector2(Speeds.Length * ButtonWidth + (Speeds.Length - 1) * Gap, ButtonHeight);

        for (int i = 0; i < Speeds.Length; i++)
        {
            int captured = i;

            var go = new GameObject("Speed_x" + Speeds[i].ToString("0"),
                                    typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(barGo.transform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(i * (ButtonWidth + Gap), 0f);
            rt.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            faces[i] = go.GetComponent<Image>();
            faces[i].color = NormalColor;

            var button = go.GetComponent<Button>();
            button.targetGraphic = faces[i];
            button.onClick.AddListener(() => SetSpeed(captured));

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var lr = (RectTransform)labelGo.transform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = lr.offsetMax = Vector2.zero;

            var label = labelGo.GetComponent<Text>();
            label.font = font;
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = "x" + Speeds[i].ToString("0");
            label.raycastTarget = false;
        }
    }
}
