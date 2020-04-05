﻿namespace LifeOfPlants.Domain.Plants
{
    public class Birch : Tree
    {
        public Birch(float x, float y, float height, float radius) : base(x, y, height, radius)
        {
        }

        public override string PlantType => "Birch";
        public override float MaxHeight => 30;
        public override float MaxRadius => 5;
        public override int MaxAge => 125;
        public override int MinAgeOfFruiting => 15;
        public override int MinHeightOfFruiting => 5;
        public override int CountOfFruitsPerTick => 4;
        public override int MaxSeedSprayingDistance => 50;
        public override float HeightGrowthPerTick => 1;
        public override float RadiusGrowthPerTick => 0.25f;
        public override float CrownDensity => 0.5f;
        public override float GrowthStopShadowImpact => 0.6f;

        public override Tree GenerateKidTree(float x, float y, float height, float radius)
        {
            return new Birch(x, y, height, radius);
        }
    }
}