using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.Domain
{
    public class ShadowImpactCalculator
    {
        private static double GetAngleBetweenAAndBInTriangle(double a, double b, double c)
        {
            return Math.Acos((Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2)) / (2 * a * b));
        }

        private static double GetAngleBetweenVectors((double x, double y) first, (double x, double y) second)
        {
            var dotProduct = first.x * second.x + first.y * second.y;
            var firstLength = Math.Sqrt(first.x * first.x + first.y * first.y);
            var secondLength = Math.Sqrt(second.x * second.x + second.y * second.y);
            return Math.Acos(dotProduct / (firstLength * secondLength));
        }

        private static double GetAngleBetweenXAxeAndVectorFrom0To2Pi((double x, double y) vector)
        {
            var angle = GetAngleBetweenVectors((vector.x, vector.y), (1, 0));
            return vector.y >= 0 ? angle : 2 * Math.PI - angle;
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

        public float GetNormalizedImpact(Plant affectedPlant, List<Plant> allPlants)
        {
            var impactEdges = new List<ImpactEdge>();
            const float TwoPi = 2 * (float)Math.PI;

            if (affectedPlant is Tree affectedTree)
            {
                foreach (var affectingTree in allPlants.Except(new[] { affectedTree }).OfType<Tree>())
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
                        var halfOfAngleToAffectingTreeFromAffected = GetAngleBetweenAAndBInTriangle(distanceBetweenTrees, distanceToSideOfAffectingTree, affectingTree.Radius);
                        var angleToAffectingTreeFromXAxe = GetAngleBetweenXAxeAndVectorFrom0To2Pi((affectingTree.X - affectedTree.X, affectingTree.Y - affectedTree.Y));
                        var startAngle = angleToAffectingTreeFromXAxe - halfOfAngleToAffectingTreeFromAffected;
                        var endAngle = angleToAffectingTreeFromXAxe + halfOfAngleToAffectingTreeFromAffected;
                        var impactPerRadian = GetShadeImpact(affectedTree, affectingTree) / TwoPi;

                        var id = Guid.NewGuid();
                        var id2 = id;
                        impactEdges.Add(new ImpactEdge(id, (float)startAngle, IntervalEdgeType.Start, impactPerRadian));
                        if (endAngle > TwoPi)
                        {
                            id2 = Guid.NewGuid();
                            impactEdges.Add(new ImpactEdge(id, TwoPi, IntervalEdgeType.End, impactPerRadian));
                            impactEdges.Add(new ImpactEdge(id2, 0, IntervalEdgeType.Start, impactPerRadian));
                            endAngle -= TwoPi;
                        }
                        impactEdges.Add(new ImpactEdge(id2, (float)endAngle, IntervalEdgeType.End, impactPerRadian));
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
            foreach (var impactEdge in impactEdges)
            {
                if (!impactStack.Any()) impactStack.Add(impactEdge);
                else
                {
                    totalImpact += impactStack.Max(i => i.ImpactPerRadian) * (impactEdge.Angle - impactStack[impactStack.Count - 1].Angle);
                    if (impactEdge.IntervalEdgeType == IntervalEdgeType.Start)
                    {
                        impactStack.Add(impactEdge);
                    }
                    else
                    {
                        impactStack.RemoveAll(i => i.Id == impactEdge.Id);
                    }
                }
            }
            return totalImpact;
        }
    }
}
