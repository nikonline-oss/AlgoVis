using testing.models.enums;

namespace testing.models
{
    public class AlgorithmConfig
    {
        public string Name { get; set; } = string.Empty;
        public int Length { get; set; } = 10;
        public bool IsArgs { get; set; } = false;
        public int[] Args {  get; set; } = Array.Empty<int>();
        public string SessionId { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } // доп параметры
    }
    public class AlgorithmResult<T>
    {
        public string AlgorithmName { get; set; } = string.Empty;
        public string SessionId {  set; get; } = string.Empty;
        public List<SortingStep> Steps { get; set; } = new();
        public AlgorithmStatistics Statistics { get; set; } = new();
        public TimeSpan ExecutionTime { get; set; }
        public Dictionary<string, T> Parameters { get; set; } = new(); // сюда заносят данные характерные для определённой структуры в данном случае массивы
        //public int[] OriginArray { get; set; } = Array.Empty<int>();
        //public int[] SortedArray { get; set; } = Array.Empty<int>();
    }
    public class SortingStep
    {
        public int StepNumber { get; set; }
        public string Operation { get; set; } = string.Empty;
        public int[] ArrayStep { get; set; } = Array.Empty<int>();
        public int[]? Comparing { get; set; }
        public int[]? Swapping { get; set; }
        public int[]? Sorted { get; set; }
        public int? PivotIndex { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class AlgorithmStatistics // статистика за алгоритм
    {
        public int Comparisons { get; set; } // количество сравнений
        public int Swaps { get; set; } // количество замен
        public int Steps { get; set; } // количество шагов
        public int RecursiveCalls { get; set; } // рекурсивных вызовов
        public int MemoryOperations { get; set; } // операций с памятью
        public double TimeComplexity { get; set; } 
        public double SpaceComplexity { get; set; }
    }
    public class OverallStatistics // статистика за все алгоритмы
    {
        public int TotalAlgorithms { get; set; }
        public int TotalSteps { get; set; }
        public int TotalComparisons { get; set; }
        public int TotalSwaps { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public Dictionary<string, int> AlgorithmDistribution { get; set; } = new();
        
    }

    ///тестовые структуры
    ///
    public class TestResult
    {
        public string Description { get; set; } = string.Empty;
        public int[] OriginalArray { get; set; } = Array.Empty<int>();
        public int[] SortedArray { get; set; } = Array.Empty<int>();
        public List<SortingStep> Steps { get; set; } = new List<SortingStep>();
        public int Comparisons { get; set; }
        public int Swaps { get; set; }
        public int TotalSteps { get; set; }
        public int ArraySize { get; set; }
        public double ExecutionTimeMs { get; set; }
        public double Efficiency { get; set; }
    }

    //public class SortingStep
    //{
    //    public int StepNumber { get; set; }
    //    public int[] Array { get; set; } = Array.Empty<int>();
    //    public int[]? Comparing { get; set; }
    //    public int[]? Swapping { get; set; }
    //    public string Description { get; set; } = string.Empty;
    //}

    public class TestConfig
    {
        public string Description { get; set; } = "BubbleSort Test Configuration";
        public bool DetailedLogging { get; set; } = true;
        public List<TestCase> TestCases { get; set; } = new List<TestCase>();
    }

    public class TestCase
    {
        public string Description { get; set; } = string.Empty;
        public int ArraySize { get; set; } = 10;
        public int[]? InputArray { get; set; }
        public int MinValue { get; set; } = 1;
        public int MaxValue { get; set; } = 100;
    }

}
