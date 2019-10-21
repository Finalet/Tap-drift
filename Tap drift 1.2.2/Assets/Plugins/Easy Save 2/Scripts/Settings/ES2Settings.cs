using System.Collections;
using UnityEngine;

public sealed class ES2Settings
{
	public enum OptimizeMode {Fast, LowMemory};
	public enum SaveLocation {PlayerPrefs, File, Resources, Memory};
	public enum Format {Binary};
	public enum EncryptionType {AES128, Obfuscate};
	
	// Filename Data
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2FilenameData filenameData;

	// Save
	public ES2Settings.SaveLocation saveLocation = ES2GlobalSettings.defaultSaveLocation;
	public ES2Settings.OptimizeMode optimizeMode = ES2GlobalSettings.defaultOptimizeMode;
	public ES2Settings.Format format = ES2GlobalSettings.defaultFormat;

	// Encryption
	public bool encrypt = ES2GlobalSettings.defaultEncrypt;
	public string encryptionPassword = ES2GlobalSettings.defaultEncryptionPassword;
	public EncryptionType encryptionType = ES2GlobalSettings.defaultEncryptionType;
	
	// Web
	public string webUsername = ES2GlobalSettings.defaultWebUsername;
	public string webPassword = ES2GlobalSettings.defaultWebPassword;
	public string webFilename = ES2GlobalSettings.defaultWebFilename;
	
	// Mesh
	public bool saveNormals = true;
	public bool saveUV = true;
	public bool saveUV2 = true;
	public bool saveTangents = true;
	public bool saveSubmeshes = true;
	public bool saveSkinning = true;
	public bool saveColors = true;

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public byte meshSettingsCount = 7;
	
	// Misc
	public string name = "ES2Settings";
	
	public enum ES2FileMode {Create, Append, Open};
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2FileMode fileMode = ES2FileMode.Create;

	public int bufferSize = ES2GlobalSettings.defaultBufferSize;
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Init()
	{
		if(!ES2.initialised)
			ES2.Init();		
					
		// Initialize global settings,
		saveLocation = ES2GlobalSettings.defaultSaveLocation;
		optimizeMode = ES2GlobalSettings.defaultOptimizeMode;
		format = ES2GlobalSettings.defaultFormat;
		encrypt = ES2GlobalSettings.defaultEncrypt;
		encryptionPassword = ES2GlobalSettings.defaultEncryptionPassword;
		encryptionType = ES2GlobalSettings.defaultEncryptionType;
		webUsername = ES2GlobalSettings.defaultWebUsername;
		webPassword = ES2GlobalSettings.defaultWebPassword;
		webFilename = ES2GlobalSettings.defaultWebFilename;
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2Settings Clone()
	{
		ES2Settings settings = new ES2Settings();
		
		settings.filenameData = filenameData;
		settings.saveLocation = saveLocation;
		settings.optimizeMode = optimizeMode;
		settings.encrypt = encrypt;
		settings.encryptionPassword = encryptionPassword;
		settings.encryptionType = encryptionType;
		settings.webUsername = webUsername;
		settings.webPassword = webPassword;
		settings.webFilename = webFilename;
		settings.saveNormals = saveNormals;
		settings.saveColors = saveColors;
		settings.saveUV = saveUV;
		settings.saveUV2 = saveUV2;
		settings.saveTangents = saveTangents;
		settings.saveSubmeshes = saveSubmeshes;
		settings.saveSkinning = saveSkinning;
		settings.meshSettingsCount = meshSettingsCount;
		settings.name = name;
		settings.fileMode = fileMode;
		
		return settings;
	}
	
	/* Clone an object and apply parameter settings to it */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2Settings Clone(string identifier)
	{
		ES2Settings settings = Clone();
		settings.filenameData =  new ES2FilenameData(identifier, settings, true);
		return settings;
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public byte[] MeshSettingsToByteArray()
	{
		return new byte[]	{	meshSettingsCount,
								saveNormals?(byte)1:(byte)0,
								saveUV?(byte)1:(byte)0,
								saveUV2?(byte)1:(byte)0,
								saveTangents?(byte)1:(byte)0,
								saveSubmeshes?(byte)1:(byte)0,
								saveSkinning?(byte)1:(byte)0,
								saveColors?(byte)1:(byte)0,
							};
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void MeshSettingsFromByteArray(byte[] bytes)
	{
		if(bytes.Length >= 4)
		{
			this.saveNormals = bytes[0]==0?false:true;
			this.saveUV = bytes[1]==0?false:true;
			this.saveUV2 = bytes[2]==0?false:true;
			this.saveTangents = bytes[3]==0?false:true;
		}
		if(bytes.Length >= 5)
			this.saveSubmeshes = bytes[4]==0?false:true;
		if(bytes.Length >= 6)
			this.saveSkinning = bytes[5]==0?false:true;
		if(bytes.Length == 7)
			this.saveColors = bytes[6]==0?false:true;
		else if(bytes.Length > 7)
			Debug.LogError("Byte array containing mesh settings is too long. Length: "+bytes.Length  +".");
	}
		
	public ES2Settings(string identifier)
	{
		Init();
		filenameData = new ES2FilenameData(identifier, this);
	}

	public ES2Settings(SaveLocation saveLocation)
	{
		Init();
		this.saveLocation = saveLocation;
	}
	
	public ES2Settings()
	{
		Init();
	}
	
	public string tag
	{
		get
		{
			return filenameData.tag;
		}
		set
		{
			filenameData.tag = value;
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool IsImageFile
	{
		get
		{
			string extension = filenameData.extension.ToLower();
			if(extension == ".jpg" || extension == ".png" || extension == ".jpeg" || extension == ".bytes")
				return true;
			return false;
		}
	}
}
