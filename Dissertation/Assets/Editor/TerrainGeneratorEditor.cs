using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator terrainGenerator = (TerrainGenerator)target;

        DrawDefaultInspector();

        if(GUILayout.Button("Generate Terrain"))
        {
            terrainGenerator.GenerateTerrain();
        }
        if(GUILayout.Button("Update Terrain Mesh"))
        {
            terrainGenerator.UpdateTerrainMesh();
        }
    }
}
