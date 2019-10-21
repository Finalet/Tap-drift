using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2GlobalSettings : MonoBehaviour
{
	public static bool hasInitialized = false;
		
	public static ES2Settings.SaveLocation _defaultSaveLocation = ES2Settings.SaveLocation.File;

	public static ES2Settings.OptimizeMode defaultOptimizeMode = ES2Settings.OptimizeMode.Fast;
	public static ES2Settings.Format defaultFormat = ES2Settings.Format.Binary;

	public static string defaultPCDataPath = "";
	public static string defaultMacDataPath = "";

	public static bool defaultEncrypt = false;
	public static string defaultEncryptionPassword = "f2WYP35djvxP2pdR";
	public static ES2Settings.EncryptionType defaultEncryptionType = ES2Settings.EncryptionType.AES128;
	
	public static string defaultWebUsername = "ES2";
	public static string defaultWebPassword = "65w84e4p994z3Oq";
	public static string defaultWebFilename = "file.es2";
	
	public static int defaultBufferSize = 4098;
	
	public ES2Settings.SaveLocation saveLocation;
	public ES2Settings.OptimizeMode optimizeMode;

	public string PCDataPath;
	public string MacDataPath;

	public bool encrypt;
	public string encryptionPassword;
	public ES2Settings.EncryptionType encryptionType;
	
	public string webUsername;
	public string webPassword;
	 
	public int bufferSize = 4098;
	
	public static ES2Settings.SaveLocation defaultSaveLocation
	{
		get
		{	
			if(Application.platform == RuntimePlatform.WebGLPlayer)
					return ES2Settings.SaveLocation.PlayerPrefs;
			return _defaultSaveLocation;
		}
		set
		{
			_defaultSaveLocation = value;
		}
	}
	
	public void Awake()
	{		
		if(Application.platform == RuntimePlatform.WebGLPlayer)
			defaultSaveLocation = ES2Settings.SaveLocation.PlayerPrefs;
		else
			defaultSaveLocation = saveLocation;

			
		defaultOptimizeMode = optimizeMode;
		defaultEncrypt = encrypt;
		defaultEncryptionPassword = encryptionPassword;
		defaultEncryptionType = encryptionType;
		defaultWebUsername = webUsername;
		defaultWebPassword = webPassword;
		defaultBufferSize = bufferSize;
		defaultPCDataPath = PCDataPath;
		defaultMacDataPath = MacDataPath;
	}
}
