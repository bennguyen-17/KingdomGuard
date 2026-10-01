#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class KingdomGuardUISetup
{
    [MenuItem("Tools/KingdomGuard/1. Configure UI Sprites (Set to Sprite 2D)")]
    public static void ConfigureUISprites()
    {
        string[] uiAssetPaths = new string[]
        {
            "Assets/Art/UI/MainMenu_BG.jpg",
            "Assets/Art/UI/KingdomGuard_Logo.jpg",
            "Assets/Art/UI/Play_Button.jpg"
        };

        foreach (string path in uiAssetPaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                    Debug.Log($"[KingdomGuardUISetup] Configured '{path}' as Sprite (2D and UI).");
                }
            }
        }
    }

    [MenuItem("Tools/KingdomGuard/2. Setup MainMenu Scene UI")]
    public static void SetupMainMenuUIInCurrentScene()
    {
        ConfigureUISprites();

        MainMenuUI menuUI = Object.FindFirstObjectByType<MainMenuUI>();
        if (menuUI == null)
        {
            GameObject menuGO = new GameObject("MainMenuManager");
            menuUI = menuGO.AddComponent<MainMenuUI>();
            Undo.RegisterCreatedObjectUndo(menuGO, "Create MainMenuManager");
            Debug.Log("[KingdomGuardUISetup] Created MainMenuManager object in scene.");
        }

        menuUI.BuildMainMenuInEditor();

        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);

        Debug.Log("✅ MainMenu UI Canvas setup successfully completed in scene: " + activeScene.name);
    }

    [MenuItem("Tools/KingdomGuard/3. Setup WIN & LOSE UI in Scene")]
    public static void SetupWinLoseUIInCurrentScene()
    {
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmGO = new GameObject("GameManager");
            gm = gmGO.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gmGO, "Create GameManager");
            Debug.Log("[KingdomGuardUISetup] Created GameManager object in scene.");
        }

        UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            GameObject uiGO = new GameObject("UIManager");
            uiManager = uiGO.AddComponent<UIManager>();
            Undo.RegisterCreatedObjectUndo(uiGO, "Create UIManager");
            Debug.Log("[KingdomGuardUISetup] Created UIManager object in scene.");
        }

        uiManager.BuildUICanvasInEditor();

        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);

        Debug.Log("✅ WIN & LOSE UI Canvas setup successfully completed in scene: " + activeScene.name);
    }
}
#endif
