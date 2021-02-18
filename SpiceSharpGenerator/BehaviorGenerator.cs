using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// Generator used for behaviors.
    /// </summary>
    [Generator]
    public class BehaviorGenerator : ISourceGenerator
    {
        private static object _lock = new object();

        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            /*
#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif
            */
        }

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            // Get all the binding contexts
            var contexts = new BindingContextCollection();
            foreach (var bindingContext in receiver.BindingContexts)
            {
                var model = context.Compilation.GetSemanticModel(bindingContext.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(bindingContext, context.CancellationToken) as INamedTypeSymbol;

                // Add binding contexts
                foreach (var attribute in symbol.GetAttributes().Where(attribute => attribute.IsAttribute(Constants.BindingContextFor)))
                {
                    var target = attribute.MakeGenericFromAttribute();
                    contexts.Add(target, symbol);
                }
            }

            // First bin all the behaviors based on the entity that they
#pragma warning disable RS1024 // Compare symbols correctly
            var behaviorMap = new Dictionary<INamedTypeSymbol, List<BehaviorData>>(SymbolEqualityComparer.Default);
            var created = new List<BehaviorData>(8);
            var required = new List<INamedTypeSymbol>(4);
#pragma warning restore RS1024 // Compare symbols correctly
            foreach (var behavior in receiver.Behaviors)
            {
                var model = context.Compilation.GetSemanticModel(behavior.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(behavior, context.CancellationToken) as INamedTypeSymbol;
                INamedTypeSymbol check = null;
                created.Clear();
                required.Clear();

                // Create the behavior for all entities that were used
                foreach (var attribute in symbol.GetAttributes())
                {
                    // Deal with BehaviorForAttribute
                    if (attribute.IsAttribute(Constants.BehaviorFor))
                    {
                        if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol target)
                        {
                            if (!behaviorMap.TryGetValue(target, out var behaviorList))
                            {
                                behaviorList = new List<BehaviorData>(8);
                                behaviorMap.Add(target, behaviorList);
                            }

                            BehaviorData data;
                            if (attribute.ConstructorArguments.Length > 1)
                                data = new BehaviorData(symbol.MakeGeneric(attribute.ConstructorArguments[1]));
                            else
                                data = new BehaviorData(symbol);
                            behaviorList.Add(data);
                            created.Add(data);
                        }
                    }

                    // Deal with AddBehaviorIfNoAttribute
                    if (attribute.IsAttribute(Constants.AddBehaviorIfNo))
                    {
                        if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol checkItf)
                            check = checkItf;
                    }

                    // Deal with BehaviorRequiresAttribute
                    if (attribute.IsAttribute(Constants.BehaviorRequires))
                    {
                        if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol requiredBehavior)
                            required.Add(requiredBehavior);
                    }
                }

                // Update all created behaviors
                var arr = required.ToArray();
                foreach (var c in created)
                {
                    c.Check = check;
                    c.Required = arr;
                }
            }

            // Let's start by generating code for incomplete entities
            foreach (var entity in receiver.Entities)
            {
                var model = context.Compilation.GetSemanticModel(entity.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(entity, context.CancellationToken) as INamedTypeSymbol;

                // Get the set of behaviors for this entity
                var bindingContext = contexts.GetBindingContext(symbol);
                var factory = new BehaviorFactoryResolver(symbol, behaviorMap[symbol], bindingContext);
                var code = factory.Create();
                context.AddSource(symbol.ToString() + ".cs", code);

#if DEBUG
                lock (_lock)
                {
                    var filename = Path.Combine("C:/tmp", symbol.ToString() + ".cs");
                    using var sw = new StreamWriter(filename, false);
                    sw.WriteLine(code);
                    sw.Close();
                }
#endif
            }
        }
    }
}
