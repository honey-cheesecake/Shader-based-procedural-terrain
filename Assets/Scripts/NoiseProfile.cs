using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseProfile {
    public Vector2 offset;
    public bool ridge;
    [Range(0, 30)] public float scale = 0.2f;
    [Range(0, 10)] public float heightMult = 1;
    [Range(0, 10)] public float pow = 1;

    public void Copy(NoiseProfile src) {
        offset = src.offset;
        ridge = src.ridge;
        scale = src.scale;
        heightMult = src.heightMult;
        pow = src.pow;
    }

    public static bool operator ==(NoiseProfile a, NoiseProfile b) {
        return (a.offset == b.offset) &&
        (a.ridge == b.ridge) &&
        (a.scale == b.scale) &&
        (a.heightMult == b.heightMult) &&
        (a.pow == b.pow);
    }

    public static bool operator !=(NoiseProfile a, NoiseProfile b) {
        return !(a == b);
    }

    // auto generated 
    public override bool Equals(object obj)
    {
        var profile = obj as NoiseProfile;
        return profile != null &&
               offset == profile.offset &&
               ridge == profile.ridge &&
               scale == profile.scale &&
               pow == profile.pow;
    }

    public override int GetHashCode()
    {
        var hashCode = 789363370;
        hashCode = hashCode * -1521134295 + offset.GetHashCode();
        hashCode = hashCode * -1521134295 + ridge.GetHashCode();
        hashCode = hashCode * -1521134295 + scale.GetHashCode();
        hashCode = hashCode * -1521134295 + pow.GetHashCode();
        return hashCode;
    }
    // end

    public RenderTexture GenerateNoiseMap(int mapWidth, int mapHeight, ComputeShader shader)
    {

        var renderTexture = new RenderTexture(mapWidth, mapHeight, 0, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };
        renderTexture.Create();

        var kernel = shader.FindKernel("CSMain");
        shader.SetTexture(kernel, "Result", renderTexture);
        shader.SetInt("width", mapWidth);
        shader.SetInt("height", mapHeight);
        shader.SetFloat("scale", scale);
        shader.SetFloats("offset", offset.x, offset.y);
        shader.SetFloat("power", pow);
        shader.SetFloat("heightMult", heightMult);
        shader.SetBool("ridge", ridge);

        shader.GetKernelThreadGroupSizes(kernel, out uint threadsX, out uint threadsY, out uint threadsZ);
        shader.Dispatch(kernel, Mathf.CeilToInt((float)mapWidth / threadsX), Mathf.CeilToInt((float)mapHeight / threadsX), 1);

        return renderTexture;
    }
}
