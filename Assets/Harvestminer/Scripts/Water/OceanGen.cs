using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class OceanGen : MonoBehaviour
{
    public int tileCount = 5;
    public float tileWidth = 10;
    public int tileResolution = 50;

    public Material waterMaterial;

    void Start()
    {
        GenerateOcean();
    }

    void GenerateOcean()
    {
        DeleteTiles();

        for (int x = 0; x < tileCount; x++)
        {
            for (int z = 0; z < tileCount; z++)
            {
                GameObject go = new GameObject("Water");
                go.transform.parent = this.transform;
                go.transform.position = 
                    this.transform.position + new Vector3(x * tileWidth, 0,z * tileWidth);

                WaterMeshGen water = go.AddComponent<WaterMeshGen>();
                water.width = tileWidth;
                water.resolution = tileResolution;

                go.GetComponent<MeshRenderer>().material = waterMaterial;
            }
        }
    }

    void DeleteTiles()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }   
    }
}
