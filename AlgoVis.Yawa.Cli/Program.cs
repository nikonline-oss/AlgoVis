using System.Text.Json;
using AlgoVis.Yawa.Yawa;
using AlgoVis.Yawa.Runtime;
using AlgoVis.Yawa.Yawa.Loader;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;
if (args.Length < 1)
{
    PrintHelp();
    return 1;
}

var cmd = args[0];
return cmd switch
{
    "load" => RunLoad(args),
    "run" => RunExec(args),
    "inspect" => RunInspect(args),
    "from-python" => RunFromPython(args),
    "from-cpp" => RunFromCpp(args),
    "-h" or "--help" or "help" => PrintHelpAndOk(),
    _ => Unknown(cmd)
};

static int PrintHelpAndOk() { PrintHelp(); return 0; }

static int Unknown(string cmd)
{
    Console.Error.WriteLine($"Unknown command: {cmd}");
    PrintHelp();
    return 1;
}

static void PrintHelp()
{
    Console.WriteLine("""
    algovis-yawa — загрузчик и интерпретатор YAWA.

    Команды:
      load <file.yawa.json>       Проверить, что файл корректно парсится
      run  <file.yawa.json>       Выполнить программу и вывести шаги
           [--out=file.json]      Сохранить полный trace в файл
           [--limit=N]            Переопределить max_steps
           [--verbose]            Печатать каждый шаг
      inspect <trace.json>        Разобрать и показать сводку trace
           [--head=N]             Сколько шагов печатать (по умолчанию 20)
           [--steps=lo-hi]        Печатать диапазон шагов
    """);
}

static int RunFromPython(string[] args)
{
    if (args.Length < 2) { Console.Error.WriteLine("Usage: from-python <file.py> [--out=<file.yawa.json>] [--entry=name]"); return 1; }
    var path = args[1];
    if (!File.Exists(path)) { Console.Error.WriteLine($"File not found: {path}"); return 1; }

    string? outPath = null;
    string? entry = null;
    for (int i = 2; i < args.Length; i++)
    {
        var a = args[i];
        if (a.StartsWith("--out=")) outPath = a[6..];
        else if (a.StartsWith("--entry=")) entry = a[8..];
    }

    try
    {
        var source = File.ReadAllText(path);
        var transpiler = new AlgoVis.Transpiler.PythonToYawa(source);
        var program = transpiler.Transpile(entry);

        var json = System.Text.Json.JsonSerializer.Serialize(program,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        if (outPath is not null)
        {
            var dir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(outPath, json, System.Text.Encoding.UTF8);
            Console.WriteLine($"💾 YAWA written to {outPath} ({json.Length} bytes)");
        }
        else
        {
            Console.WriteLine(json);
        }

        if (transpiler.Warnings.Count > 0)
        {
            Console.Error.WriteLine("⚠ Предупреждения:");
            foreach (var w in transpiler.Warnings)
                Console.Error.WriteLine("   " + w);
        }
        return 0;
    }
    catch (AlgoVis.Transpiler.UnsupportedFeatureException ex)
    {
        Console.Error.WriteLine($"❌ {ex.Message}");
        return 2;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"❌ {ex.GetType().Name}: {ex.Message}");
        return 2;
    }
}

static int RunFromCpp(string[] args)
{
    if (args.Length < 2) { Console.Error.WriteLine("Usage: from-cpp <file.cpp> [--out=<file.yawa.json>]"); return 1; }
    var path = args[1];
    if (!File.Exists(path)) { Console.Error.WriteLine($"File not found: {path}"); return 1; }

    string? outPath = null;
    for (int i = 2; i < args.Length; i++)
    {
        if (args[i].StartsWith("--out=")) outPath = args[i][6..];
    }

    try
    {
        var source = File.ReadAllText(path);
        var transpiler = new AlgoVis.Transpiler.Cpp.CppToYawa(source);
        var program = transpiler.Transpile();

        var json = System.Text.Json.JsonSerializer.Serialize(program,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        if (outPath is not null)
        {
            var dir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(outPath, json, System.Text.Encoding.UTF8);
            Console.WriteLine($"💾 YAWA written to {outPath} ({json.Length} bytes)");
        }
        else
        {
            Console.WriteLine(json);
        }

        if (transpiler.Warnings.Count > 0)
        {
            Console.Error.WriteLine("⚠ Предупреждения:");
            foreach (var w in transpiler.Warnings)
                Console.Error.WriteLine("   " + w);
        }
        return 0;
    }
    catch (AlgoVis.Transpiler.Cpp.UnsupportedFeatureException ex)
    {
        Console.Error.WriteLine($"❌ {ex.Message}");
        return 2;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"❌ {ex.GetType().Name}: {ex.Message}");
        return 2;
    }
}

