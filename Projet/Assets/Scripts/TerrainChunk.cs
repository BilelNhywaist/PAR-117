using UnityEngine;
using System.Collections.Generic;

public class TerrainChunk
{
    public GameObject meshObj;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    // Paramètres Quadtree
    TerrainChunk[] children;
    bool hasChildren;
    int lodLevel;
    Vector2 offset;
    float extent;

    // Paramètres Planète
    Vector3 localUp, axisA, axisB;
    Planet planet;
    Vector3 centerPointOnPlanet;

    // Caches statiques pour éviter les allocations (Garbage Collector)
    private static Dictionary<int, Vector3[]> verticesCache = new Dictionary<int, Vector3[]>();
    private static Dictionary<int, int[]> trianglesCache = new Dictionary<int, int[]>();
    private static Dictionary<int, Color[]> colorsCache = new Dictionary<int, Color[]>();

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
        meshObj.transform.localRotation = Quaternion.identity;

        meshRenderer = meshObj.AddComponent<MeshRenderer>();
        meshFilter = meshObj.AddComponent<MeshFilter>();
        meshCollider = meshObj.AddComponent<MeshCollider>();

        if (planet.planetMaterial != null)
        {
            meshRenderer.sharedMaterial = planet.planetMaterial;
        }

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
            if (!hasChildren)
            {
                Subdivide();
            }

            if (children != null)
            {
                foreach (TerrainChunk child in children)
                {
                    if (child != null) child.UpdateChunk(viewerPosition);
                }
            }
            // NOTE : La désactivation du parent a été retirée pour forcer l'affichage
        }
        else
        {
            if (hasChildren)
            {
                Merge();
            }
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
            if (children[i] != null)
            {
                children[i].Merge();
                if (children[i].meshObj != null)
                {
                    Object.DestroyImmediate(children[i].meshObj);
                }
                children[i] = null;
            }
        }
        children = null;
        hasChildren = false;
    }

    void ConstructMesh()
    {
        int res = planet.resolution;
        int vertexCount = res * res;
        int triangleCount = (res - 1) * (res - 1) * 6;

        if (!verticesCache.TryGetValue(res, out Vector3[] vertices) || vertices.Length < vertexCount)
        {
            vertices = new Vector3[vertexCount];
            verticesCache[res] = vertices;
        }

        if (!colorsCache.TryGetValue(res, out Color[] colors) || colors.Length < vertexCount)
        {
            colors = new Color[vertexCount];
            colorsCache[res] = colors;
        }

        if (!trianglesCache.TryGetValue(res, out int[] triangles) || triangles.Length < triangleCount)
        {
            triangles = new int[triangleCount];
            trianglesCache[res] = triangles;
        }

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
                else
                {
                    colors[i] = new Color(0.3f, 0.6f, 0.3f);
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

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }
}