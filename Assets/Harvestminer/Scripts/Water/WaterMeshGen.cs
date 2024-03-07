using JetBrains.Annotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
public class WaterMeshGen : MonoBehaviour
{
    public int resolution = 20;

    public float width = 10;

    void Start()
    {
        CreateWaterPlane();
    }

    void CreateWaterPlane()
    {
        Mesh mesh = new();

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector3> normals = new();
        List<Vector2> uv = new();

        int count = 0;
        float stepSize = width / resolution;

        //Debug.Log(Mathf.Ceil(resolution / 2));

        for (int x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                Quad quad = CreateQuad(width * x, width * z, stepSize, count * 4);

                vertices.AddRange(quad.vertices);
                triangles.AddRange(quad.triangles);
                normals.AddRange(quad.normals);
                uv.AddRange(quad.uv);

                count++;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uv.ToArray();

        mesh.RecalculateBounds();

        this.GetComponent<MeshFilter>().mesh = mesh;
    }

    Quad CreateQuad(float x, float z, float stepSize, int triCount)
    {
        float offsetX = x / resolution;
        float offsetZ = z / resolution;

        return new Quad()
        {
            vertices = new Vector3[4]
            {
                new(offsetX - 0       , 0, offsetZ - 0       ),
                new(offsetX - 0       , 0, offsetZ + stepSize),
                new(offsetX + stepSize, 0, offsetZ + stepSize),
                new(offsetX + stepSize, 0, offsetZ - 0       )
            },

            triangles = new int[6]
            {
                triCount, triCount + 1, triCount + 2,
                triCount, triCount + 2, triCount + 3
            },

            normals = new Vector3[4]
            {
                new(0, 1, 0),
                new(0, 1, 0),
                new(0, 1, 0),
                new(0, 1, 0)
            },

            uv = new Vector2[4]
            {
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0)
            }
        };
    }

    struct Quad
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector3[] normals;
        public Vector2[] uv;
    }

    void OnValidate()
    {
        CreateWaterPlane();
    }
}