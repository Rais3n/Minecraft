using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockData
{

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
    };

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
