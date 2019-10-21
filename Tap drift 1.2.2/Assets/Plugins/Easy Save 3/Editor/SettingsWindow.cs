using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ES3Internal;

namespace ES3Editor
{
	public class SettingsWindow : SubWindow
	{
		public GameObject defaultSettingsGo = null;
		public ES3DefaultSettings editorSettings = null;
		public ES3SerializableSettings settings = null;
		public SerializedObject so = null;
		public SerializedProperty assemblyNamesProperty = null;

		public SettingsWindow(EditorWindow window) : base("Settings", window){}

		public override void OnGUI()
		{
			if(settings == null || editorSettings == null || assemblyNamesProperty == null)
				Init();

			var style = EditorStyle.Get;

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginVertical(style.area);

			GUILayout.Label("Runtime Settings", style.heading);

			EditorGUILayout.BeginVertical(style.area);

			ES3SettingsEditor.Draw(settings);

			EditorGUILayout.EndVertical();

			GUILayout.Label("Editor Settings", style.heading);

			EditorGUILayout.BeginVertical(style.area);

			var wideLabel = new GUIStyle();
			wideLabel.fixedWidth = 400;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Auto Add Manager to Scene", wideLabel);
			editorSettings.addMgrToSceneAutomatically = EditorGUILayout.Toggle(editorSettings.addMgrToSceneAutomatically);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Auto Update References", wideLabel);
			editorSettings.autoUpdateReferences = EditorGUILayout.Toggle(editorSettings.autoUpdateReferences);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();


			// Show Assembly names array.
			EditorGUILayout.PropertyField(assemblyNamesProperty, new GUIContent("Assemblies containing ES3Types", "The names of assemblies we want to load ES3Types from."), true); // True means show children
			if(so.ApplyModifiedProperties())
			{
				#if UNITY_2018_3_OR_NEWER
				PrefabUtility.SaveAsPrefabAsset(defaultSettingsGo, "Assets/Plugins/Easy Save 3/Resources/ES3/ES3 Default Settings.prefab");
				#endif
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndVertical();


			if(EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(editorSettings);
		}

		public void Init()
		{
			#if UNITY_2018_3_OR_NEWER
			defaultSettingsGo = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets/Plugins/Easy Save 3/Resources/ES3/ES3 Default Settings.prefab");
			editorSettings = defaultSettingsGo.GetComponent<ES3DefaultSettings>();
			#else
			editorSettings = ES3Settings.GetDefaultSettings();
			#endif

			settings = editorSettings.settings;
			so = new SerializedObject(editorSettings);
			var settingsProperty = so.FindProperty("settings");
			assemblyNamesProperty = settingsProperty.FindPropertyRelative("assemblyNames");
			
			
			#if UNITY_2018_3_OR_NEWER
			PrefabUtility.SavePrefabAsset(defaultSettingsGo);
			#endif
		}
	}

}
