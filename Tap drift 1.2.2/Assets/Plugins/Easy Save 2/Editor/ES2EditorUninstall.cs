using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class ES2EditorUninstall : Editor
{
	private static string[] es2filenames = new string[]
	{
		"ES2Standalone.dll",
		"ES2.dll",
		"ES2Settings.dll",
		"ES2IO.dll",
		"ES2Dispose.dll",
		"ES2Dispose.dll",
		"MoodkieSecurity.dll",
		"Elixis.dll",
		"ES2AutoInspector.cs",
		"ES2ClearPlayerPrefs.cs",
		"ES2DefaultSettingsInspector.cs",
		"ES2Uninstall.cs",
		"ES2PlayMakerAction.cs",
		"UnityEngine.dll"
	};
	
	[MenuItem ("Assets/Easy Save 2/Uninstall Easy Save 2...", false, 1051)]
    private static void Uninstall() 
    {
		if(EditorUtility.DisplayDialog("Uninstall Easy Save 2?", "Are you sure you want to uninstall Easy Save 2?", "Uninstall", "Cancel")) 
		{
			RemoveKnownLocations();
			RemoveFilenames();
			AssetDatabase.Refresh();
		}
    }
	
	private static void RemoveFilenames()
	{
		string[] files = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);
		foreach(string filepath in files)
		{
			string filename = Path.GetFileName(filepath);
			foreach(string knownFilename in es2filenames)
			{
				if(filename.ToLower() == knownFilename.ToLower())
					File.Delete(filepath);
			}
		}
	}
	
	private static void RemoveKnownLocations()
	{
		string es2OldFolderPath = Application.dataPath+"/Plugins/Easy Save 2";
		if(Directory.Exists(es2OldFolderPath))
			Directory.Delete(es2OldFolderPath, true);
			
		string es2MainFolderPath = Application.dataPath+"/Easy Save 2";
		if(Directory.Exists(es2MainFolderPath))
			Directory.Delete(es2MainFolderPath, true);
	}
}