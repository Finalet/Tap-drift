using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

public class ES2EditorSettings : EditorWindow
{
	private static readonly Rect windowSize = new Rect(Screen.width/4, Screen.height/4, 512f, 350f);
	private static readonly Vector2 windowMinSize = new Vector2(512f, 350f);
	
	private Vector2 menuScroll = Vector2.zero;
	private Vector2 scroll = Vector2.zero;
	private int selected = 0;
	
	private string[] menuItems = new string[]{"Default Settings", "Tools", "Information", "Change Log", "Readme"};
	
	private ES2GlobalSettings globalSettings = null;
	
	[MenuItem ("Assets/Easy Save 2/Settings...", false, 1001)]
	public static void OpenWindow()
	{
		// Get existing open window or if none, make a new one:
		ES2EditorSettings window = (ES2EditorSettings)EditorWindow.GetWindow (typeof (ES2EditorSettings));
		window.position = windowSize;
		window.minSize = windowMinSize;
	}
	
	public void OnGUI()
	{
		try
		{
			EditorGUILayout.BeginHorizontal();
			DisplaySideMenu();
			
			scroll = EditorGUILayout.BeginScrollView(scroll);
			
			// Get the name of the selected menu item.
			string selectedName = menuItems[selected];
			if(selectedName == "Default Settings")
				DisplayDefaultSettings();
			else if(selectedName == "Tools")
				DisplayTools();
			else if(selectedName == "Information")
				DisplayInformation();
			else if(selectedName == "Change Log")
				DisplayChangeLog();
			else if(selectedName == "Readme")
				DisplayReadme();
			
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndHorizontal();
		}
		catch(System.Exception e)
		{
			Debug.LogError(e.Message);
		}
	}
	
	public void DisplayReadme()
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Readme", EditorStyles.boldLabel);
		EditorGUILayout.Space();
		
		if(System.IO.File.Exists(Application.dataPath+"/Easy Save 2/readme.txt"))
		{
			EditorGUILayout.TextArea(System.IO.File.ReadAllText(Application.dataPath+"/Easy Save 2/readme.txt"));
		}
		else
			EditorGUILayout.LabelField("Readme could not be found");
	}
	
	public void DisplayChangeLog()
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Change Log", EditorStyles.boldLabel);
		EditorGUILayout.Space();
		
		if(System.IO.File.Exists(Application.dataPath+"/Easy Save 2/changelog.txt"))
		{
			EditorGUILayout.TextArea(System.IO.File.ReadAllText(Application.dataPath+"/Easy Save 2/changelog.txt"));
		}
		else
			EditorGUILayout.LabelField("Change Log could not be found");
	}
	
	public void DisplayDefaultSettings()
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Default Save/Load Settings", EditorStyles.boldLabel);
		EditorGUILayout.Space();
		ES2DefaultSettingsInspector.ShowGUI(GetGlobalSettings());
	}
	
	public void DisplayTools()
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Clear Default Save Folder"))
			ES2EditorTools.ClearDefaultSaveFolder();
		if(GUILayout.Button("Clear PlayerPrefs"))
			ES2EditorTools.ClearPlayerPrefs();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Open Default Save Folder"))
			ES2EditorTools.ShowInFileBrowser(Application.persistentDataPath);

		EditorGUILayout.EndHorizontal();
	}
	
	public void DisplayInformation()
	{
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Information", EditorStyles.boldLabel);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Application.persistentDataPath");
		EditorGUILayout.SelectableLabel(Application.persistentDataPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
	}
	
	public void DisplaySideMenu()
	{
		menuScroll = EditorGUILayout.BeginScrollView(menuScroll, GUILayout.Width(150f));
		selected = GUILayout.SelectionGrid(selected, menuItems, 1);
		EditorGUILayout.EndScrollView();
	}
	
	/* 
		Gets the ES2GlobalSettings object from the ES2Init object in /Assets/Easy Save 2/Resources/Easy Save 2/
	*/
	public ES2GlobalSettings GetGlobalSettings()
	{
		if(globalSettings == null)
			return globalSettings = (AssetDatabase.LoadAssetAtPath("Assets/Easy Save 2/Resources/ES2/ES2Init.prefab", typeof(GameObject)) as GameObject).GetComponent<ES2GlobalSettings>();
		return globalSettings;
	}
}
