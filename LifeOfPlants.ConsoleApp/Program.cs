using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LifeOfPlants.Domain;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.App
{
    class Program
    {
        public const int tickGap = 100;

        static async Task Main(string[] args)
        {
            var simulator = new Simulator(100, 100, 8);
            var stopwatch = new Stopwatch();
            var timeValues = new List<double>();
            new List<Plant>
            {
                new Birch(0, 0, 1, 1),
                new Beech(10, 10, 15, 5),
                new Beech(-10, 10, 15, 5),
                new Beech(10, -10, 15, 5),
                new Beech(-10, -10, 15, 5)
            }.ForEach(plant => simulator.TryToAddPlants(new[] { plant }.ToList()));

            while (true)
            {
                //await Task.Delay(tickGap);
                stopwatch.Restart();
                await simulator.Tick();
                stopwatch.Stop();
                timeValues.Add(stopwatch.Elapsed.TotalMilliseconds);
                Console.WriteLine();
                Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds + "-time");
                Console.WriteLine((int)timeValues.Sum() / timeValues.Count + "-average");
                Console.WriteLine(simulator.Plants.Count + "-count");
                //Console.WriteLine("New tick");
                //foreach (var plant in simulator.Plants)
                //{
                //    Console.WriteLine(plant.ToString());
                //}
            }
        }
    }
}
