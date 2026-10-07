using UnityEngine;

public class NoiseFilter
{
    public float strength = 1f;
    public float baseRoughness = 1f;
    public float roughness = 2f;
    public float persistence = 0.5f;
    public Vector3 center;
    public float minValue = 1f;
    public int numLayers = 4;

    // Instanciation de l'OpenSimplex (on peut passer une seed)
    OpenSimplexNoise noiseGenerator = new OpenSimplexNoise(42);

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;
        float weight = 1f; // Permet de lisser le fond des vallées

        for (int i = 0; i < numLayers; i++)
        {
            double x = point.x * frequency + center.x;
            double y = point.y * frequency + center.y;
            double z = point.z * frequency + center.z;

            // On récupère la valeur brute (entre -1 et 1)
            float v = (float)noiseGenerator.Evaluate(x, y, z);

            // Transformation "Ridged" : on inverse la valeur absolue
            v = 1 - Mathf.Abs(v);
            v *= v; // On met au carré pour pincer les crêtes
            v *= weight; // On applique le poids de l'octave précédente

            weight = Mathf.Clamp01(v * 2f); // Modifie le relief des prochaines itérations

            noiseValue += v * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - minValue);
        return noiseValue * strength;
    }
}