using UnityEngine;

public class TerrainFace
{
    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    Planet planet;
    NoiseFilter noiseFilter;
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
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        Color[] colors = new Color[vertices.Length];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;

                // Mapping sphérique parfait
                float x2 = pointOnUnitCube.x * pointOnUnitCube.x;
                float y2 = pointOnUnitCube.y * pointOnUnitCube.y;
                float z2 = pointOnUnitCube.z * pointOnUnitCube.z;

                Vector3 pointOnUnitSphere;
                pointOnUnitSphere.x = pointOnUnitCube.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + (y2 * z2) / 3f);
                pointOnUnitSphere.y = pointOnUnitCube.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + (x2 * z2) / 3f);
                pointOnUnitSphere.z = pointOnUnitCube.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + (x2 * y2) / 3f);

                // Élévation
                float elevation = noiseFilter.Evaluate(pointOnUnitSphere);
                vertices[i] = pointOnUnitSphere * (1 + elevation);

                // Couleur (sécurisée pour éviter les plantages si le gradient est vide)
                if (planet.colorGradient != null)
                {
                    float colorPercent = Mathf.InverseLerp(0, planet.noiseStrength, elevation);
                    colors[i] = planet.colorGradient.Evaluate(colorPercent);
                }

                // Triangles
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

        // ORDRE CRUCIAL : On dit à Unity d'accepter les gros maillages AVANT de lui donner les vertices
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        // On laisse le moteur C++ interne de Unity calculer les ombres parfaitement
        mesh.RecalculateNormals();
    }
}