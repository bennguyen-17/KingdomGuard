using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Waypoints : MonoBehaviour
{
    [HideInInspector]
    public Transform[] points;

    void Awake()
    {
        InitializePoints();
    }

    public void InitializePoints()
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }

    public Transform GetPoint(int index)
    {
        if (points == null || points.Length == 0) InitializePoints();
        if (points != null && index >= 0 && index < points.Length)
        {
            return points[index];
        }
        return null;
    }

    public int PointCount
    {
        get
        {
            if (points == null || points.Length == 0) InitializePoints();
            return points != null ? points.Length : 0;
        }
    }

#if UNITY_EDITOR
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
#endif
}