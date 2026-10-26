using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float damage = 20f;
    public float splashRadius = 0f;
    public bool isCannon = false;
    public bool isCrit = false;

    private Vector3 lastKnownTargetPos;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Đảm bảo đạn luôn hiển thị nổi lên trên toàn bộ Tilemap (order 0-3) và quái/tháp (order 10)
            spriteRenderer.sortingOrder = 5;
        }

        // Tăng cỡ đạn một chút để người chơi dễ quan sát đường bay
        transform.localScale = Vector3.one * 0.8f;
    }

    public void Seek(Transform _target)
    {
        target = _target;
        if (target != null) lastKnownTargetPos = target.position;
    }

    public void Init(Transform _target, float _damage, float _speed)
    {
        Init(_target, _damage, _speed, 0f, 0f, 1.5f, false);
    }

    public void Init(Transform _target, float _damage, float _speed, float _splashRadius, float _critChance, float _critMultiplier, bool _isCannon)
    {
        target = _target;
        speed = _speed > 0f ? _speed : 10f;
        splashRadius = _splashRadius;
        isCannon = _isCannon;

        // Tính toán tỉ lệ bạo kích (GDD: Cannon có 15% bạo kích x1.5 sát thương)
        if (_critChance > 0f && Random.value < _critChance)
        {
            damage = _damage * _critMultiplier;
            isCrit = true;
        }
        else
        {
            damage = _damage;
            isCrit = false;
        }

        if (target != null)
        {
            lastKnownTargetPos = target.position;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            lastKnownTargetPos = target.position;
        }
        else
        {
            // Edge case GDD: Nếu quái chết khi đạn đang bay:
            // - Đạn Archer biến mất ngay
            // - Đạn Cannon bay tới vị trí cuối cùng của mục tiêu rồi nổ
            if (!isCannon)
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector3 destination = target != null ? target.position : lastKnownTargetPos;
        Vector3 dir = destination - transform.position;

        // Xoay đầu đạn theo hướng bay
        if (dir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) <= 0.2f)
        {
            ExplodeOrHit();
        }
    }

    private void ExplodeOrHit()
    {
        if (splashRadius > 0f)
        {
            // Sát thương nổ diện rộng AOE (Tháp Pháo)
            Enemy[] allEnemies = FindObjectsByType<Enemy>();
            foreach (Enemy e in allEnemies)
            {
                if (e == null) continue;
                if (Vector2.Distance(transform.position, e.transform.position) <= splashRadius)
                {
                    e.TakeDamage(damage, isExplosion: true);
                }
            }
        }
        else
        {
            // Sát thương đơn mục tiêu (Tháp Cung)
            if (target != null && target.TryGetComponent(out Enemy singleEnemy))
            {
                singleEnemy.TakeDamage(damage, isExplosion: false);
            }
        }

        Destroy(gameObject);
    }
}
