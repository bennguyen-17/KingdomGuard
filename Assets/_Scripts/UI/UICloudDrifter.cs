using UnityEngine;

public class UICloudDrifter : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float minX = -1200f;
    [SerializeField] private float maxX = 1200f;
    [SerializeField] private float minY = 120f;
    [SerializeField] private float maxY = 420f;
    [SerializeField] private float floatAmplitude = 6f;
    [SerializeField] private float floatFrequency = 0.4f;

    private RectTransform rect;
    private float startY;
    private float randomOffset;

    public void Setup(float driftSpeed, float initialY, float leftBound = -1200f, float rightBound = 1200f)
    {
        speed = driftSpeed;
        startY = initialY;
        minX = leftBound;
        maxX = rightBound;
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            startY = rect.anchoredPosition.y;
        }
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (rect == null) return;

        Vector2 pos = rect.anchoredPosition;
        pos.x += speed * Time.deltaTime;

        // Nhấp nhô nhẹ nhàng theo sóng sin tạo cảm giác lơ lửng trong gió
        float floatOffset = Mathf.Sin((Time.time + randomOffset) * floatFrequency) * floatAmplitude;
        pos.y = startY + floatOffset;

        // Khi bay hết màn hình bên phải -> quay lại mép trái với độ cao ngẫu nhiên
        if (pos.x > maxX)
        {
            pos.x = minX;
            startY = Random.Range(minY, maxY);
        }

        rect.anchoredPosition = pos;
    }
}
