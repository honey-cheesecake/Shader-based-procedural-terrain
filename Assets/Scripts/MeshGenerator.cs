using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MeshFilter meshFilter = default;
    [SerializeField] new Renderer renderer = default;
    [SerializeField] ComputeShader vertGeneratorShader = default;
    [SerializeField] ComputeShader heightmapGeneratorShader = default;
    [SerializeField] ComputeShader heightmapMixerShader = default;

    [Header("Settings")]
    [SerializeField] Vector2Int numFaces = Vector2Int.one;
    [SerializeField] Vector2 size = Vector2.one;

    [Header("Noise")]
    [SerializeField] NoiseProfile baseNoise = null;
    [SerializeField] NoiseProfile mountainNoise = null;
    [SerializeField] NoiseProfile creviceNoise = null;

    [SerializeField] RenderTexture baseTexture = default;
    [SerializeField] RenderTexture mountainTexture = default;
    [SerializeField] RenderTexture creviceTexture = default;
    [SerializeField] RenderTexture heightmap = default;

    Mesh mesh; // output

    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles; // clockwise for right side up

    NoiseProfile baseNoiseCopy = null;
    NoiseProfile mountainNoiseCopy = null;
    NoiseProfile creviceNoiseCopy = null;

    void Start()
    {
        GenerateMesh();
        GenerateHeightmap();
    }

    [ContextMenu("Test")]
    void FauxStart()
    {
        GenerateMesh();
        GenerateHeightmap();
    }

    void Update()
    {
        // saves performance by only updating mesh when dirty
        bool isDirty = false;


        if (baseNoiseCopy != baseNoise)
        {
            baseNoiseCopy.Copy(baseNoise);
            isDirty = true;
        }

        if (mountainNoiseCopy != mountainNoise)
        {
            mountainNoiseCopy.Copy(mountainNoise);
            isDirty = true;
        }

        if (creviceNoiseCopy != creviceNoise)
        {
            creviceNoiseCopy.Copy(creviceNoise);
            isDirty = true;
        }

        if (isDirty)
        {
            GenerateHeightmap();
        }
    }

    void GenerateMesh()
    {
        // create new mesh and assign to meshfilter
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.LogError("No meshfilter assigned and unable to get meshfilter compnent attached to gameobject!");
            }
        }
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // saves performance by only updating mesh when dirty
        baseNoiseCopy = new NoiseProfile();
        mountainNoiseCopy = new NoiseProfile();
        creviceNoiseCopy = new NoiseProfile();

        CreateVerts();
    }

    void CreateVerts()
    {
        vertices = new Vector3[(numFaces.x + 1) * (numFaces.y + 1)];
        uvs = new Vector2[(numFaces.x + 1) * (numFaces.y + 1)];
        triangles = new int[numFaces.x * numFaces.y * 6];

        var kernel = vertGeneratorShader.FindKernel("CSMain");

        // set variables
        vertGeneratorShader.SetFloat("dx", size.x / numFaces.x);
        vertGeneratorShader.SetFloat("dy", size.y / numFaces.y);
        vertGeneratorShader.SetInt("xCount", numFaces.x + 1);
        vertGeneratorShader.SetInt("yCount", numFaces.y + 1);

        // set buffers
        var vertsBuffer = new ComputeBuffer(vertices.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3)));
        var uvsBuffer = new ComputeBuffer(uvs.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector2)));
        var trisBuffer = new ComputeBuffer(triangles.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(int)));

        vertsBuffer.SetData(vertices);
        uvsBuffer.SetData(uvs);
        trisBuffer.SetData(triangles);

        vertGeneratorShader.SetBuffer(kernel, "verts", vertsBuffer);
        vertGeneratorShader.SetBuffer(kernel, "uvs", uvsBuffer);
        vertGeneratorShader.SetBuffer(kernel, "tris", trisBuffer);

        // diapatch
        vertGeneratorShader.GetKernelThreadGroupSizes(kernel, out uint threadsX, out uint threadsY, out uint threadsZ);
        vertGeneratorShader.Dispatch(kernel, Mathf.CeilToInt((float)(numFaces.x + 1) / threadsX), Mathf.CeilToInt((float)(numFaces.y + 1) / threadsY), 1);

        // retreive data
        vertsBuffer.GetData(vertices);
        uvsBuffer.GetData(uvs);
        trisBuffer.GetData(triangles);
        vertsBuffer.Dispose();
        uvsBuffer.Dispose();
        trisBuffer.Dispose();
    }

    void GenerateHeightmap()
    {
        //float[,] noise = NoiseMapGeneration.GenerateNoiseMap(numFaces.x + 1, numFaces.y + 1, 0, 0, noiseProfile);
        heightmap = new RenderTexture(numFaces.x + 1, numFaces.y + 1, 0, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true,
            filterMode = FilterMode.Point
        };
        heightmap.Create();

        // make component textures
        baseTexture = baseNoise.GenerateNoiseMap(numFaces.x + 1, numFaces.y + 1, heightmapGeneratorShader);
        mountainTexture = mountainNoise.GenerateNoiseMap(numFaces.x + 1, numFaces.y + 1, heightmapGeneratorShader);
        creviceTexture = creviceNoise.GenerateNoiseMap(numFaces.x + 1, numFaces.y + 1, heightmapGeneratorShader);

        // mix
        var kernel = heightmapMixerShader.FindKernel("CSMain");
        heightmapMixerShader.SetTexture(kernel, "heightmap", heightmap);
        heightmapMixerShader.SetTexture(kernel, "base", baseTexture);
        heightmapMixerShader.SetTexture(kernel, "mountain", mountainTexture);
        heightmapMixerShader.SetTexture(kernel, "crevice", creviceTexture);
        heightmapMixerShader.SetFloat("baseHeightMult", baseNoise.heightMult);
        heightmapMixerShader.SetFloat("mountainHeightMult", mountainNoise.heightMult);
        heightmapMixerShader.SetFloat("creviceHeightMult", creviceNoise.heightMult);
        heightmapMixerShader.SetInt("xCount", numFaces.x + 1);
        heightmapMixerShader.SetInt("yCount", numFaces.y + 1);

        // set buffers
        var vertsBuffer = new ComputeBuffer(vertices.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3)));
        vertsBuffer.SetData(vertices);
        heightmapMixerShader.SetBuffer(kernel, "verts", vertsBuffer);

        // dispatch
        heightmapMixerShader.GetKernelThreadGroupSizes(kernel, out uint threadsX, out uint threadsY, out uint threadsZ);
        heightmapMixerShader.Dispatch(kernel, Mathf.CeilToInt((float)(numFaces.x + 1) / threadsX), Mathf.CeilToInt((float)(numFaces.y + 1) / threadsY), 1);

        renderer.enabled = true;
        renderer.sharedMaterial.SetTexture("_Texture2D", mountainTexture);

        // retreive data
        vertsBuffer.GetData(vertices);
        vertsBuffer.Dispose();

        PushToMesh();

        baseTexture.Release();
        mountainTexture.Release();
        creviceTexture.Release();
        heightmap.Release();

        //ExportRenderTexture(heightMap, @"C:\Users\user\Desktop\Unity Projects\Shader based procedural terrain\Assets\heightmap.png");      
    }

    void PushToMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    // http://codewee.com/view.php?idx=131
    void ExportRenderTexture(RenderTexture renderTexture, string file)
    {
        RenderTexture.active = renderTexture;
        var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(texture);

        System.IO.File.WriteAllBytes(file, bytes);

    }

}
