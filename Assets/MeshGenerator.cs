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
        CreateTris();
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

        float dx = size.x / numFaces.x;
        float dy = size.y / numFaces.y;

        int idx = 0;
        for (int y = 0; y <= numFaces.y; y++) {
            for (int x = 0; x <= numFaces.x; x++) {
                vertices[idx] = new Vector3(dx * x, 0, dy * y);
                uvs[idx] = new Vector2((float)x / numFaces.x, (float)y / numFaces.y);
                ++idx;
            }
        }
    }
    void CreateTris() {
        triangles = new int[numFaces.x * numFaces.y * 6];

        int vertIdx = 0;
        int triIdx = 0;
        for (int y = 0; y < numFaces.y; y++) {
            for (int x = 0; x < numFaces.x; x++) {
                // lower left triangle of quad
                triangles[triIdx + 0] = vertIdx + 0;
                triangles[triIdx + 1] = vertIdx + numFaces.x + 1;
                triangles[triIdx + 2] = vertIdx + 1;

                // upper right triangle of quad
                triangles[triIdx + 3] = vertIdx + 1;
                triangles[triIdx + 4] = vertIdx + numFaces.x + 1;
                triangles[triIdx + 5] = vertIdx + numFaces.x + 2;

                ++vertIdx;
                triIdx += 6;
            }
            ++vertIdx; // skips triangle going from one row to the next
        }
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
