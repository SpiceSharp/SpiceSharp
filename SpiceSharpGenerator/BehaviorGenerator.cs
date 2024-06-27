using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using ClassDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax;
using FieldDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// Generator used for behaviors.
    /// </summary>
    [Generator]
    public class BehaviorGenerator : ISourceGenerator
    {
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
            var bindingContexts = GetBindingContexts(context, receiver.BindingContexts.Keys);
            var behaviorMap = GetBehaviorMap(context, receiver.Behaviors.Keys);
            CreateBehaviorFactories(context, bindingContexts, behaviorMap, receiver.Entities.Keys);
            var generatedProperties = CreatePropertyChecks(context, receiver.CheckedFields.Keys);
            CreatePropertyParameterMethods(context, receiver.ParameterSets.Keys, generatedProperties);
        }

        private BindingContextCollection GetBindingContexts(GeneratorExecutionContext context, IEnumerable<ClassDeclarationSyntax> @classes)
        {
            var contexts = new BindingContextCollection();
            foreach (var bindingContext in @classes)
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
            return contexts;
        }
        private Dictionary<INamedTypeSymbol, List<BehaviorData>> GetBehaviorMap(GeneratorExecutionContext context, IEnumerable<ClassDeclarationSyntax> @classes)
        {
            // First bin all the behaviors based on the entity that they
#pragma warning disable RS1024 // Compare symbols correctly
            var behaviorMap = new Dictionary<INamedTypeSymbol, List<BehaviorData>>(SymbolEqualityComparer.Default);
            var created = new List<BehaviorData>(8);
            var required = new List<INamedTypeSymbol>(4);
#pragma warning restore RS1024 // Compare symbols correctly
            foreach (var behavior in @classes)
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
            return behaviorMap;
        }
        private void CreateBehaviorFactories(GeneratorExecutionContext context,
            BindingContextCollection bindingContexts,
            Dictionary<INamedTypeSymbol, List<BehaviorData>> behaviorMap,
            IEnumerable<ClassDeclarationSyntax> @classes)
        {
            // Let's start by generating code for incomplete entities
            foreach (var entity in @classes)
            {
                var model = context.Compilation.GetSemanticModel(entity.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(entity, context.CancellationToken) as INamedTypeSymbol;

                // Get the set of behaviors for this entity
                var bindingContext = bindingContexts.GetBindingContext(symbol);
                var factory = new BehaviorFactoryResolver(symbol, behaviorMap[symbol], bindingContext);
                string code = factory.Create();
                context.AddSource(symbol.ToString() + ".Behaviors.cs", code);
            }
        }
        private void CreatePropertyParameterMethods(GeneratorExecutionContext context, IEnumerable<ClassDeclarationSyntax> @classes,
            Dictionary<INamedTypeSymbol, GeneratedPropertyCollection> generatedProperties)
        {
            foreach (var parameterset in @classes)
            {
                var model = context.Compilation.GetSemanticModel(parameterset.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(parameterset, context.CancellationToken) as INamedTypeSymbol;

                var factory = new ParameterImportExportResolver(symbol, generatedProperties);
                string code = factory.Create();
                context.AddSource($"{symbol}.Named.cs", code);
            }
        }
        private Dictionary<INamedTypeSymbol, GeneratedPropertyCollection> CreatePropertyChecks(GeneratorExecutionContext context, IEnumerable<FieldDeclarationSyntax> fields)
        {
#pragma warning disable RS1024 // Compare symbols correctly
            var map = new Dictionary<INamedTypeSymbol, List<(IFieldSymbol, SyntaxTriviaList)>>(SymbolEqualityComparer.Default);
            var generated = new Dictionary<INamedTypeSymbol, GeneratedPropertyCollection>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
            foreach (var field in fields)
            {
                var model = context.Compilation.GetSemanticModel(field.SyntaxTree);
                foreach (var variable in field.Declaration.Variables)
                {
                    var symbol = model.GetDeclaredSymbol(variable, context.CancellationToken) as IFieldSymbol;
                    var @class = symbol.ContainingType;
                    if (!map.TryGetValue(@class, out var list))
                {
                        list = [];
                        map.Add(@class, list);
                    }
                    list.Add((symbol, field.GetLeadingTrivia()));
                }
                }
            foreach (var pair in map)
            {
                var factory = new PropertyResolver(pair.Key, pair.Value);
                string code = factory.Create();
                context.AddSource($"{pair.Key.ContainingNamespace}.{pair.Key.Name}.AutoProperties.cs", code);
                generated.Add(pair.Key, factory.Generated);
            }
            return generated;
        }
    }
}
