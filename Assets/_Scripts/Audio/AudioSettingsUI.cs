using UnityEngine;
using UnityEngine.UI;

/// <summary>Compact persistent-access controls on the existing runtime canvas.</summary>
public class AudioSettingsUI : MonoBehaviour
{
    private GameObject panel;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.EnsureInstance();
        Button toggle = Button("SOUND", transform, new Vector2(132, 46), new Vector2(-24, -24));
        panel = Box("Sound Settings", transform, new Vector2(420, 248), new Vector2(-24, -82), new Color32(19, 27, 34, 255)).gameObject;
        Label("AUDIO", panel.transform, new Vector2(20, -16), new Vector2(380, 30), 22);
        AddSlider("Music", -62, audioManager.GetBGMVolume(), audioManager.SetBGMVolume);
        AddSlider("Effects", -124, audioManager.GetSFXVolume(), audioManager.SetSFXVolume);
        Button preview = Button("TEST SOUND", panel.transform, new Vector2(160, 40), new Vector2(-236, -190));
        preview.onClick.AddListener(() => audioManager.PlayUIClick());
        Button close = Button("CLOSE", panel.transform, new Vector2(120, 40), new Vector2(-20, -190));
        close.onClick.AddListener(() => SetOpen(false));
        toggle.onClick.AddListener(() => SetOpen(!panel.activeSelf));
        panel.SetActive(false);
    }

    public void SetOpen(bool open)
    {
        panel.SetActive(open);
        audioManager.SaveVolumeSettings();
        audioManager.PlayUIClick();
        if (open) panel.GetComponentInChildren<Slider>().Select();
    }

    private void AddSlider(string title, float y, float initial, UnityEngine.Events.UnityAction<float> changed)
    {
        Text label = Label(title, panel.transform, new Vector2(20, y), new Vector2(380, 26), 18);
        Image track = Box(title + " Slider", panel.transform, new Vector2(380, 24), new Vector2(-20, y - 27), new Color32(65, 77, 86, 255));
        Slider slider = track.gameObject.AddComponent<Slider>();
        Image handle = Box("Handle", track.transform, new Vector2(24, 32), Vector2.zero, new Color32(231, 190, 99, 255));
        RectTransform handleRect = handle.rectTransform;
        handleRect.anchorMin = handleRect.anchorMax = new Vector2(0, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(initial);
        label.text = title + "  " + Mathf.RoundToInt(initial * 100) + "%";
        slider.onValueChanged.AddListener(value => { changed(value); label.text = title + "  " + Mathf.RoundToInt(value * 100) + "%"; });
    }

    private static Image Box(string name, Transform parent, Vector2 size, Vector2 offset, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        RectTransform rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, 1);
        rect.sizeDelta = size;
        rect.anchoredPosition = offset;
        return image;
    }
    private static Text Label(string text, Transform parent, Vector2 offset, Vector2 size, int fontSize)
    {
        GameObject go = new GameObject(text, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text label = go.GetComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleLeft;
        label.raycastTarget = false;
        RectTransform rect = label.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.sizeDelta = size;
        rect.anchoredPosition = offset;
        return label;
    }
    private static Button Button(string text, Transform parent, Vector2 size, Vector2 offset)
    {
        Image face = Box(text, parent, size, offset, new Color32(76, 96, 74, 255));
        Button button = face.gameObject.AddComponent<Button>();
        button.targetGraphic = face;
        Text label = Label(text, face.transform, Vector2.zero, size, 17);
        label.alignment = TextAnchor.MiddleCenter;
        return button;
    }
    private void OnDestroy() { if (audioManager != null) audioManager.SaveVolumeSettings(); }
}
