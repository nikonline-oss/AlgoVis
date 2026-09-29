using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Models.Models.Core;
using AlgoVis.Server.converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AlgoVis.Models.Models.Custom
{
    /// <summary>
    /// Запрос на выполнение пользовательского алгоритма.
    /// 
    /// Содержит всю необходимую информацию для определения и выполнения
    /// пользовательского алгоритма визуализации.
    /// </summary>
    public class CustomAlgorithmRequest
    {
        /// <summary>Название алгоритма</summary>
        public string name { get; set; } = string.Empty;

        /// <summary>Описание алгоритма и его назначения</summary>
        public string description { get; set; } = string.Empty;

        /// <summary>Тип структуры данных (по умолчанию "array")</summary>
        public string structureType { get; set; } = "array";

        /// <summary>Последовательность шагов алгоритма</summary>
        public List<AlgorithmStep> steps { get; set; } = new();

        /// <summary>Определения переменных, используемых в алгоритме</summary>
        public List<VariableDefinition> variables { get; set; } = new();

        /// <summary>Группы функций (подпрограммы) алгоритма</summary>
        public List<FunctionGroup> functions { get; set; } = new();
    }

    /// <summary>
    /// Группа шагов, объединенных в функцию (подпрограмму).
    /// 
    /// Позволяет создавать повторно используемые блоки кода
    /// и управлять потоком выполнения.
    /// </summary>
    public class FunctionGroup
    {
        /// <summary>Название функции</summary>
        public string name { get; set; } = string.Empty;

        /// <summary>Описание назначения функции</summary>
        public string description { get; set; } = string.Empty;

        /// <summary>Список параметров функции</summary>
        public List<string> parameters { get; set; } = new();

        /// <summary>Шаги, составляющие тело функции</summary>
        public List<AlgorithmStep> steps { get; set; } = new();

        /// <summary>Идентификатор начального шага функции (по умолчанию "start")</summary>
        public string entryPoint { get; set; } = "start";
    }

    /// <summary>
    /// Отдельный шаг алгоритма.
    /// 
    /// Представляет элементарную операцию или управляющую конструкцию
    /// в пользовательском алгоритме.
    /// </summary>
    public class AlgorithmStep
    {
        /// <summary>Уникальный идентификатор шага</summary>
        public string id { get; set; } = string.Empty;

        /// <summary>
        /// Тип шага: compare, swap, assign, loop, condition.
        /// 
        /// Определяет семантику выполняемой операции.
        /// </summary>
        public string type { get; set; } = string.Empty;

        /// <summary>Конкретная операция (например, "increment", "decrement")</summary>
        public string operation { get; set; } = string.Empty;

        /// <summary>Описание цели и действия шага</summary>
        public string description { get; set; } = string.Empty;

        /// <summary>Параметры операции (например, индексы элементов)</summary>
        public List<string> parameters { get; set; } = new();

        /// <summary>Идентификатор следующего шага после выполнения текущего</summary>
        public string nextStep { get; set; } = string.Empty;

        /// <summary>Название функции для вызова (если применимо)</summary>
        public string functionName { get; set; } = string.Empty;

        /// <summary>Параметры для передачи в вызываемую функцию</summary>
        public Dictionary<string, string> functionParameters { get; set; } = new();

        /// <summary>Шаг для возврата после выполнения функции или цикла</summary>
        public string returnToStep { get; set; } = string.Empty;

        /// <summary>Варианты условий для ветвления (для type="condition")</summary>
        public List<ConditionCase> conditionCases { get; set; } = new();

        /// <summary>Дополнительные метаданные шага</summary>
        public Dictionary<string, object> metadata { get; set; } = new();

        /// <summary>
        /// Флаг включения визуализации этого шага.
        /// 
        /// Используется FlexibleBoolConverter для десериализации.
        /// </summary>
        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool visualize { get; set; } = false;

        /// <summary>Список элементов для подсветки при визуализации</summary>
        public List<string> highlightElements { get; set; } = new();

        /// <summary>Цвет подсветки элементов (по умолчанию "yellow")</summary>
        public string highlightColor { get; set; } = "yellow";

        /// <summary>Тип визуализации (по умолчанию "default")</summary>
        public string visualizationType { get; set; } = "default";
    }

    /// <summary>
    /// Вариант условия в условной конструкции.
    /// 
    /// Определяет пару "условие → следующий шаг" для ветвления алгоритма.
    /// </summary>
    public class ConditionCase
    {
        /// <summary>Условие для проверки</summary>
        public string condition { get; set; } = string.Empty;

        /// <summary>Идентификатор шага для перехода при выполнении условия</summary>
        public string nextStep { get; set; } = string.Empty;
    }

    /// <summary>
    /// Результат выполнения пользовательского алгоритма.
    /// 
    /// Содержит информацию об успешности выполнения, результат алгоритма
    /// и состояние выполнения.
    /// </summary>
    public class CustomAlgorithmResult
    {
        /// <summary>Флаг успешного выполнения алгоритма</summary>
        public bool success { get; set; }

        /// <summary>Сообщение о результате выполнения (ошибка или информация)</summary>
        public string message { get; set; } = string.Empty;

        /// <summary>Результат выполнения алгоритма (выходные данные)</summary>
        public AlgorithmResult result { get; set; } = new();

        /// <summary>Состояние выполнения (значения переменных, состояние памяти)</summary>
        public Dictionary<string, object> executionState { get; set; } = new();
    }
}