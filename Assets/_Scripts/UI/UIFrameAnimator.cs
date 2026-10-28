using UnityEngine;
using UnityEngine.UI;

public class UIFrameAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 8f;
    [SerializeField] private bool loop = true;
    [SerializeField] private int initialFrameOffset = 0;

    private Image image;
    private float timer;
    private int currentFrame;

    public void Setup(Sprite[] animFrames, float fps = 8f, bool isLooping = true, int startOffset = 0)
    {
        frames = animFrames;
        frameRate = fps;
        loop = isLooping;
        initialFrameOffset = startOffset;
        currentFrame = startOffset;
        if (image == null) image = GetComponent<Image>();
        if (image != null && frames != null && frames.Length > 0)
        {
            int idx = Mathf.Clamp(currentFrame, 0, frames.Length - 1);
            image.sprite = frames[idx];
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        currentFrame = initialFrameOffset;
        if (image != null && frames != null && frames.Length > 0)
        {
            int idx = Mathf.Clamp(currentFrame, 0, frames.Length - 1);
            image.sprite = frames[idx];
        }
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0 || image == null) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            currentFrame++;
            if (currentFrame >= frames.Length)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    enabled = false;
                    return;
                }
            }
            image.sprite = frames[currentFrame];
        }
    }
}
