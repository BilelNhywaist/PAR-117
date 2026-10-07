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
        Color[] colors = new Color[vertices.Length]; // NOUVEAU
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;

                Vector3 p = pointOnUnitCube;
                float x2 = p.x * p.x;
                float y2 = p.y * p.y;
                float z2 = p.z * p.z;

                Vector3 pointOnUnitSphere;
                pointOnUnitSphere.x = p.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + (y2 * z2) / 3f);
                pointOnUnitSphere.y = p.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + (x2 * z2) / 3f);
                pointOnUnitSphere.z = p.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + (x2 * y2) / 3f);

                // Calcul de l'élévation
                float elevation = noiseFilter.Evaluate(pointOnUnitSphere);
                vertices[i] = pointOnUnitSphere * (1 + elevation);

                // NOUVEAU : Détermination de la couleur
                // On convertit l'élévation en pourcentage (0 à 1) basé sur la force maximale du bruit
                float colorPercent = Mathf.InverseLerp(0, noiseFilter.strength, elevation);
                colors[i] = planet.colorGradient.Evaluate(colorPercent);

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
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors; 
        mesh.RecalculateNormals();
    }
}