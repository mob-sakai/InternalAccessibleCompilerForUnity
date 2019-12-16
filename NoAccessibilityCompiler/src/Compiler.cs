using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NoAccessibleCompiler
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

            log.Information($"Output Asembly Path: {opt.Out}");
            log.Information($"Configuration: {opt.Configuration}");
            log.Information($"Logfile: {opt.Logfile}");
            log.Information($"AssemblyNames: {string.Join(", ", opt.AssemblyNames)}");

            // CSharpCompilationOptions
            // MetadataImportOptions.All
            var compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    allowUnsafe: opt.Unsafe,
                    optimizationLevel: opt.Configuration,
                    deterministic: true
                )
                .WithMetadataImportOptions(MetadataImportOptions.All);

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
                .Concat(GetIgnoresAccessChecksToAttributeSyntaxTree(opt.AssemblyNames));

            // Start compiling.
            var result = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(opt.Out), syntaxTrees, metadataReferences, compilationOptions)
                .Emit(opt.Out, Path.ChangeExtension(opt.Out, "pdb"), Path.ChangeExtension(opt.Out, "xml"));

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
            foreach (var name in assemblyNames.Select(x=>Path.GetFileNameWithoutExtension(x)))
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