using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Elucidator
{
    public class CommentRewriter : CSharpSyntaxRewriter
    {
        public CommentRewriter()
        {

        }

        private T GetCommentForNode<T>(SyntaxNode node) where T : SyntaxNode
        {
            var newComment = SyntaxFactory.Comment(CommentGenerator.GetComment(node));

            SyntaxNode nodeWithComment = node;

            if (node.HasLeadingTrivia)
            {
                var endtrivia = node.GetLeadingTrivia().Last(d => d.IsKind(SyntaxKind.WhitespaceTrivia));
                nodeWithComment = node.InsertTriviaAfter(endtrivia, new List<SyntaxTrivia> { newComment });
            }
            else
            {
                nodeWithComment = node.WithLeadingTrivia(newComment).NormalizeWhitespace();
            }

            return nodeWithComment as T;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            return base.VisitClassDeclaration(GetCommentForNode<ClassDeclarationSyntax>(node));
        }

        public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            return base.VisitInterfaceDeclaration(GetCommentForNode<InterfaceDeclarationSyntax>(node));
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            return base.VisitStructDeclaration(GetCommentForNode<StructDeclarationSyntax>(node));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            return base.VisitMethodDeclaration(GetCommentForNode<MethodDeclarationSyntax>(node));
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            return base.VisitFieldDeclaration(GetCommentForNode<FieldDeclarationSyntax>(node));
        }

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            return base.VisitEnumDeclaration(GetCommentForNode<EnumDeclarationSyntax>(node));
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            return base.VisitConstructorDeclaration(GetCommentForNode<ConstructorDeclarationSyntax>(node));
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            return base.VisitPropertyDeclaration(GetCommentForNode<PropertyDeclarationSyntax>(node));
        }

        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            if (!(node.Parent is FieldDeclarationSyntax))
            {
                return base.VisitVariableDeclaration(GetCommentForNode<VariableDeclarationSyntax>(node));
            }

            return base.VisitVariableDeclaration(node);
        }

        public override SyntaxNode VisitTryStatement(TryStatementSyntax node)
        {
            return base.VisitTryStatement(GetCommentForNode<TryStatementSyntax>(node));
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            return base.VisitAssignmentExpression(GetCommentForNode<AssignmentExpressionSyntax>(node));
        }
    }
}
