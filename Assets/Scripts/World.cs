using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using static Trees;
using Random = UnityEngine.Random;


public class World : MonoBehaviour
{
    public delegate void Function(int xChunk, int zChunk);
    public static World Instance { get; private set; }

    public Material material;

    private int chunkWidth=6;
    private int maxGeneratedHeight = 60;
    private int playerVisibilityIn1Direction = 10;
    private int mapChunkLength;
    private int mapBlockLength;
    private Vector3Int previousPlayerChunkPos;

    private ArrayList chunkList = new ArrayList();
    private ArrayList blockList = new ArrayList();
    private List<Chunk> chunksToVisualizeList = new List<Chunk>();
    private List<Chunk> chunksToUpdateList = new List<Chunk>();
    private bool isWorking = false;

    private void Awake()
    {
        Instance = this;
        SetBiomeParametres();
    }
    private void Start()
    {
        int initialWordLenght = 2 * playerVisibilityIn1Direction - 1;
        mapChunkLength = 2 * playerVisibilityIn1Direction - 1;
        mapBlockLength = chunkWidth * mapChunkLength;
        
        MakeInitialChunkList(initialWordLenght);
        CreateArrayOfBlocks(initialWordLenght);
        for (int i = 0; i < mapChunkLength; i++)
            for (int j = 0; j < mapChunkLength; j++)
            {
                Chunk chunk = (Chunk)((ArrayList)chunkList[i])[j];
                PlantTrees(chunk);
            }
        DrawFirstChunks(initialWordLenght);      
    }
    private void Update()
    {
        Vector3 playerPos = Player.Instance.GetPlayerGlobalPos();
        Vector3Int playerPosInChunkCoord = PlayerPosInChunkCoord((int)playerPos.x, (int)playerPos.z);  
        if (!isWorking && playerPosInChunkCoord != previousPlayerChunkPos)
        {
            if ( Math.Abs(playerPosInChunkCoord.x - previousPlayerChunkPos.x) == 1 && Math.Abs(playerPosInChunkCoord.z - previousPlayerChunkPos.z) == 1)
            {
                playerPosInChunkCoord.x = previousPlayerChunkPos.x;                                     //this line is needed to avoid error
            }
            UpdateWorld(playerPosInChunkCoord.x, playerPosInChunkCoord.z);

            StartCoroutine("VisualizeChunks");
            previousPlayerChunkPos = playerPosInChunkCoord;
        }
    }

    private void PlantTrees(Chunk chunk)
    {
          foreach (var tree in chunk.plantList)
          {
              Vector3 pos = ConvertGlobalToBlockListCoordinates(tree.pos.x, 0, tree.pos.z);
              if (pos.x > 1 && pos.x < mapBlockLength - 2 && pos.z > 1 && pos.z < mapBlockLength - 2)
              {
                  AddTreesToBlockList(tree.pos, tree.BIOME);
              }
          }
    }

    private void AddTreesToBlockList(Vector3Int pos, string biome)
    {
        int kindOfWood;
        int kindOfLeaves;
        int treeHeight;
        Vector3Int posConverted = ConvertGlobalToBlockListCoordinates(pos.x,pos.y, pos.z);
        if (biome == Biome.PLAINS_SNOW || biome == Biome.MOUNTAIN_SNOW || biome == Biome.TAIGA || biome == Biome.HILL_SNOW) //dodanie bloku na wysokosci "treeData.pos.y + 5" moze powodowac error ze wzgledu na to ze maksymalna wysokosc w blockList to: maxGeneratedHeight + 1 (do zaktualizowania)
        {
            kindOfWood = BlockData.kindOfBlock["dark-wood"];
            kindOfLeaves = BlockData.kindOfBlock["spruce-leaves"];
            treeHeight = 7;
        }
        else
        {
            kindOfWood = BlockData.kindOfBlock["wood"];
            kindOfLeaves = BlockData.kindOfBlock["leaves"];
            treeHeight = 6;
        }
        for (int y = 1; y < treeHeight; y++)
        {
            ((ArrayList)((ArrayList)blockList[posConverted.x])[posConverted.z])[posConverted.y + y] = kindOfWood;
        }
        if (kindOfLeaves == BlockData.kindOfBlock["leaves"])
        {
            AddLeavesToBlockList(posConverted.x, posConverted.y, posConverted.z, kindOfLeaves);
        }
        else
        {
            AddSpruceLeavesToBlockList(posConverted.x, posConverted.y, posConverted.z, kindOfLeaves);
        }
    }
    private void AddLeavesToBlockList(int x, int y, int z, int kindOfLeaves)
    {

        int heightAboveGround = 4;
        y += heightAboveGround;
        for (int level = 0; level < 2; level++)
            for (int i = x - 2; i < x + 3; i++)
                for (int j = z - 2; j < z + 3; j++)
                {
                    if (GetBlock(i, y + level, j) == BlockData.kindOfBlock["none"])
                        ((ArrayList)((ArrayList)blockList[i])[j])[y + level] = kindOfLeaves;
                }
        for (int i = x - 1; i < x + 2; i++)
            for (int j = z - 1; j < z + 2; j++)
            {
                if (GetBlock(i, y + 2, j) == BlockData.kindOfBlock["none"])
                    ((ArrayList)((ArrayList)blockList[i])[j])[y + 2] = kindOfLeaves;
            }
        if (GetBlock(x - 1, y + 3, z) == BlockData.kindOfBlock["none"])
            ((ArrayList)((ArrayList)blockList[x - 1])[z])[y + 3] = kindOfLeaves;
        if (GetBlock(x + 1, y + 3, z) == BlockData.kindOfBlock["none"])
            ((ArrayList)((ArrayList)blockList[x + 1])[z])[y + 3] = kindOfLeaves;
        if (GetBlock(x, y + 3, z - 1) == BlockData.kindOfBlock["none"])
            ((ArrayList)((ArrayList)blockList[x])[z - 1])[y + 3] = kindOfLeaves;
        if (GetBlock(x, y + 3, z + 1) == BlockData.kindOfBlock["none"])
            ((ArrayList)((ArrayList)blockList[x])[z + 1])[y + 3] = kindOfLeaves;
        if (GetBlock(x, y + 3, z) == BlockData.kindOfBlock["none"])
            ((ArrayList)((ArrayList)blockList[x])[z])[y + 3] = kindOfLeaves;
    }

    private void AddSpruceLeavesToBlockList(int x, int y, int z, int kindOfLeaves)
    {
        int heightAboveGround = 3;
        y += heightAboveGround;

        for(int level = 0;level < 2; level++)
            for(int i = x - 2 + level; i < x + 3 - level; i++)
                for(int j = z - 2 + level; j < z + 3 - level; j++)
                {
                    if(Mathf.Abs(i - x) != 2 - level || Mathf.Abs(j - z) != 2 - level)
                    {
                        if (GetBlock(i, y + level, j) == BlockData.kindOfBlock["none"])
                            ((ArrayList)((ArrayList)blockList[i])[j])[y + level] = kindOfLeaves;
                    }
                }
        int offset = 3;
        y += offset;
        for (int i = x - 1; i < x + 2; i++)
            for (int j = z - 1; j < z + 2; j++)
            {
                if (Mathf.Abs(i - x) != 1 || Mathf.Abs(j - z) != 1)
                {
                    if (GetBlock(i, y, j) == BlockData.kindOfBlock["none"])
                        ((ArrayList)((ArrayList)blockList[i])[j])[y] = kindOfLeaves;
                }
            }

        if (GetBlock(x, y + 1, z) == BlockData.kindOfBlock["none"])
            ((ArrayList)((ArrayList)blockList[x])[z])[y + 1] = kindOfLeaves;
    }
    private void DrawFirstChunks(int initialWordLenght)
    {
        for (int x = 0; x < initialWordLenght; x++)
        {
            chunkList.Add(new ArrayList());
            for (int z = 0; z < initialWordLenght; z++)
            {
                ((Chunk)((ArrayList)chunkList[x])[z]).VisualizeChunk();
            }
        }
    }
    private void MakeInitialChunkList(int initialWordLenght)
    {
        for (int x = 0; x < initialWordLenght; x++)
        {
            chunkList.Add(new ArrayList());
            for (int z = 0; z < initialWordLenght; z++)
            {
                ((ArrayList)chunkList[x]).Add(new Chunk(chunkWidth * x, chunkWidth * z, transform,x,z));
            }
        }
    }

    private void HandleBlockList(int xListPos, int zListPos, int xGlobalPos, int zGlobalPos)
    {
        int max;
        for(int x = 0; x < chunkWidth;x++)
            for(int z = 0; z < chunkWidth; z++)
            {
                max = Biome.Height(x + xGlobalPos, z + zGlobalPos,out string biome);
                SetBlocksInBlockList(max, xListPos * chunkWidth + x, zListPos * chunkWidth + z, biome, xGlobalPos + x, zGlobalPos + z);
            }
    }

    private void CreateArrayOfBlocks(int initialWorldLength)
    {
        for (int x = 0; x < mapBlockLength; x++)
        {
            blockList.Add(new ArrayList());

            for (int z = 0; z < mapBlockLength; z++)
            {
                ((ArrayList)blockList[x]).Add(new ArrayList());
                int max;
                max = Biome.Height(x,z, out string biome);
                SetBlocksInBlockList(max, x, z, biome, x, z);
            }
        }
    }
    public bool IsChunkUsingGlobalPos(int xGlobalPos, int zGlobalPos)
    {
        int xChunkPos = xGlobalPos / chunkWidth;
        int zChunkPos = zGlobalPos / chunkWidth;
        try
        {
            return (Chunk)((ArrayList)chunkList[xChunkPos])[zChunkPos] != null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
    private Vector3Int PlayerPosInChunkCoord(int xPlayerPos, int zPlayerPos)
    {
        int xChunkPos = xPlayerPos / chunkWidth;
        int zChunkPos = zPlayerPos / chunkWidth;
        return new Vector3Int(xChunkPos, 0,zChunkPos);
    }
    public Chunk GetChunkUsingGlobalPos(float xPos, float zPos)
    {
        int xChunkPos = (int)xPos / chunkWidth;
        int zChunkPos = (int)zPos / chunkWidth;
        return (Chunk)((ArrayList)chunkList[xChunkPos])[zChunkPos];
    }
    public int GetBlock(int x,int y,int z)
    {
            return (int)((ArrayList)((ArrayList)blockList[x])[z])[y];
    }
    public int GetTheBlockWithCoords(int xGlobal, int yGlobal, int zGlobal)
    {
        return (int)((ArrayList)((ArrayList)blockList[xGlobal])[zGlobal])[yGlobal];

    }
    public int GetChunkWidth()
    {
        return chunkWidth;
    }
    public int GetMaxChunkHeight()
    {
        return maxGeneratedHeight;
    }
    private void SetBlocksInBlockList(int heightOfTerrain, int xIndexArrayList, int zIndexArrayList, string biome, int xGlobal, int zGlobal)
    {
        int biomeBlock = SetBlockIndexForBiome(biome);

        for (int y = 0; y < heightOfTerrain - 1; y++)
        {
            ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(biomeBlock);
        }

        int maxHeightToSpawnTree = 50;
        if (maxHeightToSpawnTree >= heightOfTerrain && !IsNonForestedAre(biome))
                if (Trees.CheckToPlantTree(xGlobal, zGlobal, biome) > Trees.spawnTreeThreshold)
                {
                    Vector2Int w = ConvertGlobalToChunkListCoordinates(xGlobal, zGlobal);
                    ((Chunk)((ArrayList)chunkList[w.x])[w.y]).AddTreesToList(new Vector3Int(xGlobal, heightOfTerrain - 1, zGlobal), biome);
                }
        if (biome == "plains-snow" || biome == "mountain-snow" || biome == "hill-snow" || biome == "taiga")
            biomeBlock = BlockData.kindOfBlock["dirt-snow"];
        else if (biome == "desert") 
            biomeBlock = BlockData.kindOfBlock["sand"];
        else biomeBlock = BlockData.kindOfBlock["greenDirt"];

        ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(biomeBlock);

        for (int y = heightOfTerrain; y < maxGeneratedHeight + 1; y++)
        {
            ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(BlockData.kindOfBlock["none"]);
        }
    }

    private bool IsNonForestedAre(string biome)
    {
        return biome == Biome.DESERT || biome == Biome.MOUNTAIN_SNOW || biome == Biome.MOUNTAIN;
    }

    private Vector2Int ConvertGlobalToChunkListCoordinates(int x, int z)
    {
        Vector3Int v = GlobalPosOfFirstChunkInList();
        Vector2Int w = new Vector2Int((x - v.x) / chunkWidth, (z - v.z) / chunkWidth);
        return w;
    }
    private Vector3Int ConvertGlobalToBlockListCoordinates(int x, int y, int z)
    {
        Vector3Int v = GlobalPosOfFirstChunkInList();
        Vector3Int w = new Vector3Int(x - v.x, y, z - v.z);
        return w;
    }
    private int SetBlockIndexForBiome(string biome)
    {
        int blockIndex;
        if (biome == "plains")
            blockIndex = BlockData.kindOfBlock["dirt"];
        else if (biome == "desert")
            blockIndex = BlockData.kindOfBlock["sand"];
        else
            blockIndex = BlockData.kindOfBlock["dirt"];

        return blockIndex;
    }
    private void UpdateWorld(int xPlayerChunkPos, int zPlayerChunkPos)
    {
        int coefficient;
        if (Math.Abs(zPlayerChunkPos - previousPlayerChunkPos.z) == 1)
        {
            coefficient = zPlayerChunkPos - previousPlayerChunkPos.z;
            UpdateChunkList("z", coefficient, xPlayerChunkPos, zPlayerChunkPos);
            RefreshBlockList("z", coefficient, xPlayerChunkPos, zPlayerChunkPos);
        }
        else if (Math.Abs(xPlayerChunkPos - previousPlayerChunkPos.x) == 1)
        {
            coefficient = xPlayerChunkPos - previousPlayerChunkPos.x;
            UpdateChunkList("x", coefficient, xPlayerChunkPos, zPlayerChunkPos);
            RefreshBlockList("x", coefficient, xPlayerChunkPos, zPlayerChunkPos);
        }
    }
    private void UpdateChunkList(string COORDINATE, int coefficient, int xPlayerChunkPos, int zPlayerChunkPos)
    {
        int offset = playerVisibilityIn1Direction - 1;
        if (COORDINATE=="x")
        {
            if(coefficient == 1) //usunac ostatni rzad,dodac pierwszy rzad, zaktualizowac drugi rzad
            {
                for (int i = 0; i < mapChunkLength; i++)
                    DestroyChunk(0, i);

                chunkList.RemoveAt(0);
                chunkList.Add(new ArrayList());
                for (int z = -playerVisibilityIn1Direction + 1; z < playerVisibilityIn1Direction; z++)
                {
                    int row = 18;
                    ((ArrayList)chunkList[row]).Add(new Chunk(chunkWidth * (xPlayerChunkPos + offset), chunkWidth * (zPlayerChunkPos + z), transform, row, z + offset));
                    chunksToVisualizeList.Add((Chunk)((ArrayList)chunkList[row])[z + offset]);          //offset - number must be non-negative
                    chunksToUpdateList.Add((Chunk)((ArrayList)chunkList[row - 1])[z + offset]);
                }
                for (int i = 0; i < 18; i++)
                    for (int j = 0; j < 19; j++)
                    {
                        ((Chunk)((ArrayList)chunkList[i])[j]).xListPos--;
                    }
            }
            else
            {
                for (int i = 0; i < mapChunkLength; i++)
                    DestroyChunk(18, i);

                chunkList.RemoveAt(18);
                chunkList.Insert(0, new ArrayList());
                for (int z = -playerVisibilityIn1Direction + 1; z < playerVisibilityIn1Direction; z++)
                {
                    int row = 0;
                    ((ArrayList)chunkList[row]).Add(new Chunk(chunkWidth * (xPlayerChunkPos - offset), chunkWidth * (zPlayerChunkPos + z), transform, row, z + offset));
                    chunksToVisualizeList.Add((Chunk)((ArrayList)chunkList[row])[z + offset]);
                    chunksToUpdateList.Add((Chunk)((ArrayList)chunkList[row + 1])[z + offset]);
                }
                for (int i = 1; i < 19; i++)
                    for (int j = 0; j < 19; j++)
                    {
                        ((Chunk)((ArrayList)chunkList[i])[j]).xListPos++;
                    }
            }
        }
        else
        {
            if (coefficient == 1) //usunac ostatni rzad,dodac pierwszy rzad, zaktualizowac drugi rzad
            {
                int lastColumn = 18;
                int prelastColumn = 17;
                for(int x=0; x < mapChunkLength; x++)
                {
                    DestroyChunk(x, 0);
                    ((ArrayList)chunkList[x]).RemoveAt(0);
                    ((ArrayList)chunkList[x]).Add(new Chunk(chunkWidth * (xPlayerChunkPos + x - offset), chunkWidth * (zPlayerChunkPos + offset), transform, x,18));

                    chunksToVisualizeList.Add((Chunk)((ArrayList)chunkList[x])[lastColumn]);
                    chunksToUpdateList.Add((Chunk)((ArrayList)chunkList[x])[prelastColumn]);
                    
                }
                for (int i = 0; i < 19; i++)
                    for (int j = 0; j < 18; j++)
                    {
                        ((Chunk)((ArrayList)chunkList[i])[j]).zListPos--;
                    }
            }
            else
            {
                int firstColumn = 0;
                int secondColumn = 1;
                for (int x = 0; x < mapChunkLength; x++)
                {
                    DestroyChunk(x, 18);
                    ((ArrayList)chunkList[x]).RemoveAt(18);
                    ((ArrayList)chunkList[x]).Insert(0, new Chunk(chunkWidth * (xPlayerChunkPos + x - offset), chunkWidth * (zPlayerChunkPos - offset), transform, x, 0));
                    chunksToVisualizeList.Add((Chunk)((ArrayList)chunkList[x])[firstColumn]);
                    chunksToUpdateList.Add((Chunk)((ArrayList)chunkList[x])[secondColumn]);
                }
                for (int i = 0; i < 19; i++)
                    for (int j = 1; j < 19; j++)
                    {
                        ((Chunk)((ArrayList)chunkList[i])[j]).zListPos++;
                    }
            }
        }
    }

    private void RefreshBlockList(string COORDINATE, int coefficient, int xChunkPos, int zChunkPos)
    {
        if(COORDINATE == "z")
        {
            for(int x = 0; x < 19 * chunkWidth; x++)
                for(int z = 0; z < chunkWidth; z++)
                {
                    if (coefficient == 1)
                        ((ArrayList)blockList[x]).RemoveAt(0);
                    else
                        ((ArrayList)blockList[x]).RemoveAt(((ArrayList)blockList[x]).Count - 1);
                }
            for (int x = 0; x < 19 * chunkWidth; x++)
                for (int z = 0; z < chunkWidth; z++)
                {
                    if (coefficient == 1)
                        ((ArrayList)blockList[x]).Add(new ArrayList());
                    else
                        ((ArrayList)blockList[x]).Insert(0, new ArrayList());
                }
            
        }
        else
        {
            if (coefficient == 1)
            {
                for(int x = 0; x < chunkWidth; x++)
                {
                    blockList.RemoveAt(0);
                    blockList.Add(new ArrayList());
                }
                for (int x = 18 * chunkWidth; x < 19 * chunkWidth; x++)
                    for (int z = 0; z < 19 * chunkWidth; z++)
                    {
                        ((ArrayList)blockList[x]).Add(new ArrayList());
                    }
            }
            else
            {
                for (int x = 0; x < chunkWidth; x++)
                {
                    blockList.RemoveAt(blockList.Count - 1);
                    blockList.Insert(0, new ArrayList());
                }
                for(int x = 0;x < chunkWidth; x++)
                    for(int z = 0; z < 19 * chunkWidth; z++)
                    {
                        ((ArrayList)blockList[x]).Add(new ArrayList());
                    }

            }
        }
    }

    private IEnumerator VisualizeChunks()
    {
            while (chunksToVisualizeList.Count > 0)
            {
                isWorking = true;
                HandleBlockList(chunksToVisualizeList[0].xListPos, chunksToVisualizeList[0].zListPos, chunksToVisualizeList[0].xGlobalPos, chunksToVisualizeList[0].zGlobalPos);
                HandleBlockList(chunksToVisualizeList[1].xListPos, chunksToVisualizeList[1].zListPos, chunksToVisualizeList[1].xGlobalPos, chunksToVisualizeList[1].zGlobalPos);
                PlantTrees(chunksToVisualizeList[0]);
                PlantTrees(chunksToUpdateList[0]);
                while (chunksToVisualizeList.Count > 2)
                {
                    HandleBlockList(chunksToVisualizeList[2].xListPos, chunksToVisualizeList[2].zListPos, chunksToVisualizeList[2].xGlobalPos, chunksToVisualizeList[2].zGlobalPos);
                    PlantTrees(chunksToVisualizeList[1]);
                    PlantTrees(chunksToUpdateList[1]);
                    chunksToVisualizeList[0].VisualizeChunk();
                    chunksToVisualizeList.RemoveAt(0);
                    chunksToUpdateList[0].VisualizeChunk();
                    chunksToUpdateList.RemoveAt(0);
                    yield return null;
                }
                PlantTrees(chunksToVisualizeList[1]);
                PlantTrees(chunksToUpdateList[1]);
                chunksToVisualizeList[0].VisualizeChunk();
                chunksToVisualizeList.RemoveAt(0);
                chunksToUpdateList[0].VisualizeChunk();
                chunksToUpdateList.RemoveAt(0);
                yield return null;

                chunksToVisualizeList[0].VisualizeChunk();
                chunksToVisualizeList.RemoveAt(0);
                chunksToUpdateList[0].VisualizeChunk();
                chunksToUpdateList.RemoveAt(0);
                isWorking = false;
                yield break;
            }

    }

    private void DestroyChunk(int xChunkListPos, int zChunkListPos)
    {
        Destroy(((Chunk)((ArrayList)chunkList[xChunkListPos])[zChunkListPos]).gameObject);
    }

    private void SetBiomeParametres()
    {
        Biome.offsetXhumadity = Random.Range(0f, 99999f);
        Biome.offsetZhumadity = Random.Range(0f, 99999f);
        Biome.offsetXtemperature = Random.Range(0f, 99999f);
        Biome.offsetZtemperature = Random.Range(0f, 99999f);

        Trees.xOffset = Random.Range(0f, 99999f);
        Trees.zOffset = Random.Range(0f, 99999f);
    }
    public Vector3Int GlobalPosOfFirstChunkInList()
    {
        int x = ((Chunk)((ArrayList)chunkList[0])[0]).xGlobalPos;
        int z = ((Chunk)((ArrayList)chunkList[0])[0]).zGlobalPos;
        Vector3Int vector = new Vector3Int(x, 0, z);
        return vector;
    }
}