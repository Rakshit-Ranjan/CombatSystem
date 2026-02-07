using UnityEngine;
using UnityEditor;

/// <summary>
/// Generates grass blade meshes suitable for the stylized grass shader.
/// The generated mesh has proper vertex heights for wind and interaction.
/// </summary>
public class GrassMeshGenerator : MonoBehaviour
{
    [Header("Grass Blade Settings")]
    [Tooltip("Height of the grass blade")]
    public float height = 1.0f;
    
    [Tooltip("Width at the base of the blade")]
    public float widthBase = 0.1f;
    
    [Tooltip("Width at the tip of the blade")]
    public float widthTip = 0.02f;
    
    [Tooltip("Number of vertical segments (more = smoother bend)")]
    [Range(2, 10)]
    public int segments = 4;
    
    [Tooltip("Bend the blade slightly for natural look")]
    [Range(0f, 0.3f)]
    public float bendAmount = 0.1f;
    
    [Header("Mesh Type")]
    public GrassType grassType = GrassType.SingleBlade;
    
    [Tooltip("For cross blades: number of blades in the cross pattern")]
    [Range(2, 4)]
    public int crossBladeCount = 2;
    
    [Header("Output")]
    public string meshName = "GrassBlade";
    
    public enum GrassType
    {
        SingleBlade,      // One flat blade
        CrossBlades,      // Multiple blades in cross pattern (better from all angles)
        Tuft            // Small cluster of grass
    }
    
    [ContextMenu("Generate Mesh")]
    public void GenerateMesh()
    {
        Mesh mesh = null;
        
        switch (grassType)
        {
            case GrassType.SingleBlade:
                mesh = GenerateSingleBlade();
                break;
            case GrassType.CrossBlades:
                mesh = GenerateCrossBlades();
                break;
            case GrassType.Tuft:
                mesh = GenerateTuft();
                break;
        }
        
        if (mesh != null)
        {
            mesh.name = meshName;
            
            // Assign to mesh filter if present
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = mesh;
                Debug.Log($"Generated {grassType} mesh and assigned to MeshFilter");
            }
            
            #if UNITY_EDITOR
            // Save as asset
            SaveMeshAsset(mesh);
            #endif
        }
    }
    
    Mesh GenerateSingleBlade()
    {
        Mesh mesh = new Mesh();
        
        int vertexCount = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        Color[] colors = new Color[vertexCount];
        int[] triangles = new int[segments * 6];
        
        // Generate vertices
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentHeight = height * t;
            float currentWidth = Mathf.Lerp(widthBase, widthTip, t) * 0.5f;
            float bend = bendAmount * t * t; // Quadratic bend
            
            // Left vertex
            vertices[i * 2] = new Vector3(-currentWidth + bend, currentHeight, 0);
            // Right vertex
            vertices[i * 2 + 1] = new Vector3(currentWidth + bend, currentHeight, 0);
            
            // UVs
            uvs[i * 2] = new Vector2(0, t);
            uvs[i * 2 + 1] = new Vector2(1, t);
            
            // Vertex colors (used for wind influence)
            // Red channel = wind influence (0 at base, 1 at tip)
            Color vertColor = new Color(t, t, t, 1.0f);
            colors[i * 2] = vertColor;
            colors[i * 2 + 1] = vertColor;
        }
        
        // Generate triangles
        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 2;
            int triIndex = i * 6;
            
            // First triangle
            triangles[triIndex] = baseIndex;
            triangles[triIndex + 1] = baseIndex + 2;
            triangles[triIndex + 2] = baseIndex + 1;
            
            // Second triangle
            triangles[triIndex + 3] = baseIndex + 1;
            triangles[triIndex + 4] = baseIndex + 2;
            triangles[triIndex + 5] = baseIndex + 3;
        }
        
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    Mesh GenerateCrossBlades()
    {
        CombineInstance[] combines = new CombineInstance[crossBladeCount];
        
        for (int i = 0; i < crossBladeCount; i++)
        {
            Mesh bladeMesh = GenerateSingleBlade();
            float angle = (360f / crossBladeCount) * i;
            
            combines[i].mesh = bladeMesh;
            combines[i].transform = Matrix4x4.Rotate(Quaternion.Euler(0, angle, 0));
        }
        
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combines, true, true);
        combinedMesh.RecalculateNormals();
        
        return combinedMesh;
    }
    
    Mesh GenerateTuft()
    {
        int bladeCount = 5;
        CombineInstance[] combines = new CombineInstance[bladeCount];
        
        for (int i = 0; i < bladeCount; i++)
        {
            Mesh bladeMesh = GenerateSingleBlade();
            
            // Random offset and rotation for natural look
            float angle = Random.Range(0f, 360f);
            float offsetX = Random.Range(-widthBase, widthBase);
            float offsetZ = Random.Range(-widthBase, widthBase);
            float heightVariation = Random.Range(0.8f, 1.2f);
            
            Matrix4x4 transform = Matrix4x4.TRS(
                new Vector3(offsetX, 0, offsetZ),
                Quaternion.Euler(Random.Range(-5f, 5f), angle, Random.Range(-5f, 5f)),
                new Vector3(1, heightVariation, 1)
            );
            
            combines[i].mesh = bladeMesh;
            combines[i].transform = transform;
        }
        
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combines, true, true);
        combinedMesh.RecalculateNormals();
        
        return combinedMesh;
    }
    
    #if UNITY_EDITOR
    void SaveMeshAsset(Mesh mesh)
    {
        string path = $"Assets/{meshName}.asset";
        
        // Check if asset already exists
        Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existingMesh != null)
        {
            // Update existing asset
            existingMesh.Clear();
            existingMesh.vertices = mesh.vertices;
            existingMesh.triangles = mesh.triangles;
            existingMesh.uv = mesh.uv;
            existingMesh.colors = mesh.colors;
            existingMesh.normals = mesh.normals;
            existingMesh.RecalculateBounds();
            
            EditorUtility.SetDirty(existingMesh);
            AssetDatabase.SaveAssets();
            Debug.Log($"Updated existing mesh asset: {path}");
        }
        else
        {
            // Create new asset
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created new mesh asset: {path}");
        }
        
        AssetDatabase.Refresh();
    }
    #endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(GrassMeshGenerator))]
public class GrassMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GrassMeshGenerator generator = (GrassMeshGenerator)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Click 'Generate Mesh' to create a grass blade mesh.\n\n" +
            "Single Blade: Simple, one-sided grass\n" +
            "Cross Blades: Multiple blades in X pattern (recommended)\n" +
            "Tuft: Small cluster of grass blades",
            MessageType.Info
        );
        
        if (GUILayout.Button("Generate Mesh", GUILayout.Height(40)))
        {
            generator.GenerateMesh();
        }
    }
}
#endif