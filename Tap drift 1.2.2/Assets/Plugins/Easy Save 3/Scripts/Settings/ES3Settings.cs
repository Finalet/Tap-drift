using UnityEngine;
using ES3Internal;

public class ES3Settings : System.ICloneable
{
	private static ES3Settings _defaults = null;

	[SerializeField]
	private ES3.Location _location;
	/// <summary>The location where we wish to store data. As it's not possible to save/load from File in WebGL, if the default location is File it will use PlayerPrefs instead.</summary>
	public ES3.Location location
	{
		get
		{
			if(_location == ES3.Location.File && Application.platform == RuntimePlatform.WebGLPlayer)
				return ES3.Location.PlayerPrefs;
			return _location;
		}
		set{ _location = value; }
	}

	/// <summary>The path associated with this ES3Settings object, if any.</summary>
	public string path;
	/// <summary>The type of encryption to use when encrypting data, if any.</summary>
	public ES3.EncryptionType encryptionType;
	/// <summary>The password to use when encrypting data.</summary>
	public string encryptionPassword;
	/// <summary>The default directory in which to store files, and the location which relative paths should be relative to.</summary>
	public ES3.Directory directory;
	/// <summary>What format to use when serialising and deserialising data.</summary>
	public ES3.Format format;
	/// <summary>Any stream buffers will be set to this length in bytes.</summary>
	public int bufferSize;
	/// <summary>The text encoding to use for text-based format. Note that changing this may invalidate previous save data.</summary>
	public System.Text.Encoding encoding = System.Text.Encoding.UTF8;

	/// <summary>Whether we should check that the data we are loading from a file matches the method we are using to load it.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool typeChecking;

	/// <summary>Enabling this ensures that only serialisable fields are serialised. Otherwise, possibly unsafe fields and properties will be serialised.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool safeReflection;
	/// <summary>Whether UnityEngine.Object members should be stored by value, reference or both.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES3.ReferenceMode memberReferenceMode;
	/// <summary>Whether the main save methods should save UnityEngine.Objects by value, reference, or both.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES3.ReferenceMode referenceMode = ES3.ReferenceMode.ByRefAndValue;

	/// <summary>The names of the Assemblies we should try to load our ES3Types from.</summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public string[] assemblyNames;

	private void CopyInto(ES3Settings newSettings)
	{
		newSettings._location = _location;
		newSettings.directory = directory;
		newSettings.format = format;
		newSettings.path = path;
		newSettings.encryptionType = encryptionType;
		newSettings.encryptionPassword = encryptionPassword;
		newSettings.bufferSize = bufferSize;
		newSettings.encoding = encoding;
		newSettings.typeChecking = typeChecking;
		newSettings.safeReflection = safeReflection;
		newSettings.memberReferenceMode = memberReferenceMode;
		newSettings.assemblyNames = assemblyNames;
	}

	/// <summary>Creates a new ES3Settings object.</summary>
	public ES3Settings()
	{
		ApplyDefaults();
	}

	/// <summary>Creates a new ES3Settings object with the given path.</summary>
	/// <param name="path">The path associated with this ES3Settings object.</param>
	public ES3Settings(string path) : this()
	{
		this.path = path;
	}

	/// <summary>Creates a new ES3Settings object with the given path.</summary>
	/// <param name="path">The path associated with this ES3Settings object.</param>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	public ES3Settings(string path, ES3Settings settings)
	{
		// if there are settings to merge, merge them.
		if(settings != null)
			settings.CopyInto(this);
		this.path = path;
	}

	/// <summary>Creates a new ES3Settings object with the given encryption settings.</summary>
	/// <param name="encryptionType">The type of encryption to use, if any.</param>
	/// <param name="encryptionPassword">The password to use when encrypting data.</param>
	public ES3Settings(ES3.EncryptionType encryptionType, string encryptionPassword) : this()
	{
		this.encryptionType = encryptionType;
		this.encryptionPassword = encryptionPassword;
	}

	/// <summary>Creates a new ES3Settings object with the given path and encryption settings.</summary>
	/// <param name="path">The path associated with this ES3Settings object.</param>
	/// <param name="encryptionType">The type of encryption to use, if any.</param>
	/// <param name="encryptionPassword">The password to use when encrypting data.</param>
	public ES3Settings(string path, ES3.EncryptionType encryptionType, string encryptionPassword) : this(path)
	{
		this.encryptionType = encryptionType;
		this.encryptionPassword = encryptionPassword;
	}

