namespace LifeOfPlants.Domain.Plants
{
    public struct TreeGrowth
    {
        public float RadiusChange { get; }
        public float HeightChange { get; }

        public TreeGrowth(float radiusChange, float heightChange)
        {
            RadiusChange = radiusChange;
            HeightChange = heightChange;
        }
    }
}