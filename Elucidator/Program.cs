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
        static SyntaxNode CommentNode(SyntaxNode node)
        {
            CommentRewriter rewriter = new CommentRewriter();
            SyntaxNode newSource = rewriter.Visit(node);
            SyntaxNode cleanSource = (CompilationUnitSyntax)Formatter.Format(newSource, MSBuildWorkspace.Create());
            
            return cleanSource;;
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string extension = Path.GetExtension(args[0]);

                if (extension != ".sln" && extension != ".cs")
                {
                    Console.WriteLine("Invalid filetype. Needs to be a visual studio solution (sln) or c# file (cs).");
                    Console.ReadLine();
                    return;
                }

                if (Directory.Exists("Elucidated_Project"))
                {
                    Directory.Delete("Elucidated_Project", true);
                }

                Directory.CreateDirectory("Elucidated_Project");

                if (extension == ".sln")
                {
                    var workspace = MSBuildWorkspace.Create();
                    var solution = workspace.OpenSolutionAsync(args[0]).Result;

                    Console.WriteLine("Loaded solution {0}", workspace.CurrentSolution.Id.Id);

                    foreach (var project in solution.Projects)
                    {
                        Console.WriteLine("\tParsing project {0}", project.Name);

                        foreach (var document in project.Documents)
                        {
                            Console.WriteLine("\t\tLoading document {0}", document.Name);
                            SyntaxTree tree = document.GetSyntaxTreeAsync().Result;

                            SyntaxNode cleanSource = CommentNode(tree.GetRoot());

                            try
                            {
                                List<string> path = new List<string>() { "Elucidated_Project" };
                                path.AddRange(document.Folders);
                                string folderpath = Path.Combine(path.ToArray());

                                if (!Directory.Exists(folderpath))
                                {
                                    Directory.CreateDirectory(folderpath);
                                }

                                File.WriteAllText(Path.Combine(folderpath, document.Name), cleanSource.ToFullString());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Couldn't save! {0}", ex.Message);
                            }

                        }
                    }
                }
                else
                {
                    string fileContent = File.ReadAllText(args[0]);
                    SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContent);
                    SyntaxNode singleCleanSource = CommentNode(tree.GetRoot());

                    Console.WriteLine(singleCleanSource.ToFullString());

                    try
                    {
                        File.WriteAllText("Elucidated_Project/" + Path.GetFileName(args[0]), singleCleanSource.ToFullString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Couldn't save! {0}", ex.Message);
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
