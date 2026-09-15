using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    public float speed = 10f; // Tốc độ bay của đạn

    private int damage; // Thêm dòng này lên đầu

    // Sửa lại hàm Seek để nhận thêm sát thương từ Tháp truyền sang
    public void Seek(Transform _target, int _damage)
    {
        target = _target;
        damage = _damage;
    }

    void Update()
    {
        // Xử lý lỗi: Nếu quái đã chết trước khi đạn kịp bay tới -> Tự hủy viên đạn
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Bắt đầu bay về phía quái
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Kiểm tra xem đã đụng trúng quái chưa
        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        // Gõ đầu con quái và trừ máu nó
        Enemy enemyScript = target.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(damage);
        }

        Destroy(gameObject); // Đạn nổ biến mất
    }
}