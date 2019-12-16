using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Coffee.InternalAccessibleCompiler
{
    [System.Serializable]
    internal class CompileSetting
    {
        public string AssemblyNamesToAccess;
        public string OutputDllPath;

        public static CompileSetting CreateFromAsmdef(string asmdefFilePath)
        {
            if (string.IsNullOrEmpty(asmdefFilePath) || !File.Exists(asmdefFilePath))
                return null;

            var importer = AssetImporter.GetAtPath(asmdefFilePath);
            if (importer == null)
                return null;

            return JsonUtility.FromJson<CompileSetting>(importer.userData);
        }
    }

    [System.Serializable]
    internal class AdfSetting
    {
        public string name = "";
    }

    internal class SettingWizard : ScriptableWizard
    {
        [SerializeField]
        [Tooltip("Target assembly names separated by semicolons to access internally (eg. UnityEditor;UnityEditor.UI)")]
        string m_AssemblyNamesToAccess = "";

        [SerializeField]
        [Tooltip("Output dll path")]
        string m_OutputDllPath;

        [SerializeField]
        [HideInInspector]
        string _csprojName;

        [SerializeField]
        [HideInInspector]
        string _assetPath;

        string[] _availableAssemblyNames = new string[0];

        void OnEnable()
        {
            helpString = "Generate a 'internal accessible' dll using InternalAccessibleCompiler.\n"
                + "\n - Assembly Names To Access:\n        Target assembly names separated by semicolons to access internally (eg. UnityEditor;UnityEditor.UI)"
                + "\n - OutputDllPath:\n        Output dll path (eg. Assets/Editor/SomeAssembly.dll)";
            maxSize = new Vector2(2000, 210);
            minSize = new Vector2(600, 210);

            var asmdef = Selection.activeObject as AssemblyDefinitionAsset;
            if (!asmdef)
            {
                if (this)
                    Close();
            }

            _assetPath = AssetDatabase.GetAssetPath(asmdef);
            _csprojName = JsonUtility.FromJson<AdfSetting>(asmdef.text).name;
            var importer = AssetImporter.GetAtPath(_assetPath);

            try
            {
                var setting = JsonUtility.FromJson<CompileSetting>(importer.userData);
                m_AssemblyNamesToAccess = setting.AssemblyNamesToAccess;
                m_OutputDllPath = setting.OutputDllPath;
            }
            catch
            {
                m_AssemblyNamesToAccess = _csprojName;
                m_OutputDllPath = Path.ChangeExtension(_assetPath, "dll");
            }

            _availableAssemblyNames = System.AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetName().Name)
                .ToArray();
        }

        [MenuItem("Assets/Internal Accessible Compiler/Setting", false)]
        static void OpenSettingWizard()
        {
            DisplayWizard<SettingWizard>("Internal Accessible Compiler Settings", "Compile", "Save");
        }

        [MenuItem("Assets/Internal Accessible Compiler/Setting", true)]
        static bool OpenSettingWizard_Valid()
        {
            return Selection.activeObject as AssemblyDefinitionAsset;
        }

        [MenuItem("Assets/Internal Accessible Compiler/Compile", false)]
        static void RunCompile()
        {
            var asmdef = Selection.activeObject as AssemblyDefinitionAsset;
            var assetPath = AssetDatabase.GetAssetPath(asmdef);
            var importer = AssetImporter.GetAtPath(assetPath);
            var csprojName = JsonUtility.FromJson<AdfSetting>(asmdef.text).name;
            var setting = JsonUtility.FromJson<CompileSetting>(importer.userData);
            Compile(csprojName + ".csproj", Path.GetFullPath(setting.OutputDllPath), setting.AssemblyNamesToAccess);
        }

        [MenuItem("Assets/Internal Accessible Compiler/Compile", true)]
        static bool RunCompile_Valid()
        {
            if (!(Selection.activeObject is AssemblyDefinitionAsset))
                return false;

            try
            {
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
                var setting = JsonUtility.FromJson<CompileSetting>(importer.userData);
                return !string.IsNullOrEmpty(setting.AssemblyNamesToAccess) && !string.IsNullOrEmpty(setting.OutputDllPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Start compile with internal accessible compiler.
        /// </summary>
        static void Compile(string proj, string dll, string assemblyNames)
        {
            // Generate/update C# project.
            System.Type.GetType("UnityEditor.SyncVS, UnityEditor")
                .GetMethod("SyncSolution", BindingFlags.Static | BindingFlags.Public)
                .Invoke(null, new object[0]);

            // Not found C# project.
            if (!File.Exists(proj))
            {
                UnityEngine.Debug.LogErrorFormat("Not found C# project file: {0}", proj);
                return;
            }

            EditorUtility.DisplayProgressBar("Internal Accessible Compiler", "Compiling " + Path.GetFileName(dll), 0.5f);

            var compiler = "Packages/com.coffee.internal-accessible-compiler/Compiler~/Compiler.csproj";
            var args = string.Format("\"{0}\" -o \"{1}\" -a \"{2}\"", Path.GetFullPath(proj), Path.GetFullPath(dll), assemblyNames);
            DotNet.Run(compiler, args, (success, stdout) =>
            {
                EditorUtility.ClearProgressBar();
                if (!success)
                {
                    UnityEngine.Debug.LogError("Compile Failed");
                    UnityEngine.Debug.LogError(stdout);
                    return;
                }

                UnityEngine.Debug.Log("Compile Complete! ");
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.ImportAsset(dll, ImportAssetOptions.ForceUpdate);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                };
            }, "Internal Accessible Compiler");
        }

        /// <summary>
        /// This is called when the user clicks on the Create button.
        /// </summary>
        void OnWizardCreate()
        {
            // Run compile.
            Compile(_csprojName + ".csproj", Path.GetFullPath(m_OutputDllPath), m_AssemblyNamesToAccess);
            OnWizardOtherButton();
        }

        /// <summary>
        /// Allows you to provide an action when the user clicks on the other button.
        /// </summary>
        void OnWizardOtherButton()
        {
            // Save compile setting.
            var importer = AssetImporter.GetAtPath(_assetPath);
            var json = JsonUtility.ToJson(new CompileSetting() { AssemblyNamesToAccess = m_AssemblyNamesToAccess, OutputDllPath = m_OutputDllPath });
            if (importer.userData != json)
            {
                importer.userData = json;
                importer.SaveAndReimport();
            }
        }

        /// <summary>
        /// This is called when the wizard is opened or whenever the user changes something in the wizard.
        /// </summary>
        void OnWizardUpdate()
        {
            // Assembly is not found current app domain.
            var notFoundAssemblies = m_AssemblyNamesToAccess
                .Split(',', ';', ' ')
                .Except(_availableAssemblyNames)
                .ToArray();

            if (notFoundAssemblies.Any())
            {
                isValid = false;
                errorString = string.Format("Not found assembly name(s): {0}", string.Join(", ", notFoundAssemblies));
                return;
            }

            // Output dll path is empty.
            if (string.IsNullOrEmpty(m_OutputDllPath.Trim()))
            {
                isValid = false;
                errorString = "Output dll path is empty";
                return;
            }

            isValid = true;
            errorString = "";
        }
    }
}