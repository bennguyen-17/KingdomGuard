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
        public float splashRadius = 0f;
        public float critChance = 0f;
        public float critMultiplier = 1.5f;
        public bool isCannon = false;
    }

    [MenuItem("Tools/KingdomGuard/Generate Towers")]
    public static void Generate()
    {
        GameObject bullet = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "/Bullet.prefab");
        if (bullet == null) Debug.LogWarning("Không thấy Assets/Prefabs/Bullet.prefab, tháp sẽ không bắn được.");

        Material mat = null;
        if (bullet != null && bullet.TryGetComponent(out SpriteRenderer bsr)) mat = bsr.sharedMaterial;

        // Chỉ số chuẩn GDD Kingdom Guard FGU301:
        // Archer: 50G, dmg 10, range 3.5, 0.5s | Lv2: 30G, dmg 13, 3.8, 0.45s | Lv3: 30G, dmg 17, 4.1, 0.40s
        // Cannon: 100G, dmg 30, range 2.5, 1.5s, AOE R=1.0, 15% Crit x1.5 | Lv2: 60G, dmg 39, 2.8, 1.35s | Lv3: 60G, dmg 51, 3.1, 1.20s
        // Magic: 150G, dmg 20, range 3.0, 1.2s | Lv2: 90G, dmg 26, 3.3, 1.08s | Lv3: 90G, dmg 34, 3.6, 0.96s
        var specs = new[]
        {
            new Spec {
                name = "Archer", key = "archer",
                cost = new[] { 50, 30, 30 },
                damage = new[] { 10f, 13f, 17f },
                range = new[] { 3.5f, 3.8f, 4.1f },
                interval = new[] { 0.5f, 0.45f, 0.40f },
                isCannon = false
            },
            new Spec {
                name = "Cannon", key = "cannon",
                cost = new[] { 100, 60, 60 },
                damage = new[] { 30f, 39f, 51f },
                range = new[] { 2.5f, 2.8f, 3.1f },
                interval = new[] { 1.5f, 1.35f, 1.20f },
                splashRadius = 1.0f,
                critChance = 0.15f,
                critMultiplier = 1.5f,
                isCannon = true
            },
            new Spec {
                name = "Magic",  key = "magic",
                cost = new[] { 150, 90, 90 },
                damage = new[] { 20f, 26f, 34f },
                range = new[] { 3.0f, 3.3f, 3.6f },
                interval = new[] { 1.2f, 1.08f, 0.96f },
                isCannon = false
            },
        };

        var defs = new TowerDefinition[specs.Length];
        for (int i = 0; i < specs.Length; i++) defs[i] = BuildDefinition(specs[i], bullet);

        GameObject towerPrefab = SaveTowerPrefab(mat);
        SaveBuildManagerPrefab(towerPrefab, defs);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Đã tạo xong tháp theo chuẩn GDD.");
    }

    private static TowerDefinition BuildDefinition(Spec s, GameObject bullet)
    {
        var levels = new TowerData[3];
        for (int lv = 0; lv < 3; lv++)
        {
            var d = LoadOrCreate<TowerData>($"{SoDir}/{s.name}_Lv{lv + 1}.asset", out bool created);
            d.cost = s.cost[lv];
            d.damage = s.damage[lv];
            d.range = s.range[lv];
            d.shootInterval = s.interval[lv];
            d.projectileSpeed = 10f;
            d.projectileDuration = 2f;
            d.splashRadius = s.splashRadius;
            d.critChance = s.critChance;
            d.critMultiplier = s.critMultiplier;
            d.isCannon = s.isCannon;

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
        sr.sortingOrder = 10;
        if (mat != null) sr.sharedMaterial = mat;
        go.AddComponent<Tower>();
        go.transform.localScale = Vector3.one * 1.8f;

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
