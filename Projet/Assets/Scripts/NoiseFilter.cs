using UnityEngine;

public class NoiseFilter
{
    public float strength = 50f;
    public float baseRoughness = 1.5f;
    public float roughness = 2f;
    public float persistence = 0.5f;
    public Vector3 center;
    public float minValue = 0f;
    public int numLayers = 5;

    public float Evaluate(Vector3 point)
    {
        float continentFrequency = 0.15f;
        float maskValue = SimplexNoise.Evaluate(point.x * continentFrequency, point.y * continentFrequency, point.z * continentFrequency);

        float continentMask = Mathf.InverseLerp(-0.1f, 0.5f, maskValue);
        continentMask = Mathf.Pow(continentMask, 3f);
        continentMask = Mathf.SmoothStep(0f, 1f, continentMask);

        float noiseValue = 0;
        float frequency = baseRoughness * 2f;
        float amplitude = 1;
        float weight = 1f;

        for (int i = 0; i < numLayers; i++)
        {
            float x = point.x * frequency + center.x;
            float y = point.y * frequency + center.y;
            float z = point.z * frequency + center.z;

            float v = SimplexNoise.Evaluate(x, y, z);

            v = 1 - Mathf.Abs(v);
            v *= v;
            v *= weight;
            weight = Mathf.Clamp01(v * 2f);

            noiseValue += v * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - minValue);

        return noiseValue * strength * continentMask;
    }
}