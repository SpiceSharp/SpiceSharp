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

        private readonly Dictionary<string, (TypeSyntax Type, ExpressionSyntax Default)> _requiredPrivateFields
            = new Dictionary<string, (TypeSyntax Type, ExpressionSyntax Default)>();
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
                            .AddVariables(nvardecl)).WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
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
            if (node.Modifiers.Count == 1 && node.Modifiers[0].ValueText.Equals("private"))
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
                SyntaxKind boundKind = SyntaxKind.None;
                string warningPrefix = null;
                switch (attr.Name.ToString())
                {
                    case "GreaterThan":
                    case "GreaterThanAttribute":
                        boundKind = SyntaxKind.LessThanOrEqualExpression;
                        warningPrefix = "Properties.Resources.Parameters_TooSmall";
                        break;

                    case "GreaterThanOrEquals":
                    case "GreaterThanOrEqualsAttribute":
                        boundKind = SyntaxKind.LessThanExpression;
                        warningPrefix = "Properties.Resources.Parameters_TooSmall";
                        break;

                    case "LessThan":
                    case "LessThanAttribute":
                        boundKind = SyntaxKind.GreaterThanOrEqualExpression;
                        warningPrefix = "Properties.Resources.Parameters_TooLarge";
                        break;

                    case "LessThanOrEquals":
                    case "LessThanOrEqualsAttribute":
                        boundKind = SyntaxKind.GreaterThanExpression;
                        warningPrefix = "Properties.Resources.Parameters_TooLarge";
                        break;

                    case "DerivedProperty":
                    case "DerivedPropertyAttribute":
                        // Don't touch derived properties
                        return node;
                }

                if (boundKind != SyntaxKind.None)
                {
                    // Get the limit
                    var limit = attr.ArgumentList.Arguments[0].ToFullString();

                    // Find out what the consquence should be
                    StatementSyntax consequence;
                    ExpressionSyntax consequenceCondition = null;
                    for (var i = 1; i < attr.ArgumentList.Arguments.Count; i++)
                    {
                        var arg = attr.ArgumentList.Arguments[i];
                        if (arg.NameEquals.Name.Identifier.ValueText.Equals("RaisesException"))
                            consequenceCondition = arg.Expression;
                    }

                    if (consequenceCondition == null)
                        consequenceCondition = ParseExpression(DefaultRaiseException ? "true" : "false");
                    switch (consequenceCondition)
                    {
                        case LiteralExpressionSyntax les when les.Token.Value.Equals(false):
                            consequence = Block(
                                ParseStatement($"{privateVariable} = {limit};"),
                                ParseStatement($"SpiceSharpWarning.Warning(this, {warningPrefix}Set.FormatString(nameof({node.Identifier.ValueText}), value, {limit}));"),
                                ReturnStatement());
                            break;
                        case LiteralExpressionSyntax les when les.Token.Value.Equals(true):
                            consequence = ParseStatement($"throw new ArgumentException({warningPrefix}.FormatString(nameof({node.Identifier.ValueText}), value, {limit}));");
                            break;
                        default:
                            consequence = Block(
                                IfStatement(consequenceCondition,
                                    ParseStatement($"throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof({node.Identifier.ValueText}), value, {limit}));"),
                                    ElseClause(
                                        Block(
                                            ParseStatement($"{privateVariable} = {limit};"),
                                            ParseStatement($"SpiceSharpWarning.Warning(this, Properties.Resources.Parameters_TooSmallSet.FormatString(nameof({node.Identifier.ValueText}), value, {limit}));"),
                                            ReturnStatement()))));
                            break;
                    }

                    // Add our check to the setter
                    var cond = IfStatement(
                        BinaryExpression(
                            boundKind,
                            ParseExpression("value"),
                            ParseExpression(limit)
                        ), consequence);

                    // Replace
                    setter = setter.AddBodyStatements(cond);
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
                _requiredPrivateFields.Add(privateVariable, (node.Type, result.Initializer?.Value));
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
