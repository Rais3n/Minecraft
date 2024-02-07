using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Chunk
{

    private int width;
    //private static readonly int forward = 2;
    //private static readonly int back = 0;
    //private static readonly int right = 1;
    //private static readonly int left = 3;
    private static readonly int up = 4;
    private static readonly int down = 5;
    public GameObject gameObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangle = new List<int>();

    List<Color> colors = new List<Color>();

    List<Vector2> uv = new List<Vector2>();
    private int triangleIndex;
    public int xGlobalPos;
    public int zGlobalPos;
    public int xListPos;
    public int zListPos;


    public Chunk(int xGlobalPos,int zGlobalPos, Transform parent, int xListPos, int zListPos)
    {
        this.xGlobalPos = xGlobalPos;
        this.zGlobalPos = zGlobalPos;
        this.xListPos = xListPos;
        this.zListPos = zListPos;
        width = World.Instance.GetChunkWidth();
        gameObject = new GameObject();
        gameObject.transform.SetParent(parent);
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer.material = World.Instance.material;
    }

    public void VisualizeChunk()
    {
        DrawChunk();
        CreateMesh();
        ClearLists();
    }
    private void ClearLists()
    {
        vertices.Clear();
        triangle.Clear();
        uv.Clear();
        colors.Clear();
        triangleIndex = 0;
    }
    private void DrawChunk()
    {
        int maxHeight = World.Instance.GetMaxChunkHeight();
        for (int x = 0; x < width; x++)
            for (int z = 0; z < width; z++)
            {
                for (int y = 0; y < maxHeight; y++)
                {
                    //int kindOfBlock = World.Instance.GetBlock(xGlobalPos + x, y, zGlobalPos + z);
                    int kindOfBlock = World.Instance.GetBlock(xListPos * width + x, y, zListPos * width + z);
                    if(kindOfBlock != BlockData.kindOfBlock["none"]) 
                    {
                        Vector3Int blockInWorldPosition = new Vector3Int(x + xGlobalPos, y, z + zGlobalPos);
                        Vector3Int blockInList = new Vector3Int(xListPos * width + x, y, zListPos * width + z);
                        DrawBlock(blockInWorldPosition, kindOfBlock, blockInList);
                    }
                    else break;
                }
            }
    }
    
    
    private bool DrawWall(int i, Vector3Int listPos)
    {
        try
        {
            if (i == 0)
            {
                if (World.Instance.GetBlock(listPos.x, listPos.y, listPos.z - 1) != BlockData.kindOfBlock["none"])
                    return false;
            }
            if (i == 1)
            {
                if (World.Instance.GetBlock(listPos.x + 1, listPos.y, listPos.z) != BlockData.kindOfBlock["none"])
                    return false;
            }
            if (i == 2)
            {
                if (World.Instance.GetBlock(listPos.x, listPos.y, listPos.z + 1) != BlockData.kindOfBlock["none"])
                    return false;
            }
            if (i == 3)
            {
                if (World.Instance.GetBlock(listPos.x - 1, listPos.y, listPos.z) != BlockData.kindOfBlock["none"])
                    return false;
            }
            if (i == 4)
            {
                if (World.Instance.GetBlock(listPos.x, listPos.y + 1, listPos.z) != BlockData.kindOfBlock["none"])
                    return false;
            }
            if (i == 5)
            {
                if (World.Instance.GetBlock(listPos.x, listPos.y - 1, listPos.z) != BlockData.kindOfBlock["none"])
                    return false;
            }
        }
        catch(ArgumentOutOfRangeException)
        {
            //return true;
            return false;
        }
        catch (NullReferenceException)
        {
            //return true;
            return false;
        }
        return true;
    }

    private void DrawBlock(Vector3Int blockInWorldPosition, int kindOfBlock, Vector3Int blockInList)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!DrawWall(i, blockInList))
                continue;
            for (int vertex = 0; vertex < 6; vertex++)
            {
                int vertexIndex = BlockData.triangles[i, vertex];
                Vector2 vectorUV = BlockData.uv[vertex];
                uv.Add(GetVectorUV(vectorUV, kindOfBlock, i));
                vertices.Add(BlockData.vertex[vertexIndex] + blockInWorldPosition);
                triangle.Add(triangleIndex);
                if (i == up && kindOfBlock == BlockData.kindOfBlock["greenDirt"])
                    colors.Add(new Color(0.509f, 1f, 0.236f, 1f));
                else
                    colors.Add(new Color(1f, 1f, 1f, 0f));
                triangleIndex++;
            }
        }
    }

    private Vector2 GetVectorUV(Vector2 offsetUV, int kindOfWall, int wall)
    {
        int blockTextureIndex=GetBlockID(kindOfWall, wall);

        int textureWidth = 256;
        int blockTextureWidth = 16;
        float coordX = (float)(blockTextureIndex % blockTextureWidth*blockTextureWidth)/textureWidth;
        float coordY = (float)(240 - blockTextureIndex / blockTextureWidth * blockTextureWidth)/textureWidth;
        Vector2 uv = new Vector2(coordX, coordY) + offsetUV/blockTextureWidth;
        return uv;
    }

    private int GetBlockID(int kindOfWall, int wall)
    {
        int blockTextureIndex;
        if (kindOfWall == BlockData.kindOfBlock["stone"])
            blockTextureIndex = 1;
        else if (kindOfWall == BlockData.kindOfBlock["greenDirt"])
        {
            if (wall == up)
                blockTextureIndex = 40;
            else if (wall == down)
                blockTextureIndex = 2;
            else blockTextureIndex = 3;
        }
        else if (kindOfWall == BlockData.kindOfBlock["dirt"])
            blockTextureIndex = 2;
        else if (kindOfWall == BlockData.kindOfBlock["sand"])
            blockTextureIndex = 18;
        else if (kindOfWall == BlockData.kindOfBlock["dirt-snow"])
        {
            if(wall == up)
                blockTextureIndex = 66;
            else if (wall == down)
                blockTextureIndex = 2;
            else blockTextureIndex = 68;
        }
        else if (kindOfWall == BlockData.kindOfBlock["leaves"])
        {
            blockTextureIndex = 52;
        }
        else
            blockTextureIndex = 17;

        return blockTextureIndex;

    }
    private void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangle.ToArray();
        mesh.uv = uv.ToArray();
        mesh.colors = colors.ToArray();

        

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}


