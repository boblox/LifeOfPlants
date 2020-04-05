using System;
using System.Collections.Generic;
using System.Linq;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.Domain
{
    public class Simulator
    {
        private readonly List<Plant> plants;
        private readonly ShadowImpactCalculator shadowImpactCalculator;

        public Simulator(List<Plant> plants)
        {
            this.plants = plants;
            this.shadowImpactCalculator = new ShadowImpactCalculator();
        }

        public void AddPlant(Plant plant)
        {
            plants.Add(plant);
        }

        public void Tick()
        {
            var treeUpdaters = new List<TreeUpdater>();
            foreach (var plant in plants)
            {
                if (plant is Tree tree)
                {
                    var normalizedShadowImpact = shadowImpactCalculator.GetNormalizedImpact(plant, plants);
                    var maxRadiusGrowth = Math.Max(
                        0,
                        plants.OfType<Tree>().Except(new[] { tree }).Min(otherTree => otherTree.DistanceTo(tree.X, tree.Y) - otherTree.Radius - tree.Radius) / 2);
                    treeUpdaters.Add(new TreeUpdater(tree, tree.GetMaximumPossibleRaiseAfterTick(normalizedShadowImpact, maxRadiusGrowth), 1));
                }
            }
            treeUpdaters.ForEach(treeUpdater => treeUpdater.UpdateTree());
            plants.OfType<Tree>().Where(i => i.IsDead).ToList().ForEach(tree => plants.Remove(tree));
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
