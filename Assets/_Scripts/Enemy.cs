using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Chỉ số của Quái")]
    public float speed = 3f;

    private int targetIndex = 0;
    private Transform targetPoint;
    private float health = 100f;
    void Start()
    {
        targetPoint = Waypoints.points[0];
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetIndex++;

            if (targetIndex >= Waypoints.points.Length)
            {
                GameManager.Lives -= 1; 
                Destroy(gameObject);
                return;
            }

            targetPoint = Waypoints.points[targetIndex];
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
        GameManager.Money += 25; 
        Destroy(gameObject);
    }
}