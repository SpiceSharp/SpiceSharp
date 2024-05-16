using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// Helper methods.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Get a fully qualified name for the class declaration syntax.
        /// </summary>
        /// <param name="class">The class declaration syntax.</param>
        /// <returns>The fully qualified name.</returns>
        public static string GetQualifiedName(this ClassDeclarationSyntax @class)
        {
            // Find the namespace by going outward
            string name = "";
            var parent = @class.Parent;
            while (parent != null)
            {
                if (parent is NamespaceDeclarationSyntax @namespace)
                {
                    if (string.IsNullOrEmpty(name))
                        name = @namespace.Name.WithoutTrivia().GetText().ToString();
                    else
                        name = @namespace.Name.WithoutTrivia().GetText().ToString() + "." + name;
                }
                parent = parent.Parent;
            }
            name += "." + @class.Identifier.ValueText;

            // Also make sure to add generic type names
            if (@class.TypeParameterList != null && @class.TypeParameterList.Parameters.Count > 0)
                name += "<" + string.Join(",", @class.TypeParameterList.Parameters.Select(p => p.Identifier.WithoutTrivia())) + ">";
            return name;
        }

        /// <summary>
        /// Get a fully qualified name for the named type symbol.
        /// </summary>
        /// <param name="type">The named type symbol.</param>
        /// <returns>The fully qualified name.</returns>
        public static string GetQualifiedName(this INamedTypeSymbol type)
        {
            string name = type.ContainingNamespace.ToString() + "." + type.Name;
            if (type.TypeParameters.Length > 0)
                name += "<" + string.Join(".", type.TypeParameters.Select(p => p.Name)) + ">";
            return name;
        }

        /// <summary>
        /// Find out whether the symbol represents an interface.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="name">The name of the interface, or <c>null</c> if not important.</param>
        /// <param name="namespace">The namespace of the interface, or <c>null</c> if not important.</param>
        /// <returns>
        ///     <c>true</c> if the symbol matches; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInterface(this INamedTypeSymbol symbol, string name = null, string @namespace = null)
        {
            if (symbol.TypeKind != TypeKind.Interface)
                return false;
            if (name != null && string.CompareOrdinal(symbol.Name, name) != 0)
                return false;
            if (@namespace != null && string.CompareOrdinal(symbol.ContainingNamespace.ToString(), @namespace) != 0)
                return false;
            return true;
        }

        /// <summary>
        /// Check whether an attribute is of the specified type.
        /// </summary>
        /// <param name="attribute">The attribute data.</param>
        /// <param name="name">The name of the attribute type.</param>
        /// <param name="namespace">The namespace.</param>
        /// <returns>
        ///     <c>true</c> if the attribute matches; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAttribute(this AttributeData attribute, string name, string @namespace = Constants.AttributeNamespace)
        {
            if (string.CompareOrdinal(attribute.AttributeClass.Name, name) != 0)
                return false;
            if (string.CompareOrdinal(attribute.AttributeClass.ContainingNamespace.ToString(), @namespace) != 0)
                return false;
            return true;
        }

        /// <summary>
        /// Make a generic type from arguments.
        /// </summary>
        /// <param name="baseType">The base type.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The generic type.</returns>
        public static INamedTypeSymbol MakeGeneric(this INamedTypeSymbol baseType, TypedConstant arguments)
        {
            if (arguments.Values.Length > 0)
                return baseType.Construct(arguments.Values.Select(v => v.Value).Cast<INamedTypeSymbol>().ToArray());
            if (arguments.Value is INamedTypeSymbol arg)
                return baseType.Construct(arg);
            return null;
        }

        /// <summary>
        /// Get a type from attribute constructor arguments, assuming the first and second parameter give its arguments.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="index">The index of the constructor argument used</param>
        /// <returns>
        /// The type.
        /// </returns>
        public static INamedTypeSymbol MakeGenericFromAttribute(this AttributeData attribute, int index = 0)
        {
            if (attribute.ConstructorArguments.Length <= index)
                return null;
            if (attribute.ConstructorArguments[index].Value is not INamedTypeSymbol btype)
                return null;
            if (attribute.ConstructorArguments.Length > index + 1)
                btype = btype.MakeGeneric(attribute.ConstructorArguments[index + 1]);
            return btype;
        }

        /// <summary>
        /// Checks whether a symbol implements another one.
        /// </summary>
        /// <param name="toCheck">The class to be checked.</param>
        /// <param name="implemented">The class or interface that needs to be implemented.</param>
        /// <returns>
        ///     <c>true</c> if the class implements another one; otherwise, <c>false</c>.
        /// </returns>
        public static bool Implements(this INamedTypeSymbol toCheck, INamedTypeSymbol implemented)
        {
            string ns = implemented.ContainingNamespace.ToString();
            if (implemented.TypeKind == TypeKind.Class)
            {
                // Check any base type
                var baseType = toCheck;
                while (baseType != null)
                {
                    if (string.CompareOrdinal(baseType.Name, implemented.Name) == 0 &&
                        string.CompareOrdinal(baseType.ContainingNamespace.ToString(), ns) == 0)
                        return true;
                    baseType = baseType.BaseType;
                }
            }
            else if (implemented.TypeKind == TypeKind.Interface)
            {
                // Easy! Check the implemented interface
                return toCheck.AllInterfaces.Any(itf => itf.IsInterface(implemented.Name, ns));
            }
            return false;
        }

        /// <summary>
        /// Gets all the attributes (including inherited ones).
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>The attributes.</returns>
        public static IEnumerable<AttributeData> GetAllAttributes(this INamedTypeSymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
                yield return attribute;
            symbol = symbol.BaseType;
            while (symbol != null)
            {
                // Go through all attributes and find those that can be inherited
                foreach (var attribute in symbol.GetAttributes())
                {
                    // Get the AttributeUsageAttribute
                    var usage = attribute.AttributeClass.GetAttributes().FirstOrDefault(a => a.IsAttribute("AttributeUsageAttribute", "System"));
                    if (usage != null && usage.NamedArguments.Any(pair => string.CompareOrdinal(pair.Key, "Inherited") == 0 && pair.Value.Value.Equals(false)))
                        continue;
                    yield return attribute;
                }

                symbol = symbol.BaseType;
            }
        }

        /// <summary>
        /// Formats an object that is meant for code.
        /// </summary>
        /// <param name="constant">The value.</param>
        /// <returns>The string representation.</returns>
        public static string Format(this object constant)
        {
            return constant switch
            {
                double dbl => dbl.ToString(System.Globalization.CultureInfo.InvariantCulture),
                _ => constant?.ToString() ?? "null",
            };
        }

        private static readonly Regex _dashes = new(@"[\-\.](?<word>\w+)");

        /// <summary>
        /// Turn a string into a name that can be used as a variable.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The variable.</returns>
        public static string ToVariable(this string name)
        {
            // Remove spaces
            name = name.Replace(' ', '_');
            name = _dashes.Replace(name, match =>
            {
                string word = match.Groups["word"].Value;
                return char.ToUpper(word[0]) + word.Substring(1);
            });
            name = char.ToUpper(name[0]) + name.Substring(1);
            return name;
        }
    }
}
