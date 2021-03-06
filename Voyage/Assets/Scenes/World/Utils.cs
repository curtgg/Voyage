﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils {

    public static int maxSkyHeight = 256;
    public static int maxFloorHeight = 16;
    public static int perlinOffset = 24000; //Used to ensure we do not calculate perlin with a negative value, as thi
    static float smooth = 0.01f;
    static int octaves = 4;
    static float persistence = 0.5f;

    public static int GenerateFloorHeight(float x, float y)
    {
        float height = Map(0, maxFloorHeight, 0, 1, fBM(x * smooth, y * smooth, octaves, persistence));
        return (int)height;
    }

    public static int GenerateIslandHeight(float x, float y)
    {
        float pers = 2f;
        float height = Map(0, maxSkyHeight, 0, 1, fBM2(x * smooth, y * smooth, octaves, pers));
        return (int)height;
    }

    static float Map(float newmin, float newmax, float origmin, float origmax, float value)
    {
        return Mathf.Lerp(newmin, newmax, Mathf.InverseLerp(origmin, origmax, value));
    }

    static float fBM(float x, float z, int oct, float pers)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        for (int i = 0; i < oct; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= pers;
            frequency *= 2;
        }

        return total / maxValue;
    }

    //Higher peaks, low freq for the islands
    static float fBM2(float x, float z, int oct, float pers)
    {
        float total = 0;
        float frequency = 0.2f;
        float amplitude = 128;
        float maxValue = 0;
        for (int i = 0; i < oct; i++)
        {
            total += Mathf.PerlinNoise((x+perlinOffset) * frequency, (z+perlinOffset) * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= pers;
            frequency *= 2;
        }

        return total / maxValue;
    }


}
