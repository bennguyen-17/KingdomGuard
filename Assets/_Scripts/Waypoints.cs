using UnityEngine;

public class Waypoints : MonoBehaviour
{
    // Lưu thẳng Transform (Định vị) để quái đọc cho nhanh
    public static Transform[] points; 

    // Dùng Awake để đảm bảo đường đi được gom lại TRƯỚC KHI quái đẻ ra
    void Awake() 
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }

    // Vẽ sợi dây tàng hình màu xám giúp bạn dễ nhìn Map
    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i < transform.childCount - 1)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
            }
        }
    }
}