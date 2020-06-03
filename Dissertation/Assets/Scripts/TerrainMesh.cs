using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerrainMesh : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    
    // Start is called before the first frame update
    public void GenerateTerrainMesh(int terrainWidth, int terrainDepth, int terrainHeightMultiplier, float[] heightMap)
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        MeshData meshData = MeshGenerator.GenerateMesh(terrainWidth, terrainDepth, terrainHeightMultiplier, 1, heightMap);

        meshFilter.sharedMesh = meshData.GenerateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
