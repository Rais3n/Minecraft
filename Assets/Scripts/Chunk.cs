using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Chunk
{

    private int width;

    public GameObject gameObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangle = new List<int>();
    List<Vector2> uv = new List<Vector2>();
    int triangleIndex = 0;
    int xChunkGlobalPos;
    int zChunkGlobalPos;


    public Chunk(int xChunkGlobalPos,int zChunkGlobalPos, Transform parent, World world)
    {
        this.xChunkGlobalPos = xChunkGlobalPos;
        this.zChunkGlobalPos = zChunkGlobalPos;
        width = world.GetChunkWidth();
        gameObject = new GameObject();
        gameObject.transform.SetParent(parent);
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer.material = world.material;
    }

    public void VisualizeChunk(World world)
    {
        GenerateBiome(world);
        CreateMesh();
        ClearLists();
    }
    private void ClearLists()
    {
        vertices.Clear();
        triangle.Clear();
        uv.Clear();
        triangleIndex = 0;
    }
    private void GenerateBiome(World world)
    {
        int maxHeight = world.GetMaxChunkHeight();
        for (int x = 0; x < width; x++)
            for (int z = 0; z < width; z++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    if (world.GetBlock(xChunkGlobalPos + x, y,zChunkGlobalPos + z) == 1)
                    {
                        Vector3Int blockInWorldPosition = new Vector3Int(x + xChunkGlobalPos, y, z + zChunkGlobalPos);
                        Vector3Int blockInLocalPosition = new Vector3Int(x, y, z);
                        DrawBlock(blockInWorldPosition, blockInLocalPosition, maxHeight, xChunkGlobalPos, zChunkGlobalPos);
                    }
                    else break;
                }
            }
    }
    private void DrawBlock(Vector3Int blockInWorldPosition, Vector3Int blockInLocalPosition, int maxHeight, int xChunkPos, int zChunkPos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!DrawWall(i,blockInWorldPosition, maxHeight))
                continue;
            for (int j = 0; j < 6; j++)
            {
                int vertexIndex = BlockData.triangles[i, j];
                Vector2 vectorUV = BlockData.uv[j];
                uv.Add(GetVectorUV(vectorUV));
                vertices.Add(BlockData.vertex[vertexIndex] + blockInWorldPosition);
                triangle.Add(triangleIndex);
                triangleIndex++;
            }
        }
    }
    
    private bool DrawWall(int i, Vector3Int globalPos,int maxHeight)
    {
        try
        {
            if (i == 0)
            {
                if (!World.Instance.IsChunkUsingGlobalPos(globalPos.x, globalPos.z - 1))
                    return true;
                if (World.Instance.GetBlock(globalPos.x, globalPos.y, globalPos.z - 1) == 1)
                    return false;
            }
            if (i == 1)
            {
                if(!World.Instance.IsChunkUsingGlobalPos(globalPos.x + 1, globalPos.z))
                    return true;
                if (World.Instance.GetBlock(globalPos.x + 1, globalPos.y, globalPos.z) == 1)
                    return false;
            }
            if (i == 2)
            {
                if(!World.Instance.IsChunkUsingGlobalPos(globalPos.x, globalPos.z + 1))
                    return true;
                if (World.Instance.GetBlock(globalPos.x, globalPos.y, globalPos.z + 1) == 1)
                    return false;
            }
            if (i == 3)
            {
                if (!World.Instance.IsChunkUsingGlobalPos(globalPos.x - 1, globalPos.z))
                    return true;
                if (World.Instance.GetBlock(globalPos.x - 1, globalPos.y, globalPos.z) == 1)
                    return false;
            }
            if (i == 4)
            {
                if (World.Instance.GetBlock(globalPos.x, globalPos.y + 1, globalPos.z) == 1)
                    return false;
            }
            if (i == 5)
            {
                if (World.Instance.GetBlock(globalPos.x, globalPos.y - 1, globalPos.z) == 1)
                    return false;
            }
        }
        catch(ArgumentOutOfRangeException)
        {
            return true;
        }
        catch (NullReferenceException)
        {
            return true;
        }
        return true;
    }


    private Vector2 GetVectorUV(Vector2 offsetUV)
    {
        int Index = 2;
        int textureWidth = 256;
        int numOfTextureInLine = 16;
        float coordX = (float)(Index % numOfTextureInLine*numOfTextureInLine)/textureWidth;
        float coordY = (float)(240 - Index / numOfTextureInLine * numOfTextureInLine)/textureWidth;
        Vector2 uv = new Vector2(coordX, coordY) + offsetUV/numOfTextureInLine;
        return uv;
    }
    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangle.ToArray();
        mesh.uv = uv.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

    }
}


