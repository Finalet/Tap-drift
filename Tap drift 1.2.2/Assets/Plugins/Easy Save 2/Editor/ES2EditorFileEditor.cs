using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

public class ES2EditorFileEditor : EditorWindow
{
	private static readonly Rect windowSize = new Rect(256f, 256f, 800f, 600f);
	
	private const string defaultFile = "ES2File.txt";
	private const string defaultTag = "New Tag";
	
	private Vector2 leftScrollPosition = Vector2.zero;
	private Vector2 rightScrollPosition = Vector2.zero;
	
	private string currentFilePath = null;
	private string[] tags = null;
	private Dictionary<string, ES2Header> headers = null;
		
	private string currentTag = null;
	private int currentIndex = -1;
	private object currentValue = null;
	private bool currentTagRequiresPassword = false;
	private string encryptionPassword = ES2GlobalSettings.defaultEncryptionPassword;
	
	private bool[] currentArrayFoldouts = null;
	
	private string newTagName = "New Tag"; // The value of the 'Tag' field in 'Add New Tag'.
	private int newTagTypeIndex = 0; // The index of the type selected in 'Type' field of 'Add New Tag'
	
	[MenuItem ("Assets/Easy Save 2/File Editor...", false, 1003)]
	public static void OpenWindow()
	{
		// Get existing open window or if none, make a new one:
		ES2EditorFileEditor window = (ES2EditorFileEditor)EditorWindow.GetWindow (typeof (ES2EditorFileEditor));
		window.position = windowSize;
	}
	
	public void OnGUI()
	{
		// If a file has previously been opened and it's path is stored in EditorPrefs, try to open it now.
		if(headers == null && EditorPrefs.HasKey("ES2EditorFileEditorFilePath"))
		{
			currentFilePath = EditorPrefs.GetString("ES2EditorFileEditorFilePath");
			OpenFile();
		}
		
		DisplayGUI();
		// Set this so we can unfocus controls so that values don't persist when we change tag.
		GUI.SetNextControlName("");
	}
	
	private void Reset()
	{
		currentFilePath = "";
		tags = null;
		headers = null;
		currentTag = null;
		currentIndex = -1;
		currentArrayFoldouts = null;
		DestroyTemporaryObjects();
	}
	
	private void DisplayGUI()
	{
		var tempAlignment = GUI.skin.button.alignment;
		GUI.skin.button.alignment = TextAnchor.MiddleLeft;

		// Menu
		DisplayTopMenu();
		
		if(headers != null) 
		{
			DisplayFileSpecificMenu();
			// If we deleted the file in the File Specific menu, return.
			if(headers == null)
				return;
				
			// File info
			float tempLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 60f; 
			
			EditorGUILayout.LabelField("Path", currentFilePath, EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Size", Mathf.Round ((((float)new FileInfo(currentFilePath).Length)/1024f)*1000f)/1000f+" kB", EditorStyles.boldLabel);
			
			EditorGUIUtility.labelWidth = tempLabelWidth;
			EditorGUILayout.Space();
			
			// Tags Title
			EditorGUILayout.LabelField("Tags", EditorStyles.boldLabel);	
			
			EditorGUILayout.BeginHorizontal();
			// Tag List
			DisplayTagList();
			// Tag info
			DisplayTagInfo();
			EditorGUILayout.EndHorizontal();
		}

		GUI.skin.button.alignment = tempAlignment;
	}
	 	
	private void DisplayTagList()
	{
		EditorGUILayout.BeginVertical(GUILayout.Width(this.position.width/3));
				
		leftScrollPosition = EditorGUILayout.BeginScrollView(leftScrollPosition);
	
		int index = GUILayout.SelectionGrid(currentIndex, tags, 1);
		
		if(index != currentIndex)
		{
			currentIndex = index;
			OnTagChange();
		}
		
		EditorGUILayout.EndScrollView();
		
		DisplayAddNewTag();
		
		EditorGUILayout.EndVertical();
	}
	
	private void DisplayAddNewTag()
	{
		// Get Type List.
		Type[] types = ES2EditorType.GetTypes();
		
		// Get Type Names for dropdown list.
		string[] typeNames = new string[types.Length];
		for(int i=0; i<types.Length; i++)
			typeNames[i] = types[i].ToString();
		
		// Display GUI for Add New Tag.
		EditorGUILayout.LabelField("Add New Tag", EditorStyles.boldLabel);
		
		EditorGUILayout.BeginHorizontal();
		
		this.newTagName = EditorGUILayout.TextField(this.newTagName); // Type of new tag.
		this.newTagTypeIndex = EditorGUILayout.Popup(this.newTagTypeIndex, typeNames);
		
		EditorGUILayout.EndHorizontal();
		
		if (GUILayout.Button("+ Add Tag", EditorStyles.toolbarButton))
			AddTag(this.newTagName, types[this.newTagTypeIndex]);
	}
	
	private void DisplayFileSpecificMenu()
	{
		EditorGUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Delete File", EditorStyles.toolbarButton)) 
		{
			if(EditorUtility.DisplayDialog("Delete this File?", "Are you sure you want to permanently delete this file?", "Delete File", "Cancel"))
			{
				ES2.Delete(currentFilePath);
				Reset();
			}
		}
		
		string showFileText = "Show in Explorer";
		if(OSFileBrowser.IsInMacOS)
			showFileText = "Show in Finder";
		
		if (GUILayout.Button(showFileText, EditorStyles.toolbarButton)) 
			ES2EditorTools.ShowInFileBrowser(currentFilePath);
			
		if (GUILayout.Button("Refresh File", EditorStyles.toolbarButton)) 
			OpenFile();
		
		EditorGUILayout.EndHorizontal();
	}
	
	private void OnTagChange()
	{
		// Unfocus field so the value doesn't get displayed in the new tag.
		GUI.FocusControl("");
		// Delete any temporary objects which may have been created when loading the previous tag.
		DestroyTemporaryObjects();
		
		if(currentIndex >= 0)
		{
			currentTag = tags[currentIndex];
			ES2Settings settings = new ES2Settings();
			settings.tag = currentTag;
			settings.encryptionPassword = this.encryptionPassword;
			
			try
			{
				currentValue = ES2.LoadObject(currentFilePath, settings);
				currentTagRequiresPassword = false;
			}
			catch(System.Security.Cryptography.CryptographicException e)
			{
				e.Message.GetType(); // Suppress warning that e is not being used.
				
				// If there's a cryptography exception, we require a password.
				// Flag that we need to request a password when we display the GUI.
				currentTagRequiresPassword = true;
			}
			catch(Exception e)
			{
				EditorUtility.DisplayDialog("Could not load tag", "This tag could not be loaded. Please ensure that the tag was added to the file using Easy Save.\nFailed with exception:\n"+e.ToString(), "Ok");
			}
		}
		else
		{
			currentTag = null;
			currentValue = null;
		}
		currentArrayFoldouts = null;
	}
	
	private void DisplayTagInfo()
	{	
		// If a tag has been selected but requires a password, show password field...
		if(currentTag != null && this.currentTagRequiresPassword)
		{
			EditorGUILayout.LabelField("This tag requires a password to view");
			this.encryptionPassword = EditorGUILayout.TextField("Password", this.encryptionPassword);
			if(GUILayout.Button("Decrypt"))
				OnTagChange();
		}
		// If a tag has been selected and doesn't explicity require a password...
		else if(currentTag != null && currentValue != null)
		{
			// Get type names from ES2Header data.
			ES2Header header = headers[currentTag];
			ES2EditorType valueType = ES2EditorType.Get(header.valueType);
			
			// Only display this data if it's there's an ES2EditorType for it.
			if(valueType != null)
			{
				EditorGUILayout.BeginVertical();
				rightScrollPosition = EditorGUILayout.BeginScrollView(rightScrollPosition);
				
				EditorGUILayout.BeginHorizontal();
				
				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Type", EditorStyles.boldLabel);
				EditorGUILayout.Space();
				EditorGUI.indentLevel++;
				EditorGUILayout.LabelField(GetTypeName(header, valueType));
				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
				
				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Encryption", EditorStyles.boldLabel);
				EditorGUILayout.Space();
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				var tempLabelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 90;
				header.settings.encrypt = EditorGUILayout.Toggle("Encrypt?", header.settings.encrypt);
				if(header.settings.encrypt)
					header.settings.encryptionPassword = EditorGUILayout.TextField(header.settings.encryptionPassword);
				EditorGUIUtility.labelWidth = tempLabelWidth;
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
				EditorGUILayout.EndVertical();
				
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				
				EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
				
				EditorGUILayout.Space();
				EditorGUI.indentLevel++;
				
				if(header.collectionType == ES2Keys.Key._Null)
					DisplayObject(currentValue, valueType, currentTag);
				else if(header.collectionType == ES2Keys.Key._NativeArray)
					DisplayArray(currentValue, valueType, header, currentTag);
				else if(header.collectionType == ES2Keys.Key._Dictionary)
					DisplayDictionary(currentValue, valueType, header, currentTag);
				else if(header.collectionType == ES2Keys.Key._List)
					DisplayList(currentValue, valueType, header, currentTag);
				else
					EditorGUILayout.LabelField("The File Editor does not currently support Collections of this type.");
				
				EditorGUI.indentLevel--;
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				
				EditorGUILayout.EndScrollView();
				
				// Save / Delete Tag Buttons
				EditorGUILayout.BeginHorizontal();
				
				if(valueType != null)
					if (GUILayout.Button("Save Tag", EditorStyles.toolbarButton))
						SaveCurrentTag(header);
				
				if (GUILayout.Button("Delete Tag", EditorStyles.toolbarButton))
					if(EditorUtility.DisplayDialog("Delete this Tag?", "Are you sure you want to permanently delete this tag?", "Delete Tag", "Cancel"))
						DeleteCurrentTag();
				
				// If adding something after 'delete tag', make sure tag hasn't been deleted first.
				
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.EndVertical();
			}
			else
			{ // There's not an ES2EditorType for this data.
				EditorGUILayout.LabelField("The File Editor does not currently support this type.");
			}
		}
	}
	
	private string GetTypeName(ES2Header header, ES2EditorType valueType)
	{
		if(header.collectionType == ES2Keys.Key._Null)
			return valueType.type.ToString();
		else if(header.collectionType == ES2Keys.Key._NativeArray)
			return valueType.type.ToString()+"[]";
		else if(header.collectionType == ES2Keys.Key._Dictionary)
			return "Dictionary<" + ES2TypeManager.GetES2Type(header.keyType).type.ToString() + "," + valueType.type.ToString() + ">";
		else
		{
			string enumName = header.collectionType.ToString();
			return enumName.Substring(1,enumName.Length-1)+"<" + valueType.type.ToString() + ">";
		}
	}
	
	private void DisplayObject(object value, ES2EditorType valueType, string tag)
	{
		object guiFields = valueType.DisplayGUI(value);
		if(guiFields == null)
			EditorGUILayout.LabelField("This type cannot be viewed.");
		else
			currentValue = guiFields;
	}
	
	private void DisplayArray(object obj, ES2EditorType valueType, ES2Header header, string tag)
	{
		object[] array = obj as object[];

		if(currentArrayFoldouts == null)
			currentArrayFoldouts = new bool[array.Length];
		
		EditorGUILayout.IntField("Length", array.Length);
		EditorGUILayout.Space();
		

		
		for(int i=0; i<array.Length; i++)
		{
			if(currentArrayFoldouts[i] = EditorGUILayout.Foldout(currentArrayFoldouts[i],i.ToString()))
			{
				EditorGUI.indentLevel++;
				object guiFields = valueType.DisplayGUI(array[i]);
				EditorGUI.indentLevel--;
				
				if(guiFields == null)
					EditorGUILayout.LabelField("This type cannot be viewed.");
				else
					array[i] = guiFields;
			}
		}
		
		currentValue = array;
	}
	
	private void DisplayList(object obj, ES2EditorType valueType, ES2Header header, string tag)
	{
		List<object> array = obj as List<object>;
		int count = array.Count;
		
		if(currentArrayFoldouts == null)
			currentArrayFoldouts = new bool[count];
		
		EditorGUILayout.IntField("Length", count);
		EditorGUILayout.Space();
		
		// If lengths are not equal, user has modified length field.
		/*if((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.Return) && GUI.GetNameOfFocusedControl()=="arrayLength")
		{
			if(length > array.Length)
			{
				int difference = length-array.Length;
				for(int i=0; i<difference; i++)
					ArrayUtility.Add<object>(ref array, null);
			}
			else if(length < array.Length)
			{
				int difference = array.Length-length;
				for(int i=0; i<difference; i++) 
					ArrayUtility.RemoveAt<object>(ref array, array.Length-difference);
			}
			
			es2Data.loadedData[tag] = array;
		}*/
		
		for(int i=0; i<count; i++)
		{
			if(currentArrayFoldouts[i] = EditorGUILayout.Foldout(currentArrayFoldouts[i],i.ToString()))
			{
				EditorGUI.indentLevel++;
				object guiFields = valueType.DisplayGUI(array[i]);
				EditorGUI.indentLevel--;
				
				if(guiFields == null)
					EditorGUILayout.LabelField("This type cannot be viewed.");
				else
					array[i] = guiFields;
			}
		}
		
		currentValue = array;
	}
	
	private void DisplayDictionary(object obj, ES2EditorType valueType, ES2Header header, string tag)
	{
		Dictionary<object, object> dict = obj as Dictionary<object, object>;
		int count = dict.Count;
		object[] keys = dict.Keys.ToArray();
		
		if(currentArrayFoldouts == null)
			currentArrayFoldouts = new bool[count];
		
		EditorGUILayout.IntField("Count", count);
		EditorGUILayout.Space();
		
		// If lengths are not equal, user has modified length field.
		/*if((Event.current.type == EventType.KeyUp) && (Event.current.keyCode == KeyCode.Return) && GUI.GetNameOfFocusedControl()=="arrayLength")
		{
			if(length > array.Length)
			{
				int difference = length-array.Length;
				for(int i=0; i<difference; i++)
					ArrayUtility.Add<object>(ref array, null);
			}
			else if(length < array.Length)
			{
				int difference = array.Length-length;
				for(int i=0; i<difference; i++) 
					ArrayUtility.RemoveAt<object>(ref array, array.Length-difference);
			} 
			
			es2Data.loadedData[tag] = array;
		}*/
		
		for(int i=0; i<count; i++)
		{
			if(currentArrayFoldouts[i] = EditorGUILayout.Foldout(currentArrayFoldouts[i],i.ToString()))
			{
				EditorGUI.indentLevel++;
				
				EditorGUILayout.LabelField("Key", EditorStyles.boldLabel);
				EditorGUILayout.Space();
				
				EditorGUI.indentLevel++;
				// Display Key
				object keyFields = ES2EditorType.Get(header.keyType).DisplayGUI(keys[i]);
				if(keyFields == null)
					EditorGUILayout.LabelField("This type cannot be viewed.");
				EditorGUI.indentLevel--;
				
				EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
				EditorGUILayout.Space();
				
				EditorGUI.indentLevel++;
				// Display Value
				object valueFields = valueType.DisplayGUI(dict[keys[i]]);
				if(valueFields == null)
					EditorGUILayout.LabelField("This type cannot be viewed.");
				else
				{
					dict[keys[i]] = valueFields;
					currentValue = dict;
				}
				EditorGUI.indentLevel--;
				
				EditorGUI.indentLevel--;
			}
		}
	}
	
	private void DisplayTopMenu()
	{
		EditorGUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Open File...", EditorStyles.toolbarButton)) 
		{
			Reset();
			currentFilePath = EditorUtility.OpenFilePanel("Open File", Application.persistentDataPath, "");
			OpenFile();
		}
		
		if (GUILayout.Button("New File...", EditorStyles.toolbarButton)) 
		{
			Reset();
			currentFilePath = EditorUtility.SaveFilePanel("New File", Application.persistentDataPath, defaultFile, "");
			NewFile();
		}
		
		EditorGUILayout.EndHorizontal();
	}
	
	// Delete any temporary objects added to the scene by the File Editor.
	private void DestroyTemporaryObjects()
	{
		object[] obj = GameObject.FindObjectsOfType(typeof (GameObject));
		for(int i=0; i<obj.Length; i++)
		{
			GameObject g = (GameObject) obj[i];
			if(g.name == "Easy Save 2 Loaded Component")
				DestroyImmediate(g);
		}
	}
	
	public void OnDestroy()
	{
		DestroyTemporaryObjects();
	}
	
	private void SaveCurrentTag(ES2Header header)
	{
		try
		{
			ES2Type valueType = ES2TypeManager.GetES2Type(header.valueType);
			ES2Type keyType = ES2TypeManager.GetES2Type(header.keyType);
			
			ES2Settings settings = new ES2Settings(currentFilePath);
			settings.encrypt = header.settings.encrypt;
			settings.encryptionType = header.settings.encryptionType;
			settings.encryptionPassword = header.settings.encryptionPassword;
			
			using(ES2Writer writer = new ES2Writer(settings))
			{
				if(header.collectionType == ES2Keys.Key._Null)
					writer.Write(currentValue, currentTag, valueType);
				else if(header.collectionType == ES2Keys.Key._NativeArray)
					writer.Write(currentValue as object[], currentTag, valueType);
				else if(header.collectionType == ES2Keys.Key._Dictionary)
					writer.Write(currentValue as Dictionary<object,object>, currentTag, keyType, valueType);
				else if(header.collectionType == ES2Keys.Key._List)
					writer.Write(currentValue as List<object>, currentTag, valueType);
				else if(header.collectionType == ES2Keys.Key._Queue)
					writer.Write(currentValue as Queue<object>, currentTag, valueType);
				else if(header.collectionType == ES2Keys.Key._Stack)
					writer.Write(currentValue as Stack<object>, currentTag, valueType);
				else if(header.collectionType == ES2Keys.Key._HashSet)
					writer.Write(currentValue as HashSet<object>, currentTag, valueType);
				
				writer.Save();
			}
		}
		catch(Exception e)
		{
			EditorUtility.DisplayDialog("Could not save file", "An error was thrown when trying to save to this file. See below for details.\n\n"+"Details: "+e.Message, "Ok");
		}
	}
	
	private void DeleteCurrentTag()
	{
		ES2.Delete(currentFilePath+"?tag="+currentTag);
		ArrayUtility.Remove(ref tags, currentTag);
		
		headers.Remove(currentTag);
		currentTag = null;
		currentIndex = -1;
		
		// Close file if no tags left in file.
		if(tags.Length == 0)
			Reset();
	}
	
	public void OpenFile()
	{
		if(string.IsNullOrEmpty(currentFilePath))
			return;
	
		if(!File.Exists(currentFilePath))
			return;
		// If this file exists, set it as the default file to be opened when we open the window.
		EditorPrefs.SetString("ES2EditorFileEditorFilePath", currentFilePath);
		// Reset the tag incase we're Refreshing a file, and the tag has been removed.
		currentTag = null;
		currentIndex = -1;
			
		try
		{
			headers = ES2EditorReflection.GetHeadersFromFile(currentFilePath);
			tags = headers.Keys.ToArray();		
		}
		catch(Exception e)
		{
			Reset();
			EditorPrefs.DeleteKey("ES2EditorFileEditorFilePath");
			if(e is ES2InvalidDataException)
				EditorUtility.DisplayDialog("Could not open file", "This file does not contain data which is readable by Easy Save.\nPlease make sure this file was created using Easy Save 2.", "Ok");
			else
				EditorUtility.DisplayDialog("Could not open file", "An error was thrown when trying to open this file. See below for details.\n\n"+"Details: "+e.Message, "Ok");
			return;
		}
	}
	
	public void NewFile()
	{
		if(string.IsNullOrEmpty(currentFilePath))
			return;
			
		File.Create(currentFilePath).Close();
		OpenFile();
	}
	
	public void AddTag(string tag, Type type)
	{
		ES2Settings settings = new ES2Settings();
		settings.tag = tag;		
		ES2.Save(ES2EditorType.Get(type).CreateInstance(), currentFilePath, settings);
		OpenFile();
	}
}
