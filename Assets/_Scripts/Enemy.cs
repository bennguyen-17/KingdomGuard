using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Chỉ số")]
    public float speed = 3f; // Tốc độ chạy
    public int health = 50;
    public int goldReward = 10; // Giết con này được bao nhiêu tiền
    [Header("Đường đi")]
    public Transform[] waypoints; // Khai báo một mảng (danh sách) các điểm mốc

    private int targetIndex = 0; // Đang đi tới mốc số mấy?

    void Update()
    {
        // 1. Nếu quái đã đi qua hết tất cả các mốc (chạm Lâu đài)
        if (targetIndex >= waypoints.Length)
        {
            GameManager.Instance.TakeCastleDamage(1); // Trừ 1 máu Lâu đài
            Destroy(gameObject);
            return;
        }

        // 2. Xác định điểm mốc tiếp theo cần đi tới
        Transform targetPoint = waypoints[targetIndex];

        // 3. Lệnh xê dịch Quái tiến về phía mốc một cách từ từ
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        // 4. Nếu Quái đã đi đến nơi (khoảng cách còn rất nhỏ) -> Đổi mục tiêu sang mốc tiếp theo
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetIndex++; // Tăng mốc lên (VD: 1 thành 2, 2 thành 3)
        }
    }
    public void TakeDamage(int damage)
    {
        health -= damage; // Bị trừ máu

        if (health <= 0)
        {
            GameManager.Instance.AddGold(goldReward); // Gọi GameManager cộng tiền
            Destroy(gameObject); // Chết
        }
    }
}