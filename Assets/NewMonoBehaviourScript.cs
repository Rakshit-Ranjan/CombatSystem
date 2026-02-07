using UnityEngine;
using UnityEditor;
using System.IO;

public class TerrainControlMapExporter : EditorWindow
{
    private Terrain terrain;

    [MenuItem("Tools/Export Terrain Control Maps")]
    public static void ShowWindow()
    {
        GetWindow<TerrainControlMapExporter>("Terrain Control Exporter");
    }

    void OnGUI()
    {
        GUILayout.Label("Export Terrain Control Maps", EditorStyles.boldLabel);
        
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        if (GUILayout.Button("Export Control Maps"))
        {
            if (terrain != null)
            {
                ExportControlMaps();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign a terrain!", "OK");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will export the terrain's control maps (splatmaps) as PNG files.\n\n" +
            "Control Map 0: Terrain layers 0-3 (RGBA)\n" +
            "Control Map 1: Terrain layers 4-7 (RGBA)\n" +
            "Control Map 2: Terrain layer 8 (R channel)",
            MessageType.Info
        );
    }

    void ExportControlMaps()
    {
        TerrainData terrainData = terrain.terrainData;
        int alphamapCount = terrainData.alphamapTextureCount;
        
        Debug.Log($"Terrain has {terrainData.alphamapLayers} layers across {alphamapCount} control maps");
        
        string folderPath = "Assets/TerrainControlMaps";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "TerrainControlMaps");
        }

        for (int i = 0; i < alphamapCount; i++)
        {
            Texture2D alphamap = terrainData.alphamapTextures[i];
            
            // Create a readable copy
            Texture2D readableTexture = new Texture2D(alphamap.width, alphamap.height, TextureFormat.RGBA32, false);
            
            // Use RenderTexture to copy since alphamap might not be readable
            RenderTexture rt = RenderTexture.GetTemporary(alphamap.width, alphamap.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(alphamap, rt);
            
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            
            readableTexture.ReadPixels(new Rect(0, 0, alphamap.width, alphamap.height), 0, 0);
            readableTexture.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            // Save as PNG
            byte[] bytes = readableTexture.EncodeToPNG();
            string fileName = $"{folderPath}/ControlMap_{i}.png";
            File.WriteAllBytes(fileName, bytes);
            
            Debug.Log($"Exported: {fileName}");
            
            DestroyImmediate(readableTexture);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", 
            $"Exported {alphamapCount} control map(s) to {folderPath}", "OK");
        
        // Also log terrain info for shader setup
        Debug.Log("=== TERRAIN INFO FOR SHADER ===");
        Debug.Log($"Terrain Position: ({terrain.transform.position.x}, {terrain.transform.position.z}, 0, 0)");
        Debug.Log($"Terrain Size: ({terrainData.size.x}, {terrainData.size.z}, 0, 0)");
        Debug.Log($"Number of Layers: {terrainData.alphamapLayers}");
    }
}