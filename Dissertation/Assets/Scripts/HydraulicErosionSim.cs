using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HydraulicErosionSim : MonoBehaviour
{
    public int droplets = 1000;
    public int dropletLifetime = 30;
    public TMPro.TextMeshProUGUI dropletsText;
    public TMPro.TextMeshProUGUI lifetimeText;
    public TMPro.TextMeshProUGUI inertiaText;
    public TMPro.TextMeshProUGUI capacityText;
    public TMPro.TextMeshProUGUI minSlopeText;
    public TMPro.TextMeshProUGUI depositionText;
    public TMPro.TextMeshProUGUI erosionText;
    public TMPro.TextMeshProUGUI erosionRadiusText;
    public TMPro.TextMeshProUGUI evaporationText;
    public TMPro.TextMeshProUGUI gravityText;

    public int width, height;
    public float terrainHeightMultiplier;
    [SerializeField] [Range(0, 1)] private float inertia = 0.05f;
    [SerializeField] [Range(0, 10)] private float capacity = 2f;
    [SerializeField] [Range(0, 0.5f)] private float minSlope = 0.05f;
    [SerializeField] [Range(0, 1)] private float deposition = 0.1f;
    [SerializeField] [Range(0, 1)] private float erosion = 0.1f;
    [SerializeField] [Range(1, 5)] private int erosionRadius = 3;
    [SerializeField] [Range(0, 1)] private float evaporation = 0.05f;
    [SerializeField] [Range(-20, 0)] private float gravity = -9.81f;
    [SerializeField] private float initialWaterAmount = 1f;
    [SerializeField] float initialVelocity = 1f;
    DropletData currentDroplet;
    float currentDropletHeight;

    float[][] elevationMapSteps;
    private int terrainStepDifference;

    public void Init(int terrainWidth, int terrainHeight, float terrainHeightMultiplier)
    {
        width = terrainWidth;
        height = terrainHeight;
        this.terrainHeightMultiplier = terrainHeightMultiplier;
        elevationMapSteps = new float[6][];
        for(int i = 0; i < 6; i++)
        {
            elevationMapSteps[i] = new float[terrainWidth * terrainHeight];
        }
        terrainStepDifference = (int)droplets / 5;

    }

    public void Simulate(float[] elevationMap)
    {
        int currentTerrainStep = 1;
        elevationMap.CopyTo(elevationMapSteps[0], 0);
        for(int droplet = 0; droplet < droplets; droplet++)
        {
            DropletData newDroplet = new DropletData();
            newDroplet.position = new Vector2(UnityEngine.Random.Range(0f, width-1), UnityEngine.Random.Range(0f, height-1));
            newDroplet.water = initialWaterAmount;
            newDroplet.velocity = initialVelocity;

            float dirX = 0f;
            float dirY = 0f;

            for(int step = 0; step < dropletLifetime; step++)
            {
                //Calculate droplet height
                float dropletHeight;
                CalculateHeight(newDroplet.position, elevationMap, out dropletHeight);

                int nodeX = (int)newDroplet.position.x;
                int nodeY = (int)newDroplet.position.y;
                int currentIndex = nodeY * width + nodeX;
                //Offset of droplet within grid cell
                float xOffset = newDroplet.position.x - nodeX;
                float yOffset = newDroplet.position.y - nodeY; 

                currentDroplet = newDroplet;
                currentDropletHeight = dropletHeight;

                //Calculate droplet gradient
                float gradientX, gradientY;
                CalculateGradient(newDroplet.position, elevationMap, out gradientX, out gradientY);
                
                //Calculate new direction based on inertia
                dirX = (dirX * inertia - gradientX * (1 - inertia));
                dirY = (dirY * inertia - gradientY * (1 - inertia));

                //Debug.Log("x: " + dirX + "y: " + dirY);

                //Get magnitude (length) of direction vector
                float dirMagnitude = Mathf.Sqrt((dirX * dirX) + (dirY * dirY));

                //If magnitude is not 0 then divide direction components by magnitude to get direction unit vector (normalised)
                //Normalise direction so velocity is not taken into account and cells aren't skipped if velocity is too high
                if(dirMagnitude != 0)
                {
                    dirX /= dirMagnitude;
                    dirY /= dirMagnitude;
                }

                //Calculate new position by adding new direction
                newDroplet.position.x += dirX;
                newDroplet.position.y += dirY;

                //Debug.Log("nor x: " + dirX + "nor y: " + dirY);

                if(dirX == 0 || dirY == 0 || newDroplet.position.x < 0 || newDroplet.position.y < 0 || newDroplet.position.x >= width - 1 || newDroplet.position.y >= height - 1)
                {
                    break;
                }

                //Calculate new droplet height
                float newDropletHeight;
                CalculateHeight(newDroplet.position, elevationMap, out newDropletHeight);

                //Get height difference between old and new positions
                float heightDifference = newDropletHeight - dropletHeight;

                float sedimentCarryCapacity = Mathf.Max(-heightDifference * newDroplet.velocity * newDroplet.water * capacity, minSlope);

                if(heightDifference > 0 || newDroplet.sediment > sedimentCarryCapacity)
                {
                    //If droplet has more sediment than the carry capacity, deposity fraction of surplus sediment according to deposition value
                    float sedimentToDeposit = (heightDifference > 0) ? Mathf.Min(heightDifference, newDroplet.sediment) : (newDroplet.sediment - sedimentCarryCapacity) * deposition;

                    //Deposit sediment
                    newDroplet.sediment -= sedimentToDeposit;

                    elevationMap[currentIndex] += sedimentToDeposit * (1 - xOffset) * (1 - yOffset);
                    //Debug.Log("currI: " + sedimentToDeposit * ((1 - xOffset) * (1 - yOffset)) + " offset val: " + (1 - xOffset) * (1 - yOffset));

                    elevationMap[currentIndex + 1] += sedimentToDeposit * xOffset * (1 - yOffset);
                    //Debug.Log("currI + 1: " + sedimentToDeposit * (xOffset * (1 - yOffset)) + " offset val: " + (xOffset * (1 - yOffset)));

                    elevationMap[currentIndex + width] += sedimentToDeposit * (1 - xOffset) * yOffset;
                    //Debug.Log("currI + w: " + sedimentToDeposit * ((1 - xOffset) * yOffset) + " offset val: " + ((1 - xOffset) * yOffset));

                    elevationMap[currentIndex + width + 1] += sedimentToDeposit * xOffset * yOffset;
                    //Debug.Log("currI + w + 1: " + sedimentToDeposit * (xOffset * yOffset) + " offset val: " + (xOffset * yOffset));

                }
                else
                {
                    float amountToErode = Mathf.Min((sedimentCarryCapacity - newDroplet.sediment) * erosion, -heightDifference);
                    int[] vertexCoordinates = new int[erosionRadius * erosionRadius * 4];
                    float[] vertexErosionWeights = new float[erosionRadius * erosionRadius * 4];
                    float totalErosionWeight = 0;
                    int numberOfVertices = 0;

                    for(int x = -erosionRadius; x <= erosionRadius; x++)
                    {
                        for(int y = -erosionRadius; y <= erosionRadius; y++)
                        {
                            int squaredDistance = (x * x) + (y * y);

                            //Check if current point is within radius
                            if(squaredDistance < (erosionRadius * erosionRadius))
                            {
                                int erodeCoordX = nodeX + x;
                                int erodeCoordY = nodeY + y;

                                //Check if current coordinate is within world
                                if(erodeCoordX >= 0 && erodeCoordY >= 0 && erodeCoordX < width && erodeCoordY < height)
                                {
                                    //Debug.Log(numberOfVertices);
                                    vertexCoordinates[numberOfVertices] = erodeCoordY * width + erodeCoordX;
                                    Vector2 distance = new Vector2(erodeCoordX, erodeCoordY) - newDroplet.position;
                                    float magnitude = Mathf.Sqrt((distance.x * distance.x) + (distance.y * distance.y));
                                    vertexErosionWeights[numberOfVertices] = Mathf.Max(0, erosionRadius - magnitude);
                                    totalErosionWeight += vertexErosionWeights[numberOfVertices];
                        
                                    numberOfVertices++; 
                                }
                            }
                        }
                    }

                    for(int i = 0; i < numberOfVertices; i++)
                    {
                        int index = vertexCoordinates[i];
                        float w = vertexErosionWeights[i];
                        float weighedErodeAmount = amountToErode * (w / totalErosionWeight);
                        float deltaSediment = (elevationMap[index] < weighedErodeAmount) ? elevationMap[index] : weighedErodeAmount;

                        if(elevationMap[index] > deltaSediment)
                        {
                            elevationMap[index] -= deltaSediment;
                            newDroplet.sediment += deltaSediment;
                        }
                        
                    }
                }

                newDroplet.velocity = Mathf.Sqrt((newDroplet.velocity * newDroplet.velocity) + heightDifference * gravity);
                newDroplet.water = newDroplet.water * (1 - evaporation);
            }

            if(currentTerrainStep < 6 && droplet == (terrainStepDifference * (currentTerrainStep)) - 1)
            {
                elevationMap.CopyTo(elevationMapSteps[currentTerrainStep], 0);
                currentTerrainStep++;
            }
        }
        
    }

    public float[] GetElevationMapStep(int step)
    {
        if(elevationMapSteps != null)
        {
            if(elevationMapSteps[step] != null)
            {
                return elevationMapSteps[step];
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
        
    }

    public void CalculateGradient(Vector2 position, float[] elevationMap, out float gradientX, out float gradientY)
    {
        int nodeX = (int)position.x;
        int nodeY = (int)position.y;

        int currentIndex = nodeY * width + nodeX;

        float SWHeight = elevationMap[currentIndex];
        float SEHeight = elevationMap[currentIndex + 1];
        float NWHeight = elevationMap[currentIndex + width];
        float NEHeight = elevationMap[currentIndex + width + 1];

        //Offset of droplet within grid cell
        float xOffset = position.x - nodeX;
        float yOffset = position.y - nodeY; 
        
        //Direction and steepness gradient
        gradientX = (SEHeight - SWHeight) * (1 - yOffset) + (NEHeight - NWHeight) * yOffset;
        gradientY = (NWHeight - SWHeight) * (1 - xOffset) + (NEHeight - SEHeight) * xOffset;
    }

    public void CalculateHeight(Vector2 position, float[] elevationMap, out float dropletHeight)
    {
        int nodeX = (int)position.x;
        int nodeY = (int)position.y;

        int currentIndex = nodeY * width + nodeX;
        float SWHeight = elevationMap[currentIndex];
        float SEHeight = elevationMap[currentIndex + 1];
        float NWHeight = elevationMap[currentIndex + width];
        float NEHeight = elevationMap[currentIndex + width + 1];

        //Offset of droplet within grid cell
        float xOffset = position.x - nodeX;
        float yOffset = position.y - nodeY; 
        
        //Interpolated height
        dropletHeight = SWHeight * ((1 - xOffset) * (1 - yOffset)) +
                        SEHeight * (xOffset * (1 - yOffset)) +
                        NWHeight * ((1 - xOffset) * yOffset) +
                        NEHeight * (xOffset * yOffset);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DrawDroplet(currentDroplet, currentDropletHeight);
    }

    void DrawDroplet(DropletData droplet, float height)
    {
        Gizmos.DrawSphere(new Vector3(droplet.position.x * 10, height * terrainHeightMultiplier, droplet.position.y * 10), 1f);
    }

    public void OnDropletsChanged(float newDroplets)
    {
        droplets = (int)newDroplets;
        dropletsText.text = newDroplets.ToString();
    }
    public void OnLifetimeChanged(float newLifetime)
    {
        dropletLifetime = (int)newLifetime;
        lifetimeText.text = newLifetime.ToString();
    }

    public void OnInertiaChanged(float newInertia)
    {
        inertia = newInertia;
        inertiaText.text = newInertia.ToString("F2");
    }

    public void OnCapacityChanged(float newCapacity)
    {
        capacity = (int)newCapacity;
        capacityText.text = newCapacity.ToString();
    }

    public void OnMinSlopeChanged(float newMinSlope)
    {
        minSlope = newMinSlope;
        minSlopeText.text = newMinSlope.ToString("F2");
    }

    public void OnDepositionChanged(float newDeposition)
    {
        deposition = newDeposition;
        depositionText.text = newDeposition.ToString("F2");
    }

    public void OnErosionChanged(float newErosion)
    {
        erosion = newErosion;
        erosionText.text = newErosion.ToString("F2");
    }

    public void OnRadiusChanged(float newRadius)
    {
        erosionRadius = (int)newRadius;
        erosionRadiusText.text = newRadius.ToString();
    }

    public void OnEvaporationChanged(float newEvaporation)
    {
        evaporation = newEvaporation;
        evaporationText.text = newEvaporation.ToString("F2");
    }

    public void OnGravityChanged(float newGravity)
    {
        gravity = newGravity;
        gravityText.text = newGravity.ToString("F2");
    }
}
    

struct DropletData
{
    public Vector2 position;
    public float velocity;
    public float water;
    public float sediment;
}
