using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Elucidator
{
    public static class Misc
    {
        public static int GetOrderForType(object type)
        {
            if (type is VariableDeclarationSyntax)
            {
                return 0;
            }

            if (type is MethodDeclarationSyntax)
            {
                return 1;
            }

            if (type is FieldDeclarationSyntax)
            {
                return 2;
            }

            if (type is PropertyDeclarationSyntax)
            {
                return 3;
            }

            if (type is TypeDeclarationSyntax)
            {
                return 4;
            }

            if (type is NamespaceDeclarationSyntax)
            {
                return 5;
            }

            return -1;
        }
    }
}
