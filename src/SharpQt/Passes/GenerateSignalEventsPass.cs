using System.Text;
using CppSharp;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Generators;
using CppSharp.Passes;

namespace SharpQt.Passes;

class GenerateSignalEventsPass : TranslationUnitPass
{
    bool _eventAdded;
    readonly HashSet<Event> _events = new();
    Generator _generator;

    public GenerateSignalEventsPass(Generator generator)
    {
        _generator = generator;
    }

    public override bool VisitTranslationUnit(TranslationUnit unit)
    {
        if (!_eventAdded)
        {
            _generator.OnUnitGenerated += OnUnitGenerated;
            _eventAdded = true;
        }

        return base.VisitTranslationUnit(unit);
    }

    private void OnUnitGenerated(GeneratorOutput generatorOutput)
    {
        GenerateSignalEvents(generatorOutput);
    }

    private void GenerateSignalEvents(GeneratorOutput generatorOutput)
    {
        foreach (
            var block in generatorOutput.Outputs.SelectMany(
                output => output.FindBlocks(BlockKind.Event)
            )
        )
        {
            if (block.Object is Event eventDecl && _events.Contains(eventDecl))
            {
                block.Text.StringBuilder.Clear();

                var fullNameBuilder = new StringBuilder("global::System.Action");
                var argNum = 1;
                foreach (var param in eventDecl.Parameters)
                {
                    argNum++;
                    if (argNum == 2)
                    {
                        fullNameBuilder.Append('<');
                    }

                    fullNameBuilder.Append(param.Type);
                    fullNameBuilder.Append(',');
                }

                if (fullNameBuilder[fullNameBuilder.Length - 1] == ',')
                {
                    fullNameBuilder[fullNameBuilder.Length - 1] = '>';
                }

                var signature =
                    $"{eventDecl.OriginalName}({string.Join(", ", eventDecl.Parameters.Select(GetOriginalParameterType))})";

                var classDecl = eventDecl.Namespace as Class;
                var existing = classDecl?.Events.FirstOrDefault(
                    e => e.OriginalName == eventDecl.OriginalName
                );

                if (existing != null && existing != eventDecl)
                {
                    if (eventDecl.Parameters.Count > 0)
                    {
                        eventDecl.Name += GetSignalEventSuffix(eventDecl);
                    }
                    else
                    {
                        existing.Name += GetSignalEventSuffix(eventDecl);
                    }
                }
                else if (
                    eventDecl.Parameters.Count > 0
                    && (
                        classDecl!.Methods.Any(
                            m => m.IsGenerated && m.OriginalName == eventDecl.OriginalName
                        )
                        || classDecl.Properties.Any(
                            p => p.IsGenerated && p.OriginalName == eventDecl.OriginalName
                        )
                    )
                )
                {
                    eventDecl.Name += GetSignalEventSuffix(eventDecl);
                }

                var finalName =
                    char.ToUpperInvariant(eventDecl.Name[0]) + eventDecl.Name.Substring(1);
                if (
                    eventDecl.Namespace.Declarations.Any(d => d != eventDecl && d.Name == finalName)
                )
                {
                    finalName += "Signal";
                }

                block.WriteLine(
                    $@"public event {fullNameBuilder} {finalName}
                    {{
                        add
                        {{
                            ConnectDynamicSlot(this, ""{signature}"", value);
                        }}
                        remove
                        {{
                            DisconnectDynamicSlot(this, ""{signature}"", value);
                        }}
                    }}"
                );
            }
        }

        var qtMetacallBlock = generatorOutput.Outputs
            .SelectMany(
                output =>
                    output
                        .FindBlocks(BlockKind.Method)
                        .Where(
                            block =>
                                block.Object is Declaration decl
                                && decl.Name == "QtMetacall"
                                && decl.Namespace.Name == "QObject"
                        )
            )
            .FirstOrDefault();

        qtMetacallBlock?.Text.StringBuilder.Replace(
            "return ___ret;",
            "return HandleQtMetacall(___ret, _0, _2);"
        );
    }

    private static string GetOriginalParameterType(ITypedDecl parameter)
    {
        return parameter.Type.Desugar().SkipPointerRefs().Desugar().TryGetClass(out var decl)
            ? decl.QualifiedOriginalName
            : parameter.Type.ToString();
    }

