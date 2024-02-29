using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Chunk
{
    public static int width = 6;
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

    public List<Trees.treeData> plantList = new List<Trees.treeData>();


    public Chunk(int xGlobalPos,int zGlobalPos, Transform parent, int xListPos, int zListPos)
    {
        this.xGlobalPos = xGlobalPos;
        this.zGlobalPos = zGlobalPos;
        this.xListPos = xListPos;
        this.zListPos = zListPos;
        gameObject = new GameObject();
        gameObject.transform.SetParent(parent);
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer.material = World.Instance.material;
    }
    public void AddTreesToList(Vector3Int pos, string BIOME)
    {
        Trees.treeData temp;
        temp.pos = pos;
        temp.BIOME = BIOME;
        plantList.Add(temp);   
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
                    int kindOfBlock = World.Instance.GetBlock(xListPos * width + x, y, zListPos * width + z);
                    if (kindOfBlock != BlockData.kindOfBlock["none"])
                    {
                        Vector3Int blockInWorldPosition = new Vector3Int(x + xGlobalPos, y, z + zGlobalPos);
                        Vector3Int blockInList = new Vector3Int(xListPos * width + x, y, zListPos * width + z);
                        DrawBlock(blockInWorldPosition, kindOfBlock, blockInList);
                    }
                    else {
                        int potentialLeafHeight = 9;
                        int maxHeightToSpawnTree = 50;
                        if(y+1 <= maxHeightToSpawnTree)
                        for (int height = y + 1; height < y + potentialLeafHeight; height++)
                        {
                            while(AreLeaves(xListPos * width + x, height, zListPos * width + z))
                            {
                                kindOfBlock = World.Instance.GetBlock(xListPos * width + x, height, zListPos * width + z);
                                Vector3Int blockInWorldPosition = new Vector3Int(x + xGlobalPos, height, z + zGlobalPos);
                                Vector3Int blockInList = new Vector3Int(xListPos * width + x, height, zListPos * width + z);
                                DrawBlock(blockInWorldPosition, kindOfBlock, blockInList);
                                height++;
                            }
                        }
                        break; 
                    }
                }
            }
    }

    private bool AreLeaves(int x, int y, int z)
    {
        return World.Instance.GetBlock(x, y, z) == BlockData.kindOfBlock["leaves"] || World.Instance.GetBlock(x, y, z) == BlockData.kindOfBlock["spruce-leaves"];
    }
    
    
    private bool DrawWall(int i, Vector3Int listPos)
    {

        if (i == 0)
        {
            if (IsNoTransparentBlock(listPos.x, listPos.y, listPos.z - 1))
                return false;
        }
        else if (i == 1)
        {
            if (IsNoTransparentBlock(listPos.x + 1, listPos.y, listPos.z))
                return false;
        }
        else if (i == 2)
        {
            if (IsNoTransparentBlock(listPos.x, listPos.y, listPos.z + 1))
                return false;
        }
        else if (i == 3)
        {
            if (IsNoTransparentBlock(listPos.x - 1, listPos.y, listPos.z))
                return false;
        }
        else if (i == 4)
        {
            if (IsNoTransparentBlock(listPos.x, listPos.y + 1, listPos.z))
                return false;
        }
        else if (i == 5)
        {
            if (IsNoTransparentBlock(listPos.x, listPos.y - 1, listPos.z))
                return false;
        }

        return true;
    }

    private bool IsNoTransparentBlock(int x, int y, int z)
    {
        if(World.Instance.GetBlockListLength(x, y, z))
            return World.Instance.GetBlock(x, y, z) != BlockData.kindOfBlock["none"] && World.Instance.GetBlock(x, y, z) != BlockData.kindOfBlock["leaves"] && World.Instance.GetBlock(x, y, z) != BlockData.kindOfBlock["spruce-leaves"];
        return false;
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
                ColorTheBlock(kindOfBlock, i);
                triangleIndex++;
            }
        }
    }

    private void ColorTheBlock(int kindOfBlock, int i)
    {
        if (i == BlockData.top && kindOfBlock == BlockData.kindOfBlock["greenDirt"])
            colors.Add(new Color(0.509f, 1f, 0.236f, 1f));
        else if (kindOfBlock == BlockData.kindOfBlock["leaves"])
            colors.Add(new Color(0.13f, 0.32f, 0f, 1f));
        else if (kindOfBlock == BlockData.kindOfBlock["spruce-leaves"])
            colors.Add(new Color(0.1f, 0.24f, 0f, 1f));
        else
            colors.Add(new Color(1f, 1f, 1f, 1f));
    }

    private Vector2 GetVectorUV(Vector2 offsetUV, int kindOfBlock, int wall)
    {
        int blockTextureIndex=BlockData.GetBlockID(kindOfBlock, wall);

        int textureWidth = 256;
        int blockTextureWidth = 16;
        float coordX = (float)(blockTextureIndex % blockTextureWidth*blockTextureWidth)/textureWidth;
        float coordY = (float)(240 - blockTextureIndex / blockTextureWidth * blockTextureWidth)/textureWidth;
        Vector2 uv = new Vector2(coordX, coordY) + offsetUV/blockTextureWidth;
        return uv;
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


