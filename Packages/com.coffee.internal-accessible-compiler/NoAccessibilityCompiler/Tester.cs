﻿using System;
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
using Debug = UnityEngine.Debug;
using UnityEditorInternal;
using UnityEditor.Scripting;
using UnityEditor.Scripting.Compilers;

namespace Coffee.BorderlessCompiler
{

    internal class SettingWizard : ScriptableWizard
    {
        //[SerializeField]
        //[Tooltip("Target assembly names separated by semicolons to access internally (eg. UnityEditor;UnityEditor.UI)")]
        //string m_AssemblyNamesToAccess = "";

        [SerializeField]
        string m_OutputPath;

        [SerializeField]
        bool m_AutoCompile;

        [SerializeField]
        [HideInInspector]
        string m_Guid;

        //[SerializeField]
        //[HideInInspector]
        //string _assetPath;

        //[SerializeField]
        //Setting m_Setting;

        //string[] _availableAssemblyNames = new string[0];

        void OnEnable()
        {
            helpString = "Generate a dll using borderless compiler\n"
                + "\n - Compile Automatically: Compile automatically"
                + "\n - Output Path: Output dll path (eg. Assets/Editor/SomeAssembly.dll)";
            maxSize = new Vector2(1600, 210);
            minSize = new Vector2(450, 210);

            if (string.IsNullOrEmpty(m_Guid))
            {
                var m_Setting = Setting.CreateFromAsmdef(AssetDatabase.GetAssetPath(Selection.activeObject));
                m_OutputPath = m_Setting.OutputPath;
                m_AutoCompile = m_Setting.AutoCompile;
                m_Guid = m_Setting.Guid;
            }

            //var asmdef = Selection.activeObject as AssemblyDefinitionAsset;
            //if (!asmdef)
            //{
            //    if (this)
            //        Close();
            //}

            //if

            //m_Setting = Setting.CreateFromAsmdef(AssetDatabase.GetAssetPath(Selection.activeObject));

            //m_OutputDllPath = m_Setting.OutputDllPath;
            //m_UseBorderlessCompiler = m_Setting.UseBorderlessCompiler;

            //_assetPath = AssetDatabase.GetAssetPath(asmdef);
            //_csprojName = JsonUtility.FromJson<AdfSetting>(asmdef.text).name;
            //var importer = AssetImporter.GetAtPath(_assetPath);

            //try
            //{
            //    var setting = JsonUtility.FromJson<CompileSetting>(importer.userData);
            //    m_AssemblyNamesToAccess = setting.AssemblyNamesToAccess;
            //    m_OutputDllPath = setting.OutputDllPath;
            //}
            //catch
            //{
            //    m_AssemblyNamesToAccess = _csprojName;
            //    m_OutputDllPath = Path.ChangeExtension(_assetPath, "dll");
            //}

            //_availableAssemblyNames = System.AppDomain.CurrentDomain.GetAssemblies()
            //    .Select(x => x.GetName().Name)
            //    .ToArray();
        }

        [MenuItem("Assets/Borderless Compiler/Setting", false)]
        static void OpenSettingWizard()
        {
            DisplayWizard<SettingWizard>("Borderless Compiler Settings", "Publish dll", "Save");
        }

        [MenuItem("Assets/Borderless Compiler/Setting", true)]
        static bool OpenSettingWizard_Valid()
        {
            return Selection.activeObject as AssemblyDefinitionAsset;
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
            return Selection.activeObject is AssemblyDefinitionAsset;
        }

        /// <summary>
        /// Start compile with internal accessible compiler.
        /// </summary>
        static void Compile(string asmdefFilePath, string dll)
        {


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
            OnWizardOtherButton();
        }

        /// <summary>
        /// Allows you to provide an action when the user clicks on the other button.
        /// </summary>
        void OnWizardOtherButton()
        {
            var setting = Setting.CreateFromAsmdef(AssetDatabase.GUIDToAssetPath(m_Guid));
            setting.OutputPath = m_OutputPath;
            setting.AutoCompile = m_AutoCompile;
            setting.Save();
        }

        /// <summary>
        /// This is called when the wizard is opened or whenever the user changes something in the wizard.
        /// </summary>
        void OnWizardUpdate()
        {
            // Output dll path is empty.
            if (string.IsNullOrEmpty(m_OutputPath.Trim()))
            {
                isValid = false;
                errorString = "Output dll path is empty";
                return;
            }

            isValid = true;
            errorString = "";
        }
    }


    [System.Serializable]
    internal class Setting
    {
        public string AssemblyNamesToAccess;
        public string OutputPath;
        public bool AutoCompile;
        public string Guid;

        public static Setting CreateFromAsmdef(string asmdefFilePath)
        {
            if (string.IsNullOrEmpty(asmdefFilePath) || !File.Exists(asmdefFilePath))
                return null;

            var importer = AssetImporter.GetAtPath(asmdefFilePath);
            if (importer == null)
                return null;

            Setting setting = null;
            try
            {
                setting = JsonUtility.FromJson<Setting>(importer.userData);

            }
            catch
            {
            }

            setting = setting ?? new Setting();
            if (string.IsNullOrEmpty(setting.OutputPath))
                setting.OutputPath = Path.ChangeExtension(asmdefFilePath, "dll");
            setting.Guid = AssetDatabase.AssetPathToGUID(asmdefFilePath);
            return setting;
        }

        public void Save()
        {
            var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(Guid));
            if (importer == null)
                return;

            var json = JsonUtility.ToJson(this);
            if (importer.userData != json)
            {
                importer.userData = json;
                importer.SaveAndReimport();
            }
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