using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Biome
{
    private const string AMPLITUDE = "amplitude";
    private const string FREQUENCY = "frequency";
    private const string OCTAVES = "octaves";

    public static float offsetXtemperature;
    public static float offsetZtemperature;
    public static float offsetXhumadity;
    public static float offsetZhumadity;

    public static string DESERT = "desert";
    public static string PLAINS = "plains";
    public static string PLAINS_SNOW = "plains-snow";
    public static string JUNGLE = "jungle";
    public static string FOREST = "forest";
    public static string TAIGA = "taiga";
    public static string HILL = "hill";
    public static string HILL_SNOW = "hill-snow";
    public static string MOUNTAIN = "mountain";
    public static string MOUNTAIN_SNOW = "mountain-snow";

    private static readonly Dictionary<string, Dictionary<string,int>> landform = new Dictionary<string, Dictionary<string, int>>()
    {
        {
        "low", new Dictionary<string, int>
            {
                {AMPLITUDE, 4},
                {FREQUENCY, 32},
                {OCTAVES, 2}
            }
        },
        {
        "mid", new Dictionary<string, int>
            {
                {AMPLITUDE, 6},
                {FREQUENCY, 10},
                {OCTAVES, 2}
            }
        },
        {
        "mid-high", new Dictionary<string, int> 
            {
                {AMPLITUDE, 24},
                {FREQUENCY, 8},
                {OCTAVES, 3}
            }
        },
        {
        "high", new Dictionary<string, int>
            {
                {AMPLITUDE, 48},
                {FREQUENCY, 4},
                {OCTAVES, 4}
            }
        }
    };

    private static readonly Dictionary<string, string> heightOfBiome = new Dictionary<string, string>()
    {
        {DESERT, "low"},
        {PLAINS, "low"},
        {PLAINS_SNOW, "low"},
        {JUNGLE, "mid"},
        {FOREST, "mid"},
        {TAIGA, "mid"},
        {HILL, "mid-high"},
        {HILL_SNOW, "mid-high"},
        {MOUNTAIN, "high"},
        {MOUNTAIN_SNOW, "high"}
    };

    private static string GetBiome(float moisture, float temperature)
    {
        if (Between(moisture, 0f, 0.3f) && Between(temperature, 0.7f, 1f))
            return "desert";
        else if (Between(moisture, 0f, 0.3f) && Between(temperature, 0.3f, 0.7f))
            return "plains";
        else if (Between(moisture, 0f, 0.3f) && Between(temperature, 0f, 0.3f))
            return "plains-snow";
        else if (Between(moisture, 0.3f, 0.5f) && Between(temperature, 0.7f, 1f))
            return "jungle";
        else if (Between(moisture, 0.3f, 0.5f) && Between(temperature, 0.3f, 0.7f))
            return "forest";
        else if (Between(moisture, 0.3f, 0.5f) && Between(temperature, 0f, 0.3f))
            return "taiga";
        else if (Between(moisture, 0.5f, 0.7f) && Between(temperature, 0.3f, 1f))
            return "hill";
        else if (Between(moisture, 0.5f, 0.7f) && Between(temperature, 0f, 0.3f))
            return "hill-snow";
        else if (Between(moisture, 0.7f, 8f) && Between(temperature, 0.3f, 1f))
            return "mountain";
        else if (Between(moisture, 0.7f, 8f) && Between(temperature, 0f, 0.3f))
            return "mountain-snow";

        else return "mountain"; // change to water later,
    }

    public static int Height(float x, float z, out string biomeInBlock)
    {
        float temperature = GenerateTemperatureParameter(x, z);
        float moisture = GenerateMoistureParameter(x, z);
        biomeInBlock = GetBiome(moisture, temperature);
        const float lacunarity = 2f;
        const float persistence = 0.5f;
        float value;
        float amplitude;
        float frequency;
        int octaveCount;
        float[] weight  = new float[3];
        string[] heights = new string[3];

        x /= 256f;
        z /= 256f;
        SetWeight(moisture, weight);
        SetBiomes(biomeInBlock, heights);
        string height;
        float sum = 0;
        for (int i = 0; i <= 2; i++)
        {
            if (i == 2 && IsHighOrLow(biomeInBlock))
                break;
            value = 0;
            height = heights[i];

            amplitude = landform[height][AMPLITUDE];
            frequency = landform[height][FREQUENCY];
            octaveCount = landform[height][OCTAVES];

            for (int j = octaveCount; j > 0; j--)
            {
                value += amplitude * Mathf.PerlinNoise(x * frequency, z * frequency);
                amplitude *= persistence;
                frequency *= lacunarity;
            }
            sum += weight[i] * value;
        }

        return (int)sum;
    }

    private static bool IsHighOrLow(string biome)
    {
        string high = GetBiomeHeight(biome);
        return high == "low" || high == "high";
    }
    private static void SetWeight(float moisture, float[] weight)
    {
        if(Between(moisture, 0f, 0.3f))
        {
            weight[0] = 1f - moisture/0.3f/2f;
            weight[1] = 1f - weight[0];
        }
        else if(Between(moisture, 0.3f, 0.5f))
        {
            weight[0] = (1f - (moisture - 0.3f)/0.2f)/2f;
            weight[1] = 0.5f - weight[0];
            weight[2] = 0.5f;
        }
        else if(Between(moisture, 0.5f, 0.7f))
        {
            weight[0] = (1f - (moisture - 0.5f)/0.2f)/2f;
            weight[1] = 0.5f - weight[0];
            weight[2] = 0.5f;
        }
        else if(Between(moisture, 0.7f, 1f))
        {
            weight[0] = (1f - (moisture - 0.7f)/0.3f)/2f;
            weight[1] = 1f - weight[0];
        }
    }

    private static void SetBiomes(string playerInBiome, string[] heights)
    {
        string height = GetBiomeHeight(playerInBiome);

        if(height == "low")
        {
            heights[0] = "low";
            heights[1] = "mid";
        }
        else if(height == "mid")
        {
            heights[0] = "low";
            heights[1] = "mid-high";
            heights[2] = "mid";

        }
        else if(height == "mid-high")
        {
            heights[0] = "mid";
            heights[1] = "high";
            heights[2] = "mid-high";
        }
        else if(height == "high")
        {
            heights[0] = "mid-high";
            heights[1] = "high";
        }
    }

    private static bool Between(float checkedValue, float lowerLimit, float upperLimit)
    {
        return checkedValue >= lowerLimit && checkedValue <= upperLimit;
    }

    private static float GenerateTemperatureParameter(float x, float z)
    {
        float scale = 2;
        float temperature;
        float coordX = x / 457f * scale + offsetXtemperature;
        float coordZ = z / 457f * scale + offsetZtemperature;

        temperature = Mathf.PerlinNoise(coordX, coordZ);
        return temperature;
    }

    private static float GenerateMoistureParameter(float x, float z)
    {
        float scale = 1.5f;
        float moisture;
        float coordX = x / 567f * scale + offsetXhumadity;
        float coordZ = z / 567f * scale + offsetZhumadity;

        moisture = Mathf.PerlinNoise(coordX, coordZ);
        return moisture;
    }

    private static string GetBiomeHeight(string biome)
    {
        string height = null;
        foreach (var environment in heightOfBiome)
        {
            if (environment.Key == biome)
            {
                height = environment.Value;
                break;
            }
        }
        return height;
    }
}
