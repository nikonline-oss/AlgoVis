using System.Text;
using AlgoVis.Parser.Model;
using AlgoVis.Parser.Parsing;

namespace AlgoVis.Parser.Mapping;

/// <summary>
/// Преобразует CST tree-sitter (TsNode) в удобную модель SyntaxTree.
/// </summary>
public static class AstMapper
{
    public static SyntaxTree ToModel(TsNode root, string fileName, string source)
    {
        var tree = new SyntaxTree { FileName = fileName, SourceCode = source };
        var ctx = new Ctx(source);

        if (root.Type != "module")
            throw new InvalidOperationException($"Expected 'module' root, got '{root.Type}'");

        foreach (var child in root.NamedChildren())
            MapTopLevel(child, tree, ctx);

        return tree;
    }

    // ───────── Top-level ─────────
    private static void MapTopLevel(TsNode node, SyntaxTree tree, Ctx ctx)
    {
        switch (node.Type)
        {
            case "import_statement":
            case "import_from_statement":
                var imp = MapImport(node, ctx);
                if (imp != null) { imp.Parent = tree; tree.Imports.Add(imp); }
                break;

            case "function_definition":
                tree.TopLevelFunctions.Add(MapFunction(node, tree, ctx, new List<string>()));
                break;

            case "class_definition":
                tree.Classes.Add(MapClass(node, tree, ctx, new List<string>()));
                break;

            case "decorated_definition":
                MapDecorated(node, tree, ctx);
                break;

            case "expression_statement":
                var assign = node.NamedChildren().FirstOrDefault(c => c.Type == "assignment");
                if (assign.IsValid && assign.Type == "assignment")
                {
                    var field = MapAssignmentAsField(assign, ctx);
                    if (field != null) { field.Parent = tree; tree.GlobalVariables.Add(field); }
                }
                break;
        }
    }

    private static void MapDecorated(TsNode decorated, SyntaxTree tree, Ctx ctx)
    {
        var decorators = decorated.NamedChildren()
            .Where(n => n.Type == "decorator")
            .Select(d => DecoratorName(d, ctx))
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        var inner = decorated.NamedChildren()
            .FirstOrDefault(n => n.Type is "function_definition" or "class_definition");

        if (!inner.IsValid) return;

        switch (inner.Type)
        {
            case "function_definition":
                tree.TopLevelFunctions.Add(MapFunction(inner, tree, ctx, decorators));
                break;
            case "class_definition":
                tree.Classes.Add(MapClass(inner, tree, ctx, decorators));
                break;
        }
    }

    private static string DecoratorName(TsNode decorator, Ctx ctx)
    {
        // decorator → @, identifier | attribute | call
        var inner = decorator.NamedChildren().FirstOrDefault();
        return inner.Type == "" ? "" : ctx.GetText(inner);
    }

    // ───────── Imports ─────────
    private static ImportNode? MapImport(TsNode node, Ctx ctx)
    {
        if (node.Type == "import_statement")
        {
            var dotted = node.NamedChildren().FirstOrDefault(c => c.Type == "dotted_name");
            if (dotted.Type == "") return null;
            return new ImportNode
            {
                Source = ctx.GetText(dotted),
                Span = ctx.Span(node)
            };
        }

        // import_from_statement
        var children = node.NamedChildren().ToList();
        var module = children.FirstOrDefault(c => c.Type == "dotted_name");
        var names = children.Where(c => c.Type == "dotted_name").Skip(1)
            .Select(c => ctx.GetText(c)).ToList();

        return new ImportNode
        {
            Source = module.Type == "" ? "" : ctx.GetText(module),
            ImportedNames = names,
            IsFromImport = true,
            Span = ctx.Span(node)
        };
    }

