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
using UnityEditor;
using UnityEditor.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using UnityEditorInternal;
using UnityEditor.Scripting;
using UnityEditor.Scripting.Compilers;
using UnityEditor.Compilation;

namespace Coffee.BorderlessCompiler
{

    internal class SettingWizard : ScriptableWizard
    {
        [SerializeField]
        [FormerlySerializedAs("OutputDllPath")]
        string m_PublishPath;

        [SerializeField]
        bool m_AutoCompile;

        [SerializeField]
        [HideInInspector]
        string m_Guid;

        void OnEnable()
        {
            helpString = "Generate a dll using borderless compiler\n"
                + "\n - Compile Automatically: Compile automatically"
                + "\n - Output Path: Output dll path (eg. Assets/Editor/SomeAssembly.dll)";
            maxSize = new Vector2(1600, 210);
            minSize = new Vector2(450, 210);

            if (string.IsNullOrEmpty(m_Guid))
            {
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                var m_Setting = Setting.CreateFromAsmdef(path);
                m_PublishPath = m_Setting.PublishPath;
                m_AutoCompile = m_Setting.AutoCompile;
                m_Guid = AssetDatabase.AssetPathToGUID(path);
            }
        }

        [MenuItem("Assets/Borderless Compiler/Setting", false)]
        static void OpenSettingWizard()
        {
            DisplayWizard<SettingWizard>("Borderless Compiler Settings", "Publish dll", "Save");
        }

        [MenuItem("Assets/Borderless Compiler/Setting", true)]
        static bool OpenSettingWizard_Valid()
        {
            return Selection.activeObject as AssemblyDefinitionAsset && !EditorApplication.isCompiling;
        }

        [MenuItem("Assets/Borderless Compiler/Publish dll", false)]
        static void RunCompile()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var setting = Setting.CreateFromAsmdef(assetPath);
            //Compile(csprojName + ".csproj", Path.GetFullPath(setting.OutputDllPath), setting.AssemblyNamesToAccess);
        }

        [MenuItem("Assets/Borderless Compiler/Publish dll", true)]
        static bool RunCompile_Valid()
        {
            return OpenSettingWizard_Valid();
        }

        /// <summary>
        /// Start compile with internal accessible compiler.
        /// </summary>
        static void Publish(string asmdefPath)
        {
            var setting = Setting.CreateFromAsmdef(asmdefPath);
            var assemblyName = JsonUtility.FromJson<AdfData>(File.ReadAllText(asmdefPath)).name;
            var assembly = CompilationPipeline.GetAssemblies().FirstOrDefault(x => x.name == assemblyName);

            Debug.LogFormat("{0}, {1}, {2}",
                assembly.name,
                assembly.outputPath,
                string.Join(", ", assembly.sourceFiles.Select(x => Path.GetFileName(x)).ToArray())
                );

            var arguments = new List<string>();
            arguments.Add("/out:" + setting.PublishPath);

            if (assembly.compilerOptions.AllowUnsafeCode)
                arguments.Add("/unsafe");

            Func<string, IEnumerable<string>, IEnumerable<string>> optionFormatter = (opt, files) => files.Select(x => "/" + opt + ":" + CommandLineFormatter.PrepareFileName(x));

            arguments.AddRange(assembly.allReferences.Select(x => "/reference:" + CommandLineFormatter.PrepareFileName(x)));
            arguments.AddRange(assembly.defines.Distinct().Select(x => "/define:" + CommandLineFormatter.PrepareFileName(x)));
            arguments.AddRange(assembly.sourceFiles.Select(x => Paths.UnifyDirectorySeparator(CommandLineFormatter.PrepareFileName(x))));

            var responseFile = CommandLineFormatter.GenerateResponseFile(arguments);
            Debug.Log(responseFile);

            //// Generate/update C# project.
            //System.Type.GetType("UnityEditor.SyncVS, UnityEditor")
            //    .GetMethod("SyncSolution", BindingFlags.Static | BindingFlags.Public)
            //    .Invoke(null, new object[0]);

            //// Not found C# project.
            //if (!File.Exists(proj))
            //{
            //    UnityEngine.Debug.LogErrorFormat("Not found C# project file: {0}", proj);
            //    return;
            //}

            //EditorUtility.DisplayProgressBar("Internal Accessible Compiler", "Compiling " + Path.GetFileName(dll), 0.5f);

            //var compiler = "Packages/com.coffee.internal-accessible-compiler/Compiler~/Compiler.csproj";
            //var args = string.Format("\"{0}\" -o \"{1}\" -a \"{2}\"", Path.GetFullPath(proj), Path.GetFullPath(dll), assemblyNames);
            //DotNet.Run(compiler, args, (success, stdout) =>
            //{
            //    EditorUtility.ClearProgressBar();
            //    if (!success)
            //    {
            //        UnityEngine.Debug.LogError("Compile Failed");
            //        UnityEngine.Debug.LogError(stdout);
            //        return;
            //    }

            //    UnityEngine.Debug.Log("Compile Complete! ");
            //    EditorApplication.delayCall += () =>
            //    {
            //        AssetDatabase.ImportAsset(dll, ImportAssetOptions.ForceUpdate);
            //        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            //    };
            //}, "Internal Accessible Compiler");
        }

        /// <summary>
        /// This is called when the user clicks on the Create button.
        /// </summary>
        void OnWizardCreate()
        {

            // Run compile.
            //Compile(_csprojName + ".csproj", Path.GetFullPath(m_OutputDllPath), m_AssemblyNamesToAccess);
            

            var assemblyName = JsonUtility.FromJson<AdfData>(File.ReadAllText(AssetDatabase.GUIDToAssetPath(m_Guid))).name;
            var assembly = CompilationPipeline.GetAssemblies().FirstOrDefault(x => x.name == assemblyName);

            Debug.LogFormat("{0}, {1}, {2}",
                assembly.name,
                assembly.outputPath,
                string.Join(", ", assembly.sourceFiles.Select(x => Path.GetFileName(x)).ToArray())
                );

            var arguments = new List<string>();
            arguments.Add("/out:" + assembly.outputPath);

            if (assembly.compilerOptions.AllowUnsafeCode)
                arguments.Add("/unsafe");

            Func<string, IEnumerable<string>, IEnumerable<string>> optionFormatter = (opt, files) => files.Select(x => "/" + opt + ":" + CommandLineFormatter.PrepareFileName(x));

            arguments.AddRange(assembly.allReferences.Select(x=>"/reference:" + CommandLineFormatter.PrepareFileName(x)));
            arguments.AddRange(assembly.defines.Distinct().Select(x=> "/define:" + CommandLineFormatter.PrepareFileName(x)));
            arguments.AddRange(assembly.sourceFiles.Select(x=> Paths.UnifyDirectorySeparator(CommandLineFormatter.PrepareFileName(x))));

            var responseFile = CommandLineFormatter.GenerateResponseFile(arguments);
            Debug.Log(responseFile);

            Save();
        }

        /// <summary>
        /// Allows you to provide an action when the user clicks on the other button.
        /// </summary>
        void OnWizardOtherButton()
        {
            Save();
        }

        /// <summary>
        /// This is called when the wizard is opened or whenever the user changes something in the wizard.
        /// </summary>
        void OnWizardUpdate()
        {
            isValid = false;
            // Output dll path is empty.
            if (string.IsNullOrEmpty(m_PublishPath.Trim()))
            {
                errorString = "Output dll path is empty";
            }
            // Output dll directory does not exist.
            else if (Directory.Exists(m_PublishPath))
            {
                errorString = "Output dll directory does not exist.";
            }
            // Output extension is not dll.
            else if (!m_PublishPath.EndsWith(".dll"))
            {
                errorString = "Output extension is not dll.";
            }
            else
            {
                isValid = !EditorApplication.isCompiling;
                errorString = "";
            }
        }

        public void Save()
        {
            var path = AssetDatabase.GUIDToAssetPath(m_Guid);
            var setting = Setting.CreateFromAsmdef(path);
            setting.PublishPath = m_PublishPath;
            setting.AutoCompile = m_AutoCompile;

            var importer = AssetImporter.GetAtPath(path);
            if (importer == null)
                return;

            var json = JsonUtility.ToJson(setting);
            if (importer.userData != json)
            {
                importer.userData = json;
                importer.SaveAndReimport();
            }
        }

