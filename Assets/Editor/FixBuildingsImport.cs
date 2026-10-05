using UnityEngine;
using UnityEditor;
using System.IO;

public class FixBuildingsImport
{
    [InitializeOnLoadMethod]
    public static void Fix()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool("FixedBuildingsImport", false))
            {
                EditorPrefs.SetBool("FixedBuildingsImport", true);
                
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/TinySwords_Pack/Buildings" });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (ti != null && ti.spriteImportMode != SpriteImportMode.Single)
                    {
                        ti.spriteImportMode = SpriteImportMode.Single;
                        ti.spritePixelsPerUnit = 64;
                        ti.filterMode = FilterMode.Point;
                        ti.textureCompression = TextureImporterCompression.Uncompressed;
                        ti.SaveAndReimport();
                    }
                }
                Debug.Log("Đã fix import settings cho Buildings về Single!");
            }
        };
    }
}
