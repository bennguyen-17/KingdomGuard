using UnityEngine;

[CreateAssetMenu(fileName = "NewTower", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string towerName;       // Tên tháp (VD: Tháp Cung)
    public int cost;               // Giá tiền xây tháp
    
    [Header("Chỉ số chiến đấu")]
    public int damage;             // Sát thương mỗi phát bắn
    public float attackRange;      // Tầm bắn (bán kính)
    public float fireRate;         // Thời gian giữa 2 lần bắn (VD: 0.5 giây)
    
    [Header("Đồ họa")]
    public GameObject towerPrefab; // File hình ảnh/Prefab của tháp sẽ kéo vào đây
}