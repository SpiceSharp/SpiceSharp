using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// A class that can resolve auto-generated properties.
    /// </summary>
    public class PropertyResolver
    {
        private readonly INamedTypeSymbol _symbol;
        private readonly IEnumerable<(IFieldSymbol Field, SyntaxTriviaList Trivia)> _fields;

        /// <summary>
        /// Gets the generated variables.
        /// </summary>
        public GeneratedPropertyCollection Generated { get; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyResolver"/> class.
        /// </summary>
        /// <param name="symbol">The class symbol.</param>
        /// <param name="fields">The field symbols and their leading trivia (remarks).</param>
        public PropertyResolver(INamedTypeSymbol symbol, IEnumerable<(IFieldSymbol, SyntaxTriviaList)> fields)
        {
            _symbol = symbol;
            _fields = fields;
        }

        private IEnumerable<string> GetCode()
        {
            Generated.Clear();
            foreach (var (field, trivia) in _fields)
            {
                var g = new GeneratedProperty(field);
                Generated.Add(g);
                string name = g.Variable;

                // We first want to copy the trivia
                foreach (string line in trivia.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        yield return line.Trim();
                }

                // then we also show the attributes
                var sb = new StringBuilder(32);
                var checks = new List<string>(4);
                sb.Append("[");
                bool isFirst = true;
                foreach (var attribute in field.GetAttributes())
                {
                    if (attribute.IsAttribute("LessThanAttribute"))
                        checks.Add($"Utility.LessThan(value, nameof({name}), {attribute.ConstructorArguments[0].Value.Format()});");
                    else if (attribute.IsAttribute("GreaterThanAttribute"))
                        checks.Add($"Utility.GreaterThan(value, nameof({name}), {attribute.ConstructorArguments[0].Value.Format()});");
                    else if (attribute.IsAttribute("LessThanOrEqualsAttribute"))
                        checks.Add($"Utility.LessThanOrEquals(value, nameof({name}), {attribute.ConstructorArguments[0].Value.Format()});");
                    else if (attribute.IsAttribute("GreaterThanOrEqualsAttribute"))
                        checks.Add($"Utility.GreaterThanOrEquals(value, nameof({name}), {attribute.ConstructorArguments[0].Value.Format()});");
                    else if (attribute.IsAttribute("LowerLimitAttribute"))
                        checks.Add($"value = Utility.LowerLimit(value, this, nameof({name}), {attribute.ConstructorArguments[0].Value.Format()});");
                    else if (attribute.IsAttribute("UpperLimitAttribute"))
                        checks.Add($"value = Utility.UpperLimit(value, this, nameof({name}), {attribute.ConstructorArguments[0].Value.Format()});");
                    else if (attribute.IsAttribute("FiniteAttribute"))
                        checks.Add($"Utility.Finite(value, nameof({name}));");
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(", ");
                    sb.Append(attribute.ToString());
                }
                sb.Append("]");
                yield return sb.ToString();

                // Now comes the actual definition
                yield return $"public{(field.IsStatic ? " static" : "")} {field.Type} {name}";
                yield return "{";
                yield return $"\tget => {field.Name};";
                yield return $"\tset";
                yield return "\t{";
                foreach (string check in checks)
                    yield return "\t\t" + check;
                yield return $"\t\t{field.Name} = value;";
                yield return "\t}";
                yield return "}";
            }
        }

        /// <summary>
        /// Create the class.
        /// </summary>
        /// <returns>
        /// The code.
        /// </returns>
        public string Create()
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"using System;
using SpiceSharp;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;

namespace {_symbol.ContainingNamespace}
{{
    public{(_symbol.IsAbstract ? " abstract" : "")} partial class {_symbol.Name}{(_symbol.IsGenericType ? $"<{string.Join(", ", _symbol.TypeArguments)}>" : "")}
    {{
        {string.Join(Environment.NewLine + "\t\t", GetCode())}
    }}
}}");
            return sb.ToString();
        }
    }
}
