using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class GameAssetBaker
{
    private static readonly string MarkerFile = "Temp/GameAssetBaker_v1.done";

    static GameAssetBaker()
    {
        EditorApplication.delayCall += CheckAndBake;
    }

    private static void CheckAndBake()
    {
        if (!File.Exists(MarkerFile))
        {
            BakeAllAssets();
            try { File.WriteAllText(MarkerFile, "v1"); } catch { }
        }
    }

    [MenuItem("Kingdom Guard/Tạo Toàn Bộ Prefab & Gán Hiệu Ứng Game (Bake All)")]
    public static void BakeAllAssets()
    {
        string prefabsDir = "Assets/Prefabs";
        if (!Directory.Exists(prefabsDir))
            Directory.CreateDirectory(prefabsDir);

        Debug.Log("[GameAssetBaker] Bắt đầu khởi tạo hệ thống Prefab & Hiệu ứng...");

        // 1. Tải toàn bộ tài nguyên đồ họa (Sprite sheets & Animation)
        Sprite[] explosionSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Effects/Explosion_01.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] arrowImpactSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Projectiles/projectile_arrow_impact_sheet.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] arrowTravelSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Projectiles/projectile_arrow_travel_sheet.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] cannonImpactSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Projectiles/projectile_cannonball_impact_sheet.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] cannonTravelSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Projectiles/projectile_cannonball_travel_sheet.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] critFlashSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Projectiles/crit_flash_sheet.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();

        Sprite[] goblinSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Custom_Enemies/Goblin_Walk.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Sprite[] orcSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Custom_Enemies/Orc_Walk.png").OfType<Sprite>().OrderBy(s => s.name).ToArray();
        RuntimeAnimatorController enemyController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Custom_Enemies/Animations/Enemy.controller");
        Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/Icon_Coin.png");
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 2. Tạo các Prefab Hiệu Ứng (VFX)
        // 2.1 Hiệu ứng nổ pháo (Cannon Explosion)
        GameObject vfxCannonObj = new GameObject("VFX_CannonExplosion", typeof(SpriteRenderer), typeof(AnimatedEffect));
        AnimatedEffect cannonAnim = vfxCannonObj.GetComponent<AnimatedEffect>();
        cannonAnim.Setup(explosionSprites.Length > 0 ? explosionSprites : cannonImpactSprites, 24f, 25);
        vfxCannonObj.transform.localScale = Vector3.one * 1.5f;
        string vfxCannonPath = "Assets/Prefabs/VFX_CannonExplosion.prefab";
        GameObject vfxCannonPrefab = PrefabUtility.SaveAsPrefabAsset(vfxCannonObj, vfxCannonPath);
        Object.DestroyImmediate(vfxCannonObj);

        // 2.2 Hiệu ứng trúng tên (Arrow Impact)
        GameObject vfxArrowObj = new GameObject("VFX_ArrowImpact", typeof(SpriteRenderer), typeof(AnimatedEffect));
        AnimatedEffect arrowAnim = vfxArrowObj.GetComponent<AnimatedEffect>();
        arrowAnim.Setup(arrowImpactSprites, 20f, 25);
        vfxArrowObj.transform.localScale = Vector3.one * 0.8f;
        string vfxArrowPath = "Assets/Prefabs/VFX_ArrowImpact.prefab";
        GameObject vfxArrowPrefab = PrefabUtility.SaveAsPrefabAsset(vfxArrowObj, vfxArrowPath);
        Object.DestroyImmediate(vfxArrowObj);

        // 2.3 Hiệu ứng bạo kích (Crit Flash)
        GameObject vfxCritObj = new GameObject("VFX_CritFlash", typeof(SpriteRenderer), typeof(AnimatedEffect));
        AnimatedEffect critAnim = vfxCritObj.GetComponent<AnimatedEffect>();
        critAnim.Setup(critFlashSprites, 16f, 26);
        vfxCritObj.transform.localScale = Vector3.one * 1.4f;
        string vfxCritPath = "Assets/Prefabs/VFX_CritFlash.prefab";
        GameObject vfxCritPrefab = PrefabUtility.SaveAsPrefabAsset(vfxCritObj, vfxCritPath);
        Object.DestroyImmediate(vfxCritObj);

        // 2.4 Hiệu ứng quái tan biến khi chết (Enemy Death Puff)
        GameObject vfxDeathObj = new GameObject("VFX_EnemyDeathPuff", typeof(SpriteRenderer), typeof(AnimatedEffect));
        AnimatedEffect deathAnim = vfxDeathObj.GetComponent<AnimatedEffect>();
        deathAnim.Setup(explosionSprites, 26f, 24);
        vfxDeathObj.transform.localScale = Vector3.one * 0.9f;
        string vfxDeathPath = "Assets/Prefabs/VFX_EnemyDeathPuff.prefab";
        GameObject vfxDeathPrefab = PrefabUtility.SaveAsPrefabAsset(vfxDeathObj, vfxDeathPath);
        Object.DestroyImmediate(vfxDeathObj);

        // 2.5 Hiệu ứng rơi tiền nảy lên & bay chữ +Gold (Coin Drop VFX)
        GameObject vfxCoinObj = new GameObject("VFX_CoinDrop", typeof(CoinDropVFX));
        GameObject coinGfx = new GameObject("CoinIcon", typeof(SpriteRenderer));
        coinGfx.transform.SetParent(vfxCoinObj.transform, false);
        coinGfx.transform.localPosition = new Vector3(-0.35f, 0f, 0f);
        coinGfx.transform.localScale = Vector3.one * 0.45f;
        SpriteRenderer coinSr = coinGfx.GetComponent<SpriteRenderer>();
        coinSr.sprite = coinSprite;
        coinSr.sortingOrder = 30;

        GameObject textObj = new GameObject("Text", typeof(TextMesh));
        textObj.transform.SetParent(vfxCoinObj.transform, false);
        textObj.transform.localPosition = new Vector3(-0.1f, 0f, 0f);
        TextMesh tm = textObj.GetComponent<TextMesh>();
        tm.font = font;
        tm.fontSize = 32;
        tm.characterSize = 0.08f;
        tm.color = new Color(1f, 0.85f, 0.1f, 1f);
        tm.text = "+5G";
        textObj.GetComponent<MeshRenderer>().sortingOrder = 30;

        SerializedObject coinSo = new SerializedObject(vfxCoinObj.GetComponent<CoinDropVFX>());
        coinSo.FindProperty("coinRenderer").objectReferenceValue = coinSr;
        coinSo.FindProperty("textMesh").objectReferenceValue = tm;
        coinSo.ApplyModifiedProperties();

        string vfxCoinPath = "Assets/Prefabs/VFX_CoinDrop.prefab";
        GameObject vfxCoinPrefab = PrefabUtility.SaveAsPrefabAsset(vfxCoinObj, vfxCoinPath);
        Object.DestroyImmediate(vfxCoinObj);

        // 3. Tạo các Prefab Đạn (Projectiles)
        // 3.1 Mũi tên (Arrow) cho Tháp Cung
        GameObject arrowObj = new GameObject("Arrow", typeof(SpriteRenderer), typeof(Bullet));
        SpriteRenderer arrowSr = arrowObj.GetComponent<SpriteRenderer>();
        if (arrowTravelSprites.Length > 0) arrowSr.sprite = arrowTravelSprites[0];
        arrowSr.sortingOrder = 15;
        arrowObj.transform.localScale = Vector3.one * 0.9f;

        SerializedObject arrowSo = new SerializedObject(arrowObj.GetComponent<Bullet>());
        arrowSo.FindProperty("speed").floatValue = 12f;
        arrowSo.FindProperty("damage").floatValue = 10f;
        arrowSo.FindProperty("splashRadius").floatValue = 0f;
        arrowSo.FindProperty("isCannon").boolValue = false;
        arrowSo.FindProperty("impactPrefab").objectReferenceValue = vfxArrowPrefab;
        arrowSo.ApplyModifiedProperties();

        string arrowPath = "Assets/Prefabs/Arrow.prefab";
        GameObject arrowPrefab = PrefabUtility.SaveAsPrefabAsset(arrowObj, arrowPath);
        Object.DestroyImmediate(arrowObj);

        // 3.2 Đạn pháo (Cannonball) cho Tháp Pháo
        GameObject cannonballObj = new GameObject("Cannonball", typeof(SpriteRenderer), typeof(Bullet));
        SpriteRenderer cbSr = cannonballObj.GetComponent<SpriteRenderer>();
        if (cannonTravelSprites.Length > 0) cbSr.sprite = cannonTravelSprites[0];
        cbSr.sortingOrder = 15;
        cannonballObj.transform.localScale = Vector3.one * 1.0f;

        SerializedObject cbSo = new SerializedObject(cannonballObj.GetComponent<Bullet>());
        cbSo.FindProperty("speed").floatValue = 8f;
        cbSo.FindProperty("damage").floatValue = 25f;
        cbSo.FindProperty("splashRadius").floatValue = 1.6f;
        cbSo.FindProperty("isCannon").boolValue = true;
        cbSo.FindProperty("impactPrefab").objectReferenceValue = vfxCannonPrefab;
        cbSo.FindProperty("critPrefab").objectReferenceValue = vfxCritPrefab;
        cbSo.ApplyModifiedProperties();

        string cannonballPath = "Assets/Prefabs/Cannonball.prefab";
        GameObject cannonballPrefab = PrefabUtility.SaveAsPrefabAsset(cannonballObj, cannonballPath);
        Object.DestroyImmediate(cannonballObj);

        // 4. Cập nhật TowerData ScriptableObjects
        string[] archerSos = { "Assets/ScriptableObject/Tower/Archer_Lv1.asset", "Assets/ScriptableObject/Tower/Archer_Lv2.asset", "Assets/ScriptableObject/Tower/Archer_Lv3.asset" };
        foreach (var p in archerSos)
        {
            TowerData td = AssetDatabase.LoadAssetAtPath<TowerData>(p);
            if (td != null)
            {
                td.bulletPrefab = arrowPrefab;
                EditorUtility.SetDirty(td);
            }
        }

        string[] cannonSos = { "Assets/ScriptableObject/Tower/Cannon_Lv1.asset", "Assets/ScriptableObject/Tower/Cannon_Lv2.asset", "Assets/ScriptableObject/Tower/Cannon_Lv3.asset" };
        foreach (var p in cannonSos)
        {
            TowerData td = AssetDatabase.LoadAssetAtPath<TowerData>(p);
            if (td != null)
            {
                td.bulletPrefab = cannonballPrefab;
                EditorUtility.SetDirty(td);
            }
        }

        // 5. Tạo các Prefab Quái Vật (Enemy Prefabs)
        // 5.1 Goblin Prefab
        GameObject goblinObj = new GameObject("Enemy_Goblin", typeof(SpriteRenderer), typeof(Animator), typeof(CircleCollider2D), typeof(Enemy));
        SpriteRenderer gobSr = goblinObj.GetComponent<SpriteRenderer>();
        if (goblinSprites.Length > 0) gobSr.sprite = goblinSprites[0];
        gobSr.sortingOrder = 10;
        goblinObj.transform.localScale = Vector3.one * 0.55f;

        Animator gobAnim = goblinObj.GetComponent<Animator>();
        gobAnim.runtimeAnimatorController = enemyController;

        CircleCollider2D gobCol = goblinObj.GetComponent<CircleCollider2D>();
        gobCol.isTrigger = true;
        gobCol.radius = 0.4f;

        SerializedObject gobSo = new SerializedObject(goblinObj.GetComponent<Enemy>());
        gobSo.FindProperty("maxHealth").floatValue = 50f;
        gobSo.FindProperty("speed").floatValue = 2.0f;
        gobSo.FindProperty("goldReward").intValue = 5;
        gobSo.FindProperty("damageToCastle").intValue = 1;
        gobSo.FindProperty("isFlying").boolValue = false;
        gobSo.FindProperty("deathEffectPrefab").objectReferenceValue = vfxDeathPrefab;
        gobSo.FindProperty("coinDropPrefab").objectReferenceValue = vfxCoinPrefab;
        gobSo.ApplyModifiedProperties();

        string goblinPath = "Assets/Prefabs/Enemy_Goblin.prefab";
        GameObject goblinPrefab = PrefabUtility.SaveAsPrefabAsset(goblinObj, goblinPath);
        PrefabUtility.SaveAsPrefabAsset(goblinObj, "Assets/Prefabs/Enemy.prefab");
        Object.DestroyImmediate(goblinObj);

        // 5.2 Orc Prefab
        GameObject orcObj = new GameObject("Enemy_Orc", typeof(SpriteRenderer), typeof(Animator), typeof(CircleCollider2D), typeof(Enemy));
        SpriteRenderer orcSr = orcObj.GetComponent<SpriteRenderer>();
        if (orcSprites.Length > 0) orcSr.sprite = orcSprites[0];
        orcSr.sortingOrder = 10;
        orcObj.transform.localScale = Vector3.one * 0.85f;

        Animator orcAnim = orcObj.GetComponent<Animator>();
        orcAnim.runtimeAnimatorController = enemyController;

        CircleCollider2D orcCol = orcObj.GetComponent<CircleCollider2D>();
        orcCol.isTrigger = true;
        orcCol.radius = 0.6f;

        SerializedObject orcSo = new SerializedObject(orcObj.GetComponent<Enemy>());
        orcSo.FindProperty("maxHealth").floatValue = 150f;
        orcSo.FindProperty("speed").floatValue = 1.2f;
        orcSo.FindProperty("goldReward").intValue = 10;
        orcSo.FindProperty("damageToCastle").intValue = 2;
        orcSo.FindProperty("isFlying").boolValue = false;
        orcSo.FindProperty("deathEffectPrefab").objectReferenceValue = vfxDeathPrefab;
        orcSo.FindProperty("coinDropPrefab").objectReferenceValue = vfxCoinPrefab;
        orcSo.ApplyModifiedProperties();

        string orcPath = "Assets/Prefabs/Enemy_Orc.prefab";
        GameObject orcPrefab = PrefabUtility.SaveAsPrefabAsset(orcObj, orcPath);
        Object.DestroyImmediate(orcObj);

        // 6. Gán Enemy Prefabs vào WaveSpawner trên cả 3 Scene Level
        string[] levelScenes = { "Assets/Scenes/Level_1.unity", "Assets/Scenes/Level_2.unity", "Assets/Scenes/Level_3.unity" };
        var currentScene = EditorSceneManager.GetActiveScene();

        foreach (var scenePath in levelScenes)
        {
            if (!File.Exists(scenePath)) continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            WaveSpawner spawner = Object.FindAnyObjectByType<WaveSpawner>();
            if (spawner != null)
            {
                spawner.goblinPrefab = goblinPrefab;
                spawner.orcPrefab = orcPrefab;
                spawner.enemyPrefab = goblinPrefab;
                EditorUtility.SetDirty(spawner);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[GameAssetBaker] Đã gán Enemy Prefabs thành công cho {scenePath}");
            }
        }

        if (!string.IsNullOrEmpty(currentScene.path) && File.Exists(currentScene.path))
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[GameAssetBaker] ĐÃ TẠO TOÀN BỘ PREFAB & GÁN HIỆU ỨNG THÀNH CÔNG RỰC RỠ!");
    }
}
