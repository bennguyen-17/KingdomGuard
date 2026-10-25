using UnityEngine;

// Một chỗ đặt tháp (nhóm các ô basefortower liền nhau). Không gắn vào scene.
public class Node
{
    public Vector3 Center;
    public Tower Tower;
    public bool Occupied => Tower != null;
}
