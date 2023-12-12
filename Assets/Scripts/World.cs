using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class World : MonoBehaviour
{
    public delegate void Function(int xChunk, int zChunk);
    public static World Instance { get; private set; }

    public Material material;

    private int chunkWidth=6;
    private int chunkMaxHeight = 60;
    private int numberOfChunksInLine = 5;
    private int playerVisibilityIn1Direction;

    private ArrayList chunkList = new ArrayList();
    private ArrayList blockList = new ArrayList();
    private List<Chunk> chunksToVisualizeList = new List<Chunk>();

    [SerializeField] private Transform playerTransform;

    private void Awake()
    {
        Instance = this;
        playerVisibilityIn1Direction = numberOfChunksInLine;

        SetBiomeParametres();
    }
    private void Start()
    {
        int initialWordLenght = 2 * numberOfChunksInLine - 1;
        CreateArrayOfBlocks(initialWordLenght);
        MakeInitialBlockList(initialWordLenght);
        MakeFirstChunks(initialWordLenght);
    }
    private void Update()
    {
        
        Vector3 playerPos = playerTransform.position;
        Vector3Int playerPosInChunkCoord = PlayerPosInChunkCoord((int)playerPos.x, (int)playerPos.z);

        UpdateWorld(playerPosInChunkCoord.x, playerPosInChunkCoord.z, AddNewBlocksToArray);
        UpdateWorld(playerPosInChunkCoord.x, playerPosInChunkCoord.z, PrepareChunkListAndAddNewChunk);
        UpdateChunks(playerPosInChunkCoord.x, playerPosInChunkCoord.z);
    }

    private void MakeFirstChunks(int initialWordLenght)
    {
        for (int x = 0; x < initialWordLenght; x++)
        {
            chunkList.Add(new ArrayList());
            for (int z = 0; z < initialWordLenght; z++)
            {
                ((Chunk)((ArrayList)chunkList[x])[z]).VisualizeChunk(this);
            }
        }
    }
    private void MakeInitialBlockList(int initialWordLenght)
    {
        for (int x = 0; x < initialWordLenght; x++)
        {
            chunkList.Add(new ArrayList());
            for (int z = 0; z < initialWordLenght; z++)
            {
                ((ArrayList)chunkList[x]).Add(new Chunk(chunkWidth * x, chunkWidth * z, transform, this));
            }
        }
    }
    private void AddNewBlocksToArray(int xChunkPos, int zChunkPos)
    {
        int blockGlobalXPosition = xChunkPos * chunkWidth;
        int blockGlobalZPosition = zChunkPos * chunkWidth;
        for (int localOffsetX = 0; localOffsetX < chunkWidth; localOffsetX++)
        {
            MakeNewListInXDimensionIfNotExist(blockGlobalXPosition + localOffsetX);

            for (int localOffsetZ = 0; localOffsetZ < chunkWidth; localOffsetZ++)
            {
                MakeNewListInZDimensionIfNotExist(blockGlobalXPosition + localOffsetX, blockGlobalZPosition + localOffsetZ);
                int max;
                //max = GenerateHeight(blockGlobalXPosition + localOffsetX, blockGlobalZPosition + localOffsetZ);
                max = Biome.Height(blockGlobalXPosition + localOffsetX, blockGlobalZPosition + localOffsetZ);

                SetBlocksInArrayInYDimension(max, blockGlobalXPosition + localOffsetX, blockGlobalZPosition + localOffsetZ);
            }
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
                //max = GenerateHeight(x, z);
                max = Biome.Height(x,z);
                //Debug.Log(max);
                SetBlocksInArrayInYDimension(max, x, z);
            }
        }
    }


    private bool IsChunk(int xChunkPos, int zChunkPos)
    {
        try
        {
            return (Chunk)((ArrayList)chunkList[xChunkPos])[zChunkPos] != null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false; 
        }
    }
    public bool IsChunkUsingGlobalPos(int xGlobalPos, int zGlobalPos)
    {
        int xChunkPos = xGlobalPos / 6;
        int zChunkPos = zGlobalPos / 6;
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
        return chunkMaxHeight;
    }
    private void SetBlocksInArrayInYDimension(int heightOfTerrain, int xIndexArrayList, int zIndexArrayList)
    {
        if (AreSetBlocks(xIndexArrayList, zIndexArrayList))
        {
            for (int y = 0; y < heightOfTerrain; y++)
            {
                ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(BlockData.kindOfBlock["stone"]);
            }
            for (int y = heightOfTerrain; y < chunkMaxHeight + 1; y++)
            {
                ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(BlockData.kindOfBlock["none"]);
            }
        }
    }

    private bool AreSetBlocks(int xIndexArrayList, int zIndexArrayList)
    {
        return ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Count < chunkMaxHeight;
    }

    private void MakeNewListInZDimensionIfNotExist(int xIndex, int zIndex)
    {
        if (((ArrayList)blockList[xIndex]).Count < zIndex + 1)
        {
            int amountOfListsToAdd = zIndex + 1 - ((ArrayList)blockList[xIndex]).Count;
            for (int i = 0; i < amountOfListsToAdd; i++)
            {
                ((ArrayList)blockList[xIndex]).Add(null);
            }

        }
        if (((ArrayList)blockList[xIndex])[zIndex] == null)
            ((ArrayList)blockList[xIndex])[zIndex] = new ArrayList();
    }

    private void MakeNewListInXDimensionIfNotExist(int xIndex)
    {
        if (blockList.Count < xIndex + 1)
        {
            int amountOfListsToAdd = xIndex + 1 - blockList.Count;
            for (int i = 0; i < amountOfListsToAdd; i++)
                blockList.Add(null);
        }
        if (blockList[xIndex] == null)
            blockList[xIndex] = new ArrayList();
    }

    private void PrepareChunkListAndAddNewChunk(int xChunkPos, int zChunkPos)
    {
        if (chunkList.Count < xChunkPos + 1)
        {
            int amountOfListsToAdd = xChunkPos + 1 - chunkList.Count;
            for (int i = 0; i < amountOfListsToAdd; i++)
                chunkList.Add(null);
        }
        if (chunkList[xChunkPos] == null)
            chunkList[xChunkPos] = new ArrayList();

        if (((ArrayList)chunkList[xChunkPos]).Count < zChunkPos + 1)
        {
            int amountOfChunksToAdd = zChunkPos + 1 - ((ArrayList)chunkList[xChunkPos]).Count;
            for (int i = 0; i < amountOfChunksToAdd; i++)
                ((ArrayList)chunkList[xChunkPos]).Add(null);
        }
        ((ArrayList)chunkList[xChunkPos])[zChunkPos] = new Chunk(xChunkPos * chunkWidth, zChunkPos * chunkWidth, transform, this);
        Chunk chunk = (Chunk)((ArrayList)chunkList[xChunkPos])[zChunkPos];
        chunksToVisualizeList.Add(chunk);
    }

    private void UpdateWorld(int xChunkPos, int zChunkPos, Function function)
    {
        for (int i = -4; i < numberOfChunksInLine; i++)
        {
            if (xChunkPos - 4 >= 0)
            {
                if (!IsChunk(xChunkPos - 4, zChunkPos + i) && zChunkPos + i >= 0)
                {
                    function(xChunkPos - 4, zChunkPos + i);
                }
            }
            if (zChunkPos - 4 >= 0)
            {
                if (!IsChunk(xChunkPos + i, zChunkPos - 4) && xChunkPos + i >= 0)
                {
                    function(xChunkPos + i, zChunkPos - 4);
                }
            }
            if (!IsChunk(xChunkPos + 4, zChunkPos + i) && zChunkPos + i >= 0)
            {
                function(xChunkPos + 4, zChunkPos + i);
            }
            if (!IsChunk(xChunkPos + i, zChunkPos + 4) && xChunkPos + i >= 0)
            {
                function(xChunkPos + i, zChunkPos + 4);
            }
        }
    }

    private void VisualizeNewChunks()
    {
        if (chunksToVisualizeList.Count > 0)
        {
            foreach (Chunk chunk in chunksToVisualizeList)
            {
                chunk.VisualizeChunk(this);
            }
            chunksToVisualizeList.Clear();
        }
    }
    private void UpdateChunks(int xChunkPos, int zChunkPos)
    {
        VisualizeNewChunks();
        int playerVisibility = playerVisibilityIn1Direction;
        for (int i = -4; i < numberOfChunksInLine; i++)
        {
            if (xChunkPos - playerVisibility >= 0) // if move right
            {
                if (IsChunk(xChunkPos - playerVisibility, zChunkPos + i) && zChunkPos + i >= 0)
                {

                    DestroyChunk(xChunkPos - playerVisibility, zChunkPos + i); //after desrtoying old chunks changeVisual active chunks

                    int lastVisibleBlockXCoord = xChunkPos - playerVisibility + 1;
                    ((Chunk)((ArrayList)chunkList[lastVisibleBlockXCoord])[zChunkPos + i]).VisualizeChunk(this);
                    int secondRawAfterAddNewChunks = xChunkPos + playerVisibility - 2;
                    ((Chunk)((ArrayList)chunkList[secondRawAfterAddNewChunks])[zChunkPos + i]).VisualizeChunk(this);

                }
            }

            if (zChunkPos - playerVisibility >= 0) 
            {
                if (IsChunk(xChunkPos + i, zChunkPos - playerVisibility) && xChunkPos + i >= 0) // if move forward
                {
                    DestroyChunk(xChunkPos + i, zChunkPos - playerVisibility);
                    int lastVisibleBlockZCoord = zChunkPos - playerVisibility + 1;
                    ((Chunk)((ArrayList)chunkList[xChunkPos + i])[lastVisibleBlockZCoord]).VisualizeChunk(this);
                    int secondRawAfterAddNewChunks = zChunkPos + playerVisibility - 2;
                    ((Chunk)((ArrayList)chunkList[xChunkPos + i])[secondRawAfterAddNewChunks]).VisualizeChunk(this);
                }
            }

            if (IsChunk(xChunkPos + playerVisibility, zChunkPos + i) && zChunkPos + i >= 0) // if move left
            {
                DestroyChunk(xChunkPos + playerVisibility, zChunkPos + i);
                int lastVisibleBlockXCoord = xChunkPos + playerVisibility - 1;
                ((Chunk)((ArrayList)chunkList[lastVisibleBlockXCoord])[zChunkPos + i]).VisualizeChunk(this);
                if (xChunkPos - playerVisibility + 2 >= 0)
                {
                    int secondRawAfterAddNewChunks = xChunkPos - playerVisibility + 2;
                    ((Chunk)((ArrayList)chunkList[secondRawAfterAddNewChunks])[zChunkPos + i]).VisualizeChunk(this);
                }
                    
            }

            if (IsChunk(xChunkPos + i, zChunkPos + playerVisibility) && xChunkPos + i >= 0) // if move down
            {
                DestroyChunk(xChunkPos + i, zChunkPos + playerVisibility);
                int lastVisibleBlockZCoord = zChunkPos + playerVisibility - 1;
                ((Chunk)((ArrayList)chunkList[xChunkPos + i])[lastVisibleBlockZCoord]).VisualizeChunk(this);
                if (zChunkPos - playerVisibility + 2 >= 0)
                {
                    int secondRawAfterAddNewChunks = zChunkPos - playerVisibility + 2;
                    ((Chunk)((ArrayList)chunkList[xChunkPos + i])[secondRawAfterAddNewChunks]).VisualizeChunk(this);
                }
            }
        }
    }

    private void DestroyChunk(int xChunkPos, int zChunkPos)
    {
        Destroy(((Chunk)((ArrayList)chunkList[xChunkPos])[zChunkPos]).gameObject);
        ((ArrayList)chunkList[xChunkPos])[zChunkPos] = null;
    }

    private void SetBiomeParametres()
    {
        Biome.offsetXhumadity = Random.Range(0f, 99999f);
        Biome.offsetZhumadity = Random.Range(0f, 99999f);
        Biome.offsetXtemperature = Random.Range(0f, 99999f);
        Biome.offsetZtemperature = Random.Range(0f, 99999f);
    }
}