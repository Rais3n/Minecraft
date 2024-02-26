using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockData
{
    public static readonly int top = 4;
    public static readonly int down = 5;

    public static readonly Dictionary<string, int> kindOfBlock = new Dictionary<string, int>()
    {
        {"none",0 },
        {"greenDirt",1},
        {"dirt",2},
        {"stone",3},
        {"cobbelstone",4},
        {"sand",5},
        {"dirt-snow", 6},
        {"leaves", 7},
        {"wood", 8},
        {"dark-wood", 9},
        { "spruce-leaves",10}
    };

    public static int GetBlockID(int kindOfWall, int wall)
    {
        int blockTextureIndex;
        if (kindOfWall == kindOfBlock["stone"])
            blockTextureIndex = 1;
        else if (kindOfWall == kindOfBlock["greenDirt"])
        {
            if (wall == top)
                blockTextureIndex = 40;
            else if (wall == down)
                blockTextureIndex = 2;
            else blockTextureIndex = 3;
        }
        else if (kindOfWall == kindOfBlock["dirt"])
            blockTextureIndex = 2;
        else if (kindOfWall == kindOfBlock["sand"])
            blockTextureIndex = 18;
        else if (kindOfWall == kindOfBlock["dirt-snow"])
        {
            if (wall == top)
                blockTextureIndex = 66;
            else if (wall == down)
                blockTextureIndex = 2;
            else blockTextureIndex = 68;
        }
        else if (kindOfWall == kindOfBlock["wood"])
        {
            if (wall == top || wall == down)
                blockTextureIndex = 21;
            else blockTextureIndex = 20;
        }
        else if(kindOfWall == kindOfBlock["dark-wood"])
        {
            if (wall == top || wall == down)
                blockTextureIndex = 21;
            else blockTextureIndex = 116;
        }
        else if(kindOfWall == kindOfBlock["spruce-leaves"])
        {
            blockTextureIndex = 132;
        }
        else if (kindOfWall == kindOfBlock["leaves"])
        {
            blockTextureIndex = 52;
        }
        else if (kindOfWall == kindOfBlock["cobbelstone"])
            blockTextureIndex = 16;
        else
            blockTextureIndex = 17;

        return blockTextureIndex;

    }

    public static readonly Vector3[] vertex = new Vector3[8]
    {
        new Vector3(0,0,0),
        new Vector3(1,0,0), 
        new Vector3(1,1,0),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(1,0,1),
        new Vector3(1,1,1),
        new Vector3(0,1,1)
    };

    public static readonly int[,] triangles = new int[6, 6]
    {
        {0,3,1,1,3,2}, //back
        {1,2,5,5,2,6}, //right
        {5,6,4,4,6,7}, //front
        {4,7,0,0,7,3}, //left
        {3,7,2,2,7,6}, //top
        {1,5,0,0,5,4}, //bot
    };

    public static readonly Vector2[] uv = new Vector2[6]
    {
        
        new Vector2(0.0f,0.0f),
        new Vector2(0.0f,1.0f),
        new Vector2(1.0f,0.0f),
        new Vector2(1.0f,0.0f),
        new Vector2(0.0f,1.0f),
        new Vector2(1.0f,1.0f),
    };
}
