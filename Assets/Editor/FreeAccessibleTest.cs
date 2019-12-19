using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Compilers;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor.Utils;

namespace hogehoge
{
    internal class LanguageX : Program
    {
    }


}

namespace Coffee.FreeAccessible
{
    public class FreeAccessibleTest
    {
        [MenuItem("FreeAccessibleTest/CallDelayed 10s")]
        static void RegisterCallDelayed()
        {
            EditorApplication.CallDelayed(CallDelayed, 2);
        }

        static void CallDelayed()
        {
            Debug.Log(
            string.Join(", ",
            typeof(CSharpLanguage)
                .Assembly.GetCustomAttributes<InternalsVisibleToAttribute>()
                .Select(x => x.AssemblyName)
                .ToArray()
                )
            );

            Debug.Log("2 seconds!");
            Debug.Log(EditorGUIUtility.EditorLockTracker.k_LockMenuText);

            EditorApplication.Internal_SwitchSkin();

        }
    }

}
