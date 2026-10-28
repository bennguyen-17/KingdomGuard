using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Chỉ số của Quái (GDD)")]
    public float maxHealth = 50f;
    public float speed = 2f;
    public int goldReward = 5;
    public int damageToCastle = 1;
    public bool isFlying = false;

    [Header("Visual Effects Prefabs")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private GameObject coinDropPrefab;

    private float currentHealth;
    private int targetIndex = 0;
    private Transform targetPoint;
    private WaveSpawner waveSpawner;
    private bool removedFromWave;
    private Waypoints path;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    public void Initialize(WaveSpawner spawner, Waypoints assignedPath)
    {
        waveSpawner = spawner;
        path = assignedPath;

        if (path != null && path.PointCount > 0)
        {
            targetPoint = path.GetPoint(0);
            if (targetPoint != null)
            {
                transform.position = targetPoint.position;
            }
        }
    }

    public void Configure(EnemyData data, float hpMultiplier = 1f)
    {
        if (data == null) return;
        maxHealth = data.baseHealth * hpMultiplier;
        currentHealth = maxHealth;
        speed = data.speed;
        goldReward = data.goldReward;
        damageToCastle = data.damageToCastle;
        isFlying = data.isFlying;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (data.sprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = data.sprite;
            spriteRenderer.color = data.tintColor;
        }

        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null && !string.IsNullOrEmpty(data.animationStateName))
        {
            animator.Play(data.animationStateName);
        }
    }

    public void ConfigureDirect(string enemyName, float baseHp, float spd, int reward, int castleDmg, float hpMultiplier = 1f, string animName = "Goblin_Walk", bool flying = false)
    {
        maxHealth = baseHp * hpMultiplier;
        currentHealth = maxHealth;
        speed = spd;
        goldReward = reward;
        damageToCastle = castleDmg;
        isFlying = flying;

        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null && !string.IsNullOrEmpty(animName))
        {
            animator.Play(animName);
        }

        // Tự động phân biệt ngoại hình rõ ràng nếu dùng chung Prefab
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (enemyName == "Orc")
        {
            transform.localScale = Vector3.one * 0.75f; // Orc to con hơn hẳn Goblin (trâu máu 150)
            if (spriteRenderer != null) spriteRenderer.color = new Color(0.95f, 0.45f, 0.45f, 1f); // Nhuộm đỏ cam
        }
        else
        {
            transform.localScale = Vector3.one * 0.5f; // Goblin nhỏ con, nhanh nhẹn (máu 50)
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
        }
    }

    private void Start()
    {
        if (targetPoint == null && path != null && path.PointCount > 0)
        {
            targetPoint = path.GetPoint(0);
        }
    }

    private void Update()
    {
        if (targetPoint == null) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetIndex++;

            if (path == null || targetIndex >= path.PointCount)
            {
                // Quái đã vào tới Lâu đài -> Trừ máu theo GDD (Goblin: 1, Orc: 2)
                GameManager.Lives = Mathf.Max(0, GameManager.Lives - damageToCastle);
                NotifyRemoved(false);
                Destroy(gameObject);
                return;
            }

            targetPoint = path.GetPoint(targetIndex);
        }
    }

    /// <summary>
    /// Tính quãng đường còn lại mà quái cần đi để chạm tới Lâu đài.
    /// Dùng cho cơ chế Targeting "First" (quái nào có khoảng cách này nhỏ nhất thì bị tháp ưu tiên bắn trước).
    /// </summary>
    public float GetDistanceToGoal()
    {
        if (path == null || path.PointCount == 0) return Vector2.Distance(transform.position, Vector2.zero);
        if (targetIndex >= path.PointCount) return 0f;

        float distance = Vector2.Distance(transform.position, path.GetPoint(targetIndex).position);
        for (int i = targetIndex; i < path.PointCount - 1; i++)
        {
            Transform pA = path.GetPoint(i);
            Transform pB = path.GetPoint(i + 1);
            if (pA != null && pB != null)
            {
                distance += Vector2.Distance(pA.position, pB.position);
            }
        }
        return distance;
    }

    public void TakeDamage(float amount, bool isExplosion = false)
    {
        // Quái bay (Bat) miễn nhiễm sát thương nổ từ Tháp Pháo
        if (isFlying && isExplosion) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (coinDropPrefab != null)
        {
            GameObject coinObj = Instantiate(coinDropPrefab, transform.position + Vector3.up * 0.2f, Quaternion.identity);
            if (coinObj.TryGetComponent(out CoinDropVFX vfx))
            {
                vfx.Init(goldReward);
            }
        }

        NotifyRemoved(true);
        Destroy(gameObject);
    }

    private void NotifyRemoved(bool defeated)
    {
        if (removedFromWave) return;
        removedFromWave = true;
        waveSpawner?.NotifyEnemyRemoved(defeated, goldReward);
    }
}
