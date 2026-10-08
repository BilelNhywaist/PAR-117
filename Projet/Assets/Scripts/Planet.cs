using UnityEngine;

public class Planet : MonoBehaviour
{
    [Header("Références")]
    public Transform viewer; // La caméra du joueur (FlyCamera)

    [Header("Paramètres de Taille")]
    public float radius = 1000f;

    [Header("Paramètres du Relief")]
    public float noiseStrength = 50f;
    public float baseRoughness = 1.5f;
    [Range(1, 8)]
    public int octaves = 5;

    [Header("Paramètres LOD (Quadtree)")]
    [Range(0, 6)]
    public int maxLodLevel = 4; // Niveau maximum de division (Attention à la RAM au-delà de 5)
    public float lodDistanceMultiplier = 2.5f; // Sensibilité de la distance pour diviser

    [Header("Apparence")]
    public Gradient colorGradient;
    public Material planetMaterial;

    [Header("Résolution par Chunk")]
    [Range(2, 64)]
    public int resolution = 32; // On baisse la résolution de base, car le Quadtree va la démultiplier localement

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
            // Actualise l'arbre Quadtree à chaque frame selon la position du joueur
            foreach (TerrainChunk chunk in rootChunks)
            {
                chunk.UpdateChunk(viewer.position);
            }
        }
    }

    // Permet de régénérer manuellement depuis l'éditeur (hors mode Play)
    void OnValidate()
    {
        if (Application.isPlaying) return;
        GeneratePlanet();
    }

    public void GeneratePlanet()
    {
        // Nettoyage de l'ancienne géométrie
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        noiseFilter = new NoiseFilter();
        noiseFilter.strength = noiseStrength;
        noiseFilter.baseRoughness = baseRoughness;
        noiseFilter.numLayers = octaves;

        rootChunks = new TerrainChunk[6];
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            // Création des 6 faces racines avec un offset de (0,0) et une taille de 1 (100% de la face)
            rootChunks[i] = new TerrainChunk(0, Vector2.zero, 1f, directions[i], this, transform);
        }
    }
}