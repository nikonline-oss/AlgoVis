// Вставьте/замените эти методы внутри класса AlgorithmInterpreter

private object EvaluateExpression(string expression, ExecutionContext context)
{
    if (string.IsNullOrWhiteSpace(expression))
        return 0;

    // Сначала подставляем доступы к главной структуре
    var preprocessed = ReplaceStructAccess(expression, context).Trim();

    // Явный null
    if (string.Equals(preprocessed, "null", StringComparison.OrdinalIgnoreCase))
        return null;

    // Передаём в основной парсер/эвальюатор
    return _expressionEvaluator.Evaluate(preprocessed, context.Variables);
}

private bool EvaluateCondition(string condition, ExecutionContext context)
{
    if (string.IsNullOrWhiteSpace(condition))
        return false;

    // Подставляем доступы к структуре
    var cond = ReplaceStructAccess(condition, context).Trim();

    // Обработка сравнения с null: "expr == null" или "expr != null"
    var nullMatch = Regex.Match(cond, @"^(.*?)(==|!=)\s*null\s*$", RegexOptions.IgnoreCase);
    if (nullMatch.Success)
    {
        var leftExpr = nullMatch.Groups[1].Value.Trim();
        var op = nullMatch.Groups[2].Value;
        var leftVal = EvaluateExpression(leftExpr, context);
        var isNull = leftVal == null;
        return op == "==" ? isNull : !isNull;
    }

    // Попробуем использовать специализированную проверку у expression evaluator
    try
    {
        return _expressionEvaluator.EvaluateCondition(cond, context.Variables);
    }
    catch
    {
        // Fallback: вычислим как выражение и проверим не-нулевость/логичность
        var val = EvaluateExpression(cond, context);
        if (val == null) return false;
        if (val is bool b) return b;
        try
        {
            var d = Convert.ToDouble(val);
            return Math.Abs(d) > 0.000001;
        }
        catch
        {
            // Любые нечисловые значения считаются true, если не пустые строки
            var s = val.ToString();
            return !string.IsNullOrEmpty(s);
        }
    }
}

/// <summary>
/// Заменяет в выражении обращения вида struct[expr] и struct.prop на литералы значений из контекста.Structure.
/// Возвращает новую строку выражения.
/// </summary>
private string ReplaceStructAccess(string expression, ExecutionContext context)
{
    if (string.IsNullOrWhiteSpace(expression) || context?.Structure == null)
        return expression ?? string.Empty;

    string result = expression;

    // Обработка struct[index] — индекс может быть выражением
    var arrayPattern = new Regex(@"struct\[(.*?)\]");
    result = arrayPattern.Replace(result, match =>
    {
        var innerExpr = match.Groups[1].Value;
        // вычисляем индекс (используем рекурсивный вызов EvaluateExpression — он сам выполнит ReplaceStructAccess)
        object idxObj;
        try
        {
            idxObj = _expressionEvaluator.Evaluate(ReplaceStructAccess(innerExpr, context), context.Variables);
        }
        catch
        {
            try { idxObj = EvaluateExpression(innerExpr, context); }
            catch { return "null"; }
        }

        if (idxObj == null) return "null";
        int idx;
        try { idx = Convert.ToInt32(idxObj); }
        catch { return "null"; }

        var val = GetStructValue(context.Structure.GetState(), idx.ToString(), idx);
        return ToLiteral(val);
    });

    // Обработка struct.prop
    var propPattern = new Regex(@"struct\.([A-Za-z_][A-Za-z0-9_]*)");
    result = propPattern.Replace(result, match =>
    {
        var propName = match.Groups[1].Value;
        var val = GetStructValue(context.Structure.GetState(), propName, null);
        return ToLiteral(val);
    });

    return result;
}

/// <summary>
/// Получает значение из состояния структуры по ключу или индексу.
/// Поддерживает массивы, IList, IDictionary и объекты через рефлексию.
/// </summary>
private object? GetStructValue(object? state, string keyOrName, int? index)
{
    if (state == null) return null;

    // Массив / IList по индексу
    if (index.HasValue)
    {
        if (state is Array arr)
        {
            if (index.Value >= 0 && index.Value < arr.Length) return arr.GetValue(index.Value);
            return null;
        }
        if (state is System.Collections.IList list)
        {
            if (index.Value >= 0 && index.Value < list.Count) return list[index.Value];
            return null;
        }
    }

    // IDictionary (ключи как строки)
    if (state is System.Collections.IDictionary dict)
    {
        if (dict.Contains(keyOrName)) return dict[keyOrName];
        // Попробуем ключ как числовой индекс
        if (int.TryParse(keyOrName, out var kidx) && dict.Contains(kidx)) return dict[kidx];
    }

    // Попробуем через свойства/поля объекта
    var t = state.GetType();
    var pi = t.GetProperty(keyOrName);
    if (pi != null) return pi.GetValue(state);
    var fi = t.GetField(keyOrName);
    if (fi != null) return fi.GetValue(state);

    // Если state сам — IDictionary<string, object> (частый случай), пробуем привести
    if (state is System.Collections.Generic.IDictionary<string, object> sd)
    {
        if (sd.ContainsKey(keyOrName)) return sd[keyOrName];
    }

    return null;
}

/// <summary>
/// Конвертирует значение в литерал для подстановки в строковое выражение:
/// - null => "null"
/// - строки => " \"text\" " (экранирование)
/// - bool => "true"/"false"
/// - числа => invariant culture
/// </summary>
private string ToLiteral(object? value)
{
    if (value == null) return "null";

    if (value is string s)
    {
        // эскейп кавычек
        var esc = s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return $"\"{esc}\"";
    }

    if (value is bool b) return b ? "true" : "false";

    if (value is IFormattable form)
    {
        return form.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
    }

    // Для прочих объектов — пытаемся сериализовать в JSON-подобный литерал (упрощённо)
    try
    {
        return System.Text.Json.JsonSerializer.Serialize(value);
    }
    catch
    {
        return $"\"{value.ToString()?.Replace("\"", "\\\"")}\"";
    }
}