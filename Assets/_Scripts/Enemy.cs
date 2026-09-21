using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Chỉ số của Quái")]
    public float speed = 3f;

    private int targetIndex = 0;
    private Transform targetPoint; // Chỉ cần Transform (GPS), không cần nguyên cái GameObject

    void Start()
    {
        // Vừa sinh ra là nhắm tới cái mốc đầu tiên trong mảng
        targetPoint = Waypoints.points[0]; 
    }

    void Update()
    {
        // Di chuyển cái xe (quái) về phía GPS (targetPoint)
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        // Nếu đi đến nơi (Khoảng cách cực kỳ nhỏ)
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            targetIndex++; // Nhảy số mốc

            // Nếu đã đi qua hết tất cả các mốc (Vào tới lâu đài)
            if (targetIndex >= Waypoints.points.Length)
            {
                Destroy(gameObject); // Tiêu diệt toàn bộ cái xe (quái)
                return; // Phanh gấp, ngắt lệnh ngay lập tức để không chạy dòng code bên dưới nữa
            }
            
            // Cập nhật mục tiêu mới để đi tiếp
            targetPoint = Waypoints.points[targetIndex]; 
        }
    }
}