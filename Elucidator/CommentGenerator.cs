using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Elucidator
{
    public class CommentGenerator
    {
        private static bool IsList(string declaration)
        {
            string[] probablyAList = {"enumerable", "list", "dictionary"};

            foreach (string entry in probablyAList)
            {
                if (declaration.ToLower().Contains(entry))
                {
                    return true;
                }
            }

            return false;
        }

        private static string OperatorComment(IdentifierNameSyntax left, IdentifierNameSyntax right, SyntaxKind op)
        {
            string ret = "";

            switch (op)
            {
                case SyntaxKind.PlusToken:
                    ret = "adding {0} and {1} together";
                    break;
                case SyntaxKind.MinusToken:
                    ret = "subtracting {0} from {1}";
                    break;
                case SyntaxKind.AsteriskToken:
                    ret = "multiplying {0} by {1}";
                    break;
                case SyntaxKind.SlashToken:
                    ret = "dividing {0} by {1}";
                    break;
                case SyntaxKind.PercentToken:
                    ret = "get the remainder (modulus) after dividing {0} by {1}";
                    break;
                case SyntaxKind.PlusEqualsToken:
                    ret = "add {1} to {0}";
                    break;
                case SyntaxKind.MinusEqualsToken:
                    ret = "remove {1} from {0}";
                    break;
                default:
                    ret = "doing some quick math with {0} and {1}";
                    break;
            }

            ret = ret.FormatWith(left.Identifier.ValueText.Humanize(LetterCasing.LowerCase),
                right.Identifier.ValueText.Humanize(LetterCasing.LowerCase));

            return ret;
        }

        private static List<string> seenNodes = new List<string>();

        public static string GetComment(SyntaxNode node)
        {
            string ret = "//This is a {0}".FormatWith(node.Kind().Humanize());

            if (node is InvocationExpressionSyntax invc && !(invc.Parent.Parent is AssignmentExpressionSyntax))
            {
                seenNodes.Add(invc.ToString());

                if (seenNodes.Contains(invc.Parent.ToString().Substring(0, invc.Parent.ToString().Length - 1)))
                {
                    return null;
                }

                ret = $"//Calling the {invc.Expression.ToString().Humanize(LetterCasing.LowerCase)} method";
            }

            if (node is TypeDeclarationSyntax)
            {
                var cur = node as TypeDeclarationSyntax;

                string type = "bit";

                if (node is ClassDeclarationSyntax)
                {
                    type = "class";
                }

                if (node is StructDeclarationSyntax)
                {
                    type = "struct";
                }

                if (node is InterfaceDeclarationSyntax)
                {
                    type = "interface";
                }

                ret = "//Defining the {0} {1}".FormatWith(cur.Identifier.ValueText.Humanize(LetterCasing.LowerCase), type);
            }

            if (node is MethodDeclarationSyntax)
            {
                var cur = node as MethodDeclarationSyntax;

                string returnType = cur.ReturnType.ToFullString().Trim();
                if (returnType == "void")
                {
                    ret = "//Declaring the {0} method".FormatWith(cur.Identifier.ValueText.Humanize(LetterCasing.LowerCase));
                }
                else
                {
                    if (IsList(cur.Identifier.GetPreviousToken().ValueText) && cur.ReturnType is GenericNameSyntax)
                    {
                        var genericSyntax = cur.ReturnType as GenericNameSyntax;
                        var generic = genericSyntax.TypeArgumentList.Arguments[0].ToFullString();

                        ret = "//Declaring the {0} method, returns a list of {1}".FormatWith(cur.Identifier.ValueText.Humanize(LetterCasing.LowerCase), generic.Pluralize(true));
                    }
                    else
                    {
                        ret = "//Declaring the {0} method, returns a {1}".FormatWith(cur.Identifier.ValueText.Humanize(LetterCasing.LowerCase), cur.ReturnType.ToFullString().Humanize());
                    }
                }
            }

            if (node is FieldDeclarationSyntax)
            {
                var cur = node as FieldDeclarationSyntax;

                if (IsList(cur.Declaration.Type.ToFullString()) && cur.Declaration.Type is GenericNameSyntax)
                {
                    var genericSyntax = cur.Declaration.Type as GenericNameSyntax;
                    var generic = genericSyntax.TypeArgumentList.Arguments[0].ToFullString();

                    ret = "//Declaring a new collection of {0}".FormatWith(generic.Pluralize(true));
                }
                else
                {
                    ret = "//A new {0} field".FormatWith(cur.Declaration.Type.ToFullString().Humanize(LetterCasing.LowerCase).Trim());
                }

            }

            if (node is ConstructorDeclarationSyntax)
            {
                var cur = node as ConstructorDeclarationSyntax;
                ret = "//Declaring the constructor for the {0} class".FormatWith(cur.Identifier.ToFullString());
            }

            if (node is EnumDeclarationSyntax)
            {
                var cur = node as EnumDeclarationSyntax;
                ret = "//Declaring the {0} enumeration.".FormatWith(cur.Identifier.ToFullString().Humanize(LetterCasing.LowerCase).Trim());
            }

            if (node is AssignmentExpressionSyntax)
            {
                var cur = node as AssignmentExpressionSyntax;

                try
                {
                    ret = String.Format("//Setting {0} to the value of {1}", cur.Left.ToFullString().Humanize(LetterCasing.LowerCase).Trim(),
                    cur.Right.ToFullString().Humanize(LetterCasing.LowerCase).Trim());
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("\t\tThat one had some issues.");
                }
                
                //ret += ", and {0}".FormatWith(OperatorComment((IdentifierNameSyntax)cur.Left, (IdentifierNameSyntax)cur.Right, cur.OperatorToken.CSharpKind()));
            }

            if (node is TryStatementSyntax)
            {
                var cur = node as TryStatementSyntax;
                ret = "//Doing some error handling";
            }

            if (node is VariableDeclarationSyntax)
            {
                var cur = node as VariableDeclarationSyntax;

                string typename = cur.Type.ToFullString().Trim();

                if (IsList(cur.Type.ToFullString()) && cur.Type is GenericNameSyntax)
                {
                    var genericSyntax = cur.Type as GenericNameSyntax;
                    var generic = genericSyntax.TypeArgumentList.Arguments[0].ToFullString();

                    ret = "//Declaring a collection of {0}".FormatWith(generic.Pluralize(true));
                }
                else
                {
                    ret = "//Declaring the {0} {1}".FormatWith(typename, cur.Variables[0].Identifier.ValueText);

                    if (cur.Variables.Any())
                    {
                        if (cur.Variables.First().Initializer != null && cur.Variables.First().Initializer.Value is BinaryExpressionSyntax)
                        {
                            var expression = (BinaryExpressionSyntax)cur.Variables.First().Initializer.Value;

                            var leftName = expression.Left as IdentifierNameSyntax;
                            var rightName = expression.Right as IdentifierNameSyntax;

                            if (leftName != null && rightName != null)
                            {
                                ret += ", and {0}".FormatWith(OperatorComment(leftName, rightName, expression.OperatorToken.Kind()));
                            }
                        }
                    }
                }
            }

            if (node is PropertyDeclarationSyntax)
            {
                var cur = node as PropertyDeclarationSyntax;

                bool isReadOnly = false;

                if (cur.AccessorList == null)
                {
                    isReadOnly = true;
                }
                else
                {
                    isReadOnly = !cur.AccessorList.Accessors.Any(d => d.IsKind(SyntaxKind.SetAccessorDeclaration));
                }
                

                if (IsList(cur.Type.ToFullString()) && cur.Type is GenericNameSyntax)
                {
                    var genericSyntax = cur.Type as GenericNameSyntax;
                    var generic = genericSyntax.TypeArgumentList.Arguments[0].ToFullString();

                    ret = "//Declaring a new {0}collection of {1}".FormatWith((isReadOnly ? "read-only " : ""), generic.Pluralize(true));
                }
                else
                {
                    ret = "//Declaring a new {0}{1} property".FormatWith((isReadOnly ? "read-only " : ""), cur.Type.ToFullString().Trim());
                }
                
            }

            return ret + Environment.NewLine;
        }
    }
}
