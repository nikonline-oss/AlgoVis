﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using testing.Models.Core;
using testing.Models.DataStructures;
using testing.Models.Visualization;

namespace testing.Algorithms.Core
{
    public abstract class BaseAlgorithm<TStructure, TState> : IAlgorithm<TStructure, TState>
        where TStructure : IDataStructure<TState>
    {
        public abstract string Name { get; }

        protected List<VisualizationStep> Steps { get; } = new();
        protected AlgorithmStatistics Statistics { get; } = new();
        protected TStructure? CurrentStructure { get; private set; }

        public AlgorithmResult Execute(AlgorithmConfig config, TStructure structure)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Steps.Clear();
            Statistics.Reset();
            CurrentStructure = structure;

            AddStep("start", "Начало выполнения алгоритма", structure);

            ExecuteAlgorithm(config, structure);

            AddStep("complete", "Алгоритм завершён", structure);

            stopwatch.Stop();

            return new AlgorithmResult
            {
                AlgorithmName = Name,
                SessionId = config.SessionId,
                StructureType = structure.Type,
                Steps = new List<VisualizationStep>(Steps),
                Statistics = Statistics.Clone(),
                ExecutionTime = stopwatch.Elapsed,
                OutputData = GetOutputData(structure)
            };
        }

        protected abstract void ExecuteAlgorithm(AlgorithmConfig config, TStructure structure);
        protected abstract Dictionary<string, object> GetOutputData(TStructure structure);

        protected void AddStep(string operation, string description, TStructure structure,
            List<HighlightedElement>? highlights = null, List<Connection>? connections = null)
        {
            var step = new VisualizationStep
            {
                StepNumber = Steps.Count + 1,
                Operation = operation,
                Description = description,
                VisualizationData = structure.ToVisualizationData()
            };

            if (highlights != null)
            {
                step.VisualizationData.Highlights.AddRange(highlights);
            }

            if (connections != null)
            {
                step.VisualizationData.Connections.AddRange(connections);
            }

            Steps.Add(step);
            Statistics.Steps++;
        }

        protected void RecordComparison() => Statistics.Comparisons++;
        protected void RecordSwap() => Statistics.Swaps++;
        protected void RecordRecursiveCall() => Statistics.RecursiveCalls++;
        protected void RecordMemoryOperation() => Statistics.MemoryOperations++;
    }
}
