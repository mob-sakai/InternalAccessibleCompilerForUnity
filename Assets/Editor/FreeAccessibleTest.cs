using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Compilers;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor.Utils;

namespace Coffee.FreeAccessible
{
	//class LanguageX : CSharpLanguage
	//{
	//}

    public static class GUIContentExtensions
	{
        public static void ChangeText(this GUIContent self, string value)
		{
			self.m_Text = value;
		}
	}

	class FreeAccessibleTest
    {
        [MenuItem("FreeAccessibleTest/CallDelayed 2s")]
        static void RegisterCallDelayed()
        {
			Debug.Log(typeof(FreeAccessibleTest).Assembly.FullName);
            EditorApplication.CallDelayed(CallDelayed, 2);
        }

        static void CallDelayed()
        {
            //Debug.Log(
            //string.Join(", ",
            //typeof(CSharpLanguage)
            //    .Assembly.GetCustomAttributes<InternalsVisibleToAttribute>()
            //    .Select(x => x.AssemblyName)
            //    .ToArray()
            //    )
            //);

            Debug.Log("2 seconds! ウホ付保");
            Debug.Log(EditorGUIUtility.EditorLockTracker.k_LockMenuText);
			//Debug.Log( Application.BuildInvocationForArguments("fugafuga", new string[0]));
			Debug.Log(GUIContent.s_Text);

			var hoge = new GUIContent("hogehoge");
			Debug.Log(hoge.m_Text);
			hoge.ChangeText("fuga");
			Debug.Log(hoge.m_Text);



			//Debug.Log();

			var lt = new EditorGUIUtility.EditorLockTracker();
			lt.FlipLocked();


			//private void FlipLocked()

			Debug.Log(typeof(EditorGUI.GUIContents.IconName));
			Debug.Log(new EditorGUI.GUIContents.IconName("hogehoge"));
			Debug.Log(new EditorGUI.GUIContents.IconName("hogehoge").name);
			Debug.Log(new EditorGUI.GUIContents.IconName("hogehoge").m_Name);

			//EditorApplication.Internal_SwitchSkin();



			//private static string BuildInvocationForArguments(string functionName, params object[] args)
		}
    }

}
