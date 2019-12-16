using UnityEditor;
using UnityEngine;

public class EnvironmentVariablePATH
{
	[InitializeOnLoadMethod]
    static void InitializeOnLoadMethod ()
    {
                EditorUtility.ClearProgressBar();
		Debug.Log ("PATH: " + System.Environment.GetEnvironmentVariable ("PATH"));





	}
}
