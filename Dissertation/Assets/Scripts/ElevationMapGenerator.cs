using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ElevationMapGenerator 
{
    public static float[] GenerateElevationMap(int width, int height, ElevationMapSettings elevationSettings)
    {
        float[] elevationMap = new float[width*height];
        float minElevation = float.MaxValue;
        float maxElevation = float.MinValue;
        float currentNoise = 0;

        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
            //Retrieve perlin value at current coordinate
            currentNoise = NoiseGenerator.GetPerlinValue(x, y, (width/2f), (height/2f), elevationSettings.noiseSettings);

            //Find min and max elevation values
            if(currentNoise > maxElevation) maxElevation = currentNoise;
            else if(currentNoise < minElevation) minElevation = currentNoise;

            elevationMap[y * width + x] = currentNoise;
            }
        }

    
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
            //Map elevation value to 0 - 1 based on min and max
            elevationMap[y * width + x] = Mathf.InverseLerp(minElevation, maxElevation, elevationMap[y * width + x]);
            }
        }

        return elevationMap;
    }
}
