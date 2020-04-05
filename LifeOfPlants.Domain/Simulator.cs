using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.Domain
{
    public class Simulator
    {
        private readonly List<Plant> plants = new List<Plant>();
        private readonly ShadowImpactCalculator shadowImpactCalculator;
        public ReadOnlyCollection<Plant> Plants { get; }

        public Simulator()
        {
            this.Plants = new ReadOnlyCollection<Plant>(plants);
            this.shadowImpactCalculator = new ShadowImpactCalculator();
        }

        public bool AddPlant(Plant plant)
        {
            if (plant is Tree tree)
            {
                if (GetMinSpaceBetweenTreeAndPlants(tree) >= 0)
                {
                    plants.Add(plant);
                    return true;
                }
                return false;
            }
            else
            {
                plants.Add(plant);
                return true;
            }
        }

        public void Tick()
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
            plants.OfType<Tree>().Where(i => i.IsDead).ToList().ForEach(tree => plants.Remove(tree));
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
