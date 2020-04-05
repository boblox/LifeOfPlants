using System.Collections.Generic;
using System.Linq;
using LifeOfPlants.Domain;
using LifeOfPlants.Domain.Plants;
using UnityEngine;
using Tree = LifeOfPlants.Domain.Plants.Tree;

public class PlaneScript : MonoBehaviour
{
    private Simulator simulator;
    private readonly Dictionary<Plant, GameObject> plantsDict = new Dictionary<Plant, GameObject>();
    public const float tickGap = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        simulator = new Simulator();
        new List<Tree>
        {
            new Beech(0, 0, 1, 1),
            new Beech(10, 10, 15, 5),
            new Beech(-10, 10, 15, 5),
            new Beech(10, -10, 15, 5),
            new Beech(-10, -10, 15, 5),
            new Birch(10, 0, 15, 3)
        }.ForEach(AddTree);
        Debug.Log("Start count of plants: " + simulator.Plants.Count);

        InvokeRepeating("GameTick", tickGap, tickGap);
    }

    void AddTree(Tree tree)
    {
        if (simulator.AddPlant(tree))
        {
            plantsDict.Add(tree, CreateGameObjectForTree(tree));
            Debug.Log($"Plant added: {tree}");
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
        if (clickedLeftButton || clickedRightButton)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) && hit.transform.gameObject == gameObject)
            {
                Tree tree = null;
                if (clickedLeftButton) tree = new Birch(hit.point.x, hit.point.z, 1, 1);
                else if (clickedRightButton) tree = new Beech(hit.point.x, hit.point.z, 1, 1);
                AddTree(tree);
            }
        }
    }

    void GameTick()
    {
        Debug.Log("New tick");
        simulator.Tick();

        plantsDict.Keys.OfType<Tree>().Where(tree => tree.IsDead).ToList().ForEach(tree =>
        {
            Destroy(plantsDict[tree]);
            plantsDict.Remove(tree);
        });
        foreach (var plantKeyValuePair in plantsDict)
        {
            if (plantKeyValuePair.Key is Tree tree)
            {
                var currentPosition = plantKeyValuePair.Value.transform.position;
                plantKeyValuePair.Value.transform.position =
                    new Vector3(currentPosition.x, tree.Height / 2, currentPosition.z);
                plantKeyValuePair.Value.transform.localScale =
                    new Vector3(tree.Radius * 2, tree.Height / 2, tree.Radius * 2);
                Debug.Log(tree.ToString());
            }
        }
    }
}
