using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LifeOfPlants.Domain;
using LifeOfPlants.Domain.Plants;

namespace LifeOfPlants.App
{
    class Program
    {
        public const int tickGap = 1000;

        static async Task Main(string[] args)
        {
            var plants = new List<Plant>
            {
                new Beech(0, 0, 1, 1),
                new Beech(10, 10, 15, 5),
                new Beech(-10, 10, 15, 5),
                new Beech(10, -10, 15, 5),
                new Beech(-10, -10, 15, 5),
                new Birch(10, 0, 15, 3)
            };
            var simulator = new Simulator(plants);

            while (true)
            {
                await Task.Delay(tickGap);
                simulator.Tick();
                Console.WriteLine("New tick");
                foreach (var plant in plants)
                {
                    Console.WriteLine(plant.ToString());
                }
            }
        }
    }
}
