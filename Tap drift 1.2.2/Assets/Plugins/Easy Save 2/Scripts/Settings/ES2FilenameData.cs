using UnityEngine;
using System.Collections;
using System.IO;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public struct ES2FilenameData
{
	public enum PathType {File, Folder, URL};
	
	public string fullString; // Full path including tags.
	public string persistentPath; // The data path to be suffixed onto the users string.
	public string userFolder; // The path the user has specified.
	public string filename; // Filename without extension.
	public string extension; // File extension.
	public string tag; // The tag specified, if any.
	
	public string playerPrefsPath; // The path for use as a PlayerPrefs key.
	public string filePath; // The path to use when saving a file.
	public PathType pathType;
	public bool isAbsolute; // Whether this path points to an absolute folder.

	public static string persistentDataPath = "";
	
	public ES2FilenameData(string path, ES2Settings settings, bool useParameters)
	{
		fullString = "";
		persistentPath = GetPersistentPath();
		userFolder = "";
		filename = "";
		extension = "";
		tag = "";
		playerPrefsPath = persistentPath;
		filePath = persistentPath;
		pathType = PathType.Folder;
		isAbsolute = false;
		
		Init(path, settings, useParameters);
	}
	
	public ES2FilenameData(string path, ES2Settings settings)
	{
		fullString = "";
		persistentPath = GetPersistentPath();
		userFolder = "";
		filename = "";
		extension = "";
		tag = "";
		playerPrefsPath = persistentPath;
		filePath = persistentPath;
		pathType = PathType.Folder;
		isAbsolute = false;
		
		Init(path, settings, true);
	}

	public void Init(string path, ES2Settings settings, bool useParameters)
	{
		// If no path specified, set defaults and return.
		if(path == "")
			return;
		
		isAbsolute = PathIsAbsolute(path);

		fullString = path;
		if(!isAbsolute)
			persistentPath = GetPersistentPath();
		else
			persistentPath = "";
		
		// Divide the filepath from the parameters
		string[] split = fullString.Split('?');
		
		pathType = GetPathType(split[0]);
		
		// Assign filepath data to variables
		userFolder = Path.GetDirectoryName(split[0]);
		if(pathType != PathType.Folder)
			filename = Path.GetFileNameWithoutExtension(split[0]);
		else
			filename = "";
		extension = Path.GetExtension(split[0]);

		// Some platforms just use the filename
		/*if(!IsURL() &&
			(Application.platform == RuntimePlatform.WSAPlayerARM ||
		   Application.platform == RuntimePlatform.WSAPlayerX64 ||
		   Application.platform == RuntimePlatform.WSAPlayerX86))
			filePath = filename+extension;
		else */if(isAbsolute || IsURL())
			filePath = split[0];
		// If user hasn't specified a save folder, don't try to include a save folder.
		else if(userFolder != "")
			filePath = persistentPath+"/"+userFolder+"/"+filename+extension;
		else
			filePath = persistentPath+"/"+filename+extension;
		
		if(userFolder != "")
			playerPrefsPath = userFolder+"/"+filename+extension;
		else
			playerPrefsPath = filename+extension;
			
		// Initialize variables to make compiler happy.
		// Prioritise ES2Settings tag over this tag.
		if(settings.filenameData.tag != null)
			tag = settings.filenameData.tag;
		else
			tag = "";
		
		// If there are parameters ...
		if(split.Length == 2)
		{
			// Split up our parameters and iterate over them.
			string[] tagSplit = split[1].Split('=','&');
			
			if(tagSplit.Length % 2 !=0)
				Debug.LogError("Easy Save 2 Error: One or more parameters are missing a value, or parameter value contains invalid character such as '=' or '&'.");
			
			if(useParameters)
			{
				for(int i=0;i<tagSplit.Length;i+=2)
				{
					string name = tagSplit[i].ToLower();
					string value = tagSplit[i+1];
					
					ProcessParameter(name, value, settings);
				}
			}
		}
		else if(split.Length > 2)
			Debug.LogError("Easy Save 2 Error: There should be no more than one '?' character in your filepath. Please check that you have not accidentally put in more than one.");
	}
	
	private void ProcessParameter(string name, string value, ES2Settings settings)
	{
		switch(name)       
      	{         
        	case "tag": // Tag
        		tag = value;
        		break;
        	case "encrypt": // Encryption
        		switch(value)
        		{
        			case "false":
        				settings.encrypt = false;
        				break;
        			case "true":
        				settings.encrypt = true;
        				break;
        			default:
        				Debug.LogError(name+" should be either 'true' of 'false', not '"+value+"'.");
						break;
        		}
        		break;
			case "encryptiontype": // Encryption Type
			case "encryptionType":
				switch(value)
				{
					case "AES128":
					case "aes128":
						settings.encryptionType = ES2Settings.EncryptionType.AES128;
						break;
					case "obfuscate":
					case "Obfuscate":
						settings.encryptionType = ES2Settings.EncryptionType.Obfuscate;
						break;
					default:
						Debug.LogError(name+" is not a valid encryption type.");
						break;
				}
			break;
        	case "savelocation": // SaveLocation
        		switch(value)
        		{
        			case "playerprefs":
        			case "PlayerPrefs":
        			case "Playerprefs":
        				settings.saveLocation = ES2Settings.SaveLocation.PlayerPrefs;
        				break;
        			case "resources":
        			case "Resources":
        				settings.saveLocation = ES2Settings.SaveLocation.Resources;
        				break;
        			case "file":
        			case "File":
        				settings.saveLocation = ES2Settings.SaveLocation.File;
						break;
					case "memory":
					case "Memory":
						settings.saveLocation = ES2Settings.SaveLocation.Memory;
						break;
        			default:
        				Debug.LogError(name+" should be either 'File', 'PlayerPrefs', 'Resources' or 'Memory', not '"+value+"'.");
						break;
        		}
        		break;
        	case "encryptionpassword": // Encryption Password
        	case "password":
        		settings.encryptionPassword = value;
        		break;
        	case "webusername": // Web Username
			case "webUsername":
        		settings.webUsername = value;
        		break;
        	case "webpassword": // Web Password
			case "webPassword":
        		settings.webPassword = value;
        		break;
        	case "filename": // Filename
			case "webfilename":
			case "webFilename":
        		settings.webFilename = value;
        		break;
        	case "savenormals": // Normals
        		switch(value)
        		{
        			case "false":
        				settings.saveNormals = false;
        				break;
        			case "true":
        				settings.saveNormals = true;
        				break;
        			default:
        				Debug.LogError(name+" should be either 'true' of 'false', not '"+value+"'.");
						break;
        		}
        		break;
        	case "saveuv": // UV
        		switch(value)
        		{
        			case "false":
        				settings.saveUV = false;
        				break;
        			case "true":
        				settings.saveUV = true;
        				break;
        			default:
        				Debug.LogError(name+" should be either 'true' of 'false', not '"+value+"'.");
						break;
        		}
        		break;
        	case "saveuv2": // UV2
        		switch(value)
        		{
        			case "false":
        				settings.saveUV2 = false;
        				break;
        			case "true":
        				settings.saveUV2 = true;
        				break;
        			default:
        				Debug.LogError(name+" should be either 'true' of 'false', not '"+value+"'.");
						break;
        		}
        		break;
        	case "savetangents": // Tangents
        		switch(value)
        		{
        			case "false":
        				settings.saveTangents = false;
        				break;
        			case "true":
        				settings.saveTangents = true;
        				break;
        			default:
        				Debug.LogError(name+" should be either 'true' of 'false', not '"+value+"'.");
						break;
        		}
        		break;
        	case "id": // ID
        		break;
        	default:
        		Debug.LogError("Easy Save 2 Error: Unknown parameter '"+name+"'.");	
				break;
		}
	}
	
	private static PathType GetPathType(string path)
	{
		if(PathIsURL(path))
			return PathType.URL;
		if(PathIsFolder(path))
			return PathType.Folder;
		return PathType.File;
	}
	
	private static bool PathIsURL(string path)
	{
		if(	path.Length > 5 && 	(path[0] == 'H' || path[0] == 'h') && 
								(path[1] == 'T' || path[1] == 't') && 
								(path[2] == 'T' || path[2] == 't') &&
								(path[3] == 'P' || path[3] == 'p') &&
								 path[4] == ':' ||
								(path.Length > 6 && (path[4] == 'S' || path[4] == 's') && path[5] == ':'))
			return true;
		return false;
	}
	
	private static bool PathIsAbsolute(string path)
	{
		if(path[0] == '/' || path[0] == '\\')
			return true;

		if(path.Length > 2)
			if(path[1] == ':' && (path[2] == '\\' || path[2] == '/'))
				return true;
		return false;
	}
	
	private static bool PathIsFolder(string path)
	{
		char lastChar = path[path.Length-1];
		if(lastChar == '/' || lastChar == '\\')
			return true;
		return false;
	}
	
	public bool HasTag()
	{
		return !string.IsNullOrEmpty(tag);	
	}
	
	public static string GetPersistentPath()
	{
		// Platforms which do not allow for absolute paths.
		/*if(Application.platform == RuntimePlatform.WSAPlayerARM ||
		   Application.platform == RuntimePlatform.WSAPlayerX64 ||
		   Application.platform == RuntimePlatform.WSAPlayerX86)
			return "";*/

		if(ES2GlobalSettings.defaultPCDataPath != "" && (Application.platform == RuntimePlatform.WindowsPlayer ||
		                                                 Application.platform == RuntimePlatform.WindowsEditor))
			return ES2GlobalSettings.defaultPCDataPath;

		if(ES2GlobalSettings.defaultMacDataPath != "" && (Application.platform == RuntimePlatform.OSXPlayer||
		                                                  Application.platform == RuntimePlatform.OSXEditor))
			return ES2GlobalSettings.defaultMacDataPath;

		if(persistentDataPath != "")
			return persistentDataPath;

		return (persistentDataPath = Application.persistentDataPath);
	}
	
	public bool IsURL()
	{
		if(pathType == PathType.URL)
			return true;
		return false;
	}
	
	public bool IsFolder()
	{
		if(pathType == PathType.Folder)
			return true;
		return false;
	}
	
	public bool IsFile()
	{
		if(pathType == PathType.File)
			return true;
		return false;
	}
	
	public bool IsAbsolute()
	{
		return isAbsolute;
	}
	
	public string GetSavePath(ES2Settings.SaveLocation saveLocation)
	{
		if(IsURL())
			return filePath;
		else if(saveLocation == ES2Settings.SaveLocation.PlayerPrefs)
			return playerPrefsPath;
		else if(saveLocation == ES2Settings.SaveLocation.File)
			return filePath;
		return "";
	}
	
	public string directoryPath
	{
		get
		{
			if(isAbsolute)
				return userFolder;
			// Only include user folder if they specified one.
			if(userFolder != "")
				return persistentPath+"/"+userFolder;
			return persistentPath;
		}
	}
	
	public string resourcesPath
	{
		get
		{
			if(userFolder != "")
				return (userFolder+"/"+filename).TrimStart('/','\\');
			return filename;
		}
	}
}

