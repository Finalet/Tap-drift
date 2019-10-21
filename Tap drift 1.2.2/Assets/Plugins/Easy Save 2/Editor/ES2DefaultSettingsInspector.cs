using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ES2GlobalSettings))]
public class ES2DefaultSettingsInspector : Editor
{
	[MenuItem ("Assets/Easy Save 2/Add Default Settings Object to Scene", false, 1052)]
    private static void AddDefaultSettings() 
    {
    	GameObject g = new GameObject();
    	g.name = "Easy Save Default Settings";
    	g.AddComponent<ES2GlobalSettings>();
    }
    
    public override void OnInspectorGUI()
	{
		ES2GlobalSettings targetObj = target as ES2GlobalSettings;
		ShowGUI(targetObj);
	}
	
	public static void ShowGUI(ES2GlobalSettings targetObj)
	{
		//targetObj.saveLocation = (ES2.SaveLocation)EditorGUILayout.EnumPopup("Default Save Location:", (System.Enum)targetObj.saveLocation);
		targetObj.PCDataPath = EditorGUILayout.TextField("Default PC Path:", targetObj.PCDataPath);
		targetObj.MacDataPath = EditorGUILayout.TextField("Default OSX Path:", targetObj.MacDataPath);
		targetObj.saveLocation = (ES2Settings.SaveLocation)EditorGUILayout.EnumPopup("Default Save Location:", targetObj.saveLocation);
		targetObj.encrypt = EditorGUILayout.Toggle("Encrypt Data:", targetObj.encrypt);
		targetObj.encryptionPassword = EditorGUILayout.TextField("Encryption Password:", targetObj.encryptionPassword);
		targetObj.encryptionType = (ES2Settings.EncryptionType)EditorGUILayout.EnumPopup("Encryption Type:", targetObj.encryptionType);
		targetObj.webUsername = EditorGUILayout.TextField("Web Username:", targetObj.webUsername);
		targetObj.webPassword = EditorGUILayout.TextField("Web Password:", targetObj.webPassword);
		
		if (GUI.changed)
			EditorUtility.SetDirty(targetObj);
	}
}