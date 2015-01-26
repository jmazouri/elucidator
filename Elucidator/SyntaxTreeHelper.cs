using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Formatting;

namespace Elucidator
{
    public class SyntaxTreeHelper
    {
        public static SyntaxNode FormatTree(SyntaxNode node)
        {
            var disp_tree = (CompilationUnitSyntax)Formatter.Format(node, MSBuildWorkspace.Create());

            return disp_tree;
        }

        public static SyntaxNode AddCommentToNode(SyntaxNode node, SemanticModel model = null)
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

            return nodeWithComment;
        }

    }
}
