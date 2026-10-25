#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

// Menu: Tools > KingdomGuard > Generate Towers
// Tạo sẵn: 3 loại tháp x 3 cấp (TowerData + TowerDefinition), Tower.prefab, BuildManager.prefab.
// Chạy lại không ghi đè chỉ số bạn đã chỉnh tay (chỉ cập nhật sprite + bullet).
public static class TowerAssetGenerator
{
    private const string SoDir = "Assets/ScriptableObject/Tower";
    private const string ArtDir = "Assets/Art/Custom_Towers";
    private const string PrefabDir = "Assets/Prefabs";

    private class Spec
    {
        public string name, key;
        public int[] cost;
        public float[] damage, range, interval;
    }

    [MenuItem("Tools/KingdomGuard/Generate Towers")]
    public static void Generate()
    {
        GameObject bullet = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Bullet.prefab");
        if (bullet == null) Debug.LogWarning("Không thấy Assets/Prefabs/Bullet.prefab, tháp sẽ không bắn được.");

        Material mat = null;
        if (bullet != null && bullet.TryGetComponent(out SpriteRenderer bsr)) mat = bsr.sharedMaterial;

        // Chỉ số khởi điểm, tự cân bằng lại sau. Quái đang hardcode 100 máu trong Enemy.cs.
        var specs = new[]
        {
            new Spec { name = "Archer", key = "archer", cost = new[] { 70, 90, 120 },
                       damage = new[] { 20f, 35f, 55f }, range = new[] { 3.0f, 3.4f, 3.8f }, interval = new[] { 1.0f, 0.8f, 0.6f } },
            new Spec { name = "Cannon", key = "cannon", cost = new[] { 120, 150, 200 },
                       damage = new[] { 45f, 80f, 130f }, range = new[] { 3.0f, 3.2f, 3.4f }, interval = new[] { 2.0f, 1.8f, 1.6f } },
            new Spec { name = "Magic",  key = "magic",  cost = new[] { 100, 130, 170 },
                       damage = new[] { 30f, 55f, 90f },  range = new[] { 3.2f, 3.6f, 4.0f }, interval = new[] { 1.4f, 1.2f, 1.0f } },
        };

        var defs = new TowerDefinition[specs.Length];
        for (int i = 0; i < specs.Length; i++) defs[i] = BuildDefinition(specs[i], bullet);

        GameObject towerPrefab = SaveTowerPrefab(mat);
        SaveBuildManagerPrefab(towerPrefab, defs);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Đã tạo xong tháp. Kéo Assets/Prefabs/BuildManager.prefab vào Level_1, Level_2, Level_3.");
    }

    private static TowerDefinition BuildDefinition(Spec s, GameObject bullet)
    {
        var levels = new TowerData[3];
        for (int lv = 0; lv < 3; lv++)
        {
            var d = LoadOrCreate<TowerData>($"{SoDir}/{s.name}_Lv{lv + 1}.asset", out bool created);
            if (created)
            {
                d.cost = s.cost[lv];
                d.damage = s.damage[lv];
                d.range = s.range[lv];
                d.shootInterval = s.interval[lv];
                d.projectileSpeed = 10f;
                d.projectileDuration = 2f;
            }
            d.sprite = AssetDatabase
                .LoadAllAssetsAtPath($"{ArtDir}/tower_{s.key}_lv{lv + 1}_64x64.png")
                .OfType<Sprite>().FirstOrDefault();
            d.bulletPrefab = bullet;
            EditorUtility.SetDirty(d);
            levels[lv] = d;
        }

        var def = LoadOrCreate<TowerDefinition>($"{SoDir}/{s.name}_Definition.asset", out _);
        def.towerName = s.name;
        def.icon = levels[0].sprite;
        def.levels = levels;
        EditorUtility.SetDirty(def);
        return def;
    }

    private static GameObject SaveTowerPrefab(Material mat)
    {
        var go = new GameObject("Tower");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;               // tilemap đang ở order 0-2, Castle order 3
        if (mat != null) sr.sharedMaterial = mat;
        go.AddComponent<Tower>();
        go.transform.localScale = Vector3.one * 1.8f;   // sprite 64px, PPU 100 => 0.64 unit, phóng lên cho vừa ô

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + "/Tower.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void SaveBuildManagerPrefab(GameObject towerPrefab, TowerDefinition[] defs)
    {
        var go = new GameObject("BuildManager");
        var bm = go.AddComponent<BuildManager>();
        bm.towerPrefab = towerPrefab;
        bm.towerTypes = defs;
        PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + "/BuildManager.prefab");
        Object.DestroyImmediate(go);
    }

    private static T LoadOrCreate<T>(string path, out bool created) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        created = asset == null;
        if (created)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }
}
#endif
