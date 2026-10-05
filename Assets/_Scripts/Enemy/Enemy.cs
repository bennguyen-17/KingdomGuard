using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Chỉ số của Quái")]
    public float speed = 3f;

    private int targetIndex = 0;
    private Transform targetPoint;
    private float health = 100f;
    private WaveSpawner waveSpawner;
    private bool removedFromWave;
    private Waypoints path;

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

    void Start()
    {
        if (targetPoint == null && path != null && path.PointCount > 0)
        {
            targetPoint = path.GetPoint(0);
        }
    }

    void Update()
    {
        if (targetPoint == null) return;

        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetIndex++;

            if (path == null || targetIndex >= path.PointCount)
            {
                GameManager.Lives -= 1;
                NotifyRemoved(false);
                Destroy(gameObject);
                return;
            }

            targetPoint = path.GetPoint(targetIndex);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount; // Bị trừ máu

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        NotifyRemoved(true);
        Destroy(gameObject);
    }

    private void NotifyRemoved(bool defeated)
    {
        if (removedFromWave) return;
        removedFromWave = true;
        waveSpawner?.NotifyEnemyRemoved(defeated);
    }
}
