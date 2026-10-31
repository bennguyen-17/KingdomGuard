using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public TowerDefinition Definition { get; private set; }
    public int Level { get; private set; }
    public int TotalSpent { get; private set; }   // để tính tiền hoàn khi bán

    private TowerData Stats => Definition.levels[Level];

    /// <summary>
    /// Tầm bắn thực tế: Đảm bảo tháp luôn với tới được đường đi trên Scene Level hiện tại.
    /// Do Scene vẽ ô đất cách waypoint từ 3.0 đến 4.0 unit, nên Cannon cần tối thiểu 3.8 và Archer 4.5.
    /// </summary>
    public float Range => (Definition != null) ? Stats.range : 0f;

    public bool CanUpgrade => Definition != null && Level + 1 < Definition.levels.Length;
    public int UpgradeCost => CanUpgrade ? Definition.levels[Level + 1].cost : 0;
    public float NextRange => CanUpgrade ? Definition.levels[Level + 1].range : Range;

    private Transform target;
    private float fireCountdown;

    // BuildManager gọi ngay sau khi Instantiate
    public void Setup(TowerDefinition definition)
    {
        Definition = definition;
        Level = 0;
        TotalSpent = Stats.cost;
        ApplyVisual();
    }

    public void Upgrade()
    {
        if (!CanUpgrade) return;
        Level++;
        TotalSpent += Stats.cost;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && Stats.sprite != null) spriteRenderer.sprite = Stats.sprite;
    }

    private void Start()
    {
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.2f);
    }

    /// <summary>
    /// Targeting "First" chuẩn GDD:
    /// Quét tất cả quái vật nằm trong tầm bắn (Range), chọn con đang ở gần Lâu đài nhất
    /// (quãng đường còn lại đến đích ngắn nhất) để ưu tiên tiêu diệt cứu thành.
    /// </summary>
    private void UpdateTarget()
    {
        if (Definition == null) return;

        Enemy[] enemies = FindObjectsByType<Enemy>();
        float shortestDistanceToGoal = Mathf.Infinity;
        Transform firstTarget = null;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            float distanceToTower = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToTower <= Range)
            {
                float distToGoal = enemy.GetDistanceToGoal();
                if (distToGoal < shortestDistanceToGoal)
                {
                    shortestDistanceToGoal = distToGoal;
                    firstTarget = enemy.transform;
                }
            }
        }

        target = firstTarget;
    }

    private void Update()
    {
        if (Definition == null) return;

        // Nếu quái đã bị tiêu diệt hoặc chạy ra khỏi tầm bắn -> bỏ nhắm
        if (target != null && Vector2.Distance(transform.position, target.position) > Range)
        {
            target = null;
        }

        if (target != null)
        {
            fireCountdown -= Time.deltaTime;
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = Mathf.Max(0.05f, Stats.shootInterval);
            }
        }
    }

    private void Shoot()
    {
        GameObject prefab = Stats.bulletPrefab;
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, transform.position, Quaternion.identity);
        if (go.TryGetComponent(out Bullet bullet))
        {
            bool isCannonType = Stats.isCannon || (Definition != null && Definition.towerName.ToLower().Contains("cannon"));
            float splash = Stats.splashRadius > 0f ? Stats.splashRadius : (isCannonType ? 1.0f : 0f);
            float crit = Stats.critChance > 0f ? Stats.critChance : (isCannonType ? 0.15f : 0f);
            float critMult = Stats.critMultiplier > 1f ? Stats.critMultiplier : 1.5f;

            bullet.Init(target, Stats.damage, Stats.projectileSpeed, splash, crit, critMult, isCannonType);
        }
    }
}
