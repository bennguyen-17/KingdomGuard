using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public TowerDefinition Definition { get; private set; }
    public int Level { get; private set; }
    public int TotalSpent { get; private set; }   // để tính tiền hoàn khi bán

    private TowerData Stats => Definition.levels[Level];
    public float Range => Definition != null ? Stats.range : 0f;
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
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.25f);
    }

    private void UpdateTarget()
    {
        if (Definition == null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortest = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject enemy in enemies)
        {
            float d = Vector2.Distance(transform.position, enemy.transform.position);
            if (d < shortest) { shortest = d; nearest = enemy.transform; }
        }

        target = (nearest != null && shortest <= Range) ? nearest : null;
    }

    private void Update()
    {
        if (Definition == null || target == null) return;

        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = Mathf.Max(0.05f, Stats.shootInterval);
        }
    }

    private void Shoot()
    {
        GameObject prefab = Stats.bulletPrefab;
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, transform.position, Quaternion.identity);
        if (go.TryGetComponent(out Bullet bullet))
            bullet.Init(target, Stats.damage, Stats.projectileSpeed);
    }
}