	/// <summary>Creates a new ES3Settings object with the given path and encryption settings.</summary>
	/// <param name="path">The path associated with this ES3Settings object.</param>
	/// <param name="encryptionType">The type of encryption to use, if any.</param>
	/// <param name="encryptionPassword">The password to use when encrypting data.</param>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	public ES3Settings(string path, ES3.EncryptionType encryptionType, string encryptionPassword, ES3Settings settings) : this(path, settings)
	{
		this.encryptionType = encryptionType;
		this.encryptionPassword = encryptionPassword;
	}
		
	/* Base constructor which allows us to bypass defaults so it can be called by Editor serialization */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES3Settings(bool applyDefaults)
	{
		if(applyDefaults)
			ApplyDefaults();
	}

	protected void ApplyDefaults()
	{
		if(_defaults == null)
			LoadDefaults();
		if(_defaults != null)
			_defaults.CopyInto(this);
	}
	
	internal static void LoadDefaults()
	{
		#if !UNITY_EDITOR
		var go = Resources.Load<GameObject>("ES3/ES3 Default Settings");
		if(go == null)
		{
			Debug.LogError("Default settings were not found in Resources folder 'ES3'.");
			return;
		}
		#else
		var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GetAssetPath(Resources.Load<GameObject>("ES3/ES3 Default Settings")));
		#endif	
		var component = go.GetComponent<ES3DefaultSettings>();
		if(component == null)
			return;
		_defaults = component.settings;
	}
	
	/*internal static void LoadDefaults()
	{
		#if !UNITY_EDITOR
		var go = Resources.Load<GameObject>("ES3/ES3 Default Settings");
		if(go == null)
		{
			Debug.LogError("Default settings were not found in Resources folder 'ES3'.");
			return;
		}
		#else
		var guids = UnityEditor.AssetDatabase.FindAssets("ES3 Default Settings t:gameobject");
		if(guids.Length != 1)
		{
			Debug.LogError("Default settings were not found, or more than one default settings GameObject was found. Number of settings objects found: "+guids.Length);
			return;
		}
		var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
		var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
		#endif	
		var component = go.GetComponent<ES3DefaultSettings>();
		if(component == null)
			return;
		_defaults = component.settings;
	}*/

	/// <summary>Gets the full, absolute path which this ES3Settings object identifies.</summary>
	public string FullPath
	{
		get
		{
			if(IsAbsolute(path))
				return path;

			if(location == ES3.Location.File)
			{
				if(directory == ES3.Directory.PersistentDataPath)
					return Application.persistentDataPath + "/" + path;
				if(directory == ES3.Directory.DataPath)
					return Application.dataPath + "/" + path;
				throw new System.NotImplementedException("File directory \""+directory+"\" has not been implemented.");
			}
			if(location == ES3.Location.Resources)
			{
				string resourcesPath = path.Replace(".bytes","");
				if(resourcesPath == path)
					throw new System.ArgumentException("Extension of file in Resources must be .bytes, but path given was \""+path+"\"");
				return resourcesPath;
			}
			return path;
		}
	}

	private static bool IsAbsolute(string path)
	{
		if(path.Length > 0 && (path[0] == '/' || path[0] == '\\'))
			return true;
		if(path.Length > 1 && path[1] == ':')
			return true;
		return false;
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public object Clone()
	{
		var settings = new ES3Settings();
		CopyInto(settings);
		return settings;
	}

	#if UNITY_EDITOR
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public static ES3DefaultSettings GetDefaultSettings()
	{
		var go = Resources.Load<GameObject>("ES3/ES3 Default Settings");
		if(go == null)
			Debug.LogError("Could not find ES3 Default Settings object in Easy Save 3/Resources/ES3.");
		var settings = go.GetComponent<ES3DefaultSettings>();
		if(settings == null)
			Debug.LogError("There is no ES3 Default Settings script attached to the ES3 Default Settings object in Easy Save 3/Resources/ES3");
		return settings;
	}
	#endif
}

/*
 * 	A serializable version of the settings we can use as a field in the Editor, which doesn't automatically
 * 	assign defaults to itself, so we get no serialization errors.
 */
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[System.Serializable]
public class ES3SerializableSettings : ES3Settings
{
	public ES3SerializableSettings() : base(false){}
	public ES3SerializableSettings(bool applyDefaults) : base(applyDefaults){}

	#if UNITY_EDITOR
	public bool showAdvancedSettings = false;
	#endif
}