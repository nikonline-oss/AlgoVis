
using testing.Models.Custom;

namespace testing.Services
{
    public partial class CustomAlgorithmInterpreter
    {
        // ������ ��� ������ � ���������: �����, �������, ����� ���� � �������
        // ExecuteFunctionCall, ExecuteReturn, GetStepAndFunction, GetNextStep
        private void ExecuteFunctionCall(AlgorithmStep step)
        {
            if (_currentCallDepth >= MAX_CALL_DEPTH)
                throw new InvalidOperationException($"��������� ������������ ������� �������: {MAX_CALL_DEPTH}");

            if (!_functions.ContainsKey(step.functionName))
                throw new ArgumentException($"������� '{step.functionName}' �� �������");

            var function = _functions[step.functionName];
            _statistics.RecursiveCalls++;
            _currentCallDepth++;

            // ������� ����� ������� ��������� ��� �������
            var localVariables = new Dictionary<string, object>();
            _variableScopes.Push(localVariables);

            // ��������� �������� ������
            var context = new FunctionContext
            {
                callerStepId = step.returnToStep,
                depth = _currentCallDepth,
                functionName = step.functionName
            };
            _callStack.Push(context);

            // �������������� ��������� �������
            foreach (var param in step.functionParameters)
            {
                try
                {
                    var value = EvaluateExpression(param.Value);
                    localVariables[param.Key] = value;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"������ ������������� ��������� {param.Key}: {ex.Message}");
                }
            }


            var description = step.description ?? $"����� �������: {step.functionName}";
            AddVisualizationStep("call_function", description, metadata: new Dictionary<string, object>
            {
                ["call_depth"] = _currentCallDepth,
                ["function_name"] = step.functionName,
                ["parameters"] = step.functionParameters
            });

            ExecuteStep(function.entryPoint);
        }
        private void ExecuteReturn(AlgorithmStep step)
        {
            if (_callStack.Count == 0)
            {
                return;
            }

            var context = _callStack.Pop();
            _currentCallDepth--;

            // ������� ������� ��������� ������� (���� ��� �� ���������� �������)
            if (_variableScopes.Count > 1)
            {
                _variableScopes.Pop();
            }

            var description = step.description ?? $"������� �� �������";
            AddVisualizationStep("return", description, metadata: new Dictionary<string, object>
            {
                ["call_depth"] = _currentCallDepth,
                ["function_name"] = context.functionName
            });

            // ������������ � ���� ����� ������
            if (!string.IsNullOrEmpty(context.callerStepId))
            {
                ExecuteStep(context.callerStepId);
            }
        }
        private (AlgorithmStep step, FunctionGroup function) GetStepAndFunction(string stepId)
        {
            // ������� ���� � �������� ���������
            if (_steps.ContainsKey(stepId))
            {
                return (_steps[stepId], null);
            }

            // ����� ���� � ��������
            foreach (var function in _functions.Values)
            {
                var step = function.steps.FirstOrDefault(s => s.id == stepId);
                if (step != null)
                {
                    return (step, function);
                }
            }

            return (null, null);
        }
        private string GetNextStep(string currentStepId, FunctionGroup function)
        {
            var steps = function?.steps ?? _request.steps;
            var stepIds = steps.Select(s => s.id).ToList();
            var currentIndex = stepIds.IndexOf(currentStepId);

            return currentIndex >= 0 && currentIndex < stepIds.Count - 1 ?
                stepIds[currentIndex + 1] : null;
        }
        private void ExecuteCondition(AlgorithmStep step, FunctionGroup function)
        {
            var conditionResult = EvaluateCondition(step.parameters.FirstOrDefault() ?? "");
            var description = step.description ?? $"�������� �������: {step.parameters.FirstOrDefault()}";

            AddVisualizationStep("condition", description, metadata: new Dictionary<string, object>
            {
                ["condition"] = step.parameters.FirstOrDefault(),
                ["result"] = conditionResult
            });

            var nextStep = conditionResult ?
                step.conditionCases.FirstOrDefault(c => c.condition == "true")?.nextStep :
                step.conditionCases.FirstOrDefault(c => c.condition == "false")?.nextStep;

            if (!string.IsNullOrEmpty(nextStep))
            {
                ExecuteStep(nextStep);
            }
        }

    }
}