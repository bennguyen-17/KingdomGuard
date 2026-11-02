using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


public class BuildManager : MonoBehaviour
{
    [Header("BuildManager.prefab")]
    public GameObject towerPrefab;
    public TowerDefinition[] towerTypes;

    [Header("Tùy chỉnh")]
    [Tooltip("Để trống: tự tìm tilemap tên 'basefortower'")]
    public Tilemap baseTilemap;
    public Camera cam;
    [Tooltip("Cỡ icon của menu = tỉ lệ chiều cao màn hình (0.1 = 10%)")]
    [Range(0.05f, 0.2f)] public float menuIconScreenFraction = 0.1f;
    [Tooltip("Khoảng cách từ tâm chỗ đặt tháp tới icon, tính theo số lần cỡ icon")]
    public float menuRadiusInIcons = 1.7f;
    [Range(0f, 1f)] public float sellRefund = 0.5f;
    [Tooltip("Độ lệch trục Y khi sinh tháp so với tâm ô đất")]
    public float towerOffsetY = 0.75f;

    private enum Kind { Build, Upgrade, Sell }

    private class Option
    {
        public Kind kind;
        public int towerIndex;
        public Vector3 pos;
        public GameObject go;
        public SpriteRenderer bg;
        public Color baseColor;
        public string title;
        public int cost;
        public bool enabled;
    }

    private static readonly Vector3Int[] Dirs =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    private readonly Dictionary<Vector3Int, Node> cellToNode = new Dictionary<Vector3Int, Node>();
    private readonly List<Option> options = new List<Option>();

    private Node openNode;
    private int pending = -1;
    private SpriteRenderer rangeRenderer;
    private Material spriteMat;
    private static Sprite squareSprite, circleSprite;

    private string message;
    private float messageTimer;
    private GUIStyle labelStyle;
    private float iconPx;      // cỡ icon theo pixel màn hình
    private float iconWorld;   // cỡ icon theo unit thế giới
    private int fontSize;

    // ---------------------------------------------------------------- khởi tạo
    private void Start()
    {
        if (cam == null) cam = Camera.main;

        if (baseTilemap == null)
        {
            foreach (Tilemap tm in FindObjectsByType<Tilemap>())
                if (tm.name == "basefortower") { baseTilemap = tm; break; }
        }

        if (towerPrefab != null)
        {
            SpriteRenderer sr = towerPrefab.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) spriteMat = sr.sharedMaterial;
        }

