using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Gold Display")]
    [SerializeField] private Text goldText;

    [Header("Lives Display")]
    [SerializeField] private Text livesText;

    [Header("Wave Info")]
    [SerializeField] private Text waveTitleText;
    [SerializeField] private Text waveSubText;

    [Header("Wave Action Button")]
    [SerializeField] private Button waveActionButton;
    [SerializeField] private Text waveActionButtonText;
    [SerializeField] private Image waveActionButtonFace;
    [SerializeField] private Text tipText;

    private WaveSpawner waveSpawner;

    private void Start()
    {
        if (waveActionButton != null)
        {
            waveActionButton.onClick.AddListener(OnActionButtonClicked);
        }
    }

    private void Update()
    {
        // 1. Cập nhật Vàng
        if (goldText != null)
            goldText.text = GameManager.Money.ToString();

        // 2. Cập nhật Máu Lâu đài
        if (livesText != null)
        {
            livesText.text = $"{GameManager.Lives} / 20";
            livesText.color = GameManager.Lives <= 5 ? new Color(1f, 0.35f, 0.35f) : Color.white;
        }

        // 3. Cập nhật Tiến trình Wave
        if (waveSpawner == null)
            waveSpawner = FindAnyObjectByType<WaveSpawner>();

        if (waveSpawner != null)
        {
            int total = waveSpawner.TotalWaves;
            int cur = waveSpawner.CurrentWaveIndex;

            if (waveSpawner.IsPreparingWave1)
            {
                bool hasTower = FindObjectsByType<Tower>().Length > 0;
                if (waveTitleText != null) waveTitleText.text = LocalizationManager.WaveTitle(1, total);
                if (waveSubText != null) waveSubText.text = LocalizationManager.PrepPhase;

                if (!hasTower)
                {
                    if (waveActionButtonText != null) waveActionButtonText.text = LocalizationManager.NeedTowerFirst;
                    if (waveActionButtonFace != null) waveActionButtonFace.color = new Color32(90, 100, 110, 240); // Xám khóa nút
                    if (waveActionButton != null) waveActionButton.interactable = false;
                    if (tipText != null)
                    {
                        tipText.gameObject.SetActive(true);
                        tipText.text = LocalizationManager.TipMustPlaceTower;
                        tipText.color = new Color32(245, 215, 110, 255); // Màu vàng ấm nhắc nhở
                    }
                }
                else
                {
                    if (waveActionButtonText != null) waveActionButtonText.text = LocalizationManager.StartWaveBtn(1);
                    if (waveActionButtonFace != null) waveActionButtonFace.color = new Color32(46, 125, 50, 255); // Xanh lá sẵn sàng
                    if (waveActionButton != null) waveActionButton.interactable = true;
                    if (tipText != null)
                    {
                        tipText.gameObject.SetActive(true);
                        tipText.text = LocalizationManager.TipPlaceTower;
                        tipText.color = Color.white;
                    }
                }
            }
            else if (waveSpawner.IsWaitingForNextWave)
            {
                int sec = Mathf.CeilToInt(waveSpawner.NextWaveTimer);
                if (waveTitleText != null) waveTitleText.text = LocalizationManager.WaveTitle(cur, total);
                if (waveSubText != null) waveSubText.text = LocalizationManager.WaveBreak(sec);
                if (waveActionButtonText != null) waveActionButtonText.text = LocalizationManager.EarlyStartBtn(sec);
                if (waveActionButtonFace != null) waveActionButtonFace.color = new Color32(211, 84, 0, 255); // Cam
                if (waveActionButton != null) waveActionButton.interactable = true;
                if (tipText != null)
                {
                    tipText.gameObject.SetActive(true);
                    tipText.text = LocalizationManager.TipSkipBreak(sec);
                }
            }
            else
            {
                int remaining = waveSpawner.AliveEnemiesCount;
                if (waveTitleText != null) waveTitleText.text = LocalizationManager.WaveTitle(cur, total);
                if (waveSubText != null) waveSubText.text = LocalizationManager.EnemiesLeft(remaining);
                if (waveActionButtonText != null) waveActionButtonText.text = LocalizationManager.BattleAction(remaining);
                if (waveActionButtonFace != null) waveActionButtonFace.color = new Color32(44, 62, 80, 220); // Xám đậm
                if (waveActionButton != null) waveActionButton.interactable = false;
                if (tipText != null) tipText.gameObject.SetActive(false);
            }
        }
    }

    private void OnActionButtonClicked()
    {
        if (waveSpawner == null)
            waveSpawner = FindAnyObjectByType<WaveSpawner>();
        if (waveSpawner == null) return;

        if (waveSpawner.IsPreparingWave1)
        {
            waveSpawner.StartFirstWave();
        }
        else if (waveSpawner.IsWaitingForNextWave)
        {
            waveSpawner.StartWaveEarly();
        }
    }
}
