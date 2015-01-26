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

        public static string GetComment(SyntaxNode node)
        {
            string ret = "//This is a {0}".FormatWith(node.CSharpKind().Humanize());

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

                ret = "//Defining the {0} {1}".FormatWith(cur.Identifier.ValueText.Humanize(), type);
            }

            if (node is MethodDeclarationSyntax)
            {
                var cur = node as MethodDeclarationSyntax;

                string returnType = cur.ReturnType.ToFullString().Trim();
                if (returnType == "void")
                {
                    ret = "//Declaring the {0} method".FormatWith(cur.Identifier.ValueText.Humanize());
                }
                else
                {
                    if (IsList(cur.Identifier.GetPreviousToken().ValueText) && cur.ReturnType is GenericNameSyntax)
                    {
                        var genericSyntax = cur.ReturnType as GenericNameSyntax;
                        var generic = genericSyntax.TypeArgumentList.Arguments[0].ToFullString();

                        ret = "//Declaring the {0} method, returns a list of {1}".FormatWith(cur.Identifier.ValueText.Humanize(), generic.Pluralize(true));
                    }
                    else
                    {
                        ret = "//Declaring the {0} method, returns a {1}".FormatWith(cur.Identifier.ValueText.Humanize(), cur.ReturnType.ToFullString().Humanize());
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
                    ret = "//A new {0} field".FormatWith(cur.Declaration.Type.ToFullString().Humanize());
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
                ret = "//Declaring the {0} enumeration.".FormatWith(cur.Identifier.ToFullString().Trim().Humanize());
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
                }
            }

            if (node is PropertyDeclarationSyntax)
            {
                var cur = node as PropertyDeclarationSyntax;

                bool isReadOnly = !cur.AccessorList.Accessors.Any(d => d.IsKind(SyntaxKind.SetAccessorDeclaration));

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