    // ───────── Classes ─────────
    private static ClassNode MapClass(TsNode node, AstNode parent, Ctx ctx, List<string> decorators)
    {
        var children = node.NamedChildren().ToList();

        var nameNode = children.FirstOrDefault(c => c.Type == "identifier");
        var argList = children.FirstOrDefault(c => c.Type == "argument_list");
        var block = children.FirstOrDefault(c => c.Type == "block");


        var cls = new ClassNode
        {
            Name = nameNode.Type == "" ? "<anonymous>" : ctx.GetText(nameNode),
            Annotations = decorators,
            Span = ctx.Span(node)
        };
        cls.Parent = parent;

        if (argList.Type == "argument_list")
        {
            cls.Bases.AddRange(argList.NamedChildren()
                .Select(b => ctx.GetText(b))
                .Where(s => !string.IsNullOrEmpty(s)));
        }

        if (block.IsValid && block.Type == "block")
        {
            MapClassBody(block, cls, ctx);
        }
        else
        {
            Console.Error.WriteLine($"[MapClass] SKIPPED MapClassBody");
        }

        return cls;
    }

    private static void MapClassBody(TsNode block, ClassNode cls, Ctx ctx)
    {
        foreach (var stmt in block.NamedChildren())
        {
            switch (stmt.Type)
            {
                case "function_definition":
                    cls.Methods.Add(MapFunction(stmt, cls, ctx, new List<string>()));
                    break;

                case "decorated_definition":
                    var decorators = stmt.NamedChildren()
                        .Where(n => n.Type == "decorator")
                        .Select(d => DecoratorName(d, ctx))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                    var fn = stmt.NamedChildren().FirstOrDefault(n => n.Type == "function_definition");
                    if (fn.Type == "function_definition")
                        cls.Methods.Add(MapFunction(fn, cls, ctx, decorators));
                    break;

                case "class_definition":
                    cls.InnerClasses.Add(MapClass(stmt, cls, ctx, new List<string>()));
                    break;

                case "expression_statement":
                    var assign = stmt.NamedChildren().FirstOrDefault(c => c.Type == "assignment");
                    if (assign.IsValid && assign.Type == "assignment")
                    {
                        var field = MapAssignmentAsField(assign, ctx);
                        if (field != null) { field.Parent = cls; cls.Fields.Add(field); }
                    }
                    else
                    {
                        var single = stmt.NamedChildren().FirstOrDefault();
                        if (single.IsValid && single.Type == "identifier")
                        {
                            var f = new FieldNode { Name = ctx.GetText(single), Span = ctx.Span(stmt) };
                            f.Parent = cls;
                            cls.Fields.Add(f);
                        }
                    }
                    break;
            }
        }
    }

    // ───────── Functions ─────────
    private static FunctionNode MapFunction(TsNode node, AstNode parent, Ctx ctx, List<string> decorators)
    {
        var children = node.NamedChildren().ToList();
        var nameNode = children.FirstOrDefault(c => c.Type == "identifier");
        var paramsNode = children.FirstOrDefault(c => c.Type == "parameters");
        var returnType = children.FirstOrDefault(c => c.Type == "type");
        var block = children.FirstOrDefault(c => c.Type == "block");

        var fn = new FunctionNode
        {
            Name = nameNode.Type == "" ? "<anonymous>" : ctx.GetText(nameNode),
            Annotations = decorators,
            ReturnType = returnType.Type == "type" ? ctx.GetText(returnType) : null,
            Span = ctx.Span(node)
        };
        fn.Parent = parent;

        if (paramsNode.Type == "parameters")
            MapParameters(paramsNode, fn, ctx);

        if (block.Type == "block")
            CollectBodyInfo(block, fn, ctx);

        return fn;
    }