    public override bool VisitClassDecl(Class decl)
    {
        if (AlreadyVisited(decl))
        {
            return false;
        }

        foreach (
            var method in decl.Methods.Where(
                m =>
                    m.IsGenerated
                    || (
                        m.Parameters.Any()
                        && m.Parameters
                            .Last()
                            .Type.Desugar()
                            .TryGetDeclaration(out Declaration? decl)
                        && decl?.OriginalName == "QPrivateSignal"
                    )
            )
        )
        {
            HandleQSignal(decl, method);
        }

        var qtMetaCallDecl = decl.FindMethod("qt_metacall");
        if (qtMetaCallDecl != null)
        {
            Context.Options.ExplicitlyPatchedVirtualFunctions.Add(
                qtMetaCallDecl.QualifiedOriginalName
            );
        }

        return true;
    }

    private void HandleQSignal(Class classDecl, Method methodDecl)
    {
        for (int i = 0; i < classDecl.Specifiers.Count; i++)
        {
            var accessSpecifierDecl = classDecl.Specifiers[i];
            if (
                accessSpecifierDecl.DebugText == "Q_SIGNALS:"
                && accessSpecifierDecl.LineNumberStart < methodDecl.LineNumberStart
                && (
                    i == classDecl.Specifiers.Count - 1
                    || methodDecl.LineNumberEnd <= classDecl.Specifiers[i + 1].LineNumberStart
                )
            )
            {
                if (methodDecl.Parameters.Any())
                {
                    if (
                        methodDecl.Parameters.Last().Type.Desugar().TryGetClass(out var decl)
                        && decl.Name == "QPrivateSignal"
                    )
                    {
                        methodDecl.Parameters.RemoveAt(methodDecl.Parameters.Count - 1);
                    }
                }

                var eventDecl = new Event
                {
                    OriginalDeclaration = methodDecl,
                    Name = methodDecl.Name,
                    OriginalName = methodDecl.OriginalName,
                    Namespace = methodDecl.Namespace,
                    QualifiedType = new QualifiedType(methodDecl.FunctionType.Type)
                };

                eventDecl.Parameters.AddRange(
                    methodDecl.Parameters.Select(p => new Parameter(p) { Namespace = eventDecl })
                );

                if (methodDecl.IsGenerated)
                {
                    methodDecl.ExplicitlyIgnore();
                }

                classDecl.Declarations.Add(eventDecl);
                _events.Add(eventDecl);

                return;
            }
        }
    }

    private static string GetSignalEventSuffix(Event signalToUse)
    {
        var suffix = signalToUse.Parameters.Last().Name;
        var indexOfSpace = suffix.IndexOf(' ');
        if (indexOfSpace > 0)
        {
            suffix = suffix[..indexOfSpace];
        }

        if (suffix.StartsWith('_'))
        {
            var lastType = signalToUse.Parameters.Last().Type.ToString();
            suffix = lastType.Substring(lastType.LastIndexOf('.') + 1);

            return char.ToUpperInvariant(suffix[0]) + suffix.Substring(1);
        }
        else
        {
            var lastParamBuilder = new StringBuilder(suffix);
            while (!char.IsLetter(lastParamBuilder[0]))
            {
                lastParamBuilder.Remove(0, 1);
            }

            lastParamBuilder[0] = char.ToUpper(lastParamBuilder[0]);
            return lastParamBuilder.ToString();
        }
    }

