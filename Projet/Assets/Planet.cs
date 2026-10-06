using UnityEngine;

public class Planet : MonoBehaviour
{

    [Range(2, 256)]
    public int resolution = 10; // Le nombre de sommets par côté pour chaque face

    // Les 6 faces de notre cube sphère
    TerrainFace[] terrainFaces;

    // Matériau appliqué à la planète (optionnel, utile pour voir le relief)
    public Material planetMaterial;

    void OnValidate()
    {
        // Appelé automatiquement par Unity quand on modifie une valeur dans l'inspecteur
        GeneratePlanet();
    }

    void Start()
    {
        // Appelé au lancement du jeu
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
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.back
        };

        for (int i = 0; i < 6; i++)
        {
            GameObject meshObj;

            // CORRECTION ICI : On vérifie le nombre d'enfants avant d'essayer d'y accéder
            if (i < transform.childCount)
            {
                // L'enfant existe déjà, on le récupère
                meshObj = transform.GetChild(i).gameObject;
            }
            else
            {
                // L'enfant n'existe pas, on le crée
                meshObj = new GameObject("Face_" + i);
                meshObj.transform.parent = transform;
                meshObj.transform.localPosition = Vector3.zero;
            }

            // Vérification et ajout des composants si manquants
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

            terrainFaces[i] = new TerrainFace(meshFilter.sharedMesh, resolution, directions[i]);
        }
    }

    void GenerateMesh()
    {
        // On demande à chaque face de générer ses sommets et triangles
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }
}