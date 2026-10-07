namespace AlgoVis.Yawa.Runtime;

/// <summary>Базовое исключение для управления потоком.</summary>
public abstract class ControlFlowException : Exception { }

/// <summary>return из функции. Value — что вернули.</summary>
public sealed class ReturnException : ControlFlowException
{
    public RuntimeValue Value { get; }
    public ReturnException(RuntimeValue value) => Value = value;
}

/// <summary>break из цикла.</summary>
public sealed class BreakException : ControlFlowException { }

/// <summary>continue из цикла.</summary>
public sealed class ContinueException : ControlFlowException { }

/// <summary>Ошибка исполнения: превышение лимитов, деление на ноль и т.п.</summary>
public sealed class YawaRuntimeException : Exception
{
    public YawaRuntimeException(string message) : base(message) { }
}