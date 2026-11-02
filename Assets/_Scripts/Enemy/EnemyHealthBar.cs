using UnityEngine;

// Thanh máu nổi phía trên đầu quái.
// Enemy.Awake() tự gắn script này, không cần sửa prefab.
// Đổi showWhenFull = true nếu muốn thanh máu luôn hiện (kể cả khi quái còn đầy máu).
[RequireComponent(typeof(Enemy))]
public class EnemyHealthBar : MonoBehaviour
{
    public bool showWhenFull = false;   // false: chỉ hiện sau khi quái bị đánh

    private const float BarHeight = 0.12f;
    private const float BorderSize = 0.03f;
    private const float GapAboveHead = 0.15f;

    private static Sprite barSprite;

    private Enemy enemy;
    private SpriteRenderer enemyRenderer;
    private Transform root;
    private SpriteRenderer back;
    private SpriteRenderer fill;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemyRenderer = GetComponent<SpriteRenderer>();

        // Thanh máu là object độc lập (không làm con của quái) để không bị ảnh hưởng bởi scale / lật hình của quái
        root = new GameObject("HealthBar").transform;
        back = CreateBar("Back", new Color(0f, 0f, 0f, 0.75f), 0);
        fill = CreateBar("Fill", Color.green, 1);
        root.gameObject.SetActive(false);
    }

    private SpriteRenderer CreateBar(string barName, Color color, int orderOffset)
    {
        var go = new GameObject(barName);
        go.transform.SetParent(root, false);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BarSprite();
        sr.color = color;

        if (enemyRenderer != null)
        {
            sr.sharedMaterial = enemyRenderer.sharedMaterial;
            sr.sortingLayerID = enemyRenderer.sortingLayerID;
            sr.sortingOrder = enemyRenderer.sortingOrder + 5 + orderOffset;
        }
        else
        {
            sr.sortingOrder = 15 + orderOffset;
        }
        return sr;
    }

    // Sprite hình chữ nhật 1 unit, gốc ở mép TRÁI để co thanh theo chiều ngang cho dễ
    private static Sprite BarSprite()
    {
        if (barSprite == null)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            barSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }
        return barSprite;
    }

    private void LateUpdate()
    {
        float ratio = enemy.maxHealth > 0f ? Mathf.Clamp01(enemy.CurrentHealth / enemy.maxHealth) : 0f;

        bool visible = showWhenFull || ratio < 0.999f;
        if (root.gameObject.activeSelf != visible) root.gameObject.SetActive(visible);
        if (!visible) return;

        float spriteWidth = enemyRenderer != null ? enemyRenderer.bounds.size.x : 1f;
        float width = Mathf.Clamp(spriteWidth * 0.7f, 0.6f, 1.4f);
        float top = enemyRenderer != null ? enemyRenderer.bounds.max.y : transform.position.y + 0.5f;

        // root đặt ở mép trái của thanh, ngay trên đầu quái
        root.position = new Vector3(transform.position.x - width * 0.5f, top + GapAboveHead, transform.position.z);

        back.transform.localPosition = new Vector3(-BorderSize, 0f, 0f);
        back.transform.localScale = new Vector3(width + BorderSize * 2f, BarHeight + BorderSize * 2f, 1f);

        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = new Vector3(width * ratio, BarHeight, 1f);
        fill.color = Color.Lerp(new Color(0.85f, 0.15f, 0.15f), new Color(0.25f, 0.85f, 0.3f), ratio);
    }

    private void OnDestroy()
    {
        if (root != null) Destroy(root.gameObject);
    }
}
