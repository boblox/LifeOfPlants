using System;
using System.Collections.Generic;
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
            var simulator = new Simulator(15, 15);
            new List<Plant>
            {
                new Birch(0, 0, 1, 1),
                new Beech(10, 10, 15, 5),
                new Beech(-10, 10, 15, 5),
                new Beech(10, -10, 15, 5),
                new Beech(-10, -10, 15, 5)
            }.ForEach(plant => simulator.TryToAddPlant(plant));

            while (true)
            {
                await Task.Delay(tickGap);
                simulator.Tick();
                Console.WriteLine("New tick");
                foreach (var plant in simulator.Plants)
                {
                    Console.WriteLine(plant.ToString());
                }
            }
        }
    }
}
