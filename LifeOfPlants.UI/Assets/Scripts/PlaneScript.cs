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
using Random = System.Random;
using Tree = LifeOfPlants.Domain.Plants.Tree;

public class PlaneScript : MonoBehaviour
{
    private Simulator simulator;
    private TreeType selectedTreeTypeToCreate = TreeType.Beech;
    private readonly ConcurrentStack<Plant> addedPlants = new ConcurrentStack<Plant>();
    private readonly Dictionary<Plant, GameObject> plantsDict = new Dictionary<Plant, GameObject>();
    private const float tickGap = 0.3f;
    private readonly Random random = new Random();

    // Start is called before the first frame update
    void Start()
    {
        var meshSize = GetComponent<Terrain>().terrainData.size;
        var scale = transform.localScale;
        simulator = new Simulator(meshSize.x / 2, meshSize.z / 2, 8);
        new List<Tree>
        {
            //new Beech(0, 0, 1, 1),
            //new Beech(10, 10, 15, 5),
            //new Beech(-10, 10, 15, 5),
            //new Beech(10, -10, 15, 5),
            //new Beech(-10, -10, 15, 5),
            //new Birch(10, 0, 15, 3)
            GetRandomTree(TreeType.Beech),
            GetRandomTree(TreeType.Birch),
            GetRandomTree(TreeType.Beech),
            GetRandomTree(TreeType.Birch),
            GetRandomTree(TreeType.Beech),
        }.ForEach(CreateGameObjectForTreeAndAddItToPlantsDictAndSimulator);
        Debug.Log("Start count of plants: " + simulator.Plants.Count);

        Task.Factory.StartNew(SimulatorTick, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        StartCoroutine(RendererTick());
    }

    Tree GetRandomTree(TreeType treeType)
    {
        var x = random.NextDouble() * 2 * simulator.PlaneSizeX - simulator.PlaneSizeX;
        var y = random.NextDouble() * 2 * simulator.PlaneSizeY - simulator.PlaneSizeY;
        var height = random.Next(10) + 10;
        var radius = random.Next(2) + 3;
        switch (treeType)
        {
            case TreeType.Birch:
                return new Birch((float)x, (float)y, height, radius);
            case TreeType.Beech:
            default:
                return new Beech((float)x, (float)y, height, radius);
        }
    }

    void CreateGameObjectForTreeAndAddItToPlantsDictAndSimulator(Tree tree)
    {
        if (simulator.TryToAddPlants(new List<Plant> { tree }).Any())
        {
            CreateGameObjectForTreeAndAddItToPlantsDict(tree);
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
        simulator.RemovePlant(plant);
    }

    void SetPosition(GameObject gameObject, Tree tree)
    {
        float scaleFactor;
        switch (tree)
        {
            case Birch _:
                scaleFactor = 1;
                break;
            case Beech _:
            default:
                scaleFactor = 0.06f;
                break;
        }
        gameObject.transform.position = new Vector3(tree.X, 0, tree.Y);
        gameObject.transform.localScale = new Vector3(tree.Radius * 2 * scaleFactor, tree.Height / 2 * scaleFactor, tree.Radius * 2 * scaleFactor);
    }

    GameObject CreateGameObjectForTree(Tree tree)
    {
        GameObject gameObj;
        switch (tree)
        {
            case Birch _:
                gameObj = GameObject.Find("Birch_1");
                break;
            case Beech _:
            default:
                gameObj = GameObject.Find("Tree9_2");
                break;
        }

        var clonedObject = Instantiate(gameObj);
        SetPosition(clonedObject, tree);
        //clonedObject.transform.position = new Vector3(tree.X, 0, tree.Y);
        //clonedObject.transform.localScale = new Vector3(tree.Radius * 2 * scaleFactor, tree.Height / 2 * scaleFactor, tree.Radius * 2 * scaleFactor);

        //var gameObj = GameObject.Find("Birch_1");
        //gameObj.AddComponent<MeshFilter>();
        //gameObj.AddComponent<MeshRenderer>();

        //var clonedObject = Instantiate(gameObj, new Vector3(tree.X, 0, tree.Y), Quaternion.identity);
        //clonedObject.transform.localScale = new Vector3(tree.Radius * 2, tree.Height / 2, tree.Radius * 2);

        //var gameObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //gameObj.transform.position = new Vector3(tree.X, tree.Height / 2, tree.Y);
        //gameObj.transform.localScale = new Vector3(tree.Radius * 2, tree.Height / 2, tree.Radius * 2);
        //Material material;
        //switch (tree)
        //{
        //    case Birch _:
        //        material = Resources.Load<Material>("Materials/Birch");
        //        break;
        //    case Beech _:
        //    default:
        //        material = Resources.Load<Material>("Materials/Beech");
        //        break;
        //}

        //gameObj.GetComponent<MeshRenderer>().material = material;
        //return gameObj;
        return clonedObject;
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
                var plants = (await simulator.Tick()).ToArray();
                if (plants.Any()) addedPlants.PushRange(plants);
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
                    SetPosition(plantKeyValuePair.Value, tree);
                    //var currentPosition = plantKeyValuePair.Value.transform.position;
                    //plantKeyValuePair.Value.transform.position =
                    //    new Vector3(currentPosition.x, 0, currentPosition.z);
                    //plantKeyValuePair.Value.transform.localScale =
                    //    new Vector3(tree.Radius * 2, tree.Height / 2, tree.Radius * 2);
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