    public static void ExtendQObject(Class decl, ITextGenerator generator)
    {
        generator.WriteLine(
            @"private struct Handler
            {
                public Handler(int signalId, Delegate @delegate)
                    : this()
                {
                    this.SignalId = signalId;
                    this.Delegate = @delegate;
                }

                public int SignalId { get; private set; }
                public Delegate Delegate { get; private set; }
            }"
        );

        generator.WriteLine(
            @"private readonly System.Collections.Generic.List<Handler> slots = new();"
        );

        generator.WriteLine(
            @"protected unsafe bool ConnectDynamicSlot(QObject sender, string signal, Delegate slot)
            {
                var normalizedSignature = System.Text.Encoding.UTF8.GetString(QMetaObject.NormalizedSignature(signal));
                int signalId = sender.MetaObject.IndexOfSignal(normalizedSignature);
                this.slots.Add(new Handler(signalId, slot));
                QMetaObject.Connection connection = QMetaObject.Connect(sender, signalId, this, this.slots.Count - 1 + MetaObject.MethodCount);

                return connection != null;
            }"
        );

        generator.WriteLine(
            @"protected bool DisconnectDynamicSlot(QObject sender, string signal, Delegate value)
            {
                int i = this.slots.FindIndex(h => h.Delegate == value);
                if (i >= 0)
                {
                    int signalId = this.slots[i].SignalId;
                    bool disconnect = QMetaObject.Disconnect(sender, signalId, this, i + MetaObject.MethodCount);
                    this.slots.RemoveAt(i);
                    return disconnect;
                }

                return false;
            }"
        );

        generator.WriteLine(
            @"protected int HandleQtMetacall(int index, QMetaObject.Call call, IntPtr* arguments)
            {
                if (index < 0 || call != QMetaObject.Call.InvokeMetaMethod)
                {
                    return index;
                }

                Handler handler = this.slots[index];
                var @params = handler.Delegate.Method.GetParameters();
                var parameters = new object[@params.Length];
                for (int i = 0; i < @params.Length; i++)
                {
                    System.Reflection.ParameterInfo parameter = @params[i];
                    var arg = new IntPtr(((void**)arguments)[1 + i]);
                    parameters[i] = GetParameterValue(handler, i, parameter, arg);
                }

                handler.Delegate.DynamicInvoke(parameters);
                return -1;
            }"
        );

        generator.WriteLine(
            @"private object GetParameterValue(Handler handler, int i, System.Reflection.ParameterInfo parameter, IntPtr arg)
            {
                if (arg == IntPtr.Zero)
                {
                    return null;
                }

                var type = parameter.ParameterType.IsEnum ? parameter.ParameterType.GetEnumUnderlyingType() : parameter.ParameterType;
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Empty:
                        return null;
                    case TypeCode.Object:
                        var constructor = type.GetMethod(""__CreateInstance"",
                                                         global::System.Reflection.BindingFlags.NonPublic |
                                                         global::System.Reflection.BindingFlags.Static |
                                                         global::System.Reflection.BindingFlags.FlattenHierarchy,
                                                         null, new[] { typeof(IntPtr), typeof(bool) }, null);
                        return constructor.Invoke(null, new object[] { arg, false });
                    case TypeCode.DBNull:
                        return DBNull.Value;
                    case TypeCode.Boolean:
                        return *(bool*) arg;
                    case TypeCode.Char:
                        return *(char*) arg;
                    case TypeCode.SByte:
                        return *(sbyte*) arg;
                    case TypeCode.Byte:
                        return *(byte*) arg;
                    case TypeCode.Int16:
                        return *(short*) arg;
                    case TypeCode.UInt16:
                        return *(ushort*) arg;
                    case TypeCode.Int32:
                        return *(int*) arg;
                    case TypeCode.UInt32:
                        return *(uint*) arg;
                    case TypeCode.Int64:
                        return *(long*) arg;
                    case TypeCode.UInt64:
                        return *(ulong*) arg;
                    case TypeCode.Single:
                        return *(float*) arg;
                    case TypeCode.Double:
                        return *(double*) arg;
                    case TypeCode.Decimal:
                        return *(decimal*) arg;
                    case TypeCode.DateTime:
                        return *(DateTime*) arg;
                    case TypeCode.String:
                        var metaMethod = this.Sender.MetaObject.Method(handler.SignalId);
                        if (metaMethod.ParameterType(i) == (int)QMetaType.Type.QString)
                        {
                            var __size = QString.__Internal.Size(arg);
                            var __constData = QString.__Internal.ConstData(arg);

                            return Marshal.PtrToStringUni(__constData, __size);
                        }

                        return Marshal.PtrToStringUni(arg);
                    default:
                        throw new ArgumentOutOfRangeException(parameter.Name, ""Parameter type with invalid type code."");
                }
            }"
        );
    }
}
