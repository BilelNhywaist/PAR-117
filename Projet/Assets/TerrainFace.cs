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
        public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        // 1. Initialisation du tableau des couleurs
        Color[] colors = new Color[vertices.Length];

        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;

                // Mapping sphérique
                float x2 = pointOnUnitCube.x * pointOnUnitCube.x;
                float y2 = pointOnUnitCube.y * pointOnUnitCube.y;
                float z2 = pointOnUnitCube.z * pointOnUnitCube.z;

                Vector3 pointOnUnitSphere;
                pointOnUnitSphere.x = pointOnUnitCube.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + (y2 * z2) / 3f);
                pointOnUnitSphere.y = pointOnUnitCube.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + (x2 * z2) / 3f);
                pointOnUnitSphere.z = pointOnUnitCube.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + (x2 * y2) / 3f);

                // Calcul de l'élévation via le NoiseFilter
                float elevation = noiseFilter.Evaluate(pointOnUnitSphere);
                vertices[i] = pointOnUnitSphere * (1 + elevation);

                // 2. CALCUL DE LA COULEUR
                // On transforme l'élévation en une valeur entre 0 (fond) et 1 (sommet maximal théorique)
                // L'élévation maximale dépend de votre "noiseStrength"
                float colorPercent = Mathf.InverseLerp(0, planet.noiseStrength, elevation);

                // On pioche la couleur correspondante dans le dégradé
                if (planet.colorGradient != null)
                {
                    colors[i] = planet.colorGradient.Evaluate(colorPercent);
                }

                // Construction des triangles
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
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // 3. Application des couleurs au maillage
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }
}