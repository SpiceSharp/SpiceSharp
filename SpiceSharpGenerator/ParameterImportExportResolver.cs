using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// A class that can override the necessary interfaces.
    /// </summary>
    public class ParameterImportExportResolver
    {
        private class TypeParametersAndProperties : Dictionary<string, (string Setter, string Getter)>
        {
            public bool HasImport { get; set; }
            public bool HasExport { get; set; }
        }
        private readonly Dictionary<string, int> _nameMap = [];
        private readonly INamedTypeSymbol _parameters;
#pragma warning disable RS1024 // Compare symbols correctly
        private readonly Dictionary<ITypeSymbol, TypeParametersAndProperties> _members = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterImportExportResolver"/> class.
        /// </summary>
        /// <param name="parameters">The parameter set type.</param>
        /// <param name="generatedProperties">The extra properties.</param>
        public ParameterImportExportResolver(INamedTypeSymbol parameters, Dictionary<INamedTypeSymbol, GeneratedPropertyCollection> generatedProperties = null)
        {
            _parameters = parameters;
            var names = new List<string>(4);
            while (parameters != null)
            {
                foreach (var member in parameters.GetMembers())
                {
                    if (member.DeclaredAccessibility != Accessibility.Public)
                        continue;

                    // Extract the names of the member
                    names.Clear();
                    foreach (var attribute in member.GetAttributes())
                    {
                        if (attribute.IsAttribute("ParameterNameAttribute"))
                            names.Add(attribute.ConstructorArguments[0].Value.ToString());
                    }
                    if (names.Count == 0)
                        continue;

                    ITypeSymbol btype;
                    string setter = null, getter = null;
                    switch (member)
                    {
                        case IPropertySymbol property:
                            btype = property.Type;
                            if (property.GetMethod != null && property.GetMethod.DeclaredAccessibility == Accessibility.Public)
                                getter = property.Name;
                            if (property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public)
                                setter = $"{property.Name} = value";
                            break;
                        case IFieldSymbol field:
                            btype = field.Type;
                            getter = field.Name;
                            setter = $"{field.Name} = value";
                            break;
                        case IMethodSymbol method:
                            if (method.Parameters.Length > 1)
                                continue;
                            if (method.Parameters.Length == 0)
                            {
                                btype = method.ReturnType;
                                getter = $"{method.Name}()";
                            }
                            else
                            {
                                btype = method.Parameters[0].Type;
                                setter = $"{method.Name}(value)";
                            }
                            break;
                        default:
                            continue;
                    }

                    // Get the parameters and properties for this type
                    foreach (var type in GetMemberTypes(btype))
                    {
                        if (!_members.TryGetValue(type, out var pp))
                        {
                            pp = new TypeParametersAndProperties();
                            _members.Add(type, pp);
                        }
                        pp.HasExport |= getter != null;
                        pp.HasImport |= setter != null;

                        foreach (string name in names)
                        {
                            if (pp.TryGetValue(name, out var gs))
                            {
                                pp[name] = (
                                    gs.Setter ?? setter,
                                    gs.Getter ?? getter);
                            }
                            else
                                pp.Add(name, (setter, getter));
                            if (!_nameMap.TryGetValue(name, out int mapped))
                                _nameMap.Add(name, _nameMap.Count + 1);
                        }
                    }
                }

                if (generatedProperties.TryGetValue(parameters, out var generated))
                {
                    foreach (var extra in generated)
                    {
                        // Extract the names of the member
                        names.Clear();
                        foreach (var attribute in extra.Field.GetAttributes())
                        {
                            if (attribute.IsAttribute("ParameterNameAttribute"))
                                names.Add(attribute.ConstructorArguments[0].Value.ToString());
                        }
                        if (names.Count == 0)
                            continue;

                        foreach (var type in GetMemberTypes(extra.Field.Type))
                        {
                            // Get the parameters and properties for this type
                            if (!_members.TryGetValue(type, out var pp))
                            {
                                pp = new TypeParametersAndProperties
                                {
                                    HasExport = true,
                                    HasImport = true
                                };
                                _members.Add(type, pp);
                            }
                            foreach (string name in names)
                            {
                                pp.Add(name, ($"{extra.Variable} = value", extra.Variable));
                                if (!_nameMap.TryGetValue(name, out int mapped))
                                    _nameMap.Add(name, _nameMap.Count + 1);
                            }
                        }
                    }
                }
                parameters = parameters.BaseType;
            }
        }

        private IEnumerable<ITypeSymbol> GetMemberTypes(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol ntype)
            {
                if (ntype.IsGenericType && string.CompareOrdinal(ntype.Name, "GivenParameter") == 0)
                    yield return ntype.TypeArguments[0];
            }
            yield return type;
        }

        private IEnumerable<string> GetInterfaces()
        {
            foreach (var pair in _members)
            {
                if (pair.Value.HasImport)
                    yield return $"IImportParameterSet<{pair.Key}>";
                if (pair.Value.HasExport)
                    yield return $"IExportPropertySet<{pair.Key}>";
            }
        }

        private IEnumerable<string> GetCode()
        {
            // Create the dictionary
            yield return "private static Dictionary<string, int> _namedMap = new Dictionary<string, int>(SpiceSharp.Constants.DefaultComparer)";
            yield return "{";
            foreach (var pair in _nameMap)
                yield return $"\t{{ \"{pair.Key}\", {pair.Value} }},";
            yield return "};";

            // Create a method for setting parameters
            foreach (var pair in _members)
            {
                if (pair.Value.HasImport)
                {
                    // CreateParameterSetter
                    yield return "/// <inheritdoc/>";
                    yield return $"Action<{pair.Key}> IImportParameterSet<{pair.Key}>.GetParameterSetter(string name)";
                    yield return "{";
                    yield return "\tif (_namedMap.TryGetValue(name, out var id))";
                    yield return "\t{";
                    yield return "\t\tswitch (id)";
                    yield return "\t\t{";
                    foreach (var pp in pair.Value)
                    {
                        if (pp.Value.Setter == null)
                            continue;
                        yield return $"\t\t\tcase {_nameMap[pp.Key]}:";
                        yield return $"\t\t\t\treturn value => {pp.Value.Setter};";
                    }
                    yield return "\t\t}";
                    yield return "\t}";
                    yield return "\treturn null;";
                    yield return "}";
                }

                if (pair.Value.HasExport)
                {
                    // CreatePropertyGetter
                    yield return "/// <inheritdoc/>";
                    yield return $"Func<{pair.Key}> IExportPropertySet<{pair.Key}>.GetPropertyGetter(string name)";
                    yield return "{";
                    yield return "\tif (_namedMap.TryGetValue(name, out var id))";
                    yield return "\t{";
                    yield return "\t\tswitch (id)";
                    yield return "\t\t{";
                    foreach (var pp in pair.Value)
                    {
                        if (pp.Value.Getter == null)
                            continue;
                        yield return $"\t\t\tcase {_nameMap[pp.Key]}:";
                        yield return $"\t\t\t\treturn () => {pp.Value.Getter};";
                    }
                    yield return "\t\t}";
                    yield return "\t}";
                    yield return "\treturn default;";
                    yield return "}";
                }
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
            if (_members.Count == 0)
                return "";

            var sb = new StringBuilder();
            sb.AppendLine($@"using System;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;

namespace {_parameters.ContainingNamespace}
{{
    public{(_parameters.IsAbstract ? " abstract" : "")} partial class {_parameters.Name} : {string.Join(", ", GetInterfaces())}
    {{
        {string.Join(Environment.NewLine + "\t\t", GetCode())}
    }}
}}");
            return sb.ToString();
        }
    }
}
