using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThermalErosionSim : MonoBehaviour
{
    [SerializeField] int iterations = 20;
    [SerializeField] float talusAngle = 0.01f;
    [SerializeField] float sedimentShift = 0.5f;
    [SerializeField] float talusConstant = 4f;
    [SerializeField] bool inverse = false;

    public TMPro.TextMeshProUGUI iterationsText;
    public TMPro.TextMeshProUGUI talusConstantText;
    public TMPro.TextMeshProUGUI sedimentShiftText;


    public int width, height;
    public float terrainHeightMultiplier;
    public void Init(int terrainWidth, int terrainHeight, float terrainHeightMultiplier)
    {
        width = terrainWidth;
        height = terrainHeight;
        this.terrainHeightMultiplier = terrainHeightMultiplier;
        talusAngle = talusConstant / width;
    }

    public void Simulate(float[] elevationMap)
    {
        for(int step = 0; step < iterations; step++)
        {
            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if(!inverse)
                    {
                        CheckNeighbours(x, y, elevationMap);
                    }
                    else
                    {
                       CheckNeighboursInverse(x, y, elevationMap);
                    }
                }
            }
        }
    }

    public void CheckNeighbours(int x, int y, float[] elevationMap)
    {
        float deltaHeightTotal = 0f;
        float deltaHeightMax = 0f;
        int[] lowestNeighbour = new int[2];

        for(int indexX = -1; indexX <= 1; indexX++)
        {
            for (int indexY = -1; indexY <= 1; indexY++)
            {
                int neighbourCoordX = x + indexX;
                int neighbourCoordY = y + indexY;

                if(neighbourCoordX < 0 || neighbourCoordX >= width -1 || neighbourCoordY < 0 || neighbourCoordY >= height - 1 || neighbourCoordX == x || neighbourCoordY == y)
                {
                    continue;
                }

                float deltaHeight = elevationMap[y * width + x] - elevationMap[neighbourCoordY * width + neighbourCoordX];

                if(deltaHeight > talusAngle)
                {
                    deltaHeightTotal += deltaHeight;

                    if(deltaHeight > deltaHeightMax)
                    {
                        deltaHeightMax = deltaHeight;
                        lowestNeighbour[0] = neighbourCoordX;
                        lowestNeighbour[1] = neighbourCoordY;
                    }
                }
            }
        }

        if(deltaHeightMax >= 0 && deltaHeightMax > talusAngle)
        {
            float deltaSediment = deltaHeightMax / 2;
            elevationMap[y * width + x] -= deltaSediment;
            elevationMap[lowestNeighbour[1] * width + lowestNeighbour[0]] += deltaSediment;
            
        }
    }

    public void CheckNeighboursInverse(int x, int y, float[] elevationMap)
    {
        float deltaHeightTotal = 0f;
        float deltaHeightMax = 0f;
        int[] lowestNeighbour = new int[2];

        for(int indexX = -1; indexX <= 1; indexX++)
        {
            for (int indexY = -1; indexY <= 1; indexY++)
            {
                int neighbourCoordX = x + indexX;
                int neighbourCoordY = y + indexY;

                if(neighbourCoordX < 0 || neighbourCoordX > width - 1 || neighbourCoordY < 0 || neighbourCoordY > height - 1 || neighbourCoordX == x || neighbourCoordY == y)
                {
                    continue;
                }

                float deltaHeight = elevationMap[y * width + x] - elevationMap[neighbourCoordY * width + neighbourCoordX];

                if(deltaHeight <= talusAngle)
                {
                    deltaHeightTotal += deltaHeight;

                    if(deltaHeight > deltaHeightMax)
                    {
                        deltaHeightMax = deltaHeight;
                        lowestNeighbour[0] = neighbourCoordX;
                        lowestNeighbour[1] = neighbourCoordY;
                    } 
                }
            }
        }

        if(deltaHeightMax >= 0 && deltaHeightMax <= talusAngle)
        {
            float deltaSediment = deltaHeightMax / 2;
            elevationMap[y * width + x] -= deltaSediment;
            elevationMap[lowestNeighbour[1] * width + lowestNeighbour[0]] += deltaSediment;
            
        }

        
    }

    public void OnIterationsChanged(float newIterations)
    {
        iterations = (int)newIterations;
        iterationsText.text = newIterations.ToString();
    }

    public void OnTalusConstantChanged(float newTalusConstant)
    {
        talusConstant = (int)newTalusConstant;
        talusConstantText.text = newTalusConstant.ToString();
    }

    public void OnSedimentShiftChanged(float newSedimentShift)
    {
        sedimentShift = newSedimentShift;
        sedimentShiftText.text = newSedimentShift.ToString("F2");
    }

    public void OnInverseToggle(bool toggle)
    {
        inverse = toggle;
    }
}