        BuildNodes();
        CreateRangeCircle();
    }

    // Gom các ô basefortower liền nhau thành 1 chỗ đặt tháp
    private void BuildNodes()
    {
        cellToNode.Clear();
        if (baseTilemap == null)
        {
            Debug.LogError("BuildManager: không tìm thấy tilemap 'basefortower' trong scene.", this);
            return;
        }

        var cells = new HashSet<Vector3Int>();
        foreach (Vector3Int pos in baseTilemap.cellBounds.allPositionsWithin)
            if (baseTilemap.HasTile(pos)) cells.Add(pos);

        var visited = new HashSet<Vector3Int>();
        foreach (Vector3Int start in cells)
        {
            if (visited.Contains(start)) continue;

            var group = new List<Vector3Int>();
            var stack = new Stack<Vector3Int>();
            stack.Push(start);
            visited.Add(start);
            while (stack.Count > 0)
            {
                Vector3Int c = stack.Pop();
                group.Add(c);
                foreach (Vector3Int d in Dirs)
                    if (cells.Contains(c + d) && visited.Add(c + d)) stack.Push(c + d);
            }

            Vector3 sum = Vector3.zero;
            foreach (Vector3Int c in group) sum += baseTilemap.GetCellCenterWorld(c);

            Vector3 center = (sum / group.Count) + new Vector3(0f, towerOffsetY, 0f);
            var node = new Node { Center = center };
            foreach (Vector3Int c in group) cellToNode[c] = node;
        }
    }

    private void CreateRangeCircle()
    {
        var go = new GameObject("RangeCircle");
        go.transform.SetParent(transform, false);
        rangeRenderer = go.AddComponent<SpriteRenderer>();
        rangeRenderer.sprite = CircleSprite();
        rangeRenderer.color = new Color(0.7f, 0.9f, 1f, 1f);
        rangeRenderer.sortingOrder = 20;
        if (spriteMat != null) rangeRenderer.sharedMaterial = spriteMat;
        rangeRenderer.enabled = false;
    }

    // ---------------------------------------------------------------- vòng lặp chính
    private void Update()
    {
        if (messageTimer > 0f) messageTimer -= Time.unscaledDeltaTime;

        bool active = GameManager.Instance != null
                      && GameManager.Instance.IsGameplayActive
                      && !GameManager.GameIsOver;
        if (!active)
        {
            if (openNode != null) CloseMenu();
            return;
        }

        if (openNode != null) RefreshOptions();

        Pointer ptr = Pointer.current;   // chuột + cảm ứng (New Input System)
        if (ptr == null || !ptr.press.wasPressedThisFrame) return;

        Vector3 world = cam.ScreenToWorldPoint(ptr.position.ReadValue());
        world.z = 0f;   // camera ở z=-10, tilemap ở z=0: nếu không đặt về 0 sẽ không tìm ra ô

        int hit = HitOption(world);
        if (hit >= 0) { Activate(hit); return; }

        Node node = NodeAt(world);
        if (node == null || node == openNode) CloseMenu();
        else OpenMenu(node);
    }

    private Node NodeAt(Vector3 world)
    {
        if (baseTilemap == null) return null;
        Vector3Int cell = baseTilemap.WorldToCell(world);
        cell.z = 0;
        return cellToNode.TryGetValue(cell, out Node n) ? n : null;
    }

    private int HitOption(Vector3 world)
    {
        for (int i = 0; i < options.Count; i++)
        {
            Vector3 p = options[i].pos;
            float half = iconWorld * 0.55f;
            if (Mathf.Abs(world.x - p.x) <= half && Mathf.Abs(world.y - p.y) <= half) return i;
        }
        return -1;
    }

    // ---------------------------------------------------------------- menu tròn
    private void OpenMenu(Node node)
    {
        CloseMenu();
        openNode = node;
        pending = -1;

        // Menu có cỡ cố định theo màn hình, không phụ thuộc camera zoom bao nhiêu
        iconPx = Mathf.Clamp(Screen.height * menuIconScreenFraction, 36f, 96f);
        iconWorld = iconPx * (cam.orthographicSize * 2f / Screen.height);
        fontSize = Mathf.Clamp(Mathf.RoundToInt(iconPx * 0.36f), 11, 22);

        if (!node.Occupied)
        {
            int n = towerTypes.Length;
            for (int i = 0; i < n; i++)
            {
                TowerDefinition def = towerTypes[i];
                float angle = 90f + ((n - 1) / 2f - i) * 55f;   // trái -> phải theo thứ tự mảng
                Sprite icon = def.icon != null ? def.icon : def.levels[0].sprite;
                AddOption(Kind.Build, i, angle, def.towerName, def.levels[0].cost, icon,
                          new Color(0.15f, 0.22f, 0.4f, 0.95f));
            }
        }
        else
        {
            Tower t = node.Tower;
            Sprite next = t.CanUpgrade ? t.Definition.levels[t.Level + 1].sprite : null;
            AddOption(Kind.Upgrade, -1, 90f, t.CanUpgrade ? "Upgrade" : "MAX", t.UpgradeCost, next,
                      new Color(0.15f, 0.45f, 0.2f, 0.95f));
            AddOption(Kind.Sell, -1, 270f, "Sell", Mathf.RoundToInt(t.TotalSpent * sellRefund), null,
                      new Color(0.55f, 0.15f, 0.15f, 0.95f));
        }

        RefreshOptions();
    }

    private void AddOption(Kind kind, int towerIndex, float angleDeg, string title, int cost,
                           Sprite icon, Color color)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 pos = openNode.Center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * (iconWorld * menuRadiusInIcons);

        var go = new GameObject("Option_" + title);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * iconWorld;

        var bg = go.AddComponent<SpriteRenderer>();
        bg.sprite = SquareSprite();
        bg.sortingOrder = 30;
        if (spriteMat != null) bg.sharedMaterial = spriteMat;

        if (icon != null)
        {
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            iconGo.transform.localScale = Vector3.one * 1.3f;
            var sr = iconGo.AddComponent<SpriteRenderer>();
            sr.sprite = icon;
            sr.sortingOrder = 31;
            if (spriteMat != null) sr.sharedMaterial = spriteMat;
        }

        options.Add(new Option
        {
            kind = kind, towerIndex = towerIndex, pos = pos, go = go, bg = bg,
            baseColor = color, title = title, cost = cost
        });
    }

    private void RefreshOptions()
    {
        for (int i = 0; i < options.Count; i++)
        {
            Option o = options[i];

            bool allowed = o.kind != Kind.Upgrade || (openNode.Tower != null && openNode.Tower.CanUpgrade);
            bool affordable = o.kind == Kind.Sell || GameManager.Money >= o.cost;
            o.enabled = allowed && affordable;

            Color c = o.enabled ? o.baseColor : new Color(0.3f, 0.3f, 0.3f, 0.85f);
            if (i == pending) c = Color.Lerp(c, Color.yellow, 0.55f);
            o.bg.color = c;
            o.go.transform.localScale = Vector3.one * (iconWorld * (i == pending ? 1.1f : 1f));
        }
        UpdateRange();
    }

    private void Activate(int i)
    {
        Option o = options[i];

        if (!o.enabled)
        {
            bool maxed = o.kind == Kind.Upgrade && openNode.Tower != null && !openNode.Tower.CanUpgrade;
            Say(maxed ? "Max level" : "Not enough gold");
            return;
        }

        if (pending != i)   // click lần 1: chỉ chọn, hiện giá + tầm bắn
        {
            pending = i;
            UpdateRange();
            return;
        }

        switch (o.kind)     // click lần 2: xác nhận
        {
            case Kind.Build:
            {
                TowerDefinition def = towerTypes[o.towerIndex];
                GameManager.Money -= def.levels[0].cost;
                GameObject go = Instantiate(towerPrefab, openNode.Center, Quaternion.identity);
                Tower tower = go.GetComponent<Tower>();
                tower.Setup(def);
                openNode.Tower = tower;
                break;
            }
            case Kind.Upgrade:
            {
                GameManager.Money -= openNode.Tower.UpgradeCost;
                openNode.Tower.Upgrade();
                break;
            }
            case Kind.Sell:
            {
                GameManager.Money += o.cost;
                Destroy(openNode.Tower.gameObject);
                openNode.Tower = null;
                break;
            }
        }
        CloseMenu();
    }

    private void CloseMenu()
    {
        foreach (Option o in options)
            if (o.go != null) Destroy(o.go);
        options.Clear();
        openNode = null;
        pending = -1;
        if (rangeRenderer != null) rangeRenderer.enabled = false;
    }

    // Vòng tròn tầm bắn: tháp đang chọn / loại tháp đang chọn xây / tầm bắn sau khi nâng cấp
    private void UpdateRange()
    {
        if (rangeRenderer == null || openNode == null) return;

        float r = 0f;
        if (openNode.Occupied)
        {
            Tower t = openNode.Tower;
            r = t.Range;
            if (pending >= 0 && options[pending].kind == Kind.Upgrade) r = t.NextRange;
        }
        else if (pending >= 0 && options[pending].kind == Kind.Build)
        {
            r = towerTypes[options[pending].towerIndex].levels[0].range;
        }

        rangeRenderer.enabled = r > 0f;
        rangeRenderer.transform.position = openNode.Center;
        rangeRenderer.transform.localScale = Vector3.one * (r * 2f);
    }

    private void Say(string msg) { message = msg; messageTimer = 1.5f; }

    // ---------------------------------------------------------------- sprite tạo bằng code
    private static Sprite SquareSprite()
    {
        if (squareSprite == null)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            squareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        return squareSprite;
    }

    private static Sprite CircleSprite()
    {
        if (circleSprite == null)
        {
            const int N = 128;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            float c = (N - 1) / 2f;
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < N; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c;
                    float a = d > 1f ? 0f : (d > 0.95f ? 0.9f : 0.15f);   // viền đậm, ruột mờ
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            circleSprite = Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N);   // đường kính = 1 unit
        }
        return circleSprite;
    }

    // ---------------------------------------------------------------- chữ + HUD tạm (IMGUI)
    // Đủ để test ngay. Sau này thay bằng UI Canvas cho đẹp.
    private void OnGUI()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsGameplayActive) return;

        if (labelStyle == null)
            labelStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };

        if (messageTimer > 0f)
            DrawText(new Rect(10, 36, 400, 28), message, new Color(1f, 0.6f, 0.4f), TextAnchor.MiddleLeft);

        if (options.Count == 0 || openNode == null) return;

        // Mỗi icon chỉ ghi giá (ngắn) ngay bên dưới để không dính nhau.
        // Tên + "click again" của icon đang chọn nằm 1 dòng riêng phía trên menu.
        labelStyle.fontSize = fontSize;
        float topY = float.MaxValue;
        for (int i = 0; i < options.Count; i++)
        {
            Option o = options[i];
            Vector3 sp = cam.WorldToScreenPoint(o.pos);
            float cy = Screen.height - sp.y;   // y kiểu IMGUI (gốc ở trên)
            topY = Mathf.Min(topY, cy);

            string text = o.kind == Kind.Sell ? $"+{o.cost}"
                        : (o.title == "MAX" ? "MAX" : o.cost.ToString());
            Color col = !o.enabled ? new Color(0.65f, 0.65f, 0.65f)
                      : (i == pending ? Color.yellow : Color.white);

            DrawText(new Rect(sp.x - iconPx * 0.6f, cy + iconPx * 0.5f, iconPx * 1.2f, fontSize + 6f),
                     text, col, TextAnchor.UpperCenter);
        }

        if (pending >= 0 && options[pending].enabled)
        {
            Option o = options[pending];
            Vector3 c = cam.WorldToScreenPoint(openNode.Center);
            string head = o.kind == Kind.Build ? LocalizationManager.GetTowerDisplayName(o.title) : (o.kind == Kind.Upgrade ? LocalizationManager.Upgrade : LocalizationManager.Sell);
            DrawText(new Rect(c.x - 120f, topY - iconPx * 0.5f - fontSize - 10f, 240f, fontSize + 6f),
                     head + " - " + LocalizationManager.ClickAgain, Color.yellow, TextAnchor.LowerCenter);
        }
    }

    private void DrawText(Rect r, string text, Color color, TextAnchor anchor)
    {
        labelStyle.alignment = anchor;
        labelStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), text, labelStyle);
        labelStyle.normal.textColor = color;
        GUI.Label(r, text, labelStyle);
    }
}
