using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

#if UNITY_2018_1_OR_NEWER

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

#elif UNITY_5_6_OR_NEWER

using UnityEditor.Build;

#endif


namespace AlmostEngine.Screenshot
{

	public class FrameworkDependency
	{
		public static void SetiOSFrameworkDependency ()
		{
			string[] utils = AssetDatabase.FindAssets ("iOSUtils");
			if (utils.Length > 0) {
				for (int i = 0; i < utils.Length; ++i) {
					string path = AssetDatabase.GUIDToAssetPath (utils [i]);
					if (path.Contains ("iOSUtils.m")) {
						FrameworkDependency.AddFrameworkDependency (path, BuildTarget.iOS, "Photos");
					}
				}
			}
		}

		public static void AddFrameworkDependency (string pluginPath, BuildTarget target, string framework)
		{
			PluginImporter plugin = AssetImporter.GetAtPath (pluginPath) as PluginImporter;
			if (plugin == null)
				return;
			string dependencies = plugin.GetPlatformData (target, "FrameworkDependencies");
			if (!dependencies.Contains (framework)) {
				plugin.SetPlatformData (target, "FrameworkDependencies", dependencies + ";" + framework);
				AssetDatabase.SaveAssets ();
				AssetDatabase.Refresh ();

				// Debug.Log ("Adding framework dependency to " + target + ": " + framework);
			}
		}
	}



	#if UNITY_2018_1_OR_NEWER && UNITY_IOS


	class iOSFrameworkDependencyPreprocess : IPreprocessBuildWithReport
	{
	public int callbackOrder { get { return 0; } }
	public void OnPreprocessBuild(BuildReport report)
	{
	FrameworkDependency.SetiOSFrameworkDependency();
	}
	}


	#elif UNITY_5_6_OR_NEWER && UNITY_IOS

	class iOSFrameworkDependencyPreprocess : IPreprocessBuild
	{
	public int callbackOrder { get { return 0; } }
	public void OnPreprocessBuild(BuildTarget target, string path)
	{
	FrameworkDependency.SetiOSFrameworkDependency();
	}
	}

	#endif

}