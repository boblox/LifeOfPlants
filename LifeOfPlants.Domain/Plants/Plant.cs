using System;

namespace LifeOfPlants.Domain.Plants
{
    public abstract class Plant
    {
        public Guid Id { get; }
        public float X { get; }
        public float Y { get; }
        public int Age { get; protected set; }
        public abstract string PlantType { get; }

        protected Plant(float x, float y)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            Age = 0;
        }

        public float DistanceTo(float x, float y)
        {
            return (float)Math.Sqrt(Math.Pow(X - x, 2) + Math.Pow(Y - y, 2));
        }
    }
}
