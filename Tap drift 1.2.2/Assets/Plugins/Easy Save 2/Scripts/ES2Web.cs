#if !DISABLE_WWW
using MoodkieSecurity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ES2Web
{
	public enum HashType {MD5, None};
	
	public bool isDone = false;
	public bool isError = false;
	public string error = "";
	public string errorCode = "";
	
	public HashType hashType = HashType.MD5;
	
	public ES2Settings settings;
    public UnityWebRequest www = null;

	/// <summary>The encoding to use when encoding and decoding data as strings.</summary>
	public System.Text.Encoding encoding = System.Text.Encoding.UTF8;


	private byte[] _data = null;
	/// <summary>Any downloaded data, if applicable. This may also contain an error message, so you should check the 'ifError' variable before reading data.</summary>
	public byte[] data
	{
		get{ return _data; }
	}

	/// <summary>The downloaded data as text, decoded using the encoding specified by the 'encoding' variable.</summary>
	public string text
	{
		get
		{
			if(data == null)
				return null;
			return encoding.GetString(data);
		}
	}

	/// <summary>An array of filenames downloaded from the server. This must only be accessed after calling the 'DownloadFilenames' routine.</summary>
	public string[] filenames
	{
		get
		{
			if(data == null || data.Length == 0)
				return new string[0];
			return text.Split(';');
		}

	}
	
	public float progress
	{
		get{ return www.downloadProgress; }
	}
	
	public float uploadProgress
	{
		get{ return www.uploadProgress; }
	}

	public float downloadProgress
	{
		get{ return www.downloadProgress; }
	}

	public ES2Web(string identifier)
	{
		this.settings = new ES2Settings(identifier);
	}
	
	public ES2Web(string identifier, ES2Settings settings)
	{
		this.settings = settings.Clone(identifier);
	}

	protected IEnumerator SendWebRequest(UnityWebRequest webRequest)
	{
		www = webRequest;
		www.chunkedTransfer = false;
		#if !UNITY_2017_2_OR_NEWER
		yield return webRequest.Send();
		#else
		yield return webRequest.SendWebRequest();
		#endif
	}
	
	#region UploadMethods
	
	public IEnumerator Upload<T>(T param)
	{
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<T>(param, settings.filenameData.tag);
			writer.Save();
			
			byte[] bytes = writer.stream.ReadAllBytes();

			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(bytes)))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator Upload<T>(T[] param)
	{
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<T>(param, settings.filenameData.tag);
			writer.Save();
	
			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(writer.stream.ReadAllBytes())))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator Upload<T>(T[,] param)
	{
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<T>(param, settings.filenameData.tag);
			writer.Save();

			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(writer.stream.ReadAllBytes())))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator Upload<T>(T[,,] param)
	{
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<T>(param, settings.filenameData.tag);
			writer.Save();
			
			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(writer.stream.ReadAllBytes())))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator Upload<TKey, TValue>(Dictionary<TKey,TValue> param)
	{
		// If we're already using this ES2Web object, throw error.
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<TKey,TValue>(param, settings.filenameData.tag);
			writer.Save();
			
			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(writer.stream.ReadAllBytes())))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator Upload<T>(List<T> param)
	{
		// If we're already using this ES2Web object, throw error.
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<T>(param, settings.filenameData.tag);
			writer.Save();
			
			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(writer.stream.ReadAllBytes())))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator Upload<T>(HashSet<T> param)
	{
		// If we're already using this ES2Web object, throw error.
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<T>(param, settings.filenameData.tag);
			writer.Save();
			
			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(writer.stream.ReadAllBytes())))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator Upload<T>(Queue<T> param)
	{
		// If we're already using this ES2Web object, throw error.
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<T>(param, settings.filenameData.tag);
			writer.Save();
			
			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(writer.stream.ReadAllBytes())))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator Upload<T>(Stack<T> param)
	{
		// If we're already using this ES2Web object, throw error.
		CheckWWWUsage();
		
		// Get our data into a byte array using Memory Writer.
		using (ES2Writer writer = ES2Writer.Create(settings))
		{	
			writer.Write<T>(param, settings.filenameData.tag);
			writer.Save();
			
			using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(writer.stream.ReadAllBytes())))
			{
				yield return SendWebRequest(webRequest);
				getError();
			}
			isDone = true;
		}
	}
	
	public IEnumerator UploadRaw(string data)
	{
		return UploadRaw(System.Text.Encoding.UTF8.GetBytes(data));
	}
	
	public IEnumerator UploadRaw(byte[] data)
	{
		// If we're already using this ES2Web object, throw error.
		CheckWWWUsage();
		
		using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(data)))
		{
			yield return SendWebRequest(webRequest);
			getError();
		}
		isDone = true;
	}
	
	public IEnumerator UploadImage(Texture2D tex)
	{
		// If we're already using this ES2Web object, throw error.
		CheckWWWUsage();
		
		using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(tex.EncodeToPNG())))
		{
			yield return SendWebRequest(webRequest);
			getError();
		}
		isDone = true;
	}
	
	public IEnumerator UploadFile(string file)
	{
		// If we're already using this ES2Web object, throw error.
		CheckWWWUsage();

		using(var webRequest = UnityWebRequest.Post(settings.filenameData.filePath, CreateUploadForm(ES2.LoadRaw(file))))
		{
			yield return SendWebRequest(webRequest);
			getError();
		}
		isDone = true;
	}
	
	#endregion
	
	#region LoadMethods
	public Texture2D LoadImage()
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		return ES2.LoadImage(data);
	}
	
	public byte[] LoadRaw()
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		return data;
	}
	
	public T Load<T>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.Read<T>(tag);
	}
	
	public void Load<T>(string tag, T c) where T : class
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			reader.Read<T>(tag, c);
	}
	
	public T[] LoadArray<T>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.ReadArray<T>(tag);
	}
	
	public void LoadArray<T>(string tag, T[] c) where T : class
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			reader.ReadArray<T>(tag, c);
	}
	
	public T[,] Load2DArray<T>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.Read2DArray<T>(tag);
	}
	
	public T[,,] Load3DArray<T>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.Read3DArray<T>(tag);
	}
	
	public Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.ReadDictionary<TKey,TValue>(tag);
	}
	
	public List<T> LoadList<T>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.ReadList<T>(tag);
	}
	
	public HashSet<T> LoadHashSet<T>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.ReadHashSet<T>(tag);
	}
	
	public Queue<T> LoadQueue<T>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.ReadQueue<T>(tag);
	}
	
	public Stack<T> LoadStack<T>(string tag)
	{
		CheckWWWUsage();
		CheckDownloadUsage();
		
		using (ES2Reader reader = ES2Reader.Create(data, settings))
			return reader.ReadStack<T>(tag);
	}
	#endregion
	
	public void SaveToFile(string identifier)
	{
		ES2.Delete (identifier);
		ES2.SaveRaw(data, identifier);
	}
	
	public void SaveToFile(string identifier, ES2Settings settings)
	{
		ES2.Delete (identifier);
		ES2.SaveRaw(data, identifier, settings.Clone(identifier));
	}
	
	public void AppendToFile(string identifier)
	{
		ES2.AppendRaw(data, identifier);
	}
	
	public void AppendToFile(string identifier, ES2Settings settings)
	{
		ES2.AppendRaw(data, identifier, settings.Clone(identifier));
	}
	
	/* Gets all of the filenames from this server. */
	public IEnumerator DownloadFilenames()
	{
		CheckWWWUsage();
	
		using(var webRequest = UnityWebRequest.Post(settings.filenameData.fullString, CreateGetFilesForm()))
		{
			yield return SendWebRequest(webRequest);
			if(!getError())
				_data = webRequest.downloadHandler.data;
		}
		isDone = true;
	}
	
	public string[] GetFilenames()
	{
		return text.Split(new char[]{'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
	}
	
	public IEnumerator Download()
	{
		CheckWWWUsage();


		using(var webRequest = UnityWebRequest.Post(settings.filenameData.fullString, CreateDownloadForm()))
		{
			yield return SendWebRequest(webRequest);
			if(!getError())
				_data = webRequest.downloadHandler.data;
		}
		isDone = true;
	}
	
	public IEnumerator Delete()
	{
		CheckWWWUsage();
		
		WWWForm form = CreateForm();
		// State that we want to download.
		form.AddField("mode", "delete");
		
		// Let the PHP file know if we want to download a file or a tag.
		if(settings.filenameData.HasTag())
			form.AddField("type", "tag");
		else
			form.AddField("type", "file");

		using(var webRequest = UnityWebRequest.Post(settings.filenameData.fullString, form))
		{
			yield return SendWebRequest(webRequest);
			getError();
		}
		isDone = true;
	}

	public string[] GetTags()
	{
		using (ES2Reader reader = ES2Reader.Create(data, settings))
		{
			return reader.GetTags();
		}
	}
	
	private WWWForm CreateUploadForm(byte[] data)
	{
		WWWForm form = CreateForm();
		// State that we want to upload.
		form.AddField("mode", "upload");
		// Add data to POST data.
		form.AddBinaryData("data", data);
		
		return form;
	}
	
	private WWWForm CreateDownloadForm()
	{
		WWWForm form = CreateForm();
		// State that we want to download.
		form.AddField("mode", "download");
		
		// Let the PHP file know if we want to download a file or a tag.
		if(settings.filenameData.HasTag())
			form.AddField("type", "tag");
		else
			form.AddField("type", "file");
		
		return form;
	}
	
	private WWWForm CreateGetFilesForm()
	{
		WWWForm form = CreateForm();
		// State that we want to download.
		form.AddField("mode", "getfilenames");
		return form;
	}
	
	private void CheckWWWUsage()
	{
		// If we're already using this ES2Web object, throw error.
		if(www != null && !www.isDone)
			Debug.LogError("Easy Save 2 Error: This ES2Web object is already being used. Please create a new ES2Web object to perform this upload.");
	}
	
	private void CheckDownloadUsage()
	{
		if(data.Length == 0)
			Debug.LogError("Easy Save 2 Error: No data to load from. Please ensure that you use Download() before calling ES2.Load methods, and that an error didn't occur while downloading.");
	}
	
	/*
		Returns true if there are errors.
	*/
	private bool getError()
	{
		// Reset isError.
		isError = false;
		error = ""; 
		errorCode = "";
		
		// Handle error if our WWW object throws one.
		if(!string.IsNullOrEmpty(www.error))
		{
			isError = true;
			error = "Unity has reported an upload/download error: "+www.error;
			errorCode = "00";
			return true;
		}
		else if(www.downloadHandler != null && www.downloadHandler.text.Length == 2)
		{
			isError = true;
			switch(www.downloadHandler.text)  
			{
				/* Error codes and their accompanying descriptions. */
			case "01":   
				errorCode = "01";
				error = "Could not connect to database. Database or login details are incorrect.";
				return true;
			case "02":   
				errorCode = "02";
				error = "Username and password specified in Unity does not match that specified in ES2.php file.";
				return true;
			case "03":   
				errorCode = "03";
				error = "No data was received at ES2.php.";
				return true;
			case "04":   
				errorCode = "04";
				error = "ES2 MySQL table could not be found on database.";
				return true;
			case "05":
				errorCode = "05";
				error = "The data you are trying to load does not exist.";
				return true;
			default:
				isError = false;
				return false;
			}
		}
		return false;
	}

	private  WWWForm CreateForm()
	{
		WWWForm form = new WWWForm();
		// Add path to POST data.
		form.AddField("filename", settings.webFilename);
		// Add tag to POST data.
		form.AddField("tag", settings.filenameData.tag);
		// Add username to POST data.
		form.AddField("username", settings.webUsername);
		// Add password to POST data.
		if(hashType == HashType.MD5)
			form.AddField("password", StringToMD5(settings.webPassword));
		else
			form.AddField("password", settings.webPassword);
		return form;
	}
	
	private static string StringToMD5(string str)
	{
		var encryptor = new MD5Encryptor();
		return encryptor.GetMD5(str).Replace("-", "").ToLower();
	}
}

#endif