using System;
using System.Collections.Generic;
using System.Text;

namespace LifeOfPlants.Domain
{
    public static class MathUtils
    {
        public static double GetAngleBetweenAAndBInTriangle(double a, double b, double c)
        {
            return Math.Acos((Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2)) / (2 * a * b));
        }

        public static double GetAngleBetweenVectors((double x, double y) first, (double x, double y) second)
        {
            var dotProduct = first.x * second.x + first.y * second.y;
            var firstLength = Math.Sqrt(first.x * first.x + first.y * first.y);
            var secondLength = Math.Sqrt(second.x * second.x + second.y * second.y);
            return Math.Acos(dotProduct / (firstLength * secondLength));
        }

        public static double GetAngleBetweenXAxeAndVectorFrom0To2Pi((double x, double y) vector)
        {
            var angle = GetAngleBetweenVectors((vector.x, vector.y), (1, 0));
            return vector.y >= 0 ? angle : 2 * Math.PI - angle;
        }
    }
}
