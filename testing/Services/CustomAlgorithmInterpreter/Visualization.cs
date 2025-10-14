using testing.Models.Visualization;

namespace testing.Services
{
    public partial class CustomAlgorithmInterpreter
    {
        // Методы для добавления шагов визуализации
        // AddVisualizationStep, GetArrayState, UpdateArrayState
        private void AddVisualizationStep(string operation, string description,
            List<HighlightedElement> highlights = null, Dictionary<string, object> metadata = null)
        {
            _statistics.Steps++;

            var step = new VisualizationStep
            {
                stepNumber = _visualizationSteps.Count + 1,
                operation = operation,
                description = description,
                visualizationData = _structure.ToVisualizationData(),
                metadata = metadata ?? new Dictionary<string, object>()
            };

            if (highlights != null)
            {
                step.visualizationData.highlights.AddRange(highlights);
            }

            var arrayState = GetArrayState();
            step.metadata["array_state"] = arrayState;
            step.metadata["array_string"] = $"[{string.Join(", ", arrayState)}]";

            _visualizationSteps.Add(step);
        }
        private int[] GetArrayState()
        {
            var state = _structure.GetState();
            if (state is int[] array)
                return array;

            throw new InvalidOperationException("Кастомные алгоритмы поддерживают только массивы");
        }

        private void UpdateArrayState(int[] array)
        {
            _structure.ApplyState(array);
        }

    }
}