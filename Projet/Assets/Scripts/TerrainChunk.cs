using UnityEngine;

public class TerrainChunk
{
    public GameObject meshObj;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    Mesh mesh;

    TerrainChunk[] children;
    bool hasChildren;
    int lodLevel;
    Vector2 offset;
    float extent;

    Vector3 localUp, axisA, axisB;
    Planet planet;
    Vector3 centerPointOnPlanet;

    public TerrainChunk(int lodLevel, Vector2 offset, float extent, Vector3 localUp, Planet planet, Transform parent)
    {
        this.lodLevel = lodLevel;
        this.offset = offset;
        this.extent = extent;
        this.localUp = localUp;
        this.planet = planet;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        meshObj = new GameObject($"Chunk_LOD{lodLevel}");
        meshObj.transform.parent = parent;
        meshObj.transform.localPosition = Vector3.zero;

        // On stocke les références pour y accéder rapidement
        meshRenderer = meshObj.AddComponent<MeshRenderer>();
        meshFilter = meshObj.AddComponent<MeshFilter>();

        if (planet.planetMaterial != null) meshRenderer.sharedMaterial = planet.planetMaterial;

        mesh = new Mesh();
        meshFilter.sharedMesh = mesh;

        Vector2 centerPercent = offset + new Vector2(0.5f, 0.5f) * extent;
        Vector3 pointOnCube = localUp + (centerPercent.x - 0.5f) * 2 * axisA + (centerPercent.y - 0.5f) * 2 * axisB;
        centerPointOnPlanet = pointOnCube.normalized * planet.radius;

        ConstructMesh();
    }

    public void UpdateChunk(Vector3 viewerPosition)
    {
        float distanceToViewer = Vector3.Distance(centerPointOnPlanet, viewerPosition);
        float lodThreshold = planet.radius * extent * planet.lodDistanceMultiplier;

        if (distanceToViewer < lodThreshold && lodLevel < planet.maxLodLevel)
        {
            if (!hasChildren) Subdivide();

            // CRUCIAL : On masque uniquement l'image, on ne désactive pas l'objet
            meshRenderer.enabled = false;

            foreach (TerrainChunk child in children)
            {
                child.UpdateChunk(viewerPosition);
            }
        }
        else
        {
            if (hasChildren) Merge();

            // Réaffiche le parent une fois les enfants détruits
            meshRenderer.enabled = true;
        }
    }

    void Subdivide()
    {
        children = new TerrainChunk[4];
        float halfExtent = extent * 0.5f;

        children[0] = new TerrainChunk(lodLevel + 1, offset + new Vector2(0, halfExtent), halfExtent, localUp, planet, meshObj.transform);
        children[1] = new TerrainChunk(lodLevel + 1, offset + new Vector2(halfExtent, halfExtent), halfExtent, localUp, planet, meshObj.transform);
        children[2] = new TerrainChunk(lodLevel + 1, offset, halfExtent, localUp, planet, meshObj.transform);
        children[3] = new TerrainChunk(lodLevel + 1, offset + new Vector2(halfExtent, 0), halfExtent, localUp, planet, meshObj.transform);

        hasChildren = true;
    }

    void Merge()
    {
        if (!hasChildren) return;

        for (int i = 0; i < 4; i++)
        {
            children[i].Merge();
            children[i].DestroyChunk(); // Appel de notre nettoyage complet
        }
        children = null;
        hasChildren = false;
    }

    public void DestroyChunk()
    {
        if (hasChildren) Merge();

        // 1. Purge du maillage dans la mémoire vidéo
        if (mesh != null)
        {
            if (Application.isPlaying) GameObject.Destroy(mesh);
            else GameObject.DestroyImmediate(mesh);
        }

        // 2. Suppression propre de l'objet dans la hiérarchie
        if (meshObj != null)
        {
            if (Application.isPlaying) GameObject.Destroy(meshObj);
            else GameObject.DestroyImmediate(meshObj);
        }
    }

    void ConstructMesh()
    {
        int res = planet.resolution;
        Vector3[] vertices = new Vector3[res * res];
        int[] triangles = new int[(res - 1) * (res - 1) * 6];
        Color[] colors = new Color[vertices.Length];
        int triIndex = 0;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                int i = x + y * res;
                Vector2 chunkPercent = offset + (new Vector2(x, y) / (res - 1)) * extent;
                Vector3 pointOnUnitCube = localUp + (chunkPercent.x - 0.5f) * 2 * axisA + (chunkPercent.y - 0.5f) * 2 * axisB;

                float x2 = pointOnUnitCube.x * pointOnUnitCube.x;
                float y2 = pointOnUnitCube.y * pointOnUnitCube.y;
                float z2 = pointOnUnitCube.z * pointOnUnitCube.z;

                Vector3 pointOnUnitSphere;
                pointOnUnitSphere.x = pointOnUnitCube.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + (y2 * z2) / 3f);
                pointOnUnitSphere.y = pointOnUnitCube.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + (x2 * z2) / 3f);
                pointOnUnitSphere.z = pointOnUnitCube.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + (x2 * y2) / 3f);

                float elevation = planet.noiseFilter.Evaluate(pointOnUnitSphere);
                vertices[i] = pointOnUnitSphere * (planet.radius + elevation);

                if (planet.colorGradient != null)
                {
                    float colorPercent = Mathf.InverseLerp(0, planet.noiseStrength, elevation);
                    colors[i] = planet.colorGradient.Evaluate(colorPercent);
                }

                if (x != res - 1 && y != res - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + res + 1;
                    triangles[triIndex + 2] = i + res;
                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + res + 1;
                    triIndex += 6;
                }
            }
        }

        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }
}