using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Steps
{
    /// <summary>
    /// Ведет историю выполнения шагов алгоритма.
    /// </summary>
    /// <remarks>
    /// Отслеживает количество выполнений каждого шага для предотвращения бесконечных циклов
    /// и ограничения максимального количества выполнений.
    /// </remarks>
    public class StepExecutionHistory
    {
        /// <summary>
        /// Словарь для хранения счетчиков выполнения шагов.
        /// Ключ - идентификатор шага, значение - количество выполнений.
        /// </summary>
        private readonly Dictionary<string, int> _stepExecutionCount = new Dictionary<string, int>();

        /// <summary>
        /// Максимально допустимое количество выполнений одного шага.
        /// </summary>
        private const int MAX_STEP_EXECUTIONS = 1000;

        /// <summary>
        /// Регистрирует выполнение шага.
        /// </summary>
        /// <param name="stepId">Идентификатор выполненного шага.</param>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="stepId"/> равен null или пустой строке.
        /// </exception>
        public void RecordStep(string stepId)
        {
            if (!_stepExecutionCount.ContainsKey(stepId))
            {
                _stepExecutionCount[stepId] = 0;
            }
            _stepExecutionCount[stepId]++;
        }

        /// <summary>
        /// Получает количество выполнений указанного шага.
        /// </summary>
        /// <param name="stepId">Идентификатор шага.</param>
        /// <returns>Количество выполнений шага. Если шаг не выполнялся, возвращает 0.</returns>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="stepId"/> равен null или пустой строке.
        /// </exception>
        public int GetStepCount(string stepId)
        {
            return _stepExecutionCount.ContainsKey(stepId) ? _stepExecutionCount[stepId] : 0;
        }

        /// <summary>
        /// Проверяет, превышено ли максимальное количество выполнений для указанного шага.
        /// </summary>
        /// <param name="stepId">Идентификатор шага для проверки.</param>
        /// <returns>
        /// true, если количество выполнений шага превышает <see cref="MAX_STEP_EXECUTIONS"/>,
        /// иначе false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается, если <paramref name="stepId"/> равен null или пустой строке.
        /// </exception>
        public bool HasExceededLimit(string stepId)
        {
            return GetStepCount(stepId) > MAX_STEP_EXECUTIONS;
        }

        /// <summary>
        /// Сбрасывает историю выполнения всех шагов.
        /// </summary>
        public void Reset()
        {
            _stepExecutionCount.Clear();
        }
    }
}
