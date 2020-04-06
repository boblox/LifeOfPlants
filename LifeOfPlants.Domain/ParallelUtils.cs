using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LifeOfPlants.Domain
{
    public static class ParallelUtils
    {
        public static List<Task<TResult>> RunInParallelBatches<TResult>(Func<int, int, TResult> func, int itemsCount, int batchesCount)
        {
            var tasks = new List<Task<TResult>>();
            if (itemsCount >= batchesCount)
            {
                var itemsPerBatch = itemsCount / batchesCount;
                for (var i = 0; i < batchesCount; i++)
                {
                    int startIndex, endIndex;
                    if (i != batchesCount - 1)
                    {
                        startIndex = i * itemsPerBatch;
                        endIndex = (i + 1) * itemsPerBatch - 1;
                    }
                    else
                    {
                        startIndex = i * itemsPerBatch;
                        endIndex = itemsCount - 1;
                    }
                    tasks.Add(Task.Run(() => func(startIndex, endIndex)));
                }
            }
            else
            {
                tasks.Add(Task.FromResult(func(0, itemsCount - 1)));
            }

            return tasks;
        }
    }
}
