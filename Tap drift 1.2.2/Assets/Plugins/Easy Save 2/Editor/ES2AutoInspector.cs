using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ES2Auto))]
[System.Serializable]
public class ES2AutoInspector : Editor
{
	public bool showTagOptions = true;
	public bool showLoadOptions = false;
	public bool showSaveOptions = false;
	public bool showAdvancedSettings = false;
	public bool showWhatToSave = false;
	public bool showSavePathInfo = false;
	
    private static void AddAutoSave() 
    {
    	if(Selection.activeGameObject != null)
    	{
    		if(Selection.activeGameObject.GetComponent<ES2Auto>() != null)
    			Debug.LogWarning("There is already an Auto Save attached to this GameObject. Adding an extra Auto Save anyway.");
    		Selection.activeGameObject.AddComponent<ES2Auto>();
    	}
    	else
    		Debug.LogWarning("You must select a GameObject to add an Auto Save component.");
    }
	
	public override void OnInspectorGUI()
	{
		//EditorGUIUtility.labelWidth = 25f;
		ES2Auto targetObj = target as ES2Auto;
			
		showTagOptions = EditorGUILayout.Foldout(showTagOptions, "1) Enter a Unique Tag for this Save:");
		if(showTagOptions)
			targetObj.uniqueTag = EditorGUILayout.TextField("", targetObj.uniqueTag);
		if(targetObj.uniqueTag == "")
			targetObj.uniqueTag = targetObj.gameObject.name;
		//EditorGUIUtility.labelWidth = 210f;
		EditorGUI.indentLevel = 0;
		showSaveOptions = EditorGUILayout.Foldout(showSaveOptions, "2) When do you want to Save?");
		if(showSaveOptions)
		{
			//EditorGUIUtility.labelWidth = 230f;
			EditorGUI.indentLevel = 2;
			targetObj.saveOnDisable = EditorGUILayout.Toggle(	"Save when Object is Disabled", 
																targetObj.saveOnDisable, 
																GUILayout.ExpandWidth(true));
			targetObj.saveOnInterval = EditorGUILayout.Toggle("Save Every Number of Seconds", targetObj.saveOnInterval);
			if(targetObj.saveOnInterval)
			{
				EditorGUI.indentLevel = 3;
				targetObj.saveInterval = EditorGUILayout.FloatField("How many Seconds between Saves?", targetObj.saveInterval);
			}
		}
		EditorGUI.indentLevel = 0;
		showLoadOptions = EditorGUILayout.Foldout(showLoadOptions, "3) When do you want to Load?");
		if(showLoadOptions)
		{
			//EditorGUIUtility.labelWidth = 200f;
			EditorGUI.indentLevel = 2;
			targetObj.loadOnEnable = EditorGUILayout.Toggle("Load when Object is Enabled", targetObj.loadOnEnable);
			targetObj.loadOnAwake = EditorGUILayout.Toggle("Load on Awake", targetObj.loadOnAwake);
			targetObj.loadOnStart = EditorGUILayout.Toggle("Load on Start", targetObj.loadOnStart);
		}
		EditorGUI.indentLevel = 0;
		showWhatToSave = EditorGUILayout.Foldout(showWhatToSave, "4) What do you want to Save?");
		if(showWhatToSave)
		{
			//EditorGUIUtility.labelWidth = 170f;
			EditorGUI.indentLevel = 2;
			targetObj.savePosition = EditorGUILayout.Toggle("Save Position", targetObj.savePosition);
			targetObj.saveRotation = EditorGUILayout.Toggle("Save Rotation", targetObj.saveRotation);
			targetObj.saveScale = EditorGUILayout.Toggle("Save Scale", targetObj.saveScale);
			targetObj.saveMesh = EditorGUILayout.Toggle("Save Mesh", targetObj.saveMesh);
			targetObj.saveSphereCollider = EditorGUILayout.Toggle("Save Sphere Collider", targetObj.saveSphereCollider);
			targetObj.saveBoxCollider = EditorGUILayout.Toggle("Save Box Collider", targetObj.saveBoxCollider);
			targetObj.saveCapsuleCollider = EditorGUILayout.Toggle("Save Capsule Collider", targetObj.saveCapsuleCollider);
			targetObj.saveMeshCollider = EditorGUILayout.Toggle("Save Mesh Collider", targetObj.saveMeshCollider);
		}
		EditorGUI.indentLevel = 0;
		showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "5) Advanced Settings (optional)");
		if(showAdvancedSettings)
		{
			//EditorGUIUtility.labelWidth = 100f;
			EditorGUI.indentLevel = 2;
			targetObj.saveFile = EditorGUILayout.TextField("Save Path:", targetObj.saveFile);
			//EditorGUIUtility.labelWidth = 200f;
			showSavePathInfo = EditorGUILayout.Foldout(showSavePathInfo, "Show Save Path Info");
			ES2FilenameData filenameData = new ES2FilenameData(targetObj.saveFile, new ES2Settings());
			if(filenameData.IsURL())
			{
				EditorGUI.indentLevel = 2;
				targetObj.webUsername = EditorGUILayout.TextField("Web Username: ", targetObj.webUsername);
				targetObj.webPassword = EditorGUILayout.TextField("Web Password: ", targetObj.webPassword);
			}
			if(showSavePathInfo)
			{
				EditorGUI.indentLevel = 2;
				EditorGUI.indentLevel = 3;
				EditorGUILayout.LabelField("Path leads to a ", filenameData.pathType.ToString());
				EditorGUILayout.LabelField("Is path absolute? ", filenameData.IsAbsolute().ToString());
				if(!filenameData.IsURL() && !filenameData.IsAbsolute())
				{
					if(targetObj.saveLocation != ES2Settings.SaveLocation.PlayerPrefs)
						EditorGUILayout.TextField("Will save to: ", filenameData.filePath);
					else
						EditorGUILayout.TextField("Will save using PlayerPrefs key: ",filenameData.playerPrefsPath);
				}
				if(filenameData.tag != "")
					EditorGUILayout.LabelField("Tag:", filenameData.tag);
				if(filenameData.persistentPath != "")
					EditorGUILayout.TextField("Persistent Path:", filenameData.persistentPath);
				if(filenameData.userFolder != "")
					EditorGUILayout.LabelField("User-Defined Folder:", filenameData.userFolder);
				if(filenameData.filename != "")
					EditorGUILayout.LabelField("Filename:", filenameData.filename);
				if(filenameData.extension != "")
					EditorGUILayout.LabelField("File Extension:", filenameData.extension);
			}
			EditorGUI.indentLevel = 2;
			//EditorGUIUtility.labelWidth = 160f;
			targetObj.saveLocation = (ES2Settings.SaveLocation)EditorGUILayout.EnumPopup("Save Location:", (System.Enum)targetObj.saveLocation);
			targetObj.encrypt = EditorGUILayout.Toggle("Use Encryption?", targetObj.encrypt);
			if(targetObj.encrypt)
			{
				EditorGUI.indentLevel = 3;
				targetObj.encryptionPassword = EditorGUILayout.TextField("Encryption Password", targetObj.encryptionPassword);
			}
		}
	}
}

