#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;


public class ES2EditorSettingsGeneral : ES2EditorWindowContent
{
	private ES2GlobalSettings _globalSettings = null;
	private ES2GlobalSettings globalSettings
	{
		get
		{
			if(_globalSettings == null)
				return _globalSettings = (AssetDatabase.LoadAssetAtPath("Assets/Easy Save 2/Resources/ES2/ES2Init.prefab", typeof(GameObject)) as GameObject).GetComponent<ES2GlobalSettings>();
			return _globalSettings;
		}
	}

	public ES2EditorSettingsGeneral()
	{
	}

	public void Draw()
	{
		ES2EditorWindowStyle style = ES2EditorWindow.instance.style;

		EditorGUILayout.BeginVertical(style.windowContentStyle);

		ES2EditorUtility.Subheading("Default Settings");

		EditorGUILayout.BeginVertical(style.sectionStyle);

		ES2GlobalSettings globals = globalSettings;
		//globals.saveLocation = (ES2.SaveLocation)EditorGUILayout.EnumPopup("Default Save Location:", (System.Enum)globals.saveLocation);
		globals.PCDataPath = ES2EditorUtility.TextField("Default Windows Path", globals.PCDataPath);
		globals.MacDataPath = ES2EditorUtility.TextField("Default OSX Path", globals.MacDataPath);

		EditorGUILayout.EndVertical();


		EditorGUILayout.BeginVertical(style.sectionStyle);
		globals.encrypt = ES2EditorUtility.Toggle("Encrypt Data", globals.encrypt);
		EditorGUILayout.BeginHorizontal();
		globals.encryptionPassword = ES2EditorUtility.TextField("Encryption Password", globals.encryptionPassword);
		globals.saveLocation = (ES2Settings.SaveLocation)ES2EditorUtility.EnumField("Default Save Location", globals.saveLocation);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical(style.sectionStyle);

		EditorGUILayout.BeginHorizontal();
		globals.webUsername = ES2EditorUtility.TextField("Web Username", globals.webUsername);
		globals.webPassword = ES2EditorUtility.TextField("Web Password", globals.webPassword);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		if(GUI.changed)
			EditorUtility.SetDirty(globals);

		EditorGUILayout.EndVertical();
	}

	public void OnHierarchyChange(){}

	public void OnProjectChange(){}
}
#endif