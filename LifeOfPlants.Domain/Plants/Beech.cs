namespace LifeOfPlants.Domain.Plants
{
    public class Beech : Tree
    {
        public Beech(float x, float y, float height = 0, float radius = 0) : base(x, y, height, radius)
        {
        }

        public override string PlantType => "Beech";
        public override float MaxHeight => 40;
        public override float MaxRadius => 6;
        public override int MaxAge => 200;
        public override float HeightGrowthPerTick => 0.5f;
        public override float RadiusGrowthPerTick => 0.2f;
        public override float CrownDensity => 0.8f;
        public override float GrowthStopShadowImpact => 0.8f;
    }
}