using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NoAccessibilityCompiler
{
    public class Compiler
    {
        /// <summary>
        /// Compile the project.
        /// </summary>
        public static int Compile(Options opt)
        {
            string outputAsemblyPath = opt.Out;
            string outputAsemblyName = Path.GetFileNameWithoutExtension(opt.Out);

            var log = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.File(opt.Logfile)
                    .CreateLogger();

            if(!string.IsNullOrEmpty(opt.ResponseFile))
            {
                var arguments = File.ReadAllLines(opt.ResponseFile);
                Regex regOption = new Regex("^/([^:]+):?(.+)*", RegexOptions.Compiled);
                var dic = arguments
                    .Select(x => regOption.Match(x))
                    .Where(x => x.Success)
                    .Select(x => new KeyValuePair<string, string>(x.Groups[1].Value, x.Groups[2].Value.Trim('"')))
                    .GroupBy(x => x.Key, x => x.Value)
                    .ToDictionary(x => x.Key, x => x.ToArray());


                opt.Out = dic.ContainsKey("out") ? dic["out"].First() : Path.ChangeExtension(opt.ResponseFile, "dll");
                opt.References = dic["reference"];
                opt.Defines = dic["define"];
                opt.InputPaths = arguments.Where(x => !regOption.IsMatch(x)).Select(x=>x.Trim('"')).ToArray();
            }

            string assemblyName = Regex.Replace(Path.GetFileName(opt.Out), "(.*)\\.dll", "$1");
            string outputDir = Path.GetDirectoryName(opt.Out);
            Encoding encoding = Encoding.UTF8;

            //log.Information($"Output Asembly Path: {opt.Out}");
            //log.Information($"Configuration: {opt.Configuration}");
            //log.Information($"Logfile: {opt.Logfile}");
            //log.Information($"Defines: {string.Join(", ", opt.Defines)}");
            //log.Information($"References: {string.Join(", ", opt.References)}");
            //log.Information($"Sources: {string.Join(", ", opt.InputPaths)}");

            // CSharpCompilationOptions
            // MetadataImportOptions.All
            var compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    allowUnsafe: opt.Unsafe,
                    optimizationLevel: opt.Configuration,
                    deterministic: true
                )
                .WithMetadataImportOptions(MetadataImportOptions.All)
                .WithPlatform(Platform.AnyCpu);

            // BindingFlags.IgnoreAccessibility
            typeof(CSharpCompilationOptions)
                .GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(compilationOptions, (uint)1 << 22);

            // Get all references.
            IEnumerable<PortableExecutableReference> metadataReferences = opt.References
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToArray();

            // Get all preprocessor symbols.
            IEnumerable<string> preprocessorSymbols = opt.Defines
                .Where(x => opt.Configuration != OptimizationLevel.Debug || x != "DEBUG")
                .ToArray();

            // Get all source codes.
            CSharpParseOptions parserOption = new CSharpParseOptions(opt.LanguageVersion, preprocessorSymbols: preprocessorSymbols);
            IEnumerable<SyntaxTree> syntaxTrees = opt.InputPaths
                .Where(x=>x.EndsWith(".cs"))
                .Select(path=>CSharpSyntaxTree.ParseText(File.ReadAllText(path), parserOption, path))
                .Concat(GetIgnoresAccessChecksToAttributeSyntaxTree(opt.References.Select(x => Path.GetFileNameWithoutExtension(x))));

            // Start compiling.
            var result = CSharpCompilation.Create(assemblyName, syntaxTrees, metadataReferences, compilationOptions)
                .Emit(opt.Out);
                //.Emit(opt.Out, Path.Combine(outputDir, assemblyName + ".pdb"));

            // Output compile errors.
            foreach (var d in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
            {
                var line = d.Location.GetMappedLineSpan();
                Console.WriteLine(string.Format("{0}({1}): {2} {3}: {4}", line.Path, line.StartLinePosition, d.Severity.ToString().ToLower(), d.Id, d.GetMessage()));
            }
            log.Information(result.Success ? "Success" : "Failed");

            return result.Success ? 0 : 1;
        }

        static IEnumerable<SyntaxTree> GetIgnoresAccessChecksToAttributeSyntaxTree(IEnumerable<string> referencePaths)
        {
            if (!referencePaths.Any())
                return Enumerable.Empty<SyntaxTree>();

            StringBuilder sb = new StringBuilder();
            foreach (var path in referencePaths)
            {
                string assemblyName = Regex.Replace(Path.GetFileName(path), "(.*)\\.dll", "$1");
                sb.AppendFormat("[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo(\"{0}\")]\n", assemblyName);
            }

            sb.AppendLine("namespace System.Runtime.CompilerServices");
            sb.AppendLine("{");
            sb.AppendLine("    [AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]");
            sb.AppendLine("    internal class IgnoresAccessChecksToAttribute : System.Attribute");
            sb.AppendLine("    {");
            sb.AppendLine("        public IgnoresAccessChecksToAttribute(string assemblyName)");
            sb.AppendLine("        {");
            sb.AppendLine("            AssemblyName = assemblyName;");
            sb.AppendLine("        }");
            sb.AppendLine("        public string AssemblyName { get; }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return new SyntaxTree[] { CSharpSyntaxTree.ParseText(sb.ToString()) };
        }
    }
}