using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoxBot.Core.Services
{
    public class Result
    {
        public object ReturnValue { get; set; }
        public string Exception { get; set; }
        public string Code { get; set; }
        public string ExceptionType { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public TimeSpan CompileTime { get; set; }
        public string ConsoleOut { get; set; }
        public string ReturnTypeName { get; set; }
    }

    public class Globals
    {
        public StringWriter ConsoleOut;
        public StringWriter ErrorOut;
    }

    public class ReplService
    {
        public static async Task<Result> ExecuteAsync(string content, CancellationToken token)
        {
            List<string> references = new List<string>()
            {
                "System.Private.CoreLib",
                "System.Console",
                "System.Runtime"
            };

            SyntaxTree tree = CSharpSyntaxTree.ParseText(content, cancellationToken: token);
            var usings = tree.GetRoot(token).DescendantNodes().OfType<UsingDirectiveSyntax>().ToArray();
            foreach (var u in usings)
            {
                references.Add(u.Name.ToString());
            }

            Result result = null;
            object executionResult;
            var globals = new Globals { ConsoleOut = new StringWriter(), ErrorOut = new StringWriter() };
            Stopwatch compileTime = new Stopwatch();
            Stopwatch executionTime = new Stopwatch();

            try
            {
                var script = CSharpScript.Create("System.Console.SetOut(ConsoleOut);", globalsType: typeof(Globals));
                script.ContinueWith(content, ScriptOptions.Default.AddImports(references));
                script.ContinueWith("ConsoleOut.Flush();");

                // Compile the script
                compileTime.Start();
                script.Compile(token);
                compileTime.Stop();

                // Execute the script
                executionTime.Start();
                var returnObject = await script.RunAsync(globals, token);
                executionTime.Stop();
                executionResult = returnObject.ReturnValue;
            }
            catch (CompilationErrorException cee)
            {
                result = new Result
                {
                    ReturnValue = null,
                    Exception = cee.Message,
                    Code = content,
                    CompileTime = compileTime.Elapsed,
                    ExecutionTime = TimeSpan.FromSeconds(0),
                    ConsoleOut = string.Empty,
                    ExceptionType = cee.GetType().Name,
                    ReturnTypeName = string.Empty
                };
                return result;
            }

            result = new Result
            {
                ReturnValue = executionResult,
                Exception = string.Empty,
                Code = content,
                CompileTime = compileTime.Elapsed,
                ExecutionTime = executionTime.Elapsed,
                ConsoleOut = globals.ConsoleOut.ToString(),
                ExceptionType = string.Empty,
                ReturnTypeName = executionResult.GetType().Name
            };

            return result;
        }
    }
}
