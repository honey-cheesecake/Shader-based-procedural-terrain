using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseProfile {
    public float seed;
    public bool ridge;
    public float scale = 30;
    [Range(0, 10)] public float pow = 1;
    [Range(1, 5)] public int octaves = 3;
    [RangeAttribute(0, 1)] public float persistance = 0.5f;
    [RangeAttribute(1, 10)] public float lacunarity = 2;

    public void Copy(NoiseProfile src) {
        seed = src.seed;
        ridge = src.ridge;
        scale = src.scale;
        pow = src.pow;
        octaves = src.octaves;
        persistance = src.persistance;
        lacunarity = src.lacunarity;
    }
    public static bool operator ==(NoiseProfile a, NoiseProfile b) {
        return (a.seed == b.seed) &&
        (a.ridge == b.ridge) &&
        (a.scale == b.scale) &&
        (a.pow == b.pow) &&
        (a.octaves == b.octaves) &&
        (a.persistance == b.persistance) &&
        (a.lacunarity == b.lacunarity);
    }

    public static bool operator !=(NoiseProfile a, NoiseProfile b) {
        return !(a == b);
    }
}
