using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LifeOfPlants.Domain;
using LifeOfPlants.Domain.Plants;
using UnityEngine;
using Tree = LifeOfPlants.Domain.Plants.Tree;

public class PlaneScript : MonoBehaviour
{
    private Simulator simulator;
    private readonly object simulatorLock = new object();
    private TreeType selectedTreeTypeToCreate = TreeType.Beech;
    private ConcurrentStack<Plant> addedPlants = new ConcurrentStack<Plant>();
    private readonly Dictionary<Plant, GameObject> plantsDict = new Dictionary<Plant, GameObject>();
    public const float tickGap = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        var meshSize = GetComponent<MeshFilter>().mesh.bounds.size;
        var scale = transform.localScale;
        simulator = new Simulator(scale.x * meshSize.x / 2, scale.z * meshSize.z / 2);
        new List<Tree>
        {
            new Beech(0, 0, 1, 1),
            new Beech(10, 10, 15, 5),
            new Beech(-10, 10, 15, 5),
            new Beech(10, -10, 15, 5),
            new Beech(-10, -10, 15, 5),
            new Birch(10, 0, 15, 3)
        }.ForEach(CreateGameObjectForTreeAndAddItToPlantsDictAndSimulator);
        Debug.Log("Start count of plants: " + simulator.Plants.Count);

        //Task.Run(SimulatorTick);
        Task.Factory.StartNew(SimulatorTick, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        StartCoroutine(RendererTick());
    }

    void CreateGameObjectForTreeAndAddItToPlantsDictAndSimulator(Tree tree)
    {
        lock (simulatorLock)
        {
            if (simulator.TryToAddPlant(tree))
            {
                CreateGameObjectForTreeAndAddItToPlantsDict(tree);
            }
        }
    }

    void CreateGameObjectForTreeAndAddItToPlantsDict(Tree tree)
    {
        plantsDict.Add(tree, CreateGameObjectForTree(tree));
        //Debug.Log($"Plant added: {tree}");
    }

    void RemovePlantFromPlantsDict(Plant plant)
    {
        Destroy(plantsDict[plant]);
        plantsDict.Remove(plant);
    }

    void RemovePlantFromPlantsDictAndSimulator(Plant plant)
    {
        RemovePlantFromPlantsDict(plant);
        lock (simulatorLock)
        {
            simulator.RemovePlant(plant);
        }
    }

    GameObject CreateGameObjectForTree(Tree tree)
    {
        var gameObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gameObj.transform.position = new Vector3(tree.X, tree.Height / 2, tree.Y);
        gameObj.transform.localScale = new Vector3(tree.Radius * 2, tree.Height / 2, tree.Radius * 2);
        Material material;
        switch (tree)
        {
            case Birch _:
                material = Resources.Load<Material>("Materials/Birch");
                break;
            case Beech _:
            default:
                material = Resources.Load<Material>("Materials/Beech");
                break;
        }

        gameObj.GetComponent<MeshRenderer>().material = material;
        return gameObj;
    }

    // Update is called once per frame
    void Update()
    {
        var clickedLeftButton = Input.GetMouseButtonDown(0);
        var clickedRightButton = Input.GetMouseButtonDown(1);
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTreeTypeToCreate = TreeType.Beech;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTreeTypeToCreate = TreeType.Birch;
        }
        if (clickedLeftButton || clickedRightButton)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hitSuccess = Physics.Raycast(ray, out var hit);
            if (hitSuccess)
            {
                if (clickedLeftButton && hit.transform.gameObject == gameObject)
                {
                    Tree tree;
                    switch (selectedTreeTypeToCreate)
                    {
                        case TreeType.Beech:
                            tree = new Beech(hit.point.x, hit.point.z, Tree.DefaultStartHeight, Tree.DefaultStartRadius);
                            break;
                        case TreeType.Birch:
                        default:
                            tree = new Birch(hit.point.x, hit.point.z, Tree.DefaultStartHeight, Tree.DefaultStartRadius);
                            break;
                    }
                    CreateGameObjectForTreeAndAddItToPlantsDictAndSimulator(tree);
                }
                else if (clickedRightButton)
                {
                    var plant = plantsDict.FirstOrDefault(i => i.Value == hit.transform.gameObject).Key;
                    if (plant != null)
                    {
                        RemovePlantFromPlantsDictAndSimulator(plant);
                    }
                }
            }
        }
    }

    async Task SimulatorTick()
    {
        while (true)
        {
            try
            {
                lock (simulatorLock)
                {
                    var plants = simulator.Tick().ToArray();
                    if (plants.Any()) addedPlants.PushRange(plants);
                }
                await Task.Delay((int)(tickGap * 1000));
            }
            catch (Exception exc)
            {
                Debug.LogError(exc.Message);
            }
        }
    }

    IEnumerator RendererTick()
    {
        while (true)
        {
            plantsDict.Keys.OfType<Tree>().Where(tree => tree.IsDead).ToList().ForEach(RemovePlantFromPlantsDict);
            foreach (var plantKeyValuePair in plantsDict)
            {
                if (plantKeyValuePair.Key is Tree tree)
                {
                    var currentPosition = plantKeyValuePair.Value.transform.position;
                    plantKeyValuePair.Value.transform.position =
                        new Vector3(currentPosition.x, tree.Height / 2, currentPosition.z);
                    plantKeyValuePair.Value.transform.localScale =
                        new Vector3(tree.Radius * 2, tree.Height / 2, tree.Radius * 2);
                    //Debug.Log(tree.ToString());
                }
            }
            addedPlants.OfType<Tree>().ToList().ForEach(CreateGameObjectForTreeAndAddItToPlantsDict);
            addedPlants.Clear();
            yield return new WaitForSeconds(tickGap);
        }
    }

    private enum TreeType
    {
        Birch,
        Beech
    }
}
