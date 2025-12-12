using AlgoVis.Models.Models.Core;
using AlgoVis.Models.Models.DataStructures.Interfaces;
using AlgoVis.Models.Models.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Core.Core.Algorithms
{
    public abstract class BaseAlgorithm<TStructure, TState, TStep> : IAlgorithm<TStructure, TState, TStep>
       where TStructure : IDataStructure<TState>
       where TStep : IStep, new()
    {
        public abstract string Name { get; }

        protected List<TStep> Steps { get; } = new();
        protected AlgorithmStatistics Statistics { get; } = new();
        protected TStructure? CurrentStructure { get; private set; }

        public AlgorithmResult<TStep> Execute(AlgorithmConfig config, TStructure structure)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Steps.Clear();
            Statistics.Reset();
            CurrentStructure = structure;

            ExecuteAlgorithm(config, structure);

            stopwatch.Stop();

            return new AlgorithmResult<TStep>
            {
                AlgorithmName = Name,
                SessionId = config.SessionId,
                StructureType = structure.Type,
                steps = Steps,
                Statistics = Statistics.Clone(),
                ExecutionTime = stopwatch.Elapsed,
                OutputData = GetOutputData(structure)
            };
        }

        protected abstract void ExecuteAlgorithm(AlgorithmConfig config, TStructure structure);
        protected abstract Dictionary<string, object> GetOutputData(TStructure structure);

        // Универсальный метод для добавления шагов с базовыми свойствами
        protected TStep AddStep(string operation, string description, Dictionary<string, object>? metadata = null)
        {
            var step = new TStep
            {
                stepNumber = Steps.Count + 1,
                operation = operation,
                description = description,
                metadata = metadata ?? new Dictionary<string, object>()
            };

            Steps.Add(step);
            Statistics.Steps++;

            return step;
        }

        // Метод для добавления кастомных шагов с дополнительной логикой
        protected void AddCustomStep(Action<TStep> configureStep)
        {
            var step = new TStep
            {
                stepNumber = Steps.Count + 1
            };

            configureStep(step);
            Steps.Add(step);
            Statistics.Steps++;
        }

        // Обратная совместимость со старым кодом
        protected void AddLegacyStep(string operation, string description, TStructure structure,
            Dictionary<string, object>? metadata = null, List<HighlightedElement>? highlights = null,
            List<Connection>? connections = null)
        {
            var step = new VisualizationStep
            {
                stepNumber = Steps.Count + 1,
                operation = operation,
                description = description,
                //visualizationData = structure.ToVisualizationData()
            };

            if (highlights != null)
            {
                //step.visualizationData.highlights.AddRange(highlights);
            }

            if (connections != null)
            {
                //step.visualizationData.connections.AddRange(connections);
            }

            if (metadata != null)
                step.metadata = metadata;

            Steps.Add((TStep)(IStep)step);
            Statistics.Steps++;
        }
        protected void RawAddStep(TStep step)
        {
            step.stepNumber = Steps.Count + 1;
            Steps.Add(step);
        }

        protected void RecordComparison() => Statistics.Comparisons++;
        protected void RecordSwap() => Statistics.Swaps++;
        protected void RecordRecursiveCall() => Statistics.RecursiveCalls++;
        protected void RecordMemoryOperation() => Statistics.MemoryOperations++;
    }
}
