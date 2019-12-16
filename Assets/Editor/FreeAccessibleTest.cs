using UnityEngine;
using UnityEditor;

namespace Coffee.FreeAccessible
{
    public class FreeAccessibleTest
    {
        [MenuItem("FreeAccessibleTest/CallDelayed 10s")]
        static void RegisterCallDelayed()
        {
            EditorApplication.CallDelayed(CallDelayed, 10);
        }

        static void CallDelayed()
        {


            Debug.Log("10 seconds!");

            EditorApplication.Internal_SwitchSkin();
        }
    }
}
