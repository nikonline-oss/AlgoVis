using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using testing.models.enums;
using testing.Interfaces;
using testing.models;
using System.Data;


namespace testing.Algorithms
{
    internal class BubbleSort : IVisualizationAlgorithm
    {
        public string Name { get; set; } = "bubblesort";
        public StructureType _structureType { get; } = StructureType.array;

        public AlgorithmResult Execute(List<SortingStep> _steps, AlgorithmConfig config)
        {
            bool trackSwaps = config.Parameters?.ContainsKey("TrackSwaps") == true
                && (bool)config.Parameters["TrackSwaps"];

            var data = config.IsArgs
                    ? config.Args
                    : GenerateRandomArray(config.Length);

            var original = (int[])data.Clone();

            var stats = new AlgorithmStatistics();
            bool detailed = config.Parameters.ContainsKey("Detailed") && (bool)config.Parameters["Detailed"];

            _steps.Add(new SortingStep
            {
                StepNumber = 1,
                ArrayStep = (int[])data.Clone(),
                Operation = "start",
                Description = "начало сортировки пузырьком"
            });

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data.Length - 1; j++)
                {
                    stats.Comparisons++;
                    _steps.Add(new SortingStep
                    {
                        StepNumber = _steps.Count + 1,
                        ArrayStep = (int[])data.Clone(),
                        Comparing = new[] { j, j + 1 },
                        Operation = "compare",
                        Description = $"Сравнение [{j}]={data[j]} и [{j + 1}]={data[j + 1]}",
                        Metadata = new Dictionary<string, object>() { ["iteration"] = i + 1 }
                    });

                    //тут остановился

                    if (data[j] > data[j + 1])
                    {
                        (data[j], data[j + 1]) = (data[j + 1], data[j]);
                        stats.Swaps++;
                        _steps.Add(new SortingStep
                        {
                            ArrayStep = (int[])data.Clone(),
                            StepNumber = _steps.Count + 1,
                            Swapping = new[] { j, j + 1 },
                            Operation = "swap",
                            Description = $"Обмен [{j}] и [{j + 1}]."
                        });

                    }
                }
                if (detailed)
                {
                    _steps.Add(new SortingStep
                    {
                        ArrayStep = (int[])data.Clone(),
                        StepNumber = _steps.Count,
                        Sorted = Enumerable.Range(data.Length - i - 1, i + 1).ToArray(),
                        Operation = "mark-sorted",
                        Description = $"Завершина итерация {i + 1}"
                    });
                }
            }

            _steps.Add(new SortingStep
            {
                ArrayStep = (int[])data.Clone(),
                StepNumber = _steps.Count,
                Sorted = Enumerable.Range(0, data.Length).ToArray(),
                Operation = "complete",
                Description = "Сортировка завершена"
            });

            stats.Steps = _steps.Count;
            stats.TimeComplexity = Math.Pow(data.Length, 2);
            stats.SpaceComplexity = data.Length;

            return new AlgorithmResult
            {
                AlgorithmName = Name,
                SessionId = config.SessionId,
                Steps = _steps,
                Statistics = stats,
                OriginArray = original,
                SortedArray = data
            };
        }

        private static int[] GenerateRandomArray(int length)
        {
            var random = new Random();
            return Enumerable.Range(0, length)
                .Select(_ => random.Next(0, 1000))
                .ToArray();
        }
    }
}
