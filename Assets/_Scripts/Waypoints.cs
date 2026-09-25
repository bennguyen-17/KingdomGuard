using UnityEditor;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    public static Transform[] points; 

    void Awake() 
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(transform.GetChild(i).position, i.ToString(), style);
            if (i < transform.childCount - 1)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i + 1).position);
            }
        }
    }
}