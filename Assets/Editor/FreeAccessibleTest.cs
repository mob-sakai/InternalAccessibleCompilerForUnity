using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Coffee.FreeAccessible
{
	public class FreeAccessibleTest
	{
		[UnityEditor.MenuItem("FreeAccessibleTest/CallDelayed 10s")]
		static void RegisterCallDelayed()
		{
			UnityEditor.EditorApplication.CallDelayed(CallDelayed, 10);
		}

		static void CallDelayed()
		{
			Debug.Log("10 seconds!");




		}
	}
}
