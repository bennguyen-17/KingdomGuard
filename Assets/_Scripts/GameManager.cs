using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Dòng này giúp các Script khác (Quái, Tháp) gọi GameManager cực kỳ dễ dàng
    public static GameManager Instance; 

    [Header("Chỉ số người chơi")]
    public int gold = 100;
    public int castleHealth = 20;

    void Awake()
    {
        Instance = this;
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log("💰 Cộng tiền! Tiền hiện tại: " + gold);
    }

    public void TakeCastleDamage(int damage)
    {
        castleHealth -= damage;
        Debug.Log("💔 Lâu đài bị cắn! Máu còn: " + castleHealth);
        
        if (castleHealth <= 0)
        {
            Debug.Log("💀 GAME OVER!!!");
        }
    }
}