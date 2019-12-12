using CommandLine;
using CommandLine.Text;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace InternalAccessibleCompiler
{
    /// <summary>
    /// Command line options.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Output path. If it is empty, a dll is generated in the same path as csproj.
        /// </summary>
        [Option('o', "output", Required = false, Default = "", HelpText = "Output path. If it is empty, a dll is generated in the same path as csproj.")]
        public string Output { get; set; }


        /// <summary>
        /// Target assembly names separated by semicolons to access internally.
        /// </summary>
        [Option('a', "assemblyNames", Required = false, Separator = ';', HelpText = "Target assembly names separated by semicolons to access internally")]
        public IEnumerable<string> AssemblyNames { get; set; }

        /// <summary>
        /// Configuration.
        /// </summary>
        [Option('c', "configuration", Required = false, Default = OptimizationLevel.Release, HelpText = "Configuration")]
        public OptimizationLevel Configuration { get; set; }

        /// <summary>
        /// Logfile path.
        /// </summary>
        [Option('l', "logfile", Required = false, Default = "compile.log", HelpText = "Logfile path")]
        public string Logfile { get; set; }

        /// <summary>
        /// Input .csproj path.
        /// </summary>
        [Value(1, MetaName = "ProjectPath", HelpText = "Input .csproj path")]
        public string ProjectPath { get; set; }

        /// <summary>
        /// Usages.
        /// </summary>
        [Usage(ApplicationAlias = "InternalAccessibleCompiler")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Compile your project to internal accessible dll", new Options { Output = "your.dll", ProjectPath = "your.csproj" })
                };
            }
        }
    }
}