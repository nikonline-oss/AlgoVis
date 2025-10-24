using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testing.Models.Steps
{
    public class StepExecutionHistory
    {
        private readonly Dictionary<string, int> _stepExecutionCount = new Dictionary<string, int>();
        private const int MAX_STEP_EXECUTIONS = 1000;

        public void RecordStep(string stepId)
        {
            if (!_stepExecutionCount.ContainsKey(stepId))
            {
                _stepExecutionCount[stepId] = 0;
            }
            _stepExecutionCount[stepId]++;
        }

        public int GetStepCount(string stepId)
        {
            return _stepExecutionCount.ContainsKey(stepId) ? _stepExecutionCount[stepId] : 0;
        }

        public bool HasExceededLimit(string stepId)
        {
            return GetStepCount(stepId) > MAX_STEP_EXECUTIONS;
        }

        public void Reset()
        {
            _stepExecutionCount.Clear();
        }
    }
}
