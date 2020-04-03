using System;
using System.Linq;

namespace LifeOfPlants.Domain.Plants
{
    public abstract class Tree : Plant
    {
        public float Height { get; protected set; }
        public float Radius { get; protected set; }
        public abstract float MaxHeight { get; }
        public abstract float MaxRadius { get; }
        public abstract int MaxAge { get; }
        public abstract float HeightGrowthPerTick { get; }
        public abstract float RadiusGrowthPerTick { get; }
        public abstract float CrownDensity { get; }
        public abstract float GrowthStopShadowImpact { get; }
        public bool IsDead => Age >= MaxAge;

        protected Tree(float x, float y, float height = 0, float radius = 0) : base(x, y)
        {
            Height = height;
            Radius = radius;
            if (Radius > MaxRadius) throw new ArgumentException("Radius cant be bigger then MaxRadius");
            if (Height > MaxHeight) throw new ArgumentException("Height Cant be bigger then MaxHeight");
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
            //var radiusGrowthShortageRatio = maxPossibleRadiusGrowth / RadiusGrowthPerTick; //TODO: review ratio!
            var maxPossibleHeightGrowth = new[] { HeightGrowthPerTick /*/ radiusGrowthShortageRatio*/, MaxHeight - Height }.Min();

            var slowdownRatio = (GrowthStopShadowImpact - normalizedShadowImpact) / GrowthStopShadowImpact;
            var radiusGrowth = slowdownRatio * maxPossibleRadiusGrowth;
            var heightGrowth = slowdownRatio * maxPossibleHeightGrowth;
            return new TreeGrowth(radiusGrowth, heightGrowth);
        }

        public override string ToString()
        {
            return $"{PlantType}: Height={Height:N}, Radius={Radius:N}, Age={Age}";
        }
    }
}