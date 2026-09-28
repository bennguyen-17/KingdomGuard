using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    public GameObject bulletPrefab;
    private Transform target;

    // Các biến lấy từ TowerData (hoặc dùng trực tiếp nếu chưa gắn TowerData)
    private float range => data != null ? data.range : 4f;
    private float fireRate => data != null ? (1f / data.shootInterval) : 1f;
    private float fireCountdown = 0f;

    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }
    
    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        float shortestDistance = Mathf.Infinity; 
        GameObject nearestEnemy = null;
        
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy; 
                nearestEnemy = enemy;
            }
        }
        
        if (nearestEnemy != null && shortestDistance <= range)
            target = nearestEnemy.transform;
        else
            target = null;
    }
   
    void Update()
    {
        if (target == null) return; 
        
        fireCountdown -= Time.deltaTime; 
        if (fireCountdown <= 0f)
        {
            Shoot(); 
            fireCountdown = 1f / fireRate; 
        }
    }
    
       void Shoot()
    {
        GameObject bulletGO = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        
        Bullet bulletScript = bulletGO.GetComponent<Bullet>();
        
        if (bulletScript != null)
        {
            bulletScript.Seek(target);
        }
    }
}