using UnityEngine;

public class Planet : MonoBehaviour
{
    [Header("Références")]
    public Transform viewer;

    [Header("Paramètres de Taille")]
    public float radius = 1000f;

    [Header("Paramètres du Relief")]
    public float noiseStrength = 50f;
    public float baseRoughness = 1.5f;
    [Range(1, 8)]
    public int octaves = 5;

    [Header("Paramètres LOD (Quadtree)")]
    [Range(0, 6)]
    public int maxLodLevel = 4;
    public float lodDistanceMultiplier = 2.5f;

    [Header("Apparence")]
    public Gradient colorGradient;
    public Material planetMaterial;

    [Header("Résolution par Chunk")]
    [Range(2, 64)]
    public int resolution = 32; 

    public NoiseFilter noiseFilter;
    TerrainChunk[] rootChunks;

    void Start()
    {
        GeneratePlanet();
    }

    void Update()
    {
        if (viewer != null && rootChunks != null)
        {
            foreach (TerrainChunk chunk in rootChunks)
            {
                chunk.UpdateChunk(viewer.position);
            }
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying) return;
        GeneratePlanet();
    }

    public void GeneratePlanet()
    {
        // 1. Nettoyage absolu (RAM + Hiérarchie)
        if (rootChunks != null)
        {
            foreach (TerrainChunk chunk in rootChunks)
            {
                if (chunk != null) chunk.DestroyChunk();
            }
        }

        // Sécurité supplémentaire pour l'éditeur
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        noiseFilter = new NoiseFilter();
        noiseFilter.strength = noiseStrength;
        noiseFilter.baseRoughness = baseRoughness;
        noiseFilter.numLayers = octaves;

        rootChunks = new TerrainChunk[6];
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            rootChunks[i] = new TerrainChunk(0, Vector2.zero, 1f, directions[i], this, transform);
        }
    }
}