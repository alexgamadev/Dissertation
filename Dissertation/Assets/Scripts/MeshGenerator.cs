using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateMesh(int xQuads, int zQuads, int terrainHeightMultiplier, float quadSize, float[] heightMap)
    {
        MeshData meshData = new MeshData(xQuads, zQuads);
        int xVerts = xQuads + 1;
        int zVerts = zQuads + 1;

        float topLeftX = (xQuads) / -2f;
        float topLeftZ = (zQuads) / 2f;
 
        for (int index = 0, z = 0; z < zVerts; z++)
        {
            for (int x = 0; x < xVerts; x++, index++)
            {
                //Add vertex at current index and position
                meshData.AddVertex(index, new Vector3((x), heightMap[z * xVerts + x] * terrainHeightMultiplier, (z)));
                meshData.AddUVs(index, new Vector2(x / (float)xVerts, z / (float)zVerts));

                //Ensure quads aren't generated from outmost vertices
                if(x < xQuads && z < zQuads)
                {
                    meshData.AddQuad(index, index + 1, index + xVerts, index + xVerts + 1); 
                }
            }
        }

        return meshData;
    }
}

public struct MeshData
{
    public int[] triangles;
    public Vector3[] vertices;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int width, int height)
    {
        int numVertices = (width + 1) * (height + 1);
        vertices = new Vector3[numVertices];
        uvs = new Vector2[numVertices];
        triangles = new int[(width * height) * 6];
        triangleIndex = 0;
    }

    public void AddVertex(int vertexIndex, Vector3 position)
    {
        vertices[vertexIndex] = position;
    }

    public void AddUVs(int vertexIndex, Vector2 uv)
    {
        uvs[vertexIndex] = uv;
    }

    public void AddQuad(int vertex0, int vertex1, int vertex2, int vertex3)
    {
        AddTriangle(vertex0, vertex2, vertex1); //Bottom left, top left, bottom right
        AddTriangle(vertex1, vertex2, vertex3); //Bottom right, top left, top right
    }

    public void AddTriangle(int vertex0, int vertex1, int vertex2)
    {
        triangles[triangleIndex] = vertex0;
        triangles[triangleIndex + 1] = vertex1;
        triangles[triangleIndex + 2] = vertex2;
        triangleIndex += 3;
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }

}
