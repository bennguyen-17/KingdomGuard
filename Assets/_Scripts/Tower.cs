using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Dữ liệu của Tháp")]
    public TowerData data; // Lát nữa bạn sẽ kéo file ArcherTowerData vào ô này

    private Transform target; // Mục tiêu hiện tại tháp đang ngắm tới
    [Header("Bắn súng")]
    public GameObject bulletPrefab; // Nơi kéo khuôn đúc viên đạn vào
    private float fireCountdown = 0f; // Bộ đếm ngược thời gian chờ giữa 2 lần bắn
    void Start()
    {
        // Ra lệnh cho Tháp cứ 0.5 giây thì quét tìm quái 1 lần (để tối ưu, không bị lag game)
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget()
    {
        // 1. Tìm TẤT CẢ các vật thể có tag là "Enemy" trên bản đồ
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        // 2. Đo khoảng cách xem con nào gần Tháp nhất
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        // 3. Kiểm tra xem con quái gần nhất có nằm trong Tầm Bắn (attackRange) không
        if (nearestEnemy != null && shortestDistance <= data.attackRange)
        {
            target = nearestEnemy.transform;
        }
        else
        {
            target = null; // Quái đi ra ngoài tầm bắn hoặc chết thì bỏ ngắm
        }
    }

    void Update()
    {
        if (target == null) return; // Không có quái thì ngồi chơi

        // Nếu hết thời gian chờ -> Bắn!
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = data.fireRate; // Reset thời gian chờ (VD: 0.5s)
        }

        // Trừ lùi thời gian chờ liên tục
        fireCountdown -= Time.deltaTime;
    }

    void Shoot()
    {
        // Tạo ra 1 viên đạn ngay tại vị trí của Tháp
        GameObject bulletGO = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Gọi viên đạn và ra lệnh: "Bay tới cắn con quái kia cho tao!"
        Bullet bullet = bulletGO.GetComponent<Bullet>();
               if (bullet != null)
        {
            // Truyền mục tiêu và sát thương (lấy từ file ArcherTowerData) cho viên đạn
            bullet.Seek(target, data.damage);
        }
    }
    // Hàm này giúp vẽ một vòng tròn tầm bắn màu đỏ trong màn hình Unity để bạn dễ quan sát
    void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.attackRange);
        }
    }
}