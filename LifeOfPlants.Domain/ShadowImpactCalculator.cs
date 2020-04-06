using System;
using System.Collections.Generic;
using System.Linq;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.Domain
{
    public class ShadowImpactCalculator
    {
        public float GetNormalizedImpact(Plant affectedPlant, List<Plant> nearbyPlants)
        {
            var impactEdges = new List<ImpactEdge>();
            const float TwoPi = 2 * (float)Math.PI;

            if (affectedPlant is Tree affectedTree)
            {
                foreach (var affectingTree in nearbyPlants.OfType<Tree>())
                {
                    var distanceBetweenTrees = affectedPlant.DistanceTo(affectingTree.X, affectingTree.Y);
                    if (distanceBetweenTrees >= affectingTree.Height) continue;

                    var distanceToSideOfAffectingTree = Math.Sqrt(Math.Pow(distanceBetweenTrees, 2) - Math.Pow(affectingTree.Radius, 2));
                    if (distanceBetweenTrees <= affectingTree.Radius)
                    {
                        //Check what to do! We totally covered center of affected tree!
                    }
                    else
                    {
                        var halfOfAngleToAffectingTreeFromAffected = MathUtils.GetAngleBetweenAAndBInTriangle(distanceBetweenTrees, distanceToSideOfAffectingTree, affectingTree.Radius);
                        var angleToAffectingTreeFromXAxe = MathUtils.GetAngleBetweenXAxeAndVectorFrom0To2Pi((affectingTree.X - affectedTree.X, affectingTree.Y - affectedTree.Y));
                        var startAngle = (float) (angleToAffectingTreeFromXAxe - halfOfAngleToAffectingTreeFromAffected);
                        var endAngle = (float) (angleToAffectingTreeFromXAxe + halfOfAngleToAffectingTreeFromAffected);
                        var impactPerRadian = GetShadeImpact(affectedTree, affectingTree) / TwoPi;

                        var id = Guid.NewGuid();
                        if (startAngle < 0)
                        {
                            impactEdges.Add(new ImpactEdge(id, startAngle + TwoPi, IntervalEdgeType.Start, impactPerRadian));
                            impactEdges.Add(new ImpactEdge(id, TwoPi, IntervalEdgeType.End, impactPerRadian));
                            var id2 = Guid.NewGuid();
                            impactEdges.Add(new ImpactEdge(id2, 0, IntervalEdgeType.Start, impactPerRadian));
                            impactEdges.Add(new ImpactEdge(id2, endAngle, IntervalEdgeType.End, impactPerRadian));
                        }
                        else if (endAngle > TwoPi)
                        {
                            impactEdges.Add(new ImpactEdge(id, startAngle, IntervalEdgeType.Start, impactPerRadian));
                            impactEdges.Add(new ImpactEdge(id, TwoPi, IntervalEdgeType.End, impactPerRadian));
                            var id2 = Guid.NewGuid();
                            impactEdges.Add(new ImpactEdge(id2, 0, IntervalEdgeType.Start, impactPerRadian));
                            impactEdges.Add(new ImpactEdge(id2, endAngle - TwoPi, IntervalEdgeType.End, impactPerRadian));
                        }
                        else
                        {
                            impactEdges.Add(new ImpactEdge(id, startAngle, IntervalEdgeType.Start, impactPerRadian));
                            impactEdges.Add(new ImpactEdge(id, endAngle, IntervalEdgeType.End, impactPerRadian));
                        }
                    }
                }

            }
            return GetTotalImpact(impactEdges);
        }

        private float GetShadeImpact(Tree affectedTree, Tree affectingTree)
        {
            var distance = affectedTree.DistanceTo(affectingTree.X, affectingTree.Y);
            var affectingTreeHeight = affectingTree.Height;
            var affectedTreeHeight = affectedTree.Height;
            float shadeCoverage;
            if (affectedTreeHeight <= affectingTreeHeight - distance)
            {
                shadeCoverage = 1;
            }
            else
            {
                shadeCoverage = (affectingTreeHeight - distance) / affectedTreeHeight;
            }
            return shadeCoverage * affectingTree.CrownDensity;
        }

        private float GetTotalImpact(List<ImpactEdge> impactEdges)
        {
            impactEdges = impactEdges.OrderBy(i => i.Angle).ThenBy(i => i.IntervalEdgeType).ToList();
            var impactStack = new List<ImpactEdge>();
            var totalImpact = 0f;
            ImpactEdge previousImpactEdge = default;
            foreach (var impactEdge in impactEdges)
            {
                if (!impactStack.Any()) impactStack.Add(impactEdge);
                else
                {
                    totalImpact += impactStack.Max(i => i.ImpactPerRadian) * (impactEdge.Angle - previousImpactEdge.Angle);
                    if (impactEdge.IntervalEdgeType == IntervalEdgeType.Start)
                    {
                        impactStack.Add(impactEdge);
                    }
                    else
                    {
                        impactStack.RemoveAll(i => i.Id == impactEdge.Id);
                    }
                }

                previousImpactEdge = impactEdge;
            }
            return totalImpact;
        }

        private enum IntervalEdgeType
        {
            Start = 0,
            End = 1
        }

        private struct ImpactEdge
        {
            public Guid Id { get; }
            public float Angle { get; }
            public IntervalEdgeType IntervalEdgeType { get; }
            public float ImpactPerRadian { get; }

            public ImpactEdge(Guid id, float angle, IntervalEdgeType intervalEdgeType, float impactPerRadian)
            {
                Id = id;
                Angle = angle;
                IntervalEdgeType = intervalEdgeType;
                ImpactPerRadian = impactPerRadian;
            }
        }
    }
}
