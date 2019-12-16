using System;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEditor.Scripting.ScriptCompilation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.Modules;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEditor.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityEditor.Scripting.Compilers
{
    [InitializeOnLoad]
    internal class RoslynCompilerBootstrap
    {
        static RoslynCompilerBootstrap()
        {
            UnityEngine.Debug.Log("RoslynCompilerBootstrap!!!!");
            var language = new CustomCSharpLanguage();
            ScriptCompilers.SupportedLanguages.RemoveAll(x => x.GetType() == typeof(CustomCSharpLanguage));
            ScriptCompilers.SupportedLanguages.Insert(0, language);
            typeof(ScriptCompilers).GetField("CSharpSupportedLanguage", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, language);


            var pa = EditorBuildRules.GetPredefinedTargetAssemblies();
            foreach (var paa in pa)
            {
                if (paa.Language.GetType() == typeof(CSharpLanguage))
                {
                    paa.Language = language;
                }
            }
        }

        internal class CustomCSharpLanguage : CSharpLanguage
        {
            public override ScriptCompilerBase CreateCompiler(ScriptAssembly scriptAssembly, MonoIsland island, bool buildingForEditor, BuildTarget targetPlatform, bool runUpdater)
            {
                Debug.Log(scriptAssembly.OriginPath);
                UnityEngine.Debug.Log("CustomCSharpLanguage.CreateCompiler!!!!");

                // 条件マッチする場合、特殊コンパイラをつかう
                if (scriptAssembly.OriginPath == "Assets/Editor/")
                    return new NoAccessibilityCSharpCompiler(island, runUpdater);

                // それ以外はデフォ
                return base.CreateCompiler(scriptAssembly, island, buildingForEditor, targetPlatform, runUpdater);
            }
        }
    }

    internal class NoAccessibilityCSharpCompiler : MicrosoftCSharpCompiler
    {
        public NoAccessibilityCSharpCompiler(MonoIsland island, bool runUpdater) : base(island, runUpdater)
        {
        }

        BuildTarget BuildTarget { get { return m_Island._target; } }

        private void FillCompilerOptions(List<string> arguments, out string argsPrefix)
        {
            // This will ensure that csc.exe won't include csc.rsp
            // csc.rsp references .NET 4.5 assemblies which cause conflicts for us
            argsPrefix = "/noconfig ";
            arguments.Add("/nostdlib+");

            // Case 755238: Always use english for outputing errors, the same way as Mono compilers do
            arguments.Add("/preferreduilang:en-US");
            arguments.Add("/langversion:latest");

            var platformSupportModule = ModuleManager.FindPlatformSupportModule(ModuleManager.GetTargetStringFromBuildTarget(BuildTarget));
            if (platformSupportModule != null && !m_Island._editor)
            {
                var compilationExtension = platformSupportModule.CreateCompilationExtension();

                arguments.AddRange(compilationExtension.GetAdditionalAssemblyReferences().Select(r => "/reference:\"" + r + "\""));
                arguments.AddRange(compilationExtension.GetWindowsMetadataReferences().Select(r => "/reference:\"" + r + "\""));
                arguments.AddRange(compilationExtension.GetAdditionalDefines().Select(d => "/define:" + d));
                arguments.AddRange(compilationExtension.GetAdditionalSourceFiles());
            }
        }

        private static void ThrowCompilerNotFoundException(string path)
        {
            throw new Exception(string.Format("'{0}' not found. Is your Unity installation corrupted?", path));
        }

        private Program StartCompilerImpl(List<string> arguments, string argsPrefix)
        {

            Debug.LogFormat("hogehoge!!!!!");

            foreach (string dll in m_Island._references)
                arguments.Add("/reference:" + PrepareFileName(dll));

            foreach (string define in m_Island._defines.Distinct())
                arguments.Add("/define:" + define);

            var filePathMappings = new List<string>(m_Island._files.Length);
            foreach (var source in m_Island._files)
            {
                var f = PrepareFileName(source);
                f = Paths.UnifyDirectorySeparator(f);
                arguments.Add(f);

                if (f != source)
                    filePathMappings.Add(f + " => " + source);
            }

            var csc = Paths.Combine(EditorApplication.applicationContentsPath, "Tools", "RoslynScripts", "unity_csc");
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                csc += ".bat";
            }
            else
            {
                csc += ".sh";
            }

            csc = Paths.UnifyDirectorySeparator(csc);

            //if (!File.Exists(csc))
            //	ThrowCompilerNotFoundException(csc);

            var responseFiles = (m_Island._responseFiles != null)
                ? m_Island._responseFiles.ToDictionary(Path.GetFileName)
                : new Dictionary<string, string>();

            KeyValuePair<string, string> obsoleteResponseFile = responseFiles
                .SingleOrDefault(x => CompilerSpecificResponseFiles.MicrosoftCSharpCompilerObsolete.Contains(x.Key));
            if (!string.IsNullOrEmpty(obsoleteResponseFile.Key))
            {
                Debug.LogWarningFormat("Using obsolete custom response file '{0}'. Please use '{1}' instead.", obsoleteResponseFile.Key, CompilerSpecificResponseFiles.MicrosoftCSharpCompiler);
            }

            foreach (var file in responseFiles)
            {
                AddResponseFileToArguments(arguments, file.Value);
            }

            var responseFile = CommandLineFormatter.GenerateResponseFile(arguments);

            RunAPIUpdaterIfRequired(responseFile, filePathMappings);

            //此処から先のみ変更
            foreach (var a in arguments)
            {
                Debug.Log(a);
            }


            Regex regOption = new Regex("^/([^:]+):?(.+)*", RegexOptions.Compiled);
            var dic = arguments
                .Select(x => regOption.Match(x))
                .Where(x => x.Success)
                .Select(x => new KeyValuePair<string, string>(x.Groups[1].Value, x.Groups[2].Value))
                .GroupBy(x => x.Key, x => x.Value)
                .ToDictionary(x => x.Key, x => string.Join(",", x.ToArray()));

            var csFiles = string.Join(" ", arguments.Where(x => !regOption.IsMatch(x)).ToArray());

            foreach (var a in dic)
            {
                Debug.LogFormat("{0}: {1}", a.Key, a.Value);
            }

            Debug.LogFormat("-o {0} -r {1} -d {2} {3}", dic["out"], dic["reference"], dic["define"], csFiles);

            var psi = new ProcessStartInfo()
            {
                Arguments = string.Format("-o {0} -r {1} -d {2} {3}", dic["out"], dic["reference"], dic["define"], csFiles),
                FileName = "Packages/com.coffee.internal-accessible-compiler/NoAccessibilityCompiler~/bin/NoAccessibilityCompiler-1.0.0-osx-x64",
                CreateNoWindow = true
            };
            var program = new Program(psi);
            program.Start();

            return program;
        }

        protected override Program StartCompiler()
        {
            var outputPath = PrepareFileName(m_Island._output);

            // Always build with "/debug:pdbonly", "/optimize+", because even if the assembly is optimized
            // it seems you can still succesfully debug C# scripts in Visual Studio
            var arguments = new List<string>
            {
                "/target:library",
                "/nowarn:0169",
                "/out:" + outputPath
            };

            if (m_Island._allowUnsafeCode)
                arguments.Add("/unsafe");

            arguments.Add("/debug:portable");

            var disableOptimizations = m_Island._development_player || (m_Island._editor && EditorPrefs.GetBool("AllowAttachedDebuggingOfEditor", true));
            if (!disableOptimizations)
            {
                arguments.Add("/optimize+");
            }
            else
            {
                arguments.Add("/optimize-");
            }

            string argsPrefix;
            FillCompilerOptions(arguments, out argsPrefix);
            return StartCompilerImpl(arguments, argsPrefix);
        }

        //protected override string[] GetSystemReferenceDirectories()
        //{
        //	return MonoLibraryHelpers.GetSystemReferenceDirectories(m_Island._api_compatibility_level);
        //}

        //protected override string[] GetStreamContainingCompilerMessages()
        //{
        //	return GetStandardOutput();
        //}

        //protected override CompilerOutputParserBase CreateOutputParser()
        //{
        //	return new MicrosoftCSharpCompilerOutputParser();
        //}
    }
}
