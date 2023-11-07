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
    private float offsetX;
    private float offsetY;
    private int chunkWidth=6;
    private float scale=10f;
    private int chunkMaxHeight = 15;
    private int numberOfChunksInLine = 5;
    private int playerVisibilityIn1Direction;

    private ArrayList chunkList = new ArrayList();
    private ArrayList blockList = new ArrayList();

    [SerializeField] private Transform playerTransform;

    public void Awake()
    {
        Instance = this;
        playerVisibilityIn1Direction = numberOfChunksInLine;
        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);
    }
    private void Start()
    {
        CreateArrayOfBlocks();
        for (int x = 0; x < numberOfChunksInLine; x++)
        {
            chunkList.Add(new ArrayList());
            for (int z = 0; z < numberOfChunksInLine; z++)
            {
                ((ArrayList)chunkList[x]).Add(new Chunk(chunkWidth * x, chunkWidth * z, transform, this));
            }
        }
    }

    private void Update()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3Int playerPosInChunkCoord = PlayerPosInChunkCoord((int)playerPos.x, (int)playerPos.z);

        UpdateWorld(playerPosInChunkCoord.x, playerPosInChunkCoord.z, AddNewBlocksToArray);
        UpdateWorld(playerPosInChunkCoord.x, playerPosInChunkCoord.z, PrepareChunkListAndAddNewChunk);
        DeleteChunksOutOfPlayerVisibility(playerPosInChunkCoord.x, playerPosInChunkCoord.z);
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
                max = GenerateHeight(blockGlobalXPosition + localOffsetX, blockGlobalZPosition + localOffsetZ);

                SetStartingBlocksInArrayInYDimension(max, blockGlobalXPosition + localOffsetX, blockGlobalZPosition + localOffsetZ);
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
    private void CreateArrayOfBlocks()
    {
        for (int x = 0; x < numberOfChunksInLine * chunkWidth; x++) {
            blockList.Add(new ArrayList());

            for (int z = 0; z < numberOfChunksInLine * chunkWidth; z++)
            {
                ((ArrayList)blockList[x]).Add(new ArrayList());
                int max;
                max = GenerateHeight(x, z);
                SetStartingBlocksInArrayInYDimension(max,x,z);
            }
        }
    }
    private int GenerateHeight(int x, int z)
    {
        int height;

        float coordX = x / 256f * scale + offsetX;
        float coordZ = z / 256f * scale + offsetY;
        float calculatedHeight = Mathf.PerlinNoise(coordX, coordZ);
        height = (int)(calculatedHeight * chunkMaxHeight);

        return height;
    }
    public int GetChunkWidth()
    {
        return chunkWidth;
    }
    public int GetMaxChunkHeight()
    {
        return chunkMaxHeight;
    }

    private void SetStartingBlocksInArrayInYDimension(int heightOfTerrain, int xIndexArrayList, int zIndexArrayList)
    {
        if (((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Count < chunkMaxHeight)
        {
            for (int y = 0; y < heightOfTerrain; y++)
            {
                ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(1);
            }
            for (int y = heightOfTerrain; y < chunkMaxHeight + 1; y++)
            {
                ((ArrayList)((ArrayList)blockList[xIndexArrayList])[zIndexArrayList]).Add(0);
            }
        }
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

    private void DeleteChunksOutOfPlayerVisibility(int xChunkPos, int zChunkPos)
    {
        int playerVibility = playerVisibilityIn1Direction;
        for (int i = -4; i < numberOfChunksInLine; i++)
        {
            if (xChunkPos - playerVibility >= 0)
            {
                if (IsChunk(xChunkPos - playerVibility, zChunkPos + i) && zChunkPos + i >= 0)
                {
                    DestroyChunk(xChunkPos - playerVibility, zChunkPos + i);
                }
            }
            if (zChunkPos - playerVibility >= 0)
            {
                if (IsChunk(xChunkPos + i, zChunkPos - playerVibility) && xChunkPos + i >= 0)
                {
                    DestroyChunk(xChunkPos + i, zChunkPos - playerVibility);
                }
            }
            if (IsChunk(xChunkPos + playerVibility, zChunkPos + i) && zChunkPos + i >= 0)
            {
                DestroyChunk(xChunkPos + playerVibility, zChunkPos + i);
            }
            if (IsChunk(xChunkPos + i, zChunkPos + playerVibility) && xChunkPos + i >= 0)
            {
                DestroyChunk(xChunkPos + i, zChunkPos + playerVibility);
            }
        }
    }

    private void DestroyChunk(int xChunkPos, int zChunkPos)
    {
        Destroy(((Chunk)((ArrayList)chunkList[xChunkPos])[zChunkPos]).gameObject);
        ((ArrayList)chunkList[xChunkPos])[zChunkPos] = null;
    }
    private void UpdateActiveChunks()
    {

    }
}