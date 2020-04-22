using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SpiceSharp.CodeGeneration
{
    /// <summary>
    /// The class that is in charge for expanding auto-generated parts.
    /// </summary>
    /// <seealso cref="CSharpSyntaxRewriter" />
    public class ClassRewriter : CSharpSyntaxRewriter
    {
        /// <summary>
        /// The default value of RaiseException for rule attributes.
        /// </summary>
        public const bool DefaultRaiseException = true;

        private readonly Dictionary<string, (SyntaxTokenList Modifiers, TypeSyntax Type, ExpressionSyntax Default)> _requiredPrivateFields
            = new Dictionary<string, (SyntaxTokenList Modifiers, TypeSyntax Type, ExpressionSyntax Default)>();
        private readonly Dictionary<string, MemberDeclarationSyntax> _definedMembers
            = new Dictionary<string, MemberDeclarationSyntax>();

        private bool _isCurrentClass = true;

        /// <summary>
        /// Visits the class declaration.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (_isCurrentClass)
            {
                _isCurrentClass = false;

                // Find all the private fields, because they may be used
                var changed = base.VisitClassDeclaration(node);

                // Add the fields that are required but not defined yet in the class
                if (changed is ClassDeclarationSyntax cl)
                {
                    foreach (var req in _requiredPrivateFields)
                    {
                        var nvardecl = VariableDeclarator(req.Key);
                        if (req.Value.Default != null)
                            nvardecl = nvardecl.WithInitializer(EqualsValueClause(req.Value.Default));

                        var nfield = FieldDeclaration(VariableDeclaration(req.Value.Type)
                            .AddVariables(nvardecl)).WithModifiers(req.Value.Modifiers);
                        if (_definedMembers.TryGetValue(req.Key, out var original))
                            cl = cl.ReplaceNode(original, nfield);
                        else
                            cl = cl.WithMembers(cl.Members.Insert(0, nfield));
                    }
                    changed = cl;
                }

                _isCurrentClass = true;
                return changed;
            }
            else
                // Avoid rewriting nested classes that aren't supposed to be rewritten
                return node;
        }

        /// <summary>
        /// Visits the field declaration.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.Modifiers != null && node.Modifiers.Any(m => m.ValueText.Equals("private")))
            {
                foreach (var name in node.Declaration.Variables.Select(v => v.Identifier.ValueText))
                    _definedMembers.Add(name, node);
            }
            return base.VisitFieldDeclaration(node);
        }

        /// <summary>
        /// Visits the property declaration.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            // Find the attributes and see if we need to generate some code
            var result = node;
            var privateVariable = node.Identifier.ValueText;
            privateVariable = $"_{char.ToLowerInvariant(privateVariable[0])}{privateVariable.Substring(1)}";

            AccessorDeclarationSyntax setter = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);

            foreach (var attr in node.AttributeLists.SelectMany(list => list.Attributes))
            {
                switch (attr.Name.ToString())
                {
                    case "GreaterThan":
                    case "GreaterThanAttribute":
                        var limit = attr.ArgumentList.Arguments[0].ToFullString();
                        setter = setter.AddBodyStatements(ParseStatement($"Utility.GreaterThan(value, nameof({result.Identifier.ValueText}), {limit});"));
                        break;

                    case "GreaterThanOrEquals":
                    case "GreaterThanOrEqualsAttribute":
                        limit = attr.ArgumentList.Arguments[0].ToFullString();
                        setter = setter.AddBodyStatements(ParseStatement($"Utility.GreaterThanOrEquals(value, nameof({result.Identifier.ValueText}), {limit});"));
                        break;

                    case "LessThan":
                    case "LessThanAttribute":
                        limit = attr.ArgumentList.Arguments[0].ToFullString();
                        setter = setter.AddBodyStatements(ParseStatement($"Utility.LessThan(value, nameof({result.Identifier.ValueText}), {limit});"));
                        break;

                    case "LessThanOrEquals":
                    case "LessThanOrEqualsAttribute":
                        limit = attr.ArgumentList.Arguments[0].ToFullString();
                        setter = setter.AddBodyStatements(ParseStatement($"Utility.LessThanOrEquals(value, nameof({result.Identifier.ValueText}), {limit});"));
                        break;

                    case "LowerLimit":
                    case "LowerLimitAttribute":
                        limit = attr.ArgumentList.Arguments[0].ToFullString();
                        setter = setter.AddBodyStatements(ParseStatement($"value = Utility.LowerLimit(value, this, nameof({result.Identifier.ValueText}), {limit});"));
                        break;

                    case "UpperLimit":
                    case "UpperLimitAttribute":
                        limit = attr.ArgumentList.Arguments[0].ToFullString();
                        setter = setter.AddBodyStatements(ParseStatement($"value = Utility.UpperLimit(value, this, nameof({result.Identifier.ValueText}), {limit});"));
                        break;

                    case "DerivedProperty":
                    case "DerivedPropertyAttribute":
                        // Don't touch derived properties
                        return node;
                }
            }

            // Replace the setter of the node if it isn't the original
            if ((setter.Body?.Statements.Count ?? 0) > 0)
            {
                setter = setter.AddBodyStatements(ParseStatement($"{privateVariable} = value;"));
                result = result.ReplaceNode(Setter(result), setter);

                // Replace the getter with an access to the private variable
                var og = Getter(result);
                var getter = AccessorDeclaration(
                    SyntaxKind.GetAccessorDeclaration,
                    List<AttributeListSyntax>(),
                    TokenList(),
                    Token(SyntaxKind.GetKeyword),
                    ArrowExpressionClause(ParseExpression(privateVariable)), 
                    Token(SyntaxKind.SemicolonToken));
                result = result.ReplaceNode(Getter(result), getter);

                // Deal with the initializer and the private variable
                // We need the same modifiers as the original property, but with public changed to private
                var modifiers = result.Modifiers;
                modifiers = modifiers.Replace(modifiers.First(t => t.ValueText.Equals("public")), Token(SyntaxKind.PrivateKeyword));

                _requiredPrivateFields.Add(privateVariable, (modifiers, node.Type, result.Initializer?.Value));
                if (result.Initializer != null)
                {
                    var trailingTrivia = result.SemicolonToken.TrailingTrivia;
                    result = result
                        .RemoveNode(result.Initializer, SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.KeepEndOfLine | SyntaxRemoveOptions.KeepDirectives)
                        .WithSemicolonToken(Token(SyntaxKind.None))
                        .WithTrailingTrivia(trailingTrivia);
                }

                return result;
            }

            // Nothing done here, so let's just continue
            return base.VisitPropertyDeclaration(node);
        }

        private static AccessorDeclarationSyntax Getter(PropertyDeclarationSyntax property)
            => property.AccessorList.Accessors.FirstOrDefault(a => a.Keyword.ValueText.Equals("get"));
        private static AccessorDeclarationSyntax Setter(PropertyDeclarationSyntax property)
            => property.AccessorList.Accessors.FirstOrDefault(a => a.Keyword.ValueText.Equals("set"));
    }
}
