using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace Coffee.InternalAccessibleCompiler
{
	internal class DotNet
	{
		public static string GetVersion()
		{
			string version = "";
			ExecuteWait("--version", (_, stdout, __) => version = stdout);
			return version;
		}

		public static void Run(string proj, string args, System.Action<bool, string> resultCallback, string progressTitle)
		{
			EditorUtility.DisplayProgressBar(progressTitle, "Run " + Path.GetFileNameWithoutExtension(proj), 0.1f);
			var commandArgs = string.Format("run -p \"{0}\" -- {1}", Path.GetFullPath(proj), args);
			ExecuteWait(commandArgs, (success, stdout, stderr) =>
			{
				EditorUtility.ClearProgressBar();
				if (success)
					resultCallback(success, stdout);
				else
					RunWithRestore(proj, commandArgs, resultCallback, progressTitle);
			});
		}

		static void RunWithRestore(string proj, string commandArgs, System.Action<bool, string> resultCallback, string progressTitle)
		{
			EditorUtility.DisplayProgressBar(progressTitle, "Restore " + Path.GetFileNameWithoutExtension(proj), 0.2f);
			Restore(proj, restoreSuccess =>
			{
				EditorUtility.ClearProgressBar();
				if (!restoreSuccess)
				{
					resultCallback(false, "");
					return;
				}

				EditorUtility.DisplayProgressBar(progressTitle, "Run " + Path.GetFileNameWithoutExtension(proj), 0.3f);
				ExecuteWait(commandArgs, (success, stdout, stderr) =>
				{
					EditorUtility.ClearProgressBar();
					if (!success)
						UnityEngine.Debug.LogError(stderr);
					resultCallback(success, stdout);
				});
			});
		}

		static void Restore(string proj, System.Action<bool> callback)
		{
			ExecuteWait(string.Format("restore \"{0}\"", Path.GetFullPath(proj)), (success, stdout, stderr) =>
			{
				if (!success)
					UnityEngine.Debug.LogError(stderr);
				callback(success);
			});
		}

		static void ExecuteWait(string args, System.Action<bool, string, string> resultCallback = null)
		{
			var startInfo = new ProcessStartInfo
			{
				Arguments = args,
				CreateNoWindow = true,
				FileName = "/usr/local/share/dotnet/dotnet",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			UnityEngine.Debug.LogFormat("[DotNet.Execute] {0} {1}", startInfo.FileName, args);

			var p = Process.Start(startInfo);
			if (p == null || p.Id == 0 || p.HasExited)
			{
				UnityEngine.Debug.LogError("[DotNet.Execute] dotnet not found. To install additional .NET Core runtimes or SDKs: https://aka.ms/dotnet-download");
				resultCallback(false, "", "");
				return;
			}

			p.WaitForExit();
			resultCallback(p.ExitCode == 0, p.StandardOutput.ReadToEnd(), p.StandardError.ReadToEnd());
		}
	}
}