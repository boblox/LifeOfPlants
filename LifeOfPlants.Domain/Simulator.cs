using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.Domain
{
    public class Simulator
    {
        private readonly float planeSizeX;
        private readonly float planeSizeY;
        private readonly List<Plant> plants = new List<Plant>();
        private readonly ShadowImpactCalculator shadowImpactCalculator;
        public ReadOnlyCollection<Plant> Plants { get; }

        public Simulator(float planeSizeX, float planeSizeY)
        {
            this.planeSizeX = planeSizeX;
            this.planeSizeY = planeSizeY;
            this.Plants = new ReadOnlyCollection<Plant>(plants);
            this.shadowImpactCalculator = new ShadowImpactCalculator();
        }

        public bool TryToAddPlant(Plant plant)
        {
            if (!FitsIntoPlane(plant)) return false;
            if (plant is Tree tree)
            {
                if (GetMinSpaceBetweenTreeAndPlants(tree) >= 0)
                {
                    plants.Add(plant);
                    return true;
                }
                return false;
            }
            plants.Add(plant);
            return true;
        }

        public void RemovePlant(Plant plant)
        {
            plants.Remove(plant);
        }

        private bool FitsIntoPlane(Plant plant)
        {
            return plant.X >= -planeSizeX && plant.X <= planeSizeX && plant.Y >= -planeSizeY && plant.Y <= planeSizeY;
        }

        public List<Plant> Tick()
        {
            UpdateTreesBecauseOfGrowing();
            RemoveDeadTrees();
            return PlantPlantsFromSeeds();
        }

        private void UpdateTreesBecauseOfGrowing()
        {
            var treeUpdaters = new List<TreeUpdater>();
            foreach (var plant in plants)
            {
                if (plant is Tree tree)
                {
                    var normalizedShadowImpact = shadowImpactCalculator.GetNormalizedImpact(plant, plants);
                    var maxRadiusGrowth = Math.Max(0, GetMinSpaceBetweenTreeAndPlants(tree) / 2);
                    treeUpdaters.Add(new TreeUpdater(tree, tree.GetMaximumPossibleRaiseAfterTick(normalizedShadowImpact, maxRadiusGrowth), 1));
                }
            }
            treeUpdaters.ForEach(treeUpdater => treeUpdater.UpdateTree());
        }

        private void RemoveDeadTrees()
        {
            plants.OfType<Tree>().Where(i => i.IsDead).ToList().ForEach(tree => plants.Remove(tree));
        }

        private List<Plant> PlantPlantsFromSeeds()
        {
            var newPlants = new List<Plant>();
            foreach (var plant in plants)
            {
                if (plant is Tree tree && tree.CanBearFruits)
                {
                    Enumerable.Range(0, tree.CountOfFruitsPerTick).ToList()
                        .ForEach(i => newPlants.Add(GenerateKidTreeFrom(tree)));
                }
            }
            return newPlants.Where(TryToAddPlant).ToList();
        }

        private Tree GenerateKidTreeFrom(Tree tree)
        {
            var random = new Random();
            var sign = random.Next(2) == 0 ? -1 : 1;
            var radius = (float)random.NextDouble() * tree.MaxSeedSprayingDistance;
            var xDiff = (float)random.NextDouble() * radius * 2 - radius;
            var yDiff = sign * (float)Math.Sqrt(radius * radius - xDiff * xDiff);
            return tree.GenerateKidTree(tree.X + xDiff, tree.Y + yDiff, Tree.DefaultStartHeight, Tree.DefaultStartRadius);
        }

        private float GetMinSpaceBetweenTreeAndPlants(Tree targetTree)
        {
            var otherTrees = plants.OfType<Tree>().Except(new[] { targetTree }).ToList();
            return otherTrees.Any() ? otherTrees.Min(tree => tree.DistanceTo(targetTree.X, targetTree.Y) - tree.Radius - targetTree.Radius) : 0;
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
