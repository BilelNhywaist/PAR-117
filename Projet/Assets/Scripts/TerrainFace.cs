using UnityEngine;
using System.Collections.Generic;

public class TerrainFace
{
    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    Planet planet;
    NoiseFilter noiseFilter;

    // Caches statiques pour éviter les allocations répétées et soulager le Garbage Collector
    private static Dictionary<int, Vector3[]> verticesCache = new Dictionary<int, Vector3[]>();
    private static Dictionary<int, int[]> trianglesCache = new Dictionary<int, int[]>();
    private static Dictionary<int, Color[]> colorsCache = new Dictionary<int, Color[]>();

    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, Planet planet)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.planet = planet;
        noiseFilter = new NoiseFilter();
        noiseFilter.strength = planet.noiseStrength;
        noiseFilter.baseRoughness = planet.baseRoughness;
        noiseFilter.numLayers = planet.octaves;
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh()
    {
        int vertexCount = resolution * resolution;
        int triangleCount = (resolution - 1) * (resolution - 1) * 6;

        // Récupération ou création sécurisée dans le cache
        if (!verticesCache.TryGetValue(resolution, out Vector3[] vertices) || vertices.Length < vertexCount)
        {
            vertices = new Vector3[vertexCount];
            verticesCache[resolution] = vertices;
        }

        if (!colorsCache.TryGetValue(resolution, out Color[] colors) || colors.Length < vertexCount)
        {
            colors = new Color[vertexCount];
            colorsCache[resolution] = colors;
        }

        if (!trianglesCache.TryGetValue(resolution, out int[] triangles) || triangles.Length < triangleCount)
        {
            triangles = new int[triangleCount];
            trianglesCache[resolution] = triangles;
        }

        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;

                // Mapping sphérique sans distorsion
                float x2 = pointOnUnitCube.x * pointOnUnitCube.x;
                float y2 = pointOnUnitCube.y * pointOnUnitCube.y;
                float z2 = pointOnUnitCube.z * pointOnUnitCube.z;

                Vector3 pointOnUnitSphere;
                pointOnUnitSphere.x = pointOnUnitCube.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + (y2 * z2) / 3f);
                pointOnUnitSphere.y = pointOnUnitCube.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + (x2 * z2) / 3f);
                pointOnUnitSphere.z = pointOnUnitCube.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + (x2 * y2) / 3f);

                // Calcul de l'élévation locale
                float elevation = noiseFilter.Evaluate(pointOnUnitSphere);

                // Positionnement spatial
                vertices[i] = pointOnUnitSphere * (planet.radius + elevation);

                // Application du dégradé selon l'altitude
                if (planet.colorGradient != null)
                {
                    float colorPercent = Mathf.InverseLerp(0, planet.noiseStrength, elevation);
                    colors[i] = planet.colorGradient.Evaluate(colorPercent);
                }

                // Configuration des triangles
                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;
                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices, 0, vertexCount);
        mesh.SetTriangles(triangles, 0, triangleCount, 0);
        mesh.SetColors(colors, 0, vertexCount);
        mesh.RecalculateNormals();
    }
}