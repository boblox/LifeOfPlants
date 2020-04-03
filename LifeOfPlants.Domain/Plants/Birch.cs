namespace LifeOfPlants.Domain.Plants
{
    public class Birch : Tree
    {
        public Birch(float x, float y, float height = 0, float radius = 0) : base(x, y, height, radius)
        {
        }

        public override string PlantType => "Birch";
        public override float MaxHeight => 30;
        public override float MaxRadius => 4;
        public override int MaxAge => 100;
        public override float HeightGrowthPerTick => 1;
        public override float RadiusGrowthPerTick => 0.4f;
        public override float CrownDensity => 0.5f;
        public override float GrowthStopShadowImpact => 0.6f;
    }
}