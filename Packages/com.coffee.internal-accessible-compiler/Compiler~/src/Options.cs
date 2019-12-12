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
		/// Output path.
		/// </summary>
		[Option('o', "output", Required = true, Default = "", HelpText = "Output path")]
		public string Output { get; set; }


		/// <summary>
		/// Assembly names to access internal.
		/// </summary>
		[Option('a', "assemblyNames", Required = true, Default = "", Separator = ';', HelpText = "Assembly names to access internal")]
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