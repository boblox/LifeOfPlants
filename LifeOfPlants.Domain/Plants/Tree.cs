using System;
using System.Linq;

namespace LifeOfPlants.Domain.Plants
{
    public abstract class Tree : Plant
    {
        public float Height { get; protected set; }
        public const float DefaultStartHeight = 1f;
        public float Radius { get; protected set; }
        public const float DefaultStartRadius = 1f;
        public abstract float DefaultMaxHeight { get; }
        public abstract int MaxHeightVariation { get; }
        public float MaxHeight { get; }
        public abstract float MaxRadius { get; }
        public abstract int DefaultMaxAge { get; }
        public abstract int MaxAgeVariation { get; }
        public int MaxAge { get; }
        public abstract int MinAgeOfFruiting { get; }
        public abstract int MinHeightOfFruiting { get; }
        public abstract int CountOfFruitsPerTick { get; }
        public abstract int MaxSeedSprayingDistance { get; }
        public abstract float HeightGrowthPerTick { get; }
        public abstract float RadiusGrowthPerTick { get; }
        public abstract float CrownDensity { get; }
        public abstract float GrowthStopShadowImpact { get; }
        public bool IsDead => Age >= MaxAge;
        public bool CanBearFruits => Age >= MinAgeOfFruiting && Height >= MinHeightOfFruiting;

        protected Tree(float x, float y, float height, float radius) : base(x, y)
        {
            Height = height;
            Radius = radius;
            MaxAge = DefaultMaxAge + (new Random().Next(2 * MaxAgeVariation) - MaxAgeVariation);
            MaxHeight = DefaultMaxHeight + (new Random().Next(2 * MaxHeightVariation) - MaxHeightVariation);
            if (Radius > MaxRadius) throw new ArgumentException("Radius cant be bigger then MaxRadius");
            if (Height > MaxHeight) throw new ArgumentException("Height Cant be bigger then DefaultMaxHeight");
        }

        public void RaiseAge(int ageChange)
        {
            Age += ageChange;
        }

        public void RaiseRadius(float radiusChange)
        {
            Radius = Math.Min(MaxRadius, Radius + radiusChange);
        }

        public void RaiseHeight(float heightChange)
        {
            Height = Math.Min(MaxHeight, Height + heightChange);
        }

        public virtual TreeGrowth GetMaximumPossibleRaiseAfterTick(float normalizedShadowImpact, float maxAllowedRadiusGrowth)
        {
            if (normalizedShadowImpact > GrowthStopShadowImpact) return new TreeGrowth(0, 0);

            var maxPossibleRadiusGrowth = new[] { maxAllowedRadiusGrowth, RadiusGrowthPerTick, MaxRadius - Radius }.Min();
            var radiusGrowthShortageRatio = 1 - maxPossibleRadiusGrowth / RadiusGrowthPerTick; //TODO: review ratio!
            var maxPossibleHeightGrowth = new[] { HeightGrowthPerTick + 0.25f * HeightGrowthPerTick * radiusGrowthShortageRatio, MaxHeight - Height }.Min();
            //var maxPossibleHeightGrowth = new[] { HeightGrowthPerTick, DefaultMaxHeight - Height }.Min();

            var slowdownRatio = (GrowthStopShadowImpact - normalizedShadowImpact) / GrowthStopShadowImpact;
            var radiusGrowth = slowdownRatio * maxPossibleRadiusGrowth;
            var heightGrowth = slowdownRatio * maxPossibleHeightGrowth;
            return new TreeGrowth(radiusGrowth, heightGrowth);
        }

        public abstract Tree GenerateKidTree(float x, float y, float height, float radius);

        public override string ToString()
        {
            return $"{PlantType}: Height={Height:N}, Radius={Radius:N}, Age={Age}";
        }
    }
}