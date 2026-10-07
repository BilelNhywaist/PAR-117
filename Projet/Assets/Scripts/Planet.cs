using UnityEngine;
using UnityEngine.AI;

public class Planet : MonoBehaviour
{

    [Range(2, 255)]
    public int resolution = 10;
    TerrainFace[] terrainFaces;
    public Material planetMaterial;
    [Header("Paramètres du Relief")]
    public float noiseStrength = 1f;
    public float baseRoughness = 1f;
    [Range(1, 8)]
    public int octaves = 4;
    [Header("Apparence")]
    public Gradient colorGradient;
    void OnValidate()
    {
        GeneratePlanet();
    }

    void Start()
    {
        GeneratePlanet();
    }

    void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
    }

    void Initialize()
    {
        if (terrainFaces == null || terrainFaces.Length == 0)
        {
            terrainFaces = new TerrainFace[6];
        }

        Vector3[] directions = {
            Vector3.up, Vector3.down, Vector3.left,
            Vector3.right, Vector3.forward, Vector3.back
        };

        for (int i = 0; i < 6; i++)
        {
            GameObject meshObj;
            string faceName = "Face_" + i;

            // On cherche l'enfant par son nom exact
            Transform existingFace = transform.Find(faceName);

            if (existingFace != null)
            {
                // La face existe, on la récupère
                meshObj = existingFace.gameObject;
            }
            else
            {
                // La face n'existe pas, on la crée proprement
                meshObj = new GameObject(faceName);
                meshObj.transform.parent = transform;
                meshObj.transform.localPosition = Vector3.zero;
            }

            // Vérification et ajout des composants
            MeshRenderer meshRenderer = meshObj.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = meshObj.AddComponent<MeshRenderer>();
            }

            MeshFilter meshFilter = meshObj.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = meshObj.AddComponent<MeshFilter>();
            }

            if (planetMaterial != null)
            {
                meshRenderer.sharedMaterial = planetMaterial;
            }

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilter.sharedMesh, resolution, directions[i], this);
        }
    }

    void GenerateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }
}