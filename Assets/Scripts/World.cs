using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
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
        CreateArrayOfBlocks(initialWordLenght);
        MakeInitialChunkList(initialWordLenght);
        MakeFirstChunks(initialWordLenght);
        
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
            Test(playerPosInChunkCoord.x, playerPosInChunkCoord.z);

            StartCoroutine("VisualizeChunks");
            previousPlayerChunkPos = playerPosInChunkCoord;
        }
    }
    private void MakeFirstChunks(int initialWordLenght)
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

    private void NewFunction(int xListPos, int zListPos, int xGlobalPos, int zGlobalPos)
    {
        int max;
        for(int x = 0; x < chunkWidth;x++)
            for(int z = 0; z < chunkWidth; z++)
            {
                max = Biome.Height(x + xGlobalPos, z + zGlobalPos,out string biome);
                SetBlocksInBlockList(max, xListPos * chunkWidth + x, zListPos * chunkWidth + z, biome);
            }
    }

    private void CreateArrayOfBlocks(int initialWorldLength)
    {
        for (int x = 0; x < initialWorldLength * chunkWidth; x++)
        {
            blockList.Add(new ArrayList());

            for (int z = 0; z < initialWorldLength * chunkWidth; z++)
            {
                ((ArrayList)blockList[x]).Add(new ArrayList());
                int max;
                max = Biome.Height(x,z, out string biome);
                SetBlocksInBlockList(max, x, z, biome);
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
    private void SetBlocksInBlockList(int heightOfTerrain, int xIndexArrayList, int zIndexArrayList, string biome)
    {
            int biomeBlock = SetBlockIndexForBiome(biome);

            for (int y = 0; y < heightOfTerrain - 1; y++)
            {
                ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(biomeBlock);
            }

            if(biome == "plains-snow" || biome == "mountain-snow" || biome == "hill-snow" || biome == "taiga")
                biomeBlock = BlockData.kindOfBlock["dirt-snow"];
            else biomeBlock = BlockData.kindOfBlock["greenDirt"];

        ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(biomeBlock);

            for (int y = heightOfTerrain; y < maxGeneratedHeight + 1; y++)
            {
                ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(BlockData.kindOfBlock["none"]);
            }
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
    private void Test(int xPlayerChunkPos, int zPlayerChunkPos)
    {
        int coefficient;
        if (Math.Abs(zPlayerChunkPos - previousPlayerChunkPos.z) == 1)
        {
            coefficient = zPlayerChunkPos - previousPlayerChunkPos.z;
            UpdateChunkList("z", coefficient, xPlayerChunkPos, zPlayerChunkPos);
            UpdateBlockList("z", coefficient, xPlayerChunkPos, zPlayerChunkPos);
        }
        else if (Math.Abs(xPlayerChunkPos - previousPlayerChunkPos.x) == 1)
        {
            coefficient = xPlayerChunkPos - previousPlayerChunkPos.x;
            UpdateChunkList("x", coefficient, xPlayerChunkPos, zPlayerChunkPos);
            UpdateBlockList("x", coefficient, xPlayerChunkPos, zPlayerChunkPos);
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

    private void UpdateBlockList(string COORDINATE, int coefficient, int xChunkPos, int zChunkPos)
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
                //AddNewBlocksToArray(chunksToVisualizeList[0].xGlobalPos / 6, chunksToVisualizeList[0].zGlobalPos / 6);
                NewFunction(chunksToVisualizeList[0].xListPos, chunksToVisualizeList[0].zListPos, chunksToVisualizeList[0].xGlobalPos, chunksToVisualizeList[0].zGlobalPos);
                while (chunksToVisualizeList.Count > 1)
                {
                    //AddNewBlocksToArray(chunksToVisualizeList[1].xGlobalPos / 6, chunksToVisualizeList[1].zGlobalPos / 6);
                    NewFunction(chunksToVisualizeList[1].xListPos, chunksToVisualizeList[1].zListPos, chunksToVisualizeList[1].xGlobalPos, chunksToVisualizeList[1].zGlobalPos);
                    chunksToVisualizeList[0].VisualizeChunk();
                    chunksToVisualizeList.RemoveAt(0);
                    chunksToUpdateList[0].VisualizeChunk();
                    chunksToUpdateList.RemoveAt(0);
                    yield return null;
                }
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
    }
    public Vector3Int GlobalPosOfFirstChunkInList()
    {
        int x = ((Chunk)((ArrayList)chunkList[0])[0]).xGlobalPos;
        int z = ((Chunk)((ArrayList)chunkList[0])[0]).zGlobalPos;
        Vector3Int vector = new Vector3Int(x, 0, z);
        return vector;
    }
}