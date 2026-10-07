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

    // La ligne 'OpenSimplexNoise noiseGenerator...' a été supprimée ici.

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;
        float weight = 1f; // Poids pour adoucir le fond des vallées

        for (int i = 0; i < numLayers; i++)
        {
            float x = point.x * frequency + center.x;
            float y = point.y * frequency + center.y;
            float z = point.z * frequency + center.z;

            // L'appel au nouveau bruit continu (méthode statique)
            float v = SimplexNoise.Evaluate(x, y, z);

            // Transformation "Ridged" pour des montagnes acérées
            v = 1 - Mathf.Abs(v);
            v *= v;
            v *= weight;
            weight = Mathf.Clamp01(v * 2f); // Modifie l'impact des prochaines octaves

            noiseValue += v * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }

        // Création d'un plancher bas (ex: pour lisser le fond des océans)
        noiseValue = Mathf.Max(0, noiseValue - minValue);

        return noiseValue * strength;
    }
}