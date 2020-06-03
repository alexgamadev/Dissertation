using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSim : MonoBehaviour
{
    public int width, height;
    public float terrainHeightMultiplier;
    [SerializeField] bool useSeed = false;
    [SerializeField] private Plant plant;
    [SerializeField] private int initialPlants = 20;
    [SerializeField] private int maxGenerationAttempts = 10;
    [SerializeField] private int maxCycles = 200;
    [SerializeField] private int steepnessFactor = 4;
    [SerializeField] private float maxSteepness = 0;
    [SerializeField] private List<Plant> plants;

    public TMPro.TextMeshProUGUI cyclesText;

    public TMPro.TextMeshProUGUI steepnessConstantText;

    public TMPro.TextMeshProUGUI initialPlantsText;

    public TMPro.TextMeshProUGUI seedSurvivalText;

    public void Init(int terrainWidth, int terrainHeight, float terrainHeightMultiplier, int seed)
    {
        width = terrainWidth;
        height = terrainHeight;
        this.terrainHeightMultiplier = terrainHeightMultiplier;
        maxSteepness = (float)steepnessFactor/(float)width;
        if(useSeed)
        {
            Random.InitState(seed);
        }
    }

    public void Simulate(float[] elevationMap)
    {
        int successfulPlants = 0;
        int iterations = 0;
        plants = new List<Plant>();

        while(successfulPlants < initialPlants && iterations < maxGenerationAttempts)
        {
            iterations++;
            Plant newPlant = plant;
            newPlant.position = new Vector2(UnityEngine.Random.Range(0f, width-1), UnityEngine.Random.Range(0f, height-1));

            if(!IsTerrainValid(newPlant.position, elevationMap)) 
            {
                continue;
            }

            float plantHeight;
            CalculateHeight(newPlant.position, elevationMap, out plantHeight);

            Vector3 worldPosition = new Vector3(newPlant.position.x, plantHeight * terrainHeightMultiplier, newPlant.position.y);
            PlacePrefab(plant.plantPrefab, worldPosition);

            newPlant.age = UnityEngine.Random.Range(1, plant.maxLifetime);

            plants.Add(newPlant);

            successfulPlants++;
        }



        for(int cycle = 0; cycle < maxCycles; cycle++)
        {
            for(int plantIndex = 0; plantIndex < plants.Count; plantIndex++)
            {
                Plant currentPlant = plants[plantIndex];
                currentPlant.age++;

                if(currentPlant.age < plant.matureAge)
                {
                    plants[plantIndex] = currentPlant;
                    continue;
                }
                if(currentPlant.age >= plant.maxLifetime)
                {
                    plants.Remove(currentPlant);
                    continue;
                }

                DisperseSeeds(currentPlant, elevationMap);
            }
        }

         
        //Check gradient of plant location
    }

    public Plant DisperseSeeds(Plant plant, float[] elevationMap)
    {
        if(UnityEngine.Random.Range(0, 1) < plant.seedSettings.seedSurvivalChance)
        {
            return new Plant();
        }
        List<Plant> seeds = new List<Plant>();
        Plant seed = plant;
        seed.age = 0;

        float angle = UnityEngine.Random.Range(0, 360);
        float offsetX = Mathf.Cos(angle);
        float offsetY = Mathf.Sin(angle);
        float distance = UnityEngine.Random.Range(plant.seedSettings.minSeedDistance, plant.seedSettings.maxSeedDistance);

        Vector2 relativeOffset = new Vector3(distance*offsetX, distance*offsetY);

        seed.position = plant.position + relativeOffset;

        if(seed.position.x < 0 || seed.position.x > width - 1 || seed.position.y < 0 || seed.position.y > height - 1)
        {
            return new Plant();
        }

        float seedHeight;
        CalculateHeight(seed.position, elevationMap, out seedHeight);

        Vector3 worldPosition = new Vector3(seed.position.x, seedHeight * terrainHeightMultiplier, seed.position.y);

        if(!IsTerrainValid(worldPosition, elevationMap))
        {
            return new Plant();
        }

        PlacePrefab(plant.plantPrefab, worldPosition);

        //Get offset values from angle
        //Find new position
        //Check gradient
        //Check if too close to other plant/seed
        //If seed survived add to list
        return seed;

    }

    public bool IsTerrainValid(Vector2 position, float[] elevationMap)
    {
        float gradientX, gradientY;
        CalculateGradient(position, elevationMap, out gradientX, out gradientY);
        float gradient = Mathf.Sqrt((gradientX * gradientX) + (gradientY * gradientY));

            if(gradient > maxSteepness)
            {
                return false;
            }
            else
            {
                return true;
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
        Debug.Log("seed pos: " + position);
        int nodeX = (int)position.x;
        int nodeY = (int)position.y;

        Debug.Log("x: " + nodeX + " y: " + nodeY);

        int currentIndex = nodeY * width + nodeX;
        Debug.Log("index: " + currentIndex);
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

    public void PlacePrefab(GameObject prefab, Vector3 position)
    {
        GameObject.Instantiate(prefab, position, prefab.transform.rotation);
    }

    public void OnCyclesChanged(float newCycles)
    {
        maxCycles = (int)newCycles;
        cyclesText.text = newCycles.ToString();
    }

    public void OnSteepnessChanged(float newSteepness)
    {
        steepnessFactor = (int)newSteepness;
        steepnessConstantText.text = newSteepness.ToString();
    }

    public void OnInitialPlantsChanged(float newInitialPlants)
    {
        initialPlants = (int)newInitialPlants;
        initialPlantsText.text = initialPlants.ToString();
    }

    public void OnSeedSurvivalChanged(float newSeedSurvival)
    {
        plant.seedSettings.seedSurvivalChance = newSeedSurvival;
        seedSurvivalText.text = newSeedSurvival.ToString("F2");
    }

    [System.Serializable]
    public struct Plant
    {
        public int age;
        public int matureAge;
        public int maxLifetime;
        public GameObject plantPrefab;
        public Vector2 position;
        public SeedSettings seedSettings;
        
    }

    [System.Serializable]
    public struct SeedSettings
    {
        public int maxNumSeeds;
        public float survivalChance;  
        public float seedSurvivalChance;
        public float minSeedDistance;
        public float maxSeedDistance;
        public GameObject seedPrefab;
    }
}
