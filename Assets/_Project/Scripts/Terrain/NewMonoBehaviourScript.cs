using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Automatically syncs terrain textures to grass materials for proper terrain blending.
/// Attach this to your Terrain object.
/// </summary>
[ExecuteInEditMode]
public class TerrainTextureSync : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The terrain to sync from")]
    public Terrain terrain;
    
    [Tooltip("Grass materials that should receive terrain texture data")]
    public Material[] grassMaterials;
    
    [Header("Settings")]
    [Tooltip("Automatically update when terrain layers change")]
    public bool autoUpdate = true;
    
    [Tooltip("Update interval in seconds (editor only)")]
    public float updateInterval = 1.0f;
    
    [Header("Info")]
    public int layerCount = 0;
    public bool hasControlMaps = false;
    
    private float lastUpdateTime = 0f;
    private TerrainLayer[] lastLayers;
    
    void OnEnable()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }
        
        #if UNITY_EDITOR
        if (!Application.isPlaying && autoUpdate)
        {
            EditorApplication.update += EditorUpdate;
        }
        #endif
    }
    
    void OnDisable()
    {
        #if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
        #endif
    }
    
    #if UNITY_EDITOR
    void EditorUpdate()
    {
        if (!autoUpdate || terrain == null)
            return;
        
        if (Time.realtimeSinceStartup - lastUpdateTime > updateInterval)
        {
            lastUpdateTime = Time.realtimeSinceStartup;
            CheckForChanges();
        }
    }
    
    void CheckForChanges()
    {
        TerrainData data = terrain.terrainData;
        if (data == null)
            return;
        
        TerrainLayer[] currentLayers = data.terrainLayers;
        
        // Check if layers have changed
        bool changed = false;
        if (lastLayers == null || lastLayers.Length != currentLayers.Length)
        {
            changed = true;
        }
        else
        {
            for (int i = 0; i < currentLayers.Length; i++)
            {
                if (lastLayers[i] != currentLayers[i])
                {
                    changed = true;
                    break;
                }
            }
        }
        
        if (changed)
        {
            Debug.Log("Terrain layers changed - updating grass materials");
            SyncTextures();
            lastLayers = currentLayers;
        }
    }
    #endif
    
    [ContextMenu("Sync Textures Now")]
    public void SyncTextures()
    {
        if (terrain == null)
        {
            Debug.LogError("No terrain assigned!");
            return;
        }
        
        if (grassMaterials == null || grassMaterials.Length == 0)
        {
            Debug.LogError("No grass materials assigned!");
            return;
        }
        
        TerrainData terrainData = terrain.terrainData;
        if (terrainData == null)
        {
            Debug.LogError("Terrain has no terrain data!");
            return;
        }
        
        // Get terrain layers
        TerrainLayer[] layers = terrainData.terrainLayers;
        layerCount = layers.Length;
        
        if (layerCount == 0)
        {
            Debug.LogWarning("Terrain has no layers!");
            return;
        }
        
        // Get control maps (splatmaps)
        Texture2D[] controlMaps = terrainData.alphamapTextures;
        hasControlMaps = controlMaps != null && controlMaps.Length > 0;
        
        // Update each grass material
        foreach (Material mat in grassMaterials)
        {
            if (mat == null)
                continue;
            
            // Enable terrain texture sampling
            mat.EnableKeyword("SAMPLE_TERRAIN_TEXTURE");
            mat.SetFloat("_SampleTerrainTexture", 1);
            
            // Assign control maps
            if (hasControlMaps)
            {
                if (controlMaps.Length > 0)
                    mat.SetTexture("_Control0", controlMaps[0]);
                if (controlMaps.Length > 1)
                    mat.SetTexture("_Control1", controlMaps[1]);
                if (controlMaps.Length > 2)
                    mat.SetTexture("_Control2", controlMaps[2]);
            }
            
            // Assign terrain layer textures and their tiling
            for (int i = 0; i < Mathf.Min(layerCount, 9); i++)
            {
                if (layers[i] != null && layers[i].diffuseTexture != null)
                {
                    mat.SetTexture($"_TerrainSplat{i}", layers[i].diffuseTexture);
                    
                    // Set tiling from terrain layer settings
                    Vector2 tileSize = layers[i].tileSize;
                    Vector2 tileOffset = layers[i].tileOffset;
                    
                    if (tileSize.x > 0 && tileSize.y > 0)
                    {
                        Vector4 st = new Vector4(1f / tileSize.x, 1f / tileSize.y, tileOffset.x, tileOffset.y);
                        mat.SetVector($"_TerrainSplat{i}_ST", st);
                    }
                }
            }
            
            // Set terrain size and position
            Vector3 terrainSize = terrainData.size;
            Vector3 terrainPosition = terrain.transform.position;
            
            mat.SetVector("_TerrainSize", new Vector4(terrainSize.x, terrainSize.z, 0, 0));
            mat.SetVector("_TerrainPosition", new Vector4(terrainPosition.x, terrainPosition.z, 0, 0));
            
            Debug.Log($"Updated material '{mat.name}' with {layerCount} terrain layers");
        }
        
        Debug.Log($"Synced {layerCount} terrain layers to {grassMaterials.Length} grass materials");
    }
    
    [ContextMenu("Disable Terrain Blending")]
    public void DisableTerrainBlending()
    {
        foreach (Material mat in grassMaterials)
        {
            if (mat != null)
            {
                mat.DisableKeyword("SAMPLE_TERRAIN_TEXTURE");
                mat.SetFloat("_SampleTerrainTexture", 0);
                Debug.Log($"Disabled terrain blending for '{mat.name}'");
            }
        }
    }
    
    [ContextMenu("Find All Grass Materials in Scene")]
    public void FindGrassMaterials()
    {
        List<Material> foundMaterials = new List<Material>();
        
        // Find all renderers in the scene
        MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (MeshRenderer renderer in renderers)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat != null && mat.shader != null && 
                    mat.shader.name.Contains("StylizedTerrainGrass") &&
                    !foundMaterials.Contains(mat))
                {
                    foundMaterials.Add(mat);
                }
            }
        }
        
        if (foundMaterials.Count > 0)
        {
            grassMaterials = foundMaterials.ToArray();
            Debug.Log($"Found {foundMaterials.Count} grass materials");
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }
        else
        {
            Debug.LogWarning("No grass materials found in scene");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TerrainTextureSync))]
public class TerrainTextureSyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TerrainTextureSync sync = (TerrainTextureSync)target;
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This script automatically syncs terrain textures to grass materials.\n\n" +
            "1. Assign your terrain\n" +
            "2. Assign grass materials (or use 'Find All Grass Materials')\n" +
            "3. Click 'Sync Textures Now'\n" +
            "4. Enable 'Auto Update' to sync automatically when terrain changes",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Find All Grass Materials in Scene", GUILayout.Height(30)))
        {
            sync.FindGrassMaterials();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Sync Textures Now", GUILayout.Height(40)))
        {
            sync.SyncTextures();
        }
        
        if (GUILayout.Button("Disable Terrain Blending", GUILayout.Height(40)))
        {
            sync.DisableTerrainBlending();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Status info
        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Terrain Layers:", sync.layerCount.ToString());
        EditorGUILayout.LabelField("Has Control Maps:", sync.hasControlMaps ? "Yes" : "No");
    }
}
#endif