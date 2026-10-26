using UnityEngine;

// Chỉ số của MỘT cấp (level) của tháp. Giữ nguyên các field cũ, chỉ thêm field mới ở cuối
// nên asset cũ vẫn không bị hỏng.
[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    public float range;
    public float shootInterval;
    public float projectileSpeed;
    public float projectileDuration;
    public float damage;

    [Header("Kingdom Rush style")]
    [Tooltip("Level đầu = giá xây. Các level sau = giá nâng cấp LÊN level đó")]
    public int cost = 70;
    public Sprite sprite;
    public GameObject bulletPrefab;

    [Header("Đặc điểm vũ khí (GDD)")]
    [Tooltip("Bán kính nổ lan AOE (Tháp Pháo Cannon = 1.0)")]
    public float splashRadius = 0f;
    [Tooltip("Tỉ lệ bạo kích (Tháp Pháo Cannon = 0.15 = 15%)")]
    public float critChance = 0f;
    [Tooltip("Hệ số sát thương bạo kích (Tháp Pháo Cannon = 1.5x)")]
    public float critMultiplier = 1.5f;
    [Tooltip("Đánh dấu loại tháp là pháo để xử lý đạn bay dở nổ tại chỗ mục tiêu")]
    public bool isCannon = false;
}
