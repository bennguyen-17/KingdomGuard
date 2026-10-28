#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace KingdomGuard.Editor
{
    public static class ArtValidationHelper
    {
        [MenuItem("Tools/KingdomGuard/Art Setup & Validate")]
        public static void ValidateAndSetupArt()
        {
            Debug.Log("<color=cyan><b>[KingdomGuard Art Validation]</b> Starting Art & Animation inspection...</color>");

            int passCount = 0;
            int warningCount = 0;

            // 1. Validate UI Assets
            string[] requiredUI = new[]
            {
                "Assets/Art/UI/Icons/icon_gold.png",
                "Assets/Art/UI/Icons/icon_heart.png",
                "Assets/Art/UI/Icons/icon_tower_archer.png",
                "Assets/Art/UI/Icons/icon_tower_cannon.png",
                "Assets/Art/UI/Icons/icon_tower_magic.png",
                "Assets/Art/UI/Buttons/btn_start_wave.png",
                "Assets/Art/UI/Buttons/btn_early_wave.png",
                "Assets/Art/UI/Buttons/btn_pause.png",
                "Assets/Art/UI/HUD/panel_hud_bar.png"
            };

            foreach (string uiPath in requiredUI)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(uiPath);
                if (sprite != null)
                {
                    passCount++;
                }
                else
                {
                    Debug.LogWarning($"[Art Validation] Missing or unimported UI Sprite at {uiPath}");
                    warningCount++;
                }
            }

            // 2. Validate Tower Shooting Frames
            string[] shootingFrames = new[]
            {
                "Assets/Art/Custom_Towers/tower_archer_lv1_shoot_64x64.png",
                "Assets/Art/Custom_Towers/tower_cannon_lv1_shoot_64x64.png",
                "Assets/Art/Custom_Towers/tower_magic_lv1_shoot_64x64.png"
            };

            foreach (string shootPath in shootingFrames)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(shootPath);
                if (sprite != null)
                {
                    passCount++;
                }
                else
                {
                    Debug.LogWarning($"[Art Validation] Missing Tower Shooting Frame at {shootPath}");
                    warningCount++;
                }
            }

            // 3. Validate Enemy Prefabs & Controllers
            string[] enemyPrefabs = new[]
            {
                "Assets/Prefabs/Goblin.prefab",
                "Assets/Prefabs/Orc.prefab",
                "Assets/Prefabs/Bat.prefab",
                "Assets/Prefabs/TrollBoss.prefab"
            };

            foreach (string prefabPath in enemyPrefabs)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (go != null)
                {
                    var anim = go.GetComponent<Animator>();
                    if (anim != null && anim.runtimeAnimatorController != null)
                    {
                        passCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[Art Validation] {prefabPath} is missing an Animator Controller.");
                        warningCount++;
                    }
                }
                else
                {
                    Debug.LogWarning($"[Art Validation] Missing Enemy Prefab at {prefabPath}");
                    warningCount++;
                }
            }

            // 4. Validate Projectiles
            string[] projectilePrefabs = new[]
            {
                "Assets/Prefabs/Bullet_Arrow.prefab",
                "Assets/Prefabs/Bullet_Cannon.prefab",
                "Assets/Prefabs/Bullet_Magic.prefab"
            };

            foreach (string projPath in projectilePrefabs)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
                if (go != null) passCount++;
                else
                {
                    Debug.LogWarning($"[Art Validation] Missing Projectile Prefab at {projPath}");
                    warningCount++;
                }
            }

            Debug.Log($"<color=green><b>[KingdomGuard Art Validation]</b> Completed: {passCount} checks passed, {warningCount} warnings.</color>");
            AssetDatabase.Refresh();
        }
    }
}
#endif