    private static void MapParameters(TsNode paramsNode, FunctionNode fn, Ctx ctx)
    {
        foreach (var p in paramsNode.NamedChildren())
        {
            switch (p.Type)
            {
                case "identifier":
                    {
                        var param = new ParameterNode { Name = ctx.GetText(p), Span = ctx.Span(p) };
                        param.Parent = fn;
                        fn.Parameters.Add(param);
                        break;
                    }

                case "typed_parameter":
                case "typed_default_parameter":
                    {
                        var name = p.NamedChildren().FirstOrDefault(c => c.Type == "identifier");
                        var type = p.NamedChildren().FirstOrDefault(c => c.Type == "type");
                        var def = p.NamedChildren().LastOrDefault(c =>
                            c.Type is "integer" or "string" or "float" or "true" or "false" or "none");
                        var param = new ParameterNode
                        {
                            Name = name.Type == "" ? "" : ctx.GetText(name),
                            Type = type.Type == "type" ? ctx.GetText(type) : null,
                            DefaultValue = def.Type == "" ? null : ctx.GetText(def),
                            Span = ctx.Span(p)
                        };
                        param.Parent = fn;
                        fn.Parameters.Add(param);
                        break;
                    }

                case "default_parameter":
                    {
                        var name = p.NamedChildren().FirstOrDefault();
                        var val = p.NamedChildren().LastOrDefault();
                        var param = new ParameterNode
                        {
                            Name = ctx.GetText(name),
                            DefaultValue = ctx.GetText(val),
                            Span = ctx.Span(p)
                        };
                        param.Parent = fn;
                        fn.Parameters.Add(param);
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// Рекурсивно обходит тело функции, собирая локальные переменные и вызовы.
    /// </summary>
    private static void CollectBodyInfo(TsNode block, FunctionNode fn, Ctx ctx)
    {
        foreach (var stmt in block.NamedChildren())
            CollectStatement(stmt, fn, ctx);
    }

    private static void CollectStatement(TsNode stmt, FunctionNode fn, Ctx ctx)
    {
        switch (stmt.Type)
        {
            case "function_definition":
                fn.InnerFunctions.Add(MapFunction(stmt, fn, ctx, new List<string>()));
                return;

            case "decorated_definition":
                var fn2 = stmt.NamedChildren().FirstOrDefault(n => n.Type == "function_definition");
                if (fn2.IsValid && fn2.Type == "function_definition")
                    fn.InnerFunctions.Add(MapFunction(fn2, fn, ctx, new List<string>()));
                return;

            case "expression_statement":
                {
                    var assign = stmt.NamedChildren().FirstOrDefault(c => c.Type == "assignment");
                    if (assign.IsValid && assign.Type == "assignment")
                    {
                        CollectAssignment(assign, fn, ctx);
                        foreach (var c in assign.NamedChildren())
                            CollectCalls(c, fn, ctx);
                    }
                    else
                    {
                        foreach (var inner in stmt.NamedChildren())
                            CollectCalls(inner, fn, ctx);
                    }
                    return;
                }

            case "return_statement":
            case "pass_statement":
            case "break_statement":
            case "continue_statement":
            case "raise_statement":
                {
                    foreach (var c in stmt.NamedChildren())
                        CollectCalls(c, fn, ctx);
                    return;
                }

            case "if_statement":
            case "elif_clause":
            case "else_clause":
            case "while_statement":
            case "for_statement":
            case "with_statement":
            case "try_statement":
            case "except_clause":
            case "finally_clause":
                {
                    foreach (var child in stmt.NamedChildren())
                    {
                        if (child.Type == "block")
                        {
                            foreach (var s in child.NamedChildren())
                                CollectStatement(s, fn, ctx);
                        }
                        else
                        {
                            CollectCalls(child, fn, ctx);
                        }
                    }
                    return;
                }

            case "block":
                foreach (var s in stmt.NamedChildren())
                    CollectStatement(s, fn, ctx);
                return;

            default:
                CollectCalls(stmt, fn, ctx);
                return;
        }
    }

    private static void CollectAssignment(TsNode assign, FunctionNode fn, Ctx ctx)
    {
        var children = assign.NamedChildren().ToList();
        if (children.Count < 2) return;

        var target = children[0];
        var value = children[children.Count - 1];

        if (target.Type == "identifier")
        {
            var lv = new LocalVariableNode
            {
                Name = ctx.GetText(target),
                Value = TrimString(GetShortValue(value, ctx)),
                Span = ctx.Span(assign)
            };
            lv.Parent = fn;
            fn.LocalVariables.Add(lv);
            return;
        }

        if (target.Type == "attribute")
            return;

    }

    /// <summary>
    /// Рекурсивно ищет все call-узлы и добавляет их в fn.Calls.
    /// </summary>
    private static void CollectCalls(TsNode node, FunctionNode fn, Ctx ctx)
    {
        if (node.Type == "call")
        {
            var call = MapCall(node, fn, ctx);
            if (call != null) fn.Calls.Add(call);
        }

        foreach (var child in node.NamedChildren())
            CollectCalls(child, fn, ctx);
    }

    private static CallNode? MapCall(TsNode call, FunctionNode fn, Ctx ctx)
    {
        var children = call.NamedChildren().ToList();
        if (children.Count == 0) return null;

        var callee = children[0];
        var argsNode = children.FirstOrDefault(c => c.Type == "argument_list");

        string name;
        string? receiver = null;
        if (callee.Type == "identifier")
        {
            name = ctx.GetText(callee);
        }
        else if (callee.Type == "attribute")
        {
            var attrChildren = callee.NamedChildren().ToList();
            if (attrChildren.Count < 2) return null;
            receiver = ctx.GetText(attrChildren[0]);
            name = ctx.GetText(attrChildren[1]);
        }
        else
        {
            return null;
        }

        var args = new List<string>();
        if (argsNode.Type == "argument_list")
            foreach (var a in argsNode.NamedChildren())
                args.Add(ctx.GetText(a));

        var cn = new CallNode
        {
            Name = name,
            Receiver = receiver,
            Arguments = args,
            Span = ctx.Span(call)
        };
        cn.Parent = fn;
        return cn;
    }

    // ───────── Fields ─────────
    private static FieldNode? MapAssignmentAsField(TsNode assign, Ctx ctx)
    {
        var children = assign.NamedChildren().ToList();
        if (children.Count < 2) return null;

        var target = children[0];
        var value = children[children.Count - 1];

        // Только target-identifier или self.X
        if (target.Type == "identifier")
        {
            return new FieldNode
            {
                Name = ctx.GetText(target),
                Value = TrimString(GetShortValue(value, ctx)),
                Span = ctx.Span(assign)
            };
        }

        if (target.Type == "attribute")
        {
            var attrChildren = target.NamedChildren().ToList();
            if (attrChildren.Count < 2) return null;
            return new FieldNode
            {
                Name = ctx.GetText(attrChildren[1]),
                Value = TrimString(GetShortValue(value, ctx)),
                Span = ctx.Span(assign)
            };
        }

        return null;
    }

    // ───────── Helpers ─────────
    private static string GetShortValue(TsNode value, Ctx ctx)
    {
        var text = ctx.GetText(value);
        return text.Length > 40 ? text[..40] + "…" : text;
    }

    private static string? TrimString(string s)
        => string.IsNullOrEmpty(s) ? null : s;

    /// <summary>Контекст: кэширует source-bytes и умеет вырезать текст узла.</summary>
    private sealed class Ctx
    {
        private readonly byte[] _bytes;
        public Ctx(string source) => _bytes = Encoding.UTF8.GetBytes(source);

        public string GetText(TsNode node)
        {
            var s = (int)node.StartByte;
            var e = (int)node.EndByte;
            if (s < 0 || e <= s || e > _bytes.Length) return "";
            return Encoding.UTF8.GetString(_bytes, s, e - s);
        }

        public CodeSpan Span(TsNode node)
        {
            var sp = node.StartPoint;
            var ep = node.EndPoint;
            return new CodeSpan(
                new CodePoint((int)sp.Row, (int)sp.Col),
                new CodePoint((int)ep.Row, (int)ep.Col));
        }
    }
}