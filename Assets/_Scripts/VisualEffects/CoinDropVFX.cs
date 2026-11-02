using UnityEngine;

public class CoinDropVFX : MonoBehaviour
{
    [SerializeField] private SpriteRenderer coinRenderer;
    [SerializeField] private TextMesh textMesh;

    private Vector3 velocity;
    private float lifeTime = 0.85f;
    private float elapsed;
    private Color startTextColor = new Color(1f, 0.84f, 0f, 1f); // Màu vàng gold

    public void Init(int goldAmount, Sprite coinSprite = null)
    {
        if (coinRenderer != null)
        {
            if (coinSprite != null) coinRenderer.sprite = coinSprite;
            coinRenderer.sortingOrder = 30;
        }

        if (textMesh != null)
        {
            textMesh.text = $"+{goldAmount}G";
            textMesh.color = startTextColor;
            textMesh.fontSize = 28;
            textMesh.characterSize = 0.08f;
            textMesh.anchor = TextAnchor.MiddleLeft;
            textMesh.alignment = TextAlignment.Left;
            var mr = textMesh.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 30;
        }

        // Tạo lực nảy nhẹ ngẫu nhiên sang trái/phải và bay lên trên
        float rx = Random.Range(-0.4f, 0.4f);
        velocity = new Vector3(rx, 1.8f, 0f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        velocity.y -= 2.2f * Time.deltaTime; // Giảm tốc độ dần theo trọng lực

        float t = elapsed / lifeTime;
        float alpha = Mathf.Clamp01(1f - t);

        if (coinRenderer != null)
        {
            Color c = coinRenderer.color;
            c.a = alpha;
            coinRenderer.color = c;
        }

        if (textMesh != null)
        {
            Color tc = startTextColor;
            tc.a = alpha;
            textMesh.color = tc;
        }

        if (elapsed >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
