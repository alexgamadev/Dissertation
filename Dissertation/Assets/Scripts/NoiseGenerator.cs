using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    public static float GetPerlinValue(int x, int y, float halfWidth, float halfHeight, NoiseSettings settings)
    {
        System.Random randomGen = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        //Create offsets for each octave, generated from seeded random generator
        for (int i = 0; i < settings.octaves; i++)
        {
            octaveOffsets[i].x = randomGen.Next(-100000, 100000);
            octaveOffsets[i].y = randomGen.Next(-100000, 100000);
        }

        //Frequency decreases every octave so each successive octave is scaled more 
        float frequency = 1f;
        //Amplitude increases every octave so that each successive octave makes less difference to the final elevation
        float amplitude = 1f;

        float finalNoise = 0;

        //Iterate through octaves, creating perlin from sample values adjusted by octave offsets (Seed)
        for (int i = 0; i < settings.octaves; i++)
        {
                    float sampleX = (x) / settings.scale * frequency;
                    float sampleY = (y) / settings.scale * frequency;

                    float noise = Mathf.PerlinNoise(sampleX, sampleY);

                    finalNoise += noise * amplitude;

                    amplitude *= 0.5f;
                    frequency *= 2f;
        }

        return finalNoise;
    }
}
