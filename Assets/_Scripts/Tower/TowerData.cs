using UnityEngine;

// Chỉ số của MỘT cấp (level) của tháp. Giữ nguyên các field cũ, chỉ thêm field mới ở cuối
// nên asset cũ (Archer.asset) vẫn không bị hỏng.
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
}
