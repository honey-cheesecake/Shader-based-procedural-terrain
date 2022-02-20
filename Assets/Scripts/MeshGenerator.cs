using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {
    [SerializeField] MeshFilter meshFilter = null;
    [SerializeField] Vector2Int numFaces = Vector2Int.one;
    [SerializeField] Vector2 size = Vector2.one;
    [SerializeField] float baseHeightMult = 1f;
    [SerializeField] NoiseProfile baseNoise = null;
    [SerializeField] float creviceHeightMult = 1f;
    [SerializeField] NoiseProfile creviceNoise = null;

    [SerializeField] ComputeShader vertGeneratorShader = default;

    Mesh mesh; // output

    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles; // clockwise for right side up

    NoiseProfile baseNoiseCopy = null;
    NoiseProfile creviceNoiseCopy = null;
    float baseHeightMultCopy;
    float creviceHeightMultCopy;
    void Start() {
        GenerateMesh();
    }

    [ContextMenu("Generate")]
    void GenerateMesh() {
        // create new mesh and assign to meshfilter
        if (meshFilter == null) {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) {
                Debug.LogError("No meshfilter assigned and unable to get meshfilter compnent attached to gameobject!");
            }
        }
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        CreateVerts();
        SetHeight();

        // saves performance by only updating mesh when dirty
        baseNoiseCopy = new NoiseProfile();
        creviceNoiseCopy = new NoiseProfile();

        PushToMesh();
    }

    void Update() {
        // saves performance by only updating mesh when dirty
        if (baseNoiseCopy != baseNoise ||
        creviceNoiseCopy != creviceNoise ||
        baseHeightMultCopy != baseHeightMult ||
        creviceHeightMultCopy != creviceHeightMult) {

            baseNoiseCopy.Copy(baseNoise);
            creviceNoiseCopy.Copy(creviceNoise);
            baseHeightMultCopy = baseHeightMult;
            creviceHeightMultCopy = creviceHeightMult;

            SetHeight();
            PushToMesh();
        }
    }

    void CreateVerts() {
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

    void SetHeight() {
        //float[,] noise = NoiseMapGeneration.GenerateNoiseMap(numFaces.x + 1, numFaces.y + 1, 0, 0, noiseProfile);

        int idx = 0;
        for (int y = 0; y <= numFaces.y; y++) {
            for (int x = 0; x <= numFaces.x; x++) {
                float baseHeight = baseHeightMult * NoiseMapGeneration.Evaluate((float)x / numFaces.x * size.x, (float)y / numFaces.y * size.y, baseNoise);
                float creviceHeight = creviceHeightMult * NoiseMapGeneration.Evaluate((float)x / numFaces.x * size.x, (float)y / numFaces.y * size.y, creviceNoise);
                vertices[idx].y = baseHeight - creviceHeight;
                ++idx;
            }
        }

    }

    void PushToMesh() {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    /*
    private void OnDrawGizmos() {
        if (vertices == null) return;
        Gizmos.color = Color.red;
        foreach (Vector3 vert in vertices) {
            if (vert.y < 0.0001) {
                Gizmos.DrawSphere(vert, 0.1f);
            }
        }
    }
    */

}