static int RunLoad(string[] args)
{
    if (args.Length < 2) { Console.Error.WriteLine("Usage: load <file>"); return 1; }
    var path = args[1];
    if (!File.Exists(path)) { Console.Error.WriteLine($"File not found: {path}"); return 1; }

    try
    {
        var program = YawaLoader.FromFile(path);
        Console.WriteLine($"✅ Loaded YAWA: {program.Metadata.Name ?? "<unnamed>"}");
        Console.WriteLine($"   yawa_version: {program.YawaVersion}");
        Console.WriteLine($"   functions:    {program.Functions.Count}");
        foreach (var f in program.Functions)
            Console.WriteLine($"     - {f.Name}({string.Join(", ", f.Params.Select(p => p.Name))}) body={f.Body.Count} stmts");
        Console.WriteLine($"   entry:        {program.Entry.Function} args={program.Entry.Args.Count}");
        Console.WriteLine($"   limits:       max_steps={program.Limits.MaxSteps}, max_depth={program.Limits.MaxDepth}, snapshot_every={program.Limits.SnapshotEvery}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"❌ Load failed: {ex.Message}");
        return 2;
    }
}

static int RunExec(string[] args)
{
    if (args.Length < 2) { Console.Error.WriteLine("Usage: run <file> [--out=file] [--verbose] [--limit=N]"); return 1; }
    var path = args[1];
    if (!File.Exists(path)) { Console.Error.WriteLine($"File not found: {path}"); return 1; }

    string? outPath = null;
    bool verbose = false;
    int? limit = null;
    for (int i = 2; i < args.Length; i++)
    {
        var a = args[i];
        if (a.StartsWith("--out=")) outPath = a[6..];
        else if (a == "--verbose") verbose = true;
        else if (a.StartsWith("--limit=")) limit = int.Parse(a[8..]);
    }

    YawaProgram program;
    try { program = YawaLoader.FromFile(path); }
    catch (Exception ex) { Console.Error.WriteLine($"❌ Load failed: {ex.Message}"); return 2; }

    var opts = new InterpreterOptions
    {
        MaxSteps = limit ?? program.Limits.MaxSteps,
        MaxDepth = program.Limits.MaxDepth,
        MaxSeconds = program.Limits.MaxSeconds,
        SnapshotEvery = program.Limits.SnapshotEvery
    };

    var interp = new Interpreter(program, opts);
    var session = interp.Run();

    Console.WriteLine($"🚀 Program: {program.Metadata.Name}");
    Console.WriteLine($"   session_id:   {session.SessionId}");
    Console.WriteLine($"   total_steps:  {session.Statistics.TotalSteps}");
    Console.WriteLine($"   comparisons:  {session.Statistics.Comparisons}");
    Console.WriteLine($"   swaps:        {session.Statistics.Swaps}");
    Console.WriteLine($"   memory:       {session.Statistics.MemoryAccesses}");

    if (session.Statistics.UserCounters.Count > 0)
    {
        Console.WriteLine("   user counters:");
        foreach (var (k, v) in session.Statistics.UserCounters)
            Console.WriteLine($"     {k} = {v}");
    }

    if (session.Statistics.StructureSizes.Count > 0)
    {
        Console.WriteLine("   structure sizes:");
        foreach (var (k, v) in session.Statistics.StructureSizes)
            Console.WriteLine($"     {k} = {v}");
    }

    Console.WriteLine();
    Console.WriteLine("📊 Final state:");
    foreach (var (k, v) in session.FinalState)
        Console.WriteLine($"   {k} = {JsonSerializer.Serialize(v)}");

    if (verbose)
    {
        Console.WriteLine();
        Console.WriteLine("📜 Steps:");
        foreach (var s in session.Steps)
        {
            var hl = s.Highlight.Count > 0 ? $" HL=[{string.Join(",", s.Highlight)}]" : "";
            var an = s.Annotation is not null ? $" // {s.Annotation}" : "";
            var df = s.Diff.Count > 0
                ? " " + string.Join(" ", s.Diff.Select(d =>
                    $"{d.Target}:{JsonShort(d.Old)}→{JsonShort(d.New)}"))
                : "";
            Console.WriteLine($"   #{s.N,-4} {s.Kind,-10}{hl}{an}{df}");
        }
    }

    if (outPath is not null)
    {
        var dir = Path.GetDirectoryName(outPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outPath, json, System.Text.Encoding.UTF8);
        Console.WriteLine();
        Console.WriteLine($"💾 Trace written to {outPath} ({json.Length} bytes)");
    }

    // Если в trace есть шаг kind="error" — программа упала в момент выполнения.
    var errorStep = session.Steps.FirstOrDefault(s => s.Kind == "error");
    if (errorStep is not null)
    {
        Console.Error.WriteLine($"❌ RUNTIME: {errorStep.Annotation}");
        return 2;
    }

    return 0;
}

static int RunInspect(string[] args)
{
    if (args.Length < 2) { Console.Error.WriteLine("Usage: inspect <trace.json> [--head=N] [--steps=lo-hi]"); return 1; }
    var path = args[1];
    if (!File.Exists(path)) { Console.Error.WriteLine($"File not found: {path}"); return 1; }

    int headN = 20;
    int? stepFrom = null, stepTo = null;
    for (int i = 2; i < args.Length; i++)
    {
        var a = args[i];
        if (a.StartsWith("--head=")) headN = int.Parse(a[7..]);
        else if (a.StartsWith("--steps="))
        {
            var parts = a[8..].Split('-');
            stepFrom = int.Parse(parts[0]);
            stepTo = parts.Length > 1 ? int.Parse(parts[1]) : stepFrom;
        }
    }

    var json = File.ReadAllText(path);
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    Console.WriteLine($"📄 Trace: {path}");
    Console.WriteLine($"   size: {json.Length} bytes");
    Console.WriteLine($"   session_id: {GetStr(root, "session_id")}");
    Console.WriteLine($"   yawa_version: {GetStr(root, "yawa_version")}");

    if (root.TryGetProperty("statistics", out var stats))
    {
        Console.WriteLine();
        Console.WriteLine("📊 Statistics:");
        Console.WriteLine($"   total_steps:      {GetInt(stats, "total_steps")}");
        Console.WriteLine($"   comparisons:      {GetInt(stats, "comparisons")}");
        Console.WriteLine($"   swaps:            {GetInt(stats, "swaps")}");
        Console.WriteLine($"   memory_accesses:  {GetInt(stats, "memory_accesses")}");

        if (stats.TryGetProperty("user_counters", out var uc))
        {
            Console.WriteLine("   user counters:");
            foreach (var p in uc.EnumerateObject())
                Console.WriteLine($"     {p.Name} = {p.Value}");
        }

        if (stats.TryGetProperty("structure_sizes", out var ss))
        {
            Console.WriteLine("   structure sizes:");
            foreach (var p in ss.EnumerateObject())
                Console.WriteLine($"     {p.Name} = {p.Value}");
        }
    }

    if (root.TryGetProperty("final_state", out var fs))
    {
        Console.WriteLine();
        Console.WriteLine("🎯 Final state:");
        foreach (var p in fs.EnumerateObject())
        {
            var raw = p.Value.GetRawText();
            if (raw.Length > 200) raw = raw[..200] + "...";
            Console.WriteLine($"   {p.Name} = {raw}");
        }
    }

    if (root.TryGetProperty("structure", out var structure) &&
        structure.TryGetProperty("variables", out var vars))
    {
        Console.WriteLine();
        Console.WriteLine("🏗 Structure descriptors:");
        foreach (var v in vars.EnumerateArray())
        {
            var name = GetStr(v, "name");
            var type = GetStr(v, "type");
            var vis = GetStr(v, "visual");
            Console.WriteLine($"   {name,-20} {type,-20} visual={vis}");
        }
    }

    if (root.TryGetProperty("steps", out var steps) && steps.ValueKind == JsonValueKind.Array)
    {
        var total = steps.GetArrayLength();
        Console.WriteLine();
        Console.WriteLine($"📜 Steps (total {total}):");

        var from = stepFrom ?? 0;
        var to = stepTo ?? Math.Min(total - 1, from + headN - 1);

        foreach (var step in steps.EnumerateArray())
        {
            var n = GetInt(step, "n");
            if (n < from || n > to) continue;

            var kind = GetStr(step, "kind");
            var an = step.TryGetProperty("annotation", out var av) && av.ValueKind == JsonValueKind.String
                ? $"  // {av.GetString()}"
                : "";

            var hl = "";
            if (step.TryGetProperty("highlight", out var hlv) && hlv.ValueKind == JsonValueKind.Array && hlv.GetArrayLength() > 0)
            {
                var items = hlv.EnumerateArray().Select(x => x.GetString()).Where(s => s != null);
                hl = $"  HL=[{string.Join(",", items)}]";
            }

            var df = "";
            if (step.TryGetProperty("diff", out var dfv) && dfv.ValueKind == JsonValueKind.Array && dfv.GetArrayLength() > 0)
            {
                var pairs = dfv.EnumerateArray().Select(d =>
                {
                    var t = GetStr(d, "target");
                    var o = d.TryGetProperty("old", out var ov) ? ov.GetRawText() : "null";
                    var nw = d.TryGetProperty("new", out var nv) ? nv.GetRawText() : "null";
                    return $"{t}:{o}→{nw}";
                });
                df = "  " + string.Join(" ", pairs);
            }

            var snap = step.TryGetProperty("snapshot", out var snv) && snv.ValueKind == JsonValueKind.Object
                ? "  [snapshot]"
                : "";

            Console.WriteLine($"   #{n,-4} {kind,-10}{hl}{an}{snap}{df}");
        }
    }

    return 0;
}

static string GetStr(JsonElement el, string key)
    => el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String
        ? v.GetString() ?? ""
        : "";

static int GetInt(JsonElement el, string key)
    => el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.Number
        ? v.GetInt32()
        : 0;

static string JsonShort(object? v)
{
    if (v is null) return "null";
    try
    {
        var s = JsonSerializer.Serialize(v);
        return s.Length > 60 ? s[..57] + "..." : s;
    }
    catch
    {
        return v.ToString() ?? "?";
    }
}