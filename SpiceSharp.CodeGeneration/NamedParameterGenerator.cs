using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SpiceSharp.CodeGeneration
{
    /// <summary>
    /// A class that can generated methods for setting or getting named parameters
    /// and properties.
    /// </summary>
    public class NamedParameterGenerator : CSharpSyntaxWalker
    {
        public CompilationUnitSyntax Result { get; private set; }

        private Dictionary<string, (SyntaxToken Name, TypeSyntax Type, bool HasSetter, bool HasGetter)> _properties
            = new Dictionary<string, (SyntaxToken Name, TypeSyntax Type, bool HasSetter, bool HasGetter)>();

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            Result = SyntaxFactory.CompilationUnit(
                SyntaxFactory.List<ExternAliasDirectiveSyntax>(),
                SyntaxFactory.List<UsingDirectiveSyntax>(),
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.List<MemberDeclarationSyntax>()
                );
            base.VisitCompilationUnit(node);
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            Result = Result.AddMembers(
                SyntaxFactory.NamespaceDeclaration(node.Name)
                );
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.PartialKeyword) && 
                node.AttributeLists.Any(
                    list => list.Attributes.Any(
                        attr =>  attr.ArgumentList != null &&
                            attr.ArgumentList.Arguments.Any(
                                arg => arg.NameEquals != null && 
                                arg.NameEquals.Name.ToString().Equals("NamedAccess") && 
                                arg.Expression.ToString().Equals("true")))))
            {
                var cl = SyntaxFactory.ClassDeclaration(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
                    node.Identifier,
                    null,
                    null,
                    SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
                    SyntaxFactory.List<MemberDeclarationSyntax>()
                    );

                base.VisitClassDeclaration(node);

                // Generate our methods now
                cl = cl.WithMembers(
                    SyntaxFactory.List(
                        CreateDictionaryDefinition().Union(
                        CreateSetParameterMethod()).Union(
                        CreateGetPropertyMethod())
                    ));

                // Replace the first member with our generated class
                Result = Result.ReplaceNode(
                    Result.Members[0],
                    ((NamespaceDeclarationSyntax)Result.Members[0]).AddMembers(cl)
                    );
            }
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            // Check for any named attribute that we can use
            bool hasGetter = false, hasSetter = false;
            if (node.AccessorList.Accessors.Any(ads => ads.Keyword.ValueText.Equals("get")))
                hasGetter = true;
            if (node.AccessorList.Accessors.Any(ads => ads.Keyword.ValueText.Equals("set")))
                hasSetter = true;
            foreach (var attribute in node
                .AttributeLists
                .SelectMany(al => al.Attributes)
                .Where(attr =>
                {
                    switch (attr.Name.ToString())
                    {
                        case "ParameterName":
                        case "ParameterNameAttribute":
                            return true;
                    }
                    return false;
                }))
            {
                _properties.Add(
                    attribute.ArgumentList.Arguments[0].Expression.ToString(),
                    (node.Identifier, node.Type, hasSetter, hasGetter));
            }

            base.VisitPropertyDeclaration(node);
        }

        private MemberDeclarationSyntax[] CreateDictionaryDefinition()
        {
            return null;
        }

        private MemberDeclarationSyntax[] CreateSetParameterMethod()
        {
            return null;
        }

        private MemberDeclarationSyntax[] CreateGetPropertyMethod()
        {
            return null;
        }
    }
}
