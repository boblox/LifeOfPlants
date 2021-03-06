﻿namespace LifeOfPlants.Domain.Plants
{
    public class Beech : Tree
    {
        public Beech(float x, float y, float height, float radius) : base(x, y, height, radius)
        {
        }

        public override string PlantType => "Beech";
        public override float DefaultMaxHeight => 40;
        public override int MaxHeightVariation => 5;
        public override float MaxRadius => 9;
        public override int DefaultMaxAge => 300;
        public override int MaxAgeVariation => 50;
        public override int MinAgeOfFruiting => 40;
        public override int MinHeightOfFruiting => 5;
        public override int CountOfFruitsPerTick => 2;
        public override int MaxSeedSprayingDistance => 30;
        public override float HeightGrowthPerTick => 0.4f;
        public override float RadiusGrowthPerTick => 0.15f;
        public override float CrownDensity => 0.8f;
        public override float GrowthStopShadowImpact => 0.8f;

        public override Tree GenerateKidTree(float x, float y, float height, float radius)
        {
            return new Beech(x, y, height, radius);
        }
    }
}