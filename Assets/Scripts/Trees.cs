using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trees
{
    public static float xOffset;
    public static float zOffset;
    private const string FREQUENCY = "frequency";
    private const string AMPLITUDE = "amplitude";
    public static int treeHeight = 4;
    public static int spawnTreeThreshold = 32;

    public static List<treeData> plantList = new List<treeData>();

    public struct treeData
    {
        public Vector3Int pos;
        public string BIOME;
    }

    private static readonly Dictionary<string, Dictionary<string, int>> density = new Dictionary<string, Dictionary<string, int>>()
    {
        {
        "plains", new Dictionary<string, int>
            {
                {FREQUENCY, 39},
                {AMPLITUDE, 28},
            }
        },
        {
        "plains-snow", new Dictionary<string, int>
            {
                {FREQUENCY, 39},
                {AMPLITUDE, 28},
            }
        },
        {
        "jungle", new Dictionary<string, int>
            {
                {FREQUENCY, 30},
                {AMPLITUDE, 29},
            }
        },
        {
        "forest", new Dictionary<string, int>
            {
                {FREQUENCY, 30},
                {AMPLITUDE, 29},
            }
        },
        {
        "taiga", new Dictionary<string, int>
            {
                {FREQUENCY, 30},
                {AMPLITUDE, 29},
            }
        },
        {
        "hill", new Dictionary<string, int>
            {
                {FREQUENCY, 23},
                {AMPLITUDE, 27},
            }
        },
        {
        "hill-snow", new Dictionary<string, int>
            {
                {FREQUENCY, 23},
                {AMPLITUDE, 27},
            }
        }
    };

    public static float CheckToPlantTree(int x, int z, string biome)
    {
        float amplitude;
        float frequency;
        amplitude = density[biome][AMPLITUDE];
        frequency = density[biome][FREQUENCY];
        const int lacunarity = 2;
        const float persistence = 0.5f;
        float value = 0;

        for(int i=0; i < 2; i++)
        {
            value += amplitude * Mathf.PerlinNoise((x + 0.1f) / 45f * frequency, (z + 0.1f) / 45f * frequency);
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        return value;
    }

}
