using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Tower Defense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Thông tin Quái")]
    public string enemyName = "Goblin";
    public float baseHealth = 50f;
    public float speed = 2.0f;
    public int goldReward = 5;
    public int damageToCastle = 1;
    public bool isFlying = false;

    [Header("Visual")]
    public string animationStateName = "Goblin_Walk";
    public Sprite sprite;
    public Color tintColor = Color.white;
}
