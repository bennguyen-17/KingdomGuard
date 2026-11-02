using UnityEngine;

// Một LOẠI tháp (Archer / Cannon / Magic) gồm nhiều cấp.
[CreateAssetMenu(fileName = "TowerDefinition", menuName = "Scriptable Objects/TowerDefinition")]
public class TowerDefinition : ScriptableObject
{
    public string towerName = "Archer";
    public Sprite icon;
    [Tooltip("levels[0] = lúc xây, levels[1], levels[2]... = các cấp nâng")]
    public TowerData[] levels;
}
