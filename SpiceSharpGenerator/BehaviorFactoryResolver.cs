using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// A class capable of resolving how behaviors need to be created.
    /// </summary>
    public class BehaviorFactoryResolver
    {
        private readonly INamedTypeSymbol _entity, _context;
#pragma warning disable RS1024 // Compare symbols correctly
        private readonly Dictionary<INamedTypeSymbol, BehaviorData> _behaviors = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
        private readonly DependencyGraph<BehaviorData> _graph = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorFactoryResolver"/> class.
        /// </summary>
        /// <param name="entity">The entity type.</param>
        /// <param name="behaviors">The behaviors that have been flagged to work with the entity.</param>
        /// <param name="context">The binding context type for this entity.</param>
        public BehaviorFactoryResolver(INamedTypeSymbol entity, IEnumerable<BehaviorData> behaviors, INamedTypeSymbol context)
        {
            _entity = entity;
            foreach (var behavior in behaviors)
            {
                _behaviors.Add(behavior.Behavior, behavior);
                _graph.Add(behavior);
            }
            _context = context;
        }

        private void Resolve()
        {
            _graph.ClearDependencies();

            // Go through each behavior in the first pass and try to resolve
            foreach (var behavior in _behaviors.Values)
            {
                _graph.Add(behavior);
                
                // Make sure that hierarchies are respected
                var baseType = behavior.Behavior.BaseType;
                while (baseType != null)
                {
                    if (_behaviors.TryGetValue(baseType, out var childBehavior))
                        _graph.MakeDependency(childBehavior, behavior);
                    baseType = baseType.BaseType;
                }

                // Add required behaviors
                foreach (var required in behavior.Required)
                {
                    if (_behaviors.TryGetValue(required, out var data))
                        _graph.MakeDependency(behavior, data);
                    else
                    {
                        if (required.TypeKind == TypeKind.Interface)
                        {
                            foreach (var possibility in _behaviors.Values.Where(data => data.Behavior.AllInterfaces.Any(itf => SymbolEqualityComparer.Default.Equals(itf, required))))
                                _graph.MakeDependency(behavior, possibility);
                        }
                    }
                }
            }
        }

        private IEnumerable<string> CreateCode()
        {
            yield return "var behaviors = new BehaviorContainer(Name);";
            yield return $"var context = new {_context}(this, simulation, behaviors);";

            var sb = new StringBuilder(32);
            bool needsBuilder = true;

            foreach (var behavior in _graph.OrderByIndependentFirst())
            {
                // No behavior needed, but still respect the order!
                if (behavior.Check == null)
                {
                    if (!needsBuilder)
                    {
                        // Terminate the last line
                        sb.Append(";");
                        yield return sb.ToString();
                        sb.Clear();
                        needsBuilder = true;
                    }
                    yield return $"behaviors.Add(new {behavior}(context));";
                }
                else
                {
                    if (needsBuilder)
                    {
                        yield return $"behaviors.Build(simulation, context)";
                        needsBuilder = false;
                    }
                    if (sb.Length > 0)
                    {
                        yield return sb.ToString();
                        sb.Clear();
                    }
                    sb.Append($"\t.AddIfNo<{behavior.Check}>(context => new {behavior.Behavior}(context))");
                }
            }
            if (!needsBuilder && sb.Length > 0)
            {
                sb.Append(";");
                yield return sb.ToString();
            }

            // Add the behaviors to the simulation entity behaviors
            yield return "simulation.EntityBehaviors.Add(behaviors);";
        }

        /// <summary>
        /// Create the code that implements the behaviors.
        /// </summary>
        /// <returns>The code that implements the behavior creation.</returns>
        public string Create()
        {
            Resolve();

            var sb = new StringBuilder();
            sb.AppendLine($@"using System;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace {_entity.ContainingNamespace}
{{
    public partial class {_entity.Name}
    {{
        /// <inheritdoc />
        public override void CreateBehaviors(ISimulation simulation)
        {{
            {string.Join(Environment.NewLine + new string('\t', 3), CreateCode())}
        }}
    }}
}}");
            return sb.ToString();
        }
    }
}
