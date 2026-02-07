using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int terrainWidth = 256;
    public int terrainHeight = 256;
    public int terrainDepth = 50;
    
    [Header("Heightmap Texture")]
    public Texture2D heightmapTexture;
    
    [Header("Slope-Based Texturing")]
    [Tooltip("Flat ground texture (0-15 degrees)")]
    public Texture2D flatTexture;
    [Tooltip("Medium slope texture (15-45 degrees)")]
    public Texture2D mediumSlopeTexture;
    [Tooltip("Steep slope texture (45-90 degrees)")]
    public Texture2D steepSlopeTexture;
    
    [Range(0, 90)]
    public float flatSlopeThreshold = 15f;
    [Range(0, 90)]
    public float steepSlopeThreshold = 45f;
    
    private Terrain terrain;
    private TerrainData terrainData;
    
    void Start()
    {
        GenerateTerrain();
    }
    
    public void GenerateTerrain()
    {
        if (heightmapTexture == null)
        {
            Debug.LogError("Heightmap texture is not assigned!");
            return;
        }
        
        // Create terrain data
        terrainData = new TerrainData();
        terrainData.heightmapResolution = heightmapTexture.width;
        terrainData.size = new Vector3(terrainWidth, terrainDepth, terrainHeight);
        
        // Load heightmap from texture
        terrainData.SetHeights(0, 0, LoadHeightmapFromTexture());
        
        // Setup textures
        SetupTerrainTextures();
        
        // Apply slope-based texturing
        ApplySlopeBasedTexture();
        
        // Create or update terrain
        if (terrain == null)
        {
            GameObject terrainObj = Terrain.CreateTerrainGameObject(terrainData);
            terrainObj.transform.position = transform.position;
            terrain = terrainObj.GetComponent<Terrain>();
        }
        else
        {
            terrain.terrainData = terrainData;
        }
    }
    
    float[,] LoadHeightmapFromTexture()
    {
        int width = heightmapTexture.width;
        int height = heightmapTexture.height;
        float[,] heights = new float[width, height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Get pixel color and convert to height value (0-1)
                Color pixelColor = heightmapTexture.GetPixel(x, y);
                heights[x, y] = pixelColor.grayscale;
            }
        }
        
        return heights;
    }
    
    void SetupTerrainTextures()
    {
        // Create default textures if none provided
        if (flatTexture == null) flatTexture = CreateDefaultTexture(Color.green);
        if (mediumSlopeTexture == null) mediumSlopeTexture = CreateDefaultTexture(Color.yellow);
        if (steepSlopeTexture == null) steepSlopeTexture = CreateDefaultTexture(Color.gray);
        
        // Setup terrain layers
        TerrainLayer[] terrainLayers = new TerrainLayer[3];
        
        terrainLayers[0] = new TerrainLayer();
        terrainLayers[0].diffuseTexture = flatTexture;
        terrainLayers[0].tileSize = new Vector2(15, 15);
        
        terrainLayers[1] = new TerrainLayer();
        terrainLayers[1].diffuseTexture = mediumSlopeTexture;
        terrainLayers[1].tileSize = new Vector2(15, 15);
        
        terrainLayers[2] = new TerrainLayer();
        terrainLayers[2].diffuseTexture = steepSlopeTexture;
        terrainLayers[2].tileSize = new Vector2(15, 15);
        
        terrainData.terrainLayers = terrainLayers;
    }
    
    void ApplySlopeBasedTexture()
    {
        float[,,] alphamaps = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, 3];
        
        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Normalize coordinates to heightmap
                float normX = x * 1.0f / (terrainData.alphamapWidth - 1);
                float normY = y * 1.0f / (terrainData.alphamapHeight - 1);
                
                // Calculate slope angle
                float slope = terrainData.GetSteepness(normX, normY);
                
                // Determine texture weights based on slope
                float[] weights = new float[3];
                
                if (slope < flatSlopeThreshold)
                {
                    // Flat terrain
                    weights[0] = 1f;
                }
                else if (slope < steepSlopeThreshold)
                {
                    // Medium slope - blend between flat and medium
                    float blend = (slope - flatSlopeThreshold) / (steepSlopeThreshold - flatSlopeThreshold);
                    weights[0] = 1f - blend;
                    weights[1] = blend;
                }
                else
                {
                    // Steep slope
                    float blend = Mathf.Clamp01((slope - steepSlopeThreshold) / (90f - steepSlopeThreshold));
                    weights[1] = 1f - blend;
                    weights[2] = blend;
                }
                
                // Normalize weights
                float totalWeight = weights[0] + weights[1] + weights[2];
                for (int i = 0; i < 3; i++)
                {
                    alphamaps[x, y, i] = weights[i] / totalWeight;
                }
            }
        }
        
        terrainData.SetAlphamaps(0, 0, alphamaps);
    }
    
    Texture2D CreateDefaultTexture(Color color)
    {
        Texture2D texture = new Texture2D(2, 2);
        Color[] pixels = new Color[4];
        for (int i = 0; i < 4; i++) pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    // Editor helper to regenerate terrain
    [ContextMenu("Regenerate Terrain")]
    void RegenerateTerrain()
    {
        GenerateTerrain();
    }
}