using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.Domain
{
    public class Simulator
    {
        public float PlaneSizeX { get; }
        public float PlaneSizeY { get; }
        private readonly int maxConcurrentNumberOfTasks;
        private readonly List<Plant> plants = new List<Plant>();
        private readonly Dictionary<Plant, List<Plant>> nearbyPlantsCache = new Dictionary<Plant, List<Plant>>();
        private readonly ShadowImpactCalculator shadowImpactCalculator;
        private readonly PlantsCreator plantsCreator;
        private const float MinGapBetweenCreatedTreeAndOtherTrees = 1.5f;
        public ReadOnlyCollection<Plant> Plants => plants.AsReadOnly();

        public Simulator(float planeSizeX, float planeSizeY, int maxConcurrentNumberOfTasks)
        {
            this.PlaneSizeX = planeSizeX;
            this.PlaneSizeY = planeSizeY;
            this.maxConcurrentNumberOfTasks = maxConcurrentNumberOfTasks;
            this.shadowImpactCalculator = new ShadowImpactCalculator();
            this.plantsCreator = new PlantsCreator();
        }

        public List<Plant> TryToAddPlants(List<Plant> potentialNewPlants)
        {
            HashSet<Plant> plantsToBeUpdated = new HashSet<Plant>();
            List<Plant> addedPlants = new List<Plant>();
            foreach (var plant in potentialNewPlants)
            {
                if (!FitsIntoPlane(plant)) continue;
                if (plant is Tree tree)
                {
                    var nearbyPlants = GetNearbyPlantsFor(plant);
                    if (GetMinGapBetweenTreeAndOtherPlants(tree, nearbyPlants) >= MinGapBetweenCreatedTreeAndOtherTrees)
                    {
                        plants.Add(plant);
                        addedPlants.Add(plant);
                        plantsToBeUpdated.Add(plant);
                        nearbyPlants.ForEach(nearbyPlant => plantsToBeUpdated.Add(nearbyPlant));
                    }
                }
                else
                {
                    throw new NotSupportedException("Unsupported plant type");
                }
            }

            foreach (var plant in plantsToBeUpdated)
            {
                nearbyPlantsCache[plant] = GetNearbyPlantsFor(plant);
            }
            return addedPlants;
        }

        public void RemovePlant(Plant plant)
        {
            foreach (var nearbyPlantsList in nearbyPlantsCache.Values)
            {
                nearbyPlantsList.Remove(plant);
            }
            this.nearbyPlantsCache.Remove(plant);
            plants.Remove(plant);
        }

        public async Task<List<Plant>> Tick()
        {
            var treeUpdaters = await Task.WhenAll(ParallelUtils.RunInParallelBatches(GetPlantsUpdates, plants.Count, maxConcurrentNumberOfTasks));
            treeUpdaters.SelectMany(list => list).ToList().ForEach(treeUpdater => treeUpdater.UpdateTree());
            var potentialNewPlants = (await Task.WhenAll(ParallelUtils.RunInParallelBatches(CreateChildPlants, plants.Count, maxConcurrentNumberOfTasks)))
                .SelectMany(list => list)
                .ToList();
            var addedPlants = TryToAddPlants(potentialNewPlants);
            RemoveDeadPlants();
            return addedPlants;
        }

        private List<Plant> GetNearbyPlantsFor(Plant plant)
        {
            return plants.Where(otherPlant =>
                otherPlant is Tree otherTree && otherTree != plant && otherTree.DistanceTo(plant.X, plant.Y) <= otherTree.MaxHeight).ToList();
        }

        private bool FitsIntoPlane(Plant plant)
        {
            return plant.X >= -PlaneSizeX && plant.X <= PlaneSizeX && plant.Y >= -PlaneSizeY && plant.Y <= PlaneSizeY;
        }

        private List<TreeUpdater> GetPlantsUpdates(int startIndex, int endIndex)
        {
            var treeUpdaters = new List<TreeUpdater>();
            for (var i = startIndex; i <= endIndex; i++)
            {
                var plant = plants[i];
                if (plant is Tree tree)
                {
                    var normalizedShadowImpact = shadowImpactCalculator.GetNormalizedImpact(plant, nearbyPlantsCache[plant]);
                    var maxRadiusGrowth = Math.Max(0, GetMinGapBetweenTreeAndOtherPlants(tree, nearbyPlantsCache[plant]) / 2);
                    treeUpdaters.Add(new TreeUpdater(tree, tree.GetMaximumPossibleRaiseAfterTick(normalizedShadowImpact, maxRadiusGrowth), 1));
                }
            }
            return treeUpdaters;
        }

        private void RemoveDeadPlants()
        {
            plants.OfType<Tree>().Where(i => i.IsDead).ToList().ForEach(RemovePlant);
        }

        private List<Plant> CreateChildPlants(int startIndex, int endIndex)
        {
            var newPlants = new List<Plant>();
            for (var i = startIndex; i <= endIndex; i++)
            {
                newPlants.AddRange(plantsCreator.CreateChildPlantsFromSeedsOf(plants[i]));
            }
            return newPlants;
        }

        private float GetMinGapBetweenTreeAndOtherPlants(Tree targetTree, List<Plant> otherPlants)
        {
            var otherTrees = otherPlants.OfType<Tree>().ToList();
            return otherTrees.Any() ?
                otherTrees.Min(tree => tree.DistanceTo(targetTree.X, targetTree.Y) - tree.Radius - targetTree.Radius) :
                float.MaxValue;
        }

        private struct TreeUpdater
        {
            private TreeGrowth TreeGrowth { get; }
            private int AgeChange { get; }
            private Tree Tree { get; }

            public TreeUpdater(Tree tree, TreeGrowth treeGrowth, int ageChange)
            {
                TreeGrowth = treeGrowth;
                AgeChange = ageChange;
                Tree = tree;
            }
            public void UpdateTree()
            {
                Tree.RaiseAge(AgeChange);
                Tree.RaiseRadius(TreeGrowth.RadiusChange);
                Tree.RaiseHeight(TreeGrowth.HeightChange);
            }
        }
    }
}
