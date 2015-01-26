using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;

namespace Elucidator
{
    public class Program
    {
        static void Main(string[] args)
        {
            CommentRewriter rewriter = new CommentRewriter();

            if (args.Length > 0)
            {
                string extension = Path.GetExtension(args[0]);

                if (extension != ".sln")
                {
                    Console.WriteLine("Invalid filetype. Needs to be a visual studio solution (sln).");
                }

                var workspace = MSBuildWorkspace.Create();
                var solution = workspace.OpenSolutionAsync(args[0]).Result;
                
                Console.WriteLine("Loaded solution {0}", workspace.CurrentSolution.Id.Id);

                if (Directory.Exists("Elucidated_Project"))
                {
                    Directory.Delete("Elucidated_Project", true);
                }

                Directory.CreateDirectory("Elucidated_Project");

                foreach (var project in solution.Projects)
                {
                    Console.WriteLine("\tParsing project {0}", project.Name);
                    
                    foreach (var document in project.Documents)
                    {
                        Console.WriteLine("\t\tLoading document {0}", document.Name);
                        SyntaxTree tree = document.GetSyntaxTreeAsync().Result;

                        SyntaxNode newSource = rewriter.Visit(tree.GetRoot());
                        SyntaxNode cleanSource = SyntaxTreeHelper.FormatTree(newSource);

                        try
                        {
                            File.WriteAllText("Elucidated_Project/" + document.Name, cleanSource.ToFullString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Couldn't save! {0}", ex.Message);
                            throw;
                        }
  
                    }
                }

                //workspace.TryApplyChanges(solution);
            }
            else
            {
                Console.WriteLine("You need to pass the path to the solution as an argument.");
            }

            Console.ReadLine();
        }
    }
}
