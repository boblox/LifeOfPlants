using System;
using System.Collections.Generic;
using System.Linq;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.Domain
{
    public class PlantsCreator
    {
        public List<Plant> CreateChildPlantsFromSeedsOf(Plant plant)
        {
            var childPlants = new List<Plant>();
            if (plant is Tree tree && tree.CanBearFruits)
            {
                Enumerable.Range(0, tree.CountOfFruitsPerTick)
                    .ToList()
                    .ForEach(index => childPlants.Add(CreateNewChildTreeFrom(tree)));
            }
            return childPlants;
        }

        private Tree CreateNewChildTreeFrom(Tree tree)
        {
            var random = new Random();
            var sign = random.Next(2) == 0 ? -1 : 1;
            var radius = (float)random.NextDouble() * tree.MaxSeedSprayingDistance;
            var xDiff = (float)random.NextDouble() * radius * 2 - radius;
            var yDiff = sign * (float)Math.Sqrt(radius * radius - xDiff * xDiff);
            return tree.GenerateKidTree(tree.X + xDiff, tree.Y + yDiff, Tree.DefaultStartHeight, Tree.DefaultStartRadius);
        }
    }
}
