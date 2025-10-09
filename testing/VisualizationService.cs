using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Algorithms;
using testing.DTO;
using testing.Interfaces;
using testing.models;
using testing.models.enums;

namespace testing
{
    internal class VisualizationService
    {
        private RegisterAlgo RegisterAlgo { get; set; }
        private BubbleSort BubbleSort { get; set; } = new BubbleSort();
        private IVisualizationAlgorithm _currentAlgo;

        private readonly List<SortingStep> _steps = new();
        private string _sessionId = string.Empty;
        private StructureType _structureType;

        public VisualizationService()
        {
            RegisterAlgo = new RegisterAlgo();

            RegisterAlgo.RegisterAlgorithm(BubbleSort);
        }

        public VisualizationResponse GenerateSteps(VisualizationRequest request)
        {
            var response = new VisualizationResponse();
            var overallStats = new OverallStatistics();
            var AlgorithmDistribution = new Dictionary<string, int>();

            var stopwatch = Stopwatch.StartNew();

            foreach (var algoConfig in request.Algorithms)
            {
                _steps.Clear(); 

                var algorithmStopwatch = Stopwatch.StartNew();
                _currentAlgo = RegisterAlgo.GetAlgorithm(algoConfig.Name.ToLower());

                algorithmStopwatch.Stop();
                var result = _currentAlgo.Execute(_steps, algoConfig);

                result.ExecutionTime = algorithmStopwatch.Elapsed;
                response.Results.Add(result);

                overallStats.TotalSteps += result.Statistics.Steps;
                overallStats.TotalComparisons += result.Statistics.Comparisons;
                overallStats.TotalSwaps += result.Statistics.Swaps;
                overallStats.TotalExecutionTime += result.ExecutionTime;

                if (AlgorithmDistribution.ContainsKey(algoConfig.Name.ToLower()))
                    AlgorithmDistribution[algoConfig.Name.ToLower()]++;
                else
                    AlgorithmDistribution[algoConfig.Name.ToLower()] = 0;

            }
            stopwatch.Stop();

            overallStats.TotalAlgorithms = request.Algorithms.Count;
            overallStats.AlgorithmDistribution = AlgorithmDistribution;
            response.overallStats = overallStats;
            response.Success = true;
            response.Message = $"Успешно выполнено {request.Algorithms.Count} алгоритмов";

            return response;
        }
    }
}
