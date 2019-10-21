using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class ES2 
{
	public static bool initialised = false;
	
	/*
	 * 	Initializes the type list by instantiating the ES2Init object from Resources
	 */
	public static void Init()
	{
		GameObject.Instantiate(Resources.Load("ES2/ES2Init"));
	}
		
	#region Save Methods

	public static void Save<T>(T param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(T param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}

	public static void Save<T>(T[] param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}

	public static void Save<T>(T[] param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(T[,] param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}

	// Explicit method to work around stripping issue.
	public static void Save<T>(T[,] param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}

	// Explicit method to work around stripping issue.
	public static void Save2DArray<T>(T[,] param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save2DArray<T>(T[,] param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(T[,,] param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	// Explicit method to work around stripping issue.
	public static void Save<T>(T[,,] param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	// Explicit method to work around stripping issue.
	public static void Save3DArray<T>(T[,,] param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save3DArray<T>(T[,,] param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<TKey, TValue>(Dictionary<TKey,TValue> param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<TKey,TValue>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<TKey,TValue>(Dictionary<TKey,TValue> param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<TKey,TValue>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(List<T> param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(HashSet<T> param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(Queue<T> param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(Stack<T> param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(List<T> param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(HashSet<T> param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(Queue<T> param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}
	
	public static void Save<T>(Stack<T> param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{	
			mWriter.Write<T>(param, newSettings.filenameData.tag);
			mWriter.Save();
		}
	}

	public static void SaveRaw(string param, string identifier)
	{
		SaveRaw(System.Text.Encoding.UTF8.GetBytes(param), identifier);
	}
	
	public static void SaveRaw(string param, string identifier, ES2Settings settings)
	{
		SaveRaw(System.Text.Encoding.UTF8.GetBytes(param), identifier, settings);
	}

	public static void SaveRaw(byte[] param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(param);
			mWriter.Save(false);
		}
	}
	
	public static void SaveRaw(byte[] param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(param);
			mWriter.Save(false);
		}
	}
	
	public static void SaveRaw(TextAsset param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(param.bytes);
			mWriter.Save(false);
		}
	}
	
	public static void SaveRaw(TextAsset param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(param.bytes);
			mWriter.Save(false);
		}
	}

	public static void AppendRaw(string param, string identifier)
	{
		AppendRaw(System.Text.Encoding.UTF8.GetBytes(param), identifier);
	}
	
	public static void AppendRaw(string param, string identifier, ES2Settings settings)
	{
		AppendRaw(System.Text.Encoding.UTF8.GetBytes(param), identifier, settings);
	}
	
	public static void AppendRaw(byte[] param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		newSettings.fileMode = ES2Settings.ES2FileMode.Append;
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(param);
			mWriter.Save(false);
		}
	}
	
	public static void AppendRaw(byte[] param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		newSettings.fileMode = ES2Settings.ES2FileMode.Append;
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(param);
			mWriter.Save(false);
		}
	}
	
	public static void AppendRaw(TextAsset param, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		newSettings.fileMode = ES2Settings.ES2FileMode.Append;
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(param.bytes);
			mWriter.Save(false);
		}
	}
	
	public static void AppendRaw(TextAsset param, string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		newSettings.fileMode = ES2Settings.ES2FileMode.Append;
		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(param.bytes);
			mWriter.Save(false);
		}
	}

	public static void SaveImage(Texture2D tex, string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		if(newSettings.filenameData.extension.ToLower() != ".png")
			Debug.LogError("Easy Save 2 Error: ES2.SaveImage can only be used to save PNG files. Please change your path extension to PNG.");

		using (ES2Writer mWriter = ES2Writer.Create(newSettings))
		{
			mWriter.WriteRaw(tex.EncodeToPNG());
			mWriter.Save(false);
		}
	}

	#endregion

	#region Load Methods

	public static byte[] LoadRaw(string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadRaw();
	}
	
	public static byte[] LoadRaw(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadRaw();
	}
	
	public static System.Object LoadObject(string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadObject(newSettings.filenameData.tag);
	}
	
	public static System.Object LoadObject(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadObject(newSettings.filenameData.tag);
	}
	
	public static T Load<T>(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.Read<T>(newSettings.filenameData.tag);
	}
	
	public static T Load<T>(string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.Read<T>(newSettings.filenameData.tag);
	}
	
	public static void Load<T>(string identifier, T c) where T : class
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			reader.Read<T>(newSettings.filenameData.tag, c);
	}
	
	public static void Load<T>(string identifier, T c, ES2Settings settings) where T : class
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			reader.Read<T>(newSettings.filenameData.tag, c);
	}
	
	public static T[] LoadArray<T>(string identifier)
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadArray<T>(newSettings.filenameData.tag);
	}
	
	public static T[] LoadArray<T>(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadArray<T>(newSettings.filenameData.tag);
	}
	
	public static void LoadArray<T>(string identifier, T[] c) where T : class
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			reader.ReadArray<T>(newSettings.filenameData.tag, c);
	}
	
	public static void LoadArray<T>(string identifier, T[] c, ES2Settings settings) where T : class
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			reader.ReadArray<T>(newSettings.filenameData.tag, c);
	}
	
	public static T[,] Load2DArray<T>(string identifier)
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.Read2DArray<T>(newSettings.filenameData.tag);
	}
	
	public static T[,] Load2DArray<T>(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.Read2DArray<T>(newSettings.filenameData.tag);
	}
	
	public static T[,,] Load3DArray<T>(string identifier)
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.Read3DArray<T>(newSettings.filenameData.tag);
	}
	
	public static T[,,] Load3DArray<T>(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.Read3DArray<T>(newSettings.filenameData.tag);
	}
	
	public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(string identifier)
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadDictionary<TKey,TValue>(newSettings.filenameData.tag);
	}
	
	public static Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadDictionary<TKey,TValue>(newSettings.filenameData.tag);
	}
		
	public static List<T> LoadList<T>(string identifier)
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadList<T>(newSettings.filenameData.tag);
	}
	
	public static List<T> LoadList<T>(string identifier, ES2Settings settings)
	{	
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadList<T>(newSettings.filenameData.tag);
	}
	
	public static void LoadList<T>(string identifier, List<T> c) where T : class
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			reader.ReadList<T>(newSettings.filenameData.tag, c);
	}
	
	public static void LoadList<T>(string identifier, List<T> c, ES2Settings settings) where T : class
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			reader.ReadList<T>(newSettings.filenameData.tag, c);
	}
	
	public static HashSet<T> LoadHashSet<T>(string identifier)
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadHashSet<T>(newSettings.filenameData.tag);
	}
	
	public static HashSet<T> LoadHashSet<T>(string identifier, ES2Settings settings)
	{	
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadHashSet<T>(newSettings.filenameData.tag);
	}
	
	public static Queue<T> LoadQueue<T>(string identifier)
	{	
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadQueue<T>(newSettings.filenameData.tag);
	}
	
	public static Queue<T> LoadQueue<T>(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadQueue<T>(newSettings.filenameData.tag);
	}
	
	public static Stack<T> LoadStack<T>(string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadStack<T>(newSettings.filenameData.tag);
	}
	
	public static Stack<T> LoadStack<T>(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadStack<T>(newSettings.filenameData.tag);
	}

	public static ES2Data LoadAll(string path)
	{
		ES2Settings settings = new ES2Settings(path);
		using (ES2Reader reader = ES2Reader.Create(settings))
			return reader.ReadAll();
	}

	public static ES2Data LoadAll(string identifier, ES2Settings settings)
	{
		ES2Settings newSettings = settings.Clone(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			return reader.ReadAll();
	}

	public static Texture2D LoadImage(string path)
	{
		if(Application.platform == RuntimePlatform.WebGLPlayer)
			Debug.LogError("Easy Save 2 Error: You cannot use LoadImage with WebGL");

		ES2Settings settings = new ES2Settings(path);

		if(!settings.IsImageFile)
			Debug.LogError ("ES2.LoadImage can only be used to load JPG and PNG files.\nThe exception thrown is as follows:\n");

		using (ES2Reader reader = ES2Reader.Create(settings))
			return LoadImage(reader.ReadRaw());
	}

	public static Texture2D LoadImage(byte[] bytes)
	{
		Texture2D tex = new Texture2D(1,1);
		tex.LoadImage(bytes);
		return tex;
	}

	#if !DISABLE_WWW
	public static AudioClip LoadAudio(string path)
	{
	#if !UNITY_2018_1_OR_NEWER
		if(Application.platform == RuntimePlatform.WebGLPlayer)
			Debug.LogError("Easy Save 2 Error: You cannot use LoadAudio with WebGL.");

		ES2Settings settings = new ES2Settings(path);

		if(settings.filenameData.extension.ToLower() == ".mp3" 
		   && (Application.platform == RuntimePlatform.WindowsPlayer
		   || Application.platform == RuntimePlatform.OSXPlayer))
			Debug.LogError("Easy Save 2 Error: You can only load Ogg, WAV,XM, IT, MOD or S3M with Unity Standalone");

		if(settings.filenameData.extension.ToLower() == ".ogg" 
		   && (Application.platform == RuntimePlatform.IPhonePlayer
		    || Application.platform == RuntimePlatform.Android))
			Debug.LogError("Easy Save 2 Error: You can only load MP3, WAV,XM, IT, MOD or S3M with Unity on Mobile Platforms");

		WWW www = new WWW("file://"+settings.filenameData.filePath);

		while(!www.isDone)
		{
			// Wait for it to load.
		}

		if(!string.IsNullOrEmpty(www.error))
			Debug.LogError(www.error);

		float unityVersion = float.Parse(Application.unityVersion.Substring(0, 3));
		AudioClip clip;
		if(unityVersion < 5.6f)
#if NETFX_CORE
            clip = (AudioClip)typeof(WWW).GetTypeInfo().GetDeclaredProperty("audioClip").GetValue(www, null);
		else
			clip = (AudioClip)System.Type.GetType("WWWAudioExtensions").GetMethod("GetAudioClip").Invoke(null, new object[]{www});
#else
            clip = (AudioClip)typeof(WWW).GetProperty("audioClip").GetValue(www, null);
        else
            clip = (AudioClip)System.Type.GetType("WWWAudioExtensions").GetMethod("GetAudioClip").Invoke(null, new object[] { www });
#endif


        return clip;
		#else
		Debug.LogError("ES2.LoadAudio was deprecated in Unity 2018. Please use Easy Save 3's ES3.LoadAudio method instead.");
		return null;
		#endif
	}
#endif

#endregion

            #region File & Folder Methods

            public static void CacheFile(string identifier)
	{
		ES2Settings newSettings = new ES2Settings(identifier);
		using (ES2Reader reader = ES2Reader.Create(newSettings))
			reader.CacheFile();
	}

	public static bool Exists(string identifier)
	{
		return ES2File.Exists(new ES2Settings(identifier));
	}
	
	public static bool Exists(string identifier, ES2Settings settings)
	{
		return ES2File.Exists(settings.Clone(identifier));
	}
	
	public static void Delete(string identifier)
	{
		ES2File.Delete(new ES2Settings(identifier));
	}

	public static void Delete(string identifier, ES2Settings settings)
	{
		ES2File.Delete(settings.Clone(identifier));
	}

	public static void DeleteDefaultFolder()
	{
		if(ES2GlobalSettings.defaultSaveLocation == ES2Settings.SaveLocation.PlayerPrefs)
			PlayerPrefs.DeleteAll();
		else
			ES2.Delete(ES2FilenameData.GetPersistentPath()+"/");
	}
	
	public static void Rename(string identifier, string newIdentifier)
	{
		ES2File.Rename(new ES2Settings(identifier), new ES2Settings(newIdentifier));
	}
	
	public static void Rename(string identifier, string newIdentifier, ES2Settings settings)
	{
		ES2File.Rename(settings.Clone(identifier), settings.Clone(newIdentifier));
	}
	
	public static string[] GetFiles(string identifier)
	{
		return ES2File.GetFiles(new ES2Settings(identifier));
	}

	public static string[] GetFiles(string identifier, ES2Settings settings)
	{
		return ES2File.GetFiles(settings.Clone(identifier));
	}

	public static string[] GetFiles(string identifier, string extension)
	{
		return ES2File.GetFiles(new ES2Settings(identifier), extension);
	}
	
	public static string[] GetFolders(string identifier)
	{
		return ES2File.GetFolders(new ES2Settings(identifier));
	}

	public static string[] GetFolders(string identifier, ES2Settings settings)
	{
		return ES2File.GetFolders(settings.Clone(identifier));
	}
	
	public static string[] GetTags(string identifier)
	{
		return ES2File.GetTags(new ES2Settings(identifier));
	}
	
	public static string[] GetTags(string identifier, ES2Settings settings)
	{
		return ES2File.GetTags(settings.Clone(identifier));
	}

	#endregion
}
