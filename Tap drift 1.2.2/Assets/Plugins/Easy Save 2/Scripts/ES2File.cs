using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2File
{
	public static void Delete(ES2Settings settings)
	{
		// Resources
		if(settings.saveLocation == ES2Settings.SaveLocation.Resources)
			Debug.LogError("Easy Save 2 Error: You can not delete from Resources.");
		
		if(!settings.filenameData.HasTag())
		{
			// Delete file from cache.
			ES2Cache.DeleteCachedFile(settings.filenameData.filePath);

			// PlayerPrefs
			if(settings.saveLocation == ES2Settings.SaveLocation.PlayerPrefs)
			{
				if(settings.filenameData.IsFile())
					PlayerPrefs.DeleteKey(settings.filenameData.playerPrefsPath);
				else if(settings.filenameData.IsFolder())
					Debug.LogError("Easy Save 2 Error: You cannot delete folders using Easy Save 2 for PlayerPrefs.");
			}
			// File
			else if(settings.saveLocation == ES2Settings.SaveLocation.File)
				DeleteFile (settings);
		}
		else
		{
			// Delete Tag
			using (ES2Writer writer = ES2Writer.Create(settings))
			{
				writer.Delete(settings.tag);
				writer.Save();
			}
		}	
	}

	public static void DeleteFile(ES2Settings settings)
	{
		if(settings.filenameData.IsFile())
		{
			if(ES2FileUtility.Exists(settings.filenameData.filePath))
				ES2FileUtility.Delete(settings.filenameData.filePath);
		}
		else if(settings.filenameData.IsFolder())
		{
			if(ES2DirectoryUtility.Exists(settings.filenameData.filePath))
				ES2DirectoryUtility.Delete(settings.filenameData.filePath, true);
		}
	}
	
	public static void Delete(string identifier)
	{
		Delete(new ES2Settings(identifier));
	}
	
	public static bool Exists(ES2Settings settings)
	{
		// Resources
		if(settings.saveLocation == ES2Settings.SaveLocation.Resources)
		{
			if(settings.filenameData.extension != ".bytes")
				Debug.LogError("Easy Save 2 Error: Can only check existence of files from Resources with the extension '.bytes'.");
				
			TextAsset data = Resources.Load(settings.filenameData.resourcesPath) as TextAsset;

			if(settings.filenameData.HasTag())
			{
				return CheckForTagInBytes(settings.filenameData.tag, data.bytes);
			}
			else if(data != null)
				return true;
			return false;
		}
		else if(settings.saveLocation == ES2Settings.SaveLocation.PlayerPrefs)
		{
			if(!settings.filenameData.HasTag())
				return PlayerPrefs.HasKey(settings.filenameData.playerPrefsPath);

			if(!PlayerPrefs.HasKey(settings.filenameData.playerPrefsPath))
				return false;
			
			using (ES2Reader reader = ES2Reader.Create(settings))
			{
				bool exists = reader.TagExists();
				return exists;
			}
		}
		else if(settings.filenameData.IsURL())
			return false;

		else if(settings.filenameData.HasTag())
		{
			if(!ES2FileUtility.Exists(settings.filenameData.filePath))
				return false;
			
			using (ES2Reader reader = ES2Reader.Create(settings))
			{
				bool exists = reader.TagExists();
				return exists;
			}
		}
		else
		{
			if(settings.filenameData.IsFolder())
				return ES2DirectoryUtility.Exists(settings.filenameData.directoryPath);
			return ES2FileUtility.Exists(settings.filenameData.filePath);
		}

	}
	
	private static bool CheckForTagInBytes(string tag, byte[] bytes)
	{
		using(ES2Reader reader = new ES2Reader(bytes, new ES2Settings(ES2Settings.SaveLocation.Memory)))
		{
			return reader.TagExists(tag);
		}
	}
	
	public static void CreateFolder(string identifier)
	{
		ES2DirectoryUtility.CreateDirectory(identifier);
	}
	
	public static void Rename(ES2Settings settings, ES2Settings newSettings)
	{
		// Resources
		if(settings.saveLocation == ES2Settings.SaveLocation.Resources)
			Debug.LogError("Easy Save 2 Error: You can not rename a file in Resources.");
		
		if(!settings.filenameData.HasTag())
		{
			// PlayerPrefs
			if(settings.saveLocation == ES2Settings.SaveLocation.PlayerPrefs)
			{
				if(settings.filenameData.IsFile())
				{
					string saveData = PlayerPrefs.GetString(settings.filenameData.playerPrefsPath);
					PlayerPrefs.DeleteKey(settings.filenameData.playerPrefsPath);
					PlayerPrefs.SetString(newSettings.filenameData.playerPrefsPath, saveData);
				}
				else if(settings.filenameData.IsFolder())
					Debug.LogError("Easy Save 2 Error: You cannot rename folders using Easy Save 2 for PlayerPrefs.");
			}
			// File
			else if(settings.saveLocation == ES2Settings.SaveLocation.File)
			{
				MoveFile(settings, newSettings);
			}
		}
		else
		{
			// Rename Tag
			using (ES2Writer writer = ES2Writer.Create(settings))
			{
				writer.Rename(settings.tag, newSettings.tag);
				writer.Save(false);
			}
		}	
	}
	
	public static void MoveFile(ES2Settings settings, ES2Settings newSettings)
	{
		if(settings.filenameData.IsFile())
		{
			if(ES2FileUtility.Exists(settings.filenameData.filePath))
			{
				if(!ES2DirectoryUtility.Exists(newSettings.filenameData.directoryPath))
					ES2DirectoryUtility.CreateDirectory(newSettings.filenameData.directoryPath);
				ES2FileUtility.Move(settings.filenameData.filePath, newSettings.filenameData.filePath);
			}
		}
		else if(settings.filenameData.IsFolder() && ES2DirectoryUtility.Exists(settings.filenameData.filePath))
		{
			if(ES2DirectoryUtility.Exists(settings.filenameData.filePath))
				ES2DirectoryUtility.Move(settings.filenameData.directoryPath, newSettings.filenameData.directoryPath);
		}
	}
	
	public static string[] GetFolders(ES2Settings settings)
	{
		if(settings.saveLocation == ES2Settings.SaveLocation.Resources || settings.saveLocation == ES2Settings.SaveLocation.PlayerPrefs)
			Debug.LogError("Easy Save 2 Error: You can only use GetFolders when using the File save location.");
			
		if(!settings.filenameData.IsFolder())
			Debug.LogError("Easy Save 2 Error: You should only provide GetFolders with a path to a folder, not a file or tag.");

		string[] folders = ES2DirectoryUtility.GetDirectories(settings.filenameData.directoryPath);
		
		for(int i=0;i<folders.Length;i++)
		{
			string[] strings = folders[i].Split(new char[]{'/','\\'});
			folders[i] = strings[strings.Length-1];
		}
		return folders;
	}

	public static string[] GetFiles(ES2Settings settings)
	{
		return GetFiles(settings, "*");
	}

	public static string[] GetFiles(ES2Settings settings, string extension)
	{
		if(settings.saveLocation == ES2Settings.SaveLocation.Resources || settings.saveLocation == ES2Settings.SaveLocation.PlayerPrefs)
			Debug.LogError("Easy Save 2 Error: You can only use GetFiles when using the File save location.");
			
		if(!settings.filenameData.IsFolder())
			Debug.LogError("Easy Save 2 Error: You should only provide GetFiles with a path to a folder, not a file or tag.");
			
		string[] files = ES2DirectoryUtility.GetFiles(settings.filenameData.directoryPath, extension);
		
		for(int i=0;i<files.Length;i++)
		{
			string[] strings = files[i].Split(new char[]{'/','\\'});
			files[i] = strings[strings.Length-1];
		}
		return files;
	}
	
	public static string[] GetTags(ES2Settings settings)
	{		
		using (ES2Reader reader = ES2Reader.Create(settings))
		{
			return reader.GetTags();
		}
	}
}
