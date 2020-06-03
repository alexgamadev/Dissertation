using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] Renderer textureRenderer = null;
    [SerializeField] private int terrainWidth = 101;
    [SerializeField] private int terrainDepth = 101;
    [SerializeField] private int terrainHeightMultiplier = 10;

    [SerializeField] private bool useHydraulicSim = false;
    [SerializeField] private bool useHydraulicErosionSlider = false;
    [SerializeField] private bool useThermalSim = false;
    [SerializeField] private bool usePlantSim = false;

    public TMPro.TextMeshProUGUI hydraulicStepText;

    [SerializeField] [Range(0, 5)] private int terrainStep = 5;

    [SerializeField] private ElevationMapSettings elevationMapSettings = null;
    [SerializeField] private TerrainMesh terrainMesh;
    float[] elevationMap;
    public HydraulicErosionSim hydraulicSim;
    public ThermalErosionSim thermalSim;
    public PlantSim plantSim;

    public void GenerateTerrain()
    {
        terrainStep = 5;
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        Debug.Log(plants.Length);
        foreach (GameObject plant in plants)
        {
            GameObject.DestroyImmediate(plant);
        }
        GenerateTerrainMaps();
        RenderTerrain();

    }

    public void Start()
    {
        GenerateTerrain();
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void UpdateTerrainMesh()
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        Debug.Log(plants.Length);
        foreach (GameObject plant in plants)
        {
            GameObject.DestroyImmediate(plant);
        }
        if(useHydraulicSim)
        {
            hydraulicSim.Init(terrainWidth, terrainDepth, terrainHeightMultiplier);
            hydraulicSim.Simulate(elevationMap);
        }
        if(useThermalSim)
        {
            thermalSim.Init(terrainWidth, terrainDepth, terrainHeightMultiplier);
            thermalSim.Simulate(elevationMap);
        }

        if(usePlantSim)
        {
            plantSim.Init(terrainWidth, terrainDepth, terrainHeightMultiplier, elevationMapSettings.noiseSettings.seed);
            plantSim.Simulate(elevationMap);
        }
        
        RenderTerrain();
    }
    
    void GenerateTerrainMaps()
    {
        elevationMap = ElevationMapGenerator.GenerateElevationMap(terrainWidth, terrainDepth, elevationMapSettings);
    }

    void RenderTerrain()
    {
        terrainMesh.GenerateTerrainMesh(terrainWidth-1, terrainDepth-1, terrainHeightMultiplier, elevationMap);
    }

    void OnValidate()
    {
        if(terrainWidth < 1 )
        {
            terrainWidth = 1;
        }
        if(terrainDepth < 1 )
        {
            terrainDepth = 1;
        }

        if(elevationMapSettings.noiseSettings.octaves < 1)
        {
            elevationMapSettings.noiseSettings.octaves = 1;
        }
        if(elevationMapSettings.noiseSettings.scale < 1)
        {
            elevationMapSettings.noiseSettings.scale = 1;
        }
    }

    public void OnHydraulicSimToggle(bool toggle)
    {
        useHydraulicSim = toggle;
    }

    public void OnThermalSimToggle(bool toggle)
    {
        useThermalSim = toggle;
    }

    public void OnPlantSimToggle(bool toggle)
    {
        usePlantSim = toggle;
    }

    public void OnHydraulicStepChange(float step)
    {
        int intStep = (int)step;
        hydraulicStepText.text = intStep.ToString();
        float[] newElevationMap = hydraulicSim.GetElevationMapStep(intStep);
        if(newElevationMap != null)
        {
            if(useThermalSim)
            {
            thermalSim.Init(terrainWidth, terrainDepth, terrainHeightMultiplier);
            thermalSim.Simulate(elevationMap);
            }

            if(usePlantSim)
            {
                plantSim.Init(terrainWidth, terrainDepth, terrainHeightMultiplier, elevationMapSettings.noiseSettings.seed);
                plantSim.Simulate(elevationMap);
            }
            elevationMap = newElevationMap;

            RenderTerrain();
        }
    }

    public void OnResetTerrain()
    {
        GenerateTerrain();
    }

    public void OnSimulateAndRender()
    {
        UpdateTerrainMesh();
    }
}