        [MenuItem("test/hoge")]
        static void hoge()
        {
            foreach (var a in CompilationPipeline.GetAssemblies(AssembliesType.Editor))
            {
                Debug.LogFormat("{0}, {1}, {2}",
                    a.name,
                    a.outputPath,
                    string.Join(", ", a.sourceFiles.Select(x => Path.GetFileName(x)).ToArray())
                    );

                var arguments = new List<string>();

                arguments.Add("/out:"+a.outputPath);

                if (a.compilerOptions.AllowUnsafeCode)
                    arguments.Add("/unsafe");

                foreach (string dll in a.allReferences)
                    arguments.Add("/reference:" + CommandLineFormatter.PrepareFileName(dll));

                foreach (string define in a.defines.Distinct())
                    arguments.Add("/define:" + CommandLineFormatter.PrepareFileName(define));

                foreach (var source in a.sourceFiles)
                {
                    var f = CommandLineFormatter.PrepareFileName(source);
                    arguments.Add(Paths.UnifyDirectorySeparator(f));
                }

                var responseFile = CommandLineFormatter.GenerateResponseFile(arguments);
                Debug.Log(responseFile);
            }
        }

    }

    [System.Serializable]
    internal class AdfData
    {
        public string name;
    }

    [System.Serializable]
    internal class Setting
    {
        public string PublishPath;
        public bool AutoCompile;

        public static Setting CreateFromAsmdef(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            var importer = AssetImporter.GetAtPath(path);
            if (importer == null)
                return null;

            Setting setting = new Setting();
            try
            {
                setting = JsonUtility.FromJson<Setting>(importer.userData);
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(setting.PublishPath))
                setting.PublishPath = Path.ChangeExtension(path, "dll");
            return setting;
        }

    }


    [InitializeOnLoad]
    internal class Bootstrap
    {
        static Bootstrap()
        {
            var language = new Language();
            ScriptCompilers.SupportedLanguages.RemoveAll(x => x.GetType() == typeof(Language));
            ScriptCompilers.SupportedLanguages.Insert(0, language);
            typeof(ScriptCompilers).GetField("CSharpSupportedLanguage", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, language);

            var pa = EditorBuildRules.GetPredefinedTargetAssemblies();
            foreach (var paa in pa)
            {
                if (paa != null && paa.Language != null && paa.Language.GetType() == typeof(CSharpLanguage))
                {
                    paa.Language = language;
                }
            }
        }
    }

    internal class Language : CSharpLanguage
    {
        public override ScriptCompilerBase CreateCompiler(ScriptAssembly scriptAssembly, MonoIsland island, bool buildingForEditor, BuildTarget targetPlatform, bool runUpdater)
        {
            // Not asmdef -> default compiler
            if (string.IsNullOrEmpty(scriptAssembly.OriginPath))
                return base.CreateCompiler(scriptAssembly, island, buildingForEditor, targetPlatform, runUpdater);

            // Do not use borderless compiler -> default compiler
            var asmdefPath = Directory.GetFiles(scriptAssembly.OriginPath, "*.asmdef").FirstOrDefault();

            Debug.Log(asmdefPath);
            var setting = Setting.CreateFromAsmdef(asmdefPath);
            if (setting == null || !setting.AutoCompile)
                return base.CreateCompiler(scriptAssembly, island, buildingForEditor, targetPlatform, runUpdater);

            // Use borderless compiler
            return new Compiler(island, runUpdater);
        }
    }

    internal class Compiler : MicrosoftCSharpCompiler
    {
        public Compiler(MonoIsland island, bool runUpdater) : base(island, runUpdater)
        {
        }

        protected override Program StartCompiler()
        {
            // Kill previous process.
            var p = base.StartCompiler();
            p.Kill();

            // Get last responsefile.
            var outopt = "out:" + PrepareFileName(m_Island._output);
            var responsefile = Directory.GetFiles("Temp", "UnityTempFile*")
                    .OrderByDescending(f => File.GetLastWriteTime(f))
                    .First(file => File.ReadAllLines(file).First(x => x.Contains("out:")).Contains(outopt));

            Debug.Log("pick - " + responsefile);

            // Start compiling with dotnet app
            const string compiler = "Packages/com.coffee.internal-accessible-compiler/NoAccessibilityCompiler~";
            var psi = new ProcessStartInfo()
            {
                Arguments = string.Format("run -p {0} -- {1}", compiler, responsefile),
                FileName = "dotnet",
                CreateNoWindow = true
            };

            // On MacOS or Linux, PATH environmant is not correct.
            if (Application.platform != RuntimePlatform.WindowsEditor)
                psi.FileName = "/usr/local/share/dotnet/dotnet";

            var program = new Program(psi);
            program.Start();

            return program;
        }
    }
}
