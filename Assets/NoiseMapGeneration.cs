using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGeneration : MonoBehaviour {

    public static float MaxHeight(NoiseProfile noiseProfile) {
        float output = 0;
        for (int p = 0; p < noiseProfile.octaves; p++) {
            output += Mathf.Pow(noiseProfile.persistance, p);
        }
        return output;
    }
    public static float Evaluate(float x, float y, NoiseProfile noiseProfile) {
        float noise = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float normalization = MaxHeight(noiseProfile); // maximum amplitude possible

        for (int i = 0; i < noiseProfile.octaves; i++) {
            // calculate sample indices based on the coordinates and the scale
            float sampleX = x / noiseProfile.scale * frequency;
            float sampleZ = y / noiseProfile.scale * frequency;

            // generate noise value using PerlinNoise for a given Wave
            noise += amplitude * Mathf.PerlinNoise(sampleX + noiseProfile.seed, sampleZ + noiseProfile.seed);
            normalization += amplitude;

            amplitude *= noiseProfile.persistance;
            frequency *= noiseProfile.lacunarity;
        }

        // ridge at 0.5
        if (noiseProfile.ridge) {
            noise = 2 * Mathf.Abs(0.5f - noise);
            noise = 1 - noise;
        }

        // normalize the noise value so that it is within 0 and 1
        if (noise > normalization) Debug.LogWarning("Noise level was bigger than normalization");
        noise /= normalization;

        // prevents NaN
        if (noise < 0) noise = 0;
        noise = Mathf.Pow(noise, noiseProfile.pow);

        return noise;
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float offsetX, float offsetY, NoiseProfile noiseProfile) {
        // create an empty noise map with the mapDepth and mapWidth coordinates
        float[,] noiseMap = new float[mapHeight, mapWidth];

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                noiseMap[y, x] = Evaluate(x + offsetX, y + offsetY, noiseProfile);
            }
        }
        return noiseMap;
    }

}
