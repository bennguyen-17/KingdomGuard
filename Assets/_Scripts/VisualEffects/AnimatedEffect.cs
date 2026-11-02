using UnityEngine;

public class AnimatedEffect : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 18f;
    [SerializeField] private bool destroyOnFinish = true;
    [SerializeField] private Vector3 scaleVelocity = Vector3.zero;

    private SpriteRenderer sr;
    private float timer;
    private int currentFrame;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 25; // Hiển thị trên đầu quái và tháp
        }
    }

    public void Setup(Sprite[] animFrames, float fps = 18f, int sortingOrder = 25)
    {
        frames = animFrames;
        frameRate = fps;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = sortingOrder;
            if (frames != null && frames.Length > 0)
                sr.sprite = frames[0];
        }
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (scaleVelocity != Vector3.zero)
        {
            transform.localScale += scaleVelocity * Time.deltaTime;
        }

        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            currentFrame++;
            if (currentFrame >= frames.Length)
            {
                if (destroyOnFinish)
                {
                    Destroy(gameObject);
                    return;
                }
                currentFrame = 0;
            }

            if (sr != null && currentFrame < frames.Length)
            {
                sr.sprite = frames[currentFrame];
            }
        }
    }
}
