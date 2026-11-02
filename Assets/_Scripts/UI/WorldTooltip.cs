using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Hiện TÊN (và thông tin ngắn) khi rê chuột lên quái hoặc tháp:
//   Quái: tên môn học + máu hiện tại / máu tối đa
//   Tháp: tên thành viên dev + cấp + tầm bắn
// alwaysShowEnemyNames = true để luôn hiện tên môn học phía trên đầu mọi con quái.
public class WorldTooltip : MonoBehaviour
{
    public bool alwaysShowEnemyNames = true;

    private const float HoverRadius = 0.75f;
    private const float RefreshInterval = 0.2f;

    private Enemy[] enemies = new Enemy[0];
    private Tower[] towers = new Tower[0];
    private float refreshTimer;
    private Camera cam;
    private UIManager ui;
    private GUIStyle style;

    private void Update()
    {
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = RefreshInterval;
            enemies = FindObjectsByType<Enemy>();
            towers = FindObjectsByType<Tower>();
        }
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsGameplayActive || GameManager.GameIsOver) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (ui == null) ui = FindAnyObjectByType<UIManager>();
        if (ui != null && ui.IsPaused) return;

        if (style == null)
            style = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        style.fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height / 55f), 13, 22);

        if (alwaysShowEnemyNames)
        {
            foreach (Enemy e in enemies)
            {
                if (e == null) continue;
                var sr = e.GetComponent<SpriteRenderer>();
                float top = sr != null ? sr.bounds.max.y : e.transform.position.y + 0.5f;
                Vector3 sp = cam.WorldToScreenPoint(new Vector3(e.transform.position.x, top + 0.38f, 0f));

                string name = LocalizationManager.GetEnemyDisplayName(e.EnemyKey);
                Color col = Color.white;
                if (e.EnemyKey.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0 || e.EnemyKey.IndexOf("Troll", StringComparison.OrdinalIgnoreCase) >= 0)
                    col = new Color(1f, 0.35f, 0.35f);
                else if (e.EnemyKey.IndexOf("Orc", StringComparison.OrdinalIgnoreCase) >= 0)
                    col = new Color(1f, 0.8f, 0.3f);
                else
                    col = new Color(0.95f, 1f, 0.6f);

                DrawShadowText(new Vector2(sp.x, Screen.height - sp.y), name, col);
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mouseScreen = mouse.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(mouseScreen);
        world.z = 0f;

        string text = null;

        Enemy hoverEnemy = Nearest(enemies, world);
        if (hoverEnemy != null)
        {
            text = "★ " + LocalizationManager.GetEnemyDisplayName(hoverEnemy.EnemyKey) + " ★\n"
                 + "HP " + Mathf.CeilToInt(hoverEnemy.CurrentHealth) + " / " + Mathf.CeilToInt(hoverEnemy.maxHealth);
        }
        else
        {
            Tower hoverTower = Nearest(towers, world);
            if (hoverTower != null && hoverTower.Definition != null)
            {
                bool vi = LocalizationManager.IsVietnamese;
                text = "🛡 " + LocalizationManager.GetTowerDisplayName(hoverTower.Definition.towerName) + "\n"
                     + (vi ? "Cấp " : "Lv ") + (hoverTower.Level + 1) + "/" + hoverTower.Definition.levels.Length + "  |  "
                     + (vi ? "Tầm: " : "Range: ") + hoverTower.Range.ToString("0.0");
            }
        }

        if (text != null) DrawTooltipBox(mouseScreen, text);
    }

    private static T Nearest<T>(T[] items, Vector3 world) where T : Component
    {
        T best = null;
        float bestDist = HoverRadius;
        foreach (T item in items)
        {
            if (item == null) continue;
            float d = Vector2.Distance(item.transform.position, world);
            if (d <= bestDist) { bestDist = d; best = item; }
        }
        return best;
    }

    private void DrawTooltipBox(Vector2 mouseScreen, string text)
    {
        Vector2 size = style.CalcSize(new GUIContent(text)) + new Vector2(24f, 14f);
        float x = mouseScreen.x + 18f;
        float y = Screen.height - mouseScreen.y - size.y - 18f;
        x = Mathf.Clamp(x, 4f, Screen.width - size.x - 4f);
        y = Mathf.Clamp(y, 4f, Screen.height - size.y - 4f);

        var rect = new Rect(x, y, size.x, size.y);
        GUI.color = new Color(0.08f, 0.1f, 0.14f, 0.92f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        style.normal.textColor = Color.white;
        GUI.Label(rect, text, style);
    }

    private void DrawShadowText(Vector2 centerGui, string text, Color textColor)
    {
        Vector2 size = style.CalcSize(new GUIContent(text));
        var rect = new Rect(centerGui.x - size.x * 0.5f, centerGui.y - size.y * 0.5f, size.x, size.y);

        style.normal.textColor = Color.black;
        GUI.Label(new Rect(rect.x + 1f, rect.y, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x - 1f, rect.y, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x, rect.y + 1f, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x, rect.y - 1f, rect.width, rect.height), text, style);

        style.normal.textColor = textColor;
        GUI.Label(rect, text, style);
    }
}
