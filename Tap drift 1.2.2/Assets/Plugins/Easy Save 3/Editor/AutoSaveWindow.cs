using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ES3Internal;

namespace ES3Editor
{
	[System.Serializable]
	public class AutoSaveWindow : SubWindow
	{
		public bool showAdvancedSettings = false;

		public ES3AutoSaveMgr mgr = null;

		public AutoSaveWindow(EditorWindow window) : base("Auto Save", window){}

		public override void OnGUI()
		{
			if(mgr == null)
				Init();

			if(mgr == null)
			{
				EditorGUILayout.LabelField("Enable Auto Save for Scene");
				return;
			}

			var style = EditorStyle.Get;

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginVertical(style.area);

			GUILayout.Label("Settings for Current Scene", style.heading);

			EditorGUILayout.BeginVertical(style.area);

			mgr.saveEvent = (ES3AutoSaveMgr.SaveEvent)EditorGUILayout.EnumPopup("Save Event", mgr.saveEvent);
			mgr.loadEvent = (ES3AutoSaveMgr.LoadEvent)EditorGUILayout.EnumPopup("Load Event", mgr.loadEvent);

			mgr.key = EditorGUILayout.TextField("Key", mgr.key);

			EditorGUILayout.Space();

			showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Show Advanced Settings");
			if(showAdvancedSettings)
			{
				EditorGUI.indentLevel++;
				ES3SettingsEditor.Draw(mgr.settings);
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndVertical();

			if(EditorGUI.EndChangeCheck())
				EditorUtility.SetDirty(mgr);

			EditorGUILayout.EndVertical();
		}

		public void Init()
		{
			var mgrs = Resources.FindObjectsOfTypeAll<ES3AutoSaveMgr>();
			if(mgrs.Length > 0)
				mgr = mgrs [0];
		}
	}

}
