using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace InternalAccessibleCompiler
{
    public class Compiler
    {
        /// <summary>
        /// Compile the project.
        /// </summary>
        public static int Compile(Options opt)
        {
            string inputCsProjPath = opt.ProjectPath;
            string inputCsProjDir = Path.GetDirectoryName(inputCsProjPath);
            string outputAsemblyPath = string.IsNullOrEmpty(opt.Output) ? Path.ChangeExtension(opt.ProjectPath, "dll") : opt.Output;
            string outputAsemblyName = Path.GetFileNameWithoutExtension(outputAsemblyPath);

            var log = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.File(opt.Logfile)
                    .CreateLogger();

            log.Information($"Input Project File: {inputCsProjPath}");
            log.Information($"Input Project Dir: {inputCsProjDir}");
            log.Information($"Output Asembly Path: {outputAsemblyPath}");
            log.Information($"Output Asembly Name: {outputAsemblyName}");
            log.Information($"Configuration: {opt.Configuration}");
            log.Information($"Logfile: {opt.Logfile}");
            log.Information($"AssemblyNames: {string.Join(", ", opt.AssemblyNames)}");

            var csproj = File.ReadAllLines(inputCsProjPath);

            // CSharpCompilationOptions
            // MetadataImportOptions.All
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: opt.Configuration)
                .WithMetadataImportOptions(MetadataImportOptions.All);

            // BindingFlags.IgnoreAccessibility
            typeof(CSharpCompilationOptions)
                .GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(compilationOptions, (uint)1 << 22);

            // Get all references.
            var reg_dll = new Regex("<HintPath>(.*)</HintPath>", RegexOptions.Compiled);
            var metadataReferences = csproj
                .Select(line => reg_dll.Match(line))
                .Where(match => match.Success)
                .Select(match => match.Groups[1].Value)
                .Select(path => MetadataReference.CreateFromFile(path));

            // Get all preprocessor symbols.
            var reg_preprocessorSymbols = new Regex("<DefineConstants>(.*)</DefineConstants>", RegexOptions.Compiled);
            var preprocessorSymbols = csproj
                .Select(line => reg_preprocessorSymbols.Match(line))
                .Where(match => match.Success)
                .SelectMany(match => match.Groups[1].Value.Split(';'))
                .Where(x => opt.Configuration != OptimizationLevel.Debug || x != "DEBUG");

            // Get all source codes.
            var parserOption = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: preprocessorSymbols);
            var reg_cs = new Regex("<Compile Include=\"(.*\\.cs)\"", RegexOptions.Compiled);
            var syntaxTrees = csproj
                .Select(line => reg_cs.Match(line))
                .Where(match => match.Success)
                .Select(match => match.Groups[1].Value.Replace('\\', Path.DirectorySeparatorChar))
                .Select(path => Path.Combine(inputCsProjDir, path))
                .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parserOption, path))
                .Concat(GetIgnoresAccessChecksToAttributeSyntaxTree(opt.AssemblyNames));

            // Start compiling.
            var result = CSharpCompilation.Create(outputAsemblyName, syntaxTrees, metadataReferences, compilationOptions)
                .Emit(outputAsemblyPath);

            // Output compile errors.
            foreach (var d in result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error))
            {
                log.Error(string.Format("{0} ({1}): {2} {3}", d.Severity, d.Id, d.GetMessage(), d.Location.GetMappedLineSpan()));
            }
            log.Information(result.Success ? "Success" : "Failed");

            return result.Success ? 0 : 1;
        }

        static IEnumerable<SyntaxTree> GetIgnoresAccessChecksToAttributeSyntaxTree(IEnumerable<string> assemblyNames)
        {
            if (!assemblyNames.Any())
                return Enumerable.Empty<SyntaxTree>();

            StringBuilder sb = new StringBuilder();
            foreach (var name in assemblyNames)
            {
                sb.AppendFormat("[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo(\"{0}\")]\n", name);
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