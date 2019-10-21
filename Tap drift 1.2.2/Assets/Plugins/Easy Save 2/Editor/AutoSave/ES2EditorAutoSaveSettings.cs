#if !UNITY_4 && !UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using System.Collections;


public class ES2EditorAutoSaveSettings : ES2EditorWindowContent
{
	public ES2EditorAutoSaveSettings()
	{
	}

	public void Draw()
	{
		ES2EditorWindowStyle style = ES2EditorWindow.instance.style;

		// Don't allow Auto Save to be modified when playing.
		if(Application.isPlaying)
		{
			EditorGUILayout.BeginHorizontal(style.windowContentStyle);
			GUIStyle centerStyle = new GUIStyle(style.contentTextStyle);
			centerStyle.stretchHeight = true;
			centerStyle.alignment = TextAnchor.MiddleCenter;
			EditorGUILayout.LabelField("Auto Save can not be modified in Play mode.", centerStyle);
			EditorGUILayout.EndHorizontal();
			return;
		}

		// If a manager hasn't been added to the scene, require that it is added.
		ES2AutoSaveManager mgr = ES2EditorAutoSaveUtility.mgr;

		if(mgr == null)
		{
			EditorGUILayout.BeginVertical(style.windowContentStyle);
			if(ES2EditorUtility.Button("Click to enable Auto Save for this scene"))
				ES2EditorAutoSaveUtility.AddManagerToScene();
			EditorGUILayout.EndVertical();
			return;
		}

		Undo.RecordObject(mgr, "Auto Save Settings");

		EditorGUILayout.BeginVertical(style.windowContentStyle);

		EditorGUILayout.BeginHorizontal(style.sectionStyle);
		mgr.loadEvent = (ES2AutoSaveManager.LoadEvent)ES2EditorUtility.EnumField("When to Load", mgr.loadEvent);
		mgr.saveEvent = (ES2AutoSaveManager.SaveEvent)ES2EditorUtility.EnumField("When to Save", mgr.saveEvent);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(style.sectionStyle);
		mgr.filePath = ES2EditorUtility.TextField("Filename/Path", mgr.filePath);
		mgr.tag = ES2EditorUtility.TextField("Tag", mgr.tag);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical(style.sectionStyle);

		mgr.encrypt = ES2EditorUtility.Toggle("Use Encryption", mgr.encrypt);


		if(mgr.encrypt)
		{
			EditorGUILayout.BeginHorizontal(style.indentStyle);
			mgr.encryptionPassword = ES2EditorUtility.TextField("Encryption Password", mgr.encryptionPassword);
			mgr.encryptionType = (ES2Settings.EncryptionType)ES2EditorUtility.EnumField("Encryption Type", mgr.encryptionType);
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal(style.sectionStyle);

		mgr.deletePrefabsOnLoad = ES2EditorUtility.Toggle("Delete Instantiated Prefabs on Load", mgr.deletePrefabsOnLoad);
		mgr.convertPrefabsToSceneObjectsOnImport = ES2EditorUtility.Toggle("Convert Prefabs To Scene Objects Upon Dragging Into Scene", mgr.convertPrefabsToSceneObjectsOnImport);

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(style.sectionStyle);

		bool autoSaveComponentsAreHidden = ES2EditorAutoSaveUtility.AutoSaveComponentsAreHidden();
		if(ES2EditorUtility.Toggle("Hide Auto Save Components in Editor", autoSaveComponentsAreHidden) != autoSaveComponentsAreHidden)
			ES2EditorAutoSaveUtility.ToggleHideAutoSaveComponents();

		ES2EditorAutoSaveUtility.AutomaticallyRefreshSceneAutoSaves = ES2EditorUtility.Toggle("Automatically refresh Auto Saves when window is open", ES2EditorAutoSaveUtility.AutomaticallyRefreshSceneAutoSaves);

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(style.sectionStyle);
		if(ES2EditorUtility.Button("Refresh Auto Saves in Scene"))
			ES2EditorAutoSaveUtility.RefreshSceneAutoSaves();

		if(ES2EditorUtility.Button("Refresh Auto Saves in Prefabs"))
			ES2EditorAutoSaveUtility.RefreshPrefabAutoSaves();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();

		if(ES2EditorUtility.Button("Remove Auto Save from Scene"))
			ES2EditorAutoSaveUtility.RemoveAutoSaveFromScene();
		if(ES2EditorUtility.Button("Remove Auto Save from all Prefabs"))
			ES2EditorAutoSaveUtility.RemoveAutoSaveFromAllPrefabs();
		EditorGUILayout.EndHorizontal();


		EditorGUILayout.EndVertical();
	}

	public void OnHierarchyChange(){}

	public void OnProjectChange(){}
}
#endif