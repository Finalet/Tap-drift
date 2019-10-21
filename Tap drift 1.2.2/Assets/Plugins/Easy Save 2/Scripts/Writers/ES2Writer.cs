using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MoodkieSecurity;

public partial class ES2Writer : System.IDisposable
{
	public ES2Settings settings;

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2Stream stream;

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public HashSet<string> tagsToDelete = new HashSet<string>();

	/* Start ES2BinaryWriter functionality */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public BinaryWriter writer;

	// The position of the integer we write to the stream to store the length of the stored data.
	// i.e. If we seek to this position and write an integer, it will overwrite the previous length.
	// If it equals zero, this means a position has not yet been written.
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	private long lengthPosition = 0;


	#region Write Methods (Without ES2Type)
	
	public void WriteRaw(byte[] param)
	{
		if(param != null && param.Length > 0)
			writer.BaseStream.Write(param, 0, param.Length);
	}
	
	public void Write<T>(T param)
	{
		Write(param, ES2TypeManager.GetES2Type(param.GetType()));
	}
	
	public void Write<T>(T[] param)
	{	
		Write(param, ES2TypeManager.GetES2Type(param.GetType().GetElementType()));
	}
	
	public void Write<T>(T[,] param)
	{	
		Write(param, ES2TypeManager.GetES2Type(param.GetType().GetElementType()));
	}
	
	public void Write<T>(T[,,] param)
	{	
		Write(param, ES2TypeManager.GetES2Type(param.GetType().GetElementType()));
	}
	
	public void Write<TKey, TValue>(Dictionary<TKey, TValue> param)
	{
		Write<TKey,TValue>(param, ES2TypeManager.GetES2Type(typeof(TKey)), ES2TypeManager.GetES2Type(typeof(TValue)));
	}
	
	public void Write<T>(List<T> param)
	{
		Write<T>(param, ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public void Write<T>(HashSet<T> param)
	{
		Write<T>(param, ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public void Write<T>(Queue<T> param)
	{
		Write<T>(param, ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public void Write<T>(Stack<T> param)
	{
		Write<T>(param, ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	#endregion
	
	#region Internal Write Methods (with ES2Type)

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(T param, ES2Type type)
	{
		if(settings.encrypt)
		{
			WriteEncrypted(param, type);
			return;
		}
		
		if(type == null)
		{
			Debug.LogError ("Easy Save does not support saving of type "+param.GetType().ToString()+".");
			return;
		}
			
		type.Write(param, this);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(T[] param, ES2Type type)
	{	
		if(settings.encrypt)
		{
			WriteEncrypted(param, type);
			return;
		}
		
		writer.Write((int)param.Length);

		// Write each object of array sequentially.
		foreach(System.Object obj in param)
		{
			Write(obj, type);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(T[,] param, ES2Type type)
	{	
		if(settings.encrypt)
		{
			WriteEncrypted(param, type);
			return;
		}
		
		// If multidimensional, write length of dimensions.
		if(param.Rank > 1)
		{
			// Write no of dimensions, followed by length of each dimension.
			writer.Write(param.Rank);
			for(int i=0; i<param.Rank; i++)
				writer.Write(param.GetUpperBound(i));
		}
		// Else, just write length.
		else
			writer.Write(param.Length);
		
		// Write each object of array sequentially.
		foreach(System.Object obj in param)
		{
			Write(obj, type);
		}
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(T[,,] param, ES2Type type)
	{	
		if(settings.encrypt)
		{
			WriteEncrypted(param, type);
			return;
		}
		
		// If multidimensional, write length of dimensions.
		if(param.Rank > 2)
		{
			// Write no of dimensions, followed by length of each dimension.
			writer.Write(param.Rank);
			for(int i=0; i<param.Rank; i++)
				writer.Write(param.GetUpperBound(i));
		}
		// Else, just write length.
		else
			writer.Write(param.Length);
		
		// Write each object of array sequentially.
		foreach(System.Object obj in param)
		{
			Write(obj, type);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<TKey, TValue>(Dictionary<TKey, TValue> param, ES2Type keyType, ES2Type valueType)
	{
		if(settings.encrypt)
		{
			WriteEncrypted(param, keyType, valueType);
			return;
		}
		
		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write(param.Count);
		foreach(KeyValuePair<TKey, TValue> obj in param)
		{
			Write(obj.Key, keyType);
			Write(obj.Value, valueType);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(List<T> param, ES2Type type)
	{
		if(settings.encrypt)
		{
			WriteEncrypted(param, type);
			return;
		}
		
		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write(param.Count);
		foreach(System.Object obj in param)
		{
			Write(obj, type);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(HashSet<T> param, ES2Type type)
	{
		if(settings.encrypt)
		{
			WriteEncrypted(param, type);
			return;
		}
		
		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write(param.Count);
		foreach(System.Object obj in param)
		{
			Write(obj, type);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(Queue<T> param, ES2Type type)
	{
		if(settings.encrypt)
		{
			WriteEncrypted(param, type);
			return;
		}
		
		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write(param.Count);
		foreach(System.Object obj in param)
		{
			Write(obj, type);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(Stack<T> param, ES2Type type)
	{
		if(settings.encrypt)
		{
			WriteEncrypted(param, type);
			return;
		}
		
		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write(param.Count);
		
		T[] stackAsArray = param.ToArray();
		for(int i=stackAsArray.Length-1;i>=0;i--)
		{
			Write(stackAsArray[i], type);
		}
	}

	/* 	Writes a System.Array object. Useful when using Reflection and need to cast arrays. */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void WriteSystemArray(System.Array param, ES2Type type)
	{	
		if(settings.encrypt)
		{
			WriteEncryptedSystemArray(param, type);
			return; 
		}
			
		writer.Write(param.Rank);
		for(int i=0; i<param.Rank; i++)
		{
			writer.Write(param.GetUpperBound(i));
		}

		// Write each object of array sequentially.
		foreach(System.Object obj in param)
			Write(obj, type);
	}

	/* 	Writes a List without generics. Useful when using Reflection and need to cast arrays. */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void WriteICollection(ICollection param, ES2Type type)
	{
		if(settings.encrypt)
		{
			WriteEncryptedICollection(param, type);
			return;
		}

		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write(param.Count);
		foreach(System.Object obj in param)
		{
			Write(obj, type);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void WriteIDictionary(IDictionary param, ES2Type keyType, ES2Type valueType)
	{
		if(settings.encrypt)
		{
			WriteEncryptedIDictionary(param, keyType, valueType);
			return;
		}

		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write((byte)0); // TODO : Remove this line when next changing the save format.
		writer.Write(param.Count);
		foreach(DictionaryEntry obj in param)
		{
			Write(obj.Key, keyType);
			Write(obj.Value, valueType);
		}
	}
	
	#endregion
	
	#region Internal Write Methods (Encrypted)
	
	private void WriteEncrypted<T>(T param, ES2Type type)
	{
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	private void WriteEncrypted<T>(T[] param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	private void WriteEncrypted<T>(T[,] param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	private void WriteEncrypted<T>(T[,,] param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	private void WriteEncrypted<TKey, TValue>(Dictionary<TKey, TValue> param, ES2Type keyType, ES2Type valueType)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write<TKey, TValue>(param, keyType, valueType);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	private void WriteEncrypted<T>(List<T> param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	private void WriteEncrypted<T>(HashSet<T> param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	private void WriteEncrypted<T>(Queue<T> param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	private void WriteEncrypted<T>(Stack<T> param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.Write(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	private void WriteEncryptedSystemArray(System.Array param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.WriteSystemArray(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	private void WriteEncryptedICollection(ICollection param, ES2Type type)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.WriteICollection(param, type);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	private void WriteEncryptedIDictionary(IDictionary param, ES2Type keyType, ES2Type valueType)
	{	
		using (ES2Writer encryptedWriter = CreateEncryptedWriter())
		{
			encryptedWriter.WriteIDictionary(param, keyType, valueType);
			byte[] encryptedBytes = encryptedWriter.GetEncryptedBytes(settings.encryptionPassword);
			writer.Write(encryptedBytes.Length);
			writer.Write(encryptedBytes);
		}
	}
	
	#endregion
	
	#region General Complexity
		
	/*
	 * Writes the header for the data we're saving, but the length is stored as zero.
	 * This allows us to seek back and write the length later.
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void WriteHeader(ES2Keys.Key collectionType, ES2Type valueType, ES2Type keyType)
	{
		WriteHeader(settings.filenameData.tag, collectionType, valueType, keyType);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void WriteHeader(string tag, ES2Keys.Key collectionType, ES2Type valueType, ES2Type keyType)
	{
		if(valueType == null) 
		{ 
			Debug.LogError ("Easy Save does not support saving of this type. For more information on supported types, and how to add support for your own types, please go to http://docs.moodkie.com/easy-save-2/supported-types/");  
			return;
		}
		
		writer.Write((byte)ES2Keys.Key._Tag);
		writer.Write(tag);
		lengthPosition = writer.BaseStream.Position;
		writer.Write((int)0); // Write blank length so we can store it later.
		if(settings.encrypt)
		{
			if(settings.encryptionType == ES2Settings.EncryptionType.AES128)
				writer.Write((byte)ES2Keys.Key._Encrypt);
			else
				writer.Write((byte)ES2Keys.Key._Obfuscate);
		}
		
		// Write type data.
		if(collectionType != ES2Keys.Key._Null)
			writer.Write((byte)collectionType);
		writer.Write((byte)255); // We write the 255 byte to say that this uses a hash integer to identify types rather than an ES2Key.
		writer.Write(valueType.hash);
		if(keyType != null)
			writer.Write(keyType.hash);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void WriteLength()
	{
		// Get original position so we can seek back to where we need to be.
		long originalPosition = writer.BaseStream.Position;
		// Seek to the position of the stream where our blank length integer is.
		writer.BaseStream.Position = lengthPosition;
		// Write our calculated length. We minus the byte count of the length integer itself (i.e. 4).
		writer.Write((int)(originalPosition-lengthPosition-4));
		// Seek back to the original position.
		writer.BaseStream.Position = originalPosition;
		// Reset the lengthPosition.
		lengthPosition = 0;
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void WriteTerminator()
	{
		writer.Write((byte)ES2Keys.Key._Terminator);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2Writer CreateEncryptedWriter()
	{
		ES2Settings encryptedSettings = new ES2Settings();
		encryptedSettings.saveLocation = ES2Settings.SaveLocation.Memory;
		// Make sure encrypt=false so we don't enter an infinite loop/Stackoverflow situation.
		encryptedSettings.encrypt = false;
		// But make sure we get it to use the correct encryption type the one time it does encrypt.
		encryptedSettings.encryptionType = settings.encryptionType;
		return new ES2Writer(encryptedSettings);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public byte[] GetEncryptedBytes(string password)
	{
		if(settings.encryptionType == ES2Settings.EncryptionType.AES128) // AES 128-bit encryption
		{
			AESEncryptor aesEncryptor = new AESEncryptor(password, MoodkieSecurity.AESBits.BITS128);
			return aesEncryptor.Encrypt((writer.BaseStream as MemoryStream).ToArray());
		}
		else // XOR Obfuscation
			return Obfuscator.Obfuscate((writer.BaseStream as MemoryStream).ToArray(), password);
	}
	
	public void Dispose()
	{
		ES2Dispose.Dispose(writer);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool Rename(string newTag)
	{
		using (ES2Reader reader = ES2Reader.Create(settings))
		{
			return reader.RenameTag(settings.tag, newTag, this);
		}
	}
	#endregion

	/* END ES2BinaryWriter functionality */

	#region User-Exposed Write Methods (With Tag)
	
	// These are user-exposed methods.
	
	public void Write<T>(T param, string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(param.GetType());
		WriteHeader(tag, ES2Keys.Key._Null, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}

	public void Write<T>(T[] param, string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(param.GetType().GetElementType());
		if(type == null)
			Debug.LogError(typeof(T).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");

		WriteHeader(tag, ES2Keys.Key._NativeArray, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}

	public void Write<T>(T[,] param, string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(param.GetType().GetElementType());
		if(type == null)
			Debug.LogError(typeof(T).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");
		WriteHeader(tag, ES2Keys.Key._NativeArray, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	public void Write<T>(T[,,] param, string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(param.GetType().GetElementType());
		if(type == null)
			Debug.LogError(typeof(T).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");
		WriteHeader(tag, ES2Keys.Key._NativeArray, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	public void Write<TKey, TValue>(Dictionary<TKey,TValue> param, string tag)
	{
		ES2Type keyType = ES2TypeManager.GetES2Type(typeof(TKey));
		ES2Type valueType = ES2TypeManager.GetES2Type(typeof(TValue));
		if(keyType == null)
			Debug.LogError(typeof(TKey).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");
		if(valueType == null)
			Debug.LogError(typeof(TValue).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");
		WriteHeader(tag, ES2Keys.Key._Dictionary, valueType, keyType);
		Write<TKey,TValue>(param, keyType, valueType);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	public void Write<T>(List<T> param, string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		if(type == null)
			Debug.LogError(typeof(T).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");
		WriteHeader(tag, ES2Keys.Key._List, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	public void Write<T>(HashSet<T> param, string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		if(type == null)
			Debug.LogError(typeof(T).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");
		WriteHeader(tag, ES2Keys.Key._HashSet, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	public void Write<T>(Queue<T> param, string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		if(type == null)
			Debug.LogError(typeof(T).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");
		WriteHeader(tag, ES2Keys.Key._Queue, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	public void Write<T>(Stack<T> param, string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		if(type == null)
			Debug.LogError(typeof(T).ToString()+" is not currently supported or it's ES2Type was not found, but you may be able to add support for it through Assets > Easy Save 2 > Manage Types");
		WriteHeader(tag, ES2Keys.Key._Stack, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	#endregion
	
	#region Write Methods (With Tag and ES2Type)
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(T param, string tag, ES2Type type)
	{
		WriteHeader(tag, ES2Keys.Key._Null, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(T[] param, string tag, ES2Type type)
	{
		WriteHeader(tag, ES2Keys.Key._NativeArray, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(T[,] param, string tag, ES2Type type)
	{
		WriteHeader(tag, ES2Keys.Key._NativeArray, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(T[,,] param, string tag, ES2Type type)
	{
		WriteHeader(tag, ES2Keys.Key._NativeArray, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<TKey, TValue>(Dictionary<TKey,TValue> param, string tag, ES2Type keyType, ES2Type valueType)
	{
		WriteHeader(tag, ES2Keys.Key._Dictionary, valueType, keyType);
		Write<TKey,TValue>(param, keyType, valueType);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(List<T> param, string tag, ES2Type type)
	{
		WriteHeader(tag, ES2Keys.Key._List, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(HashSet<T> param, string tag, ES2Type type)
	{
		WriteHeader(tag, ES2Keys.Key._HashSet, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(Queue<T> param, string tag, ES2Type type)
	{
		WriteHeader(tag, ES2Keys.Key._Queue, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Write<T>(Stack<T> param, string tag, ES2Type type)
	{
		WriteHeader(tag, ES2Keys.Key._Stack, type, null);
		Write<T>(param, type);
		WriteTerminator();
		WriteLength();
		tagsToDelete.Add(tag);
	}
	#endregion

	#region Write Property Methods

	public void WriteProperty<T>(T property)
	{
		int startPosition = WritePropertyPrefix();
		Write(property);
		WritePropertyLength(startPosition);
	}
	
	public void WriteProperty<T>(T[] property)
	{
		int startPosition = WritePropertyPrefix();
		Write(property);
		WritePropertyLength(startPosition);
	}
	
	public void WriteProperty<T>(T[,] property)
	{
		int startPosition = WritePropertyPrefix();
		Write(property);
		WritePropertyLength(startPosition);
	}
	
	public void WriteProperty<T>(T[,,] property)
	{
		int startPosition = WritePropertyPrefix();
		Write(property);
		WritePropertyLength(startPosition);
	}
	
	public void WriteProperty<T>(List<T> property)
	{
		int startPosition = WritePropertyPrefix();
		Write(property);
		WritePropertyLength(startPosition);
	}
	
	public void WriteProperty<T>(Queue<T> property)
	{
		int startPosition = WritePropertyPrefix();
		Write(property);
		WritePropertyLength(startPosition);
	}
	
	public void WriteProperty<T>(Stack<T> property)
	{
		int startPosition = WritePropertyPrefix();
		Write(property);
		WritePropertyLength(startPosition);
	}
	
	public void WriteProperty<TKey, TValue>(Dictionary<TKey,TValue> property)
	{
		int startPosition = WritePropertyPrefix();
		Write(property);
		WritePropertyLength(startPosition);
	}

	/*
	 * 	Writes data to be prefixed to a property.
	 * 	Returns the start position of the property.
	 */
	public int WritePropertyPrefix()
	{
		int startPosition = (int)stream.Position;
		// Write blank length, to be written to later.
		writer.Write((int)1);
		return startPosition;
	}

	/*
	 * 	Seeks before a property and writes it's length, then returns to original position.
	 * 	Must only be called immediately after writing a property.
	 */
	public void WritePropertyLength(int startPosition)
	{
		int endPosition = (int)stream.Position;
		int length = endPosition - startPosition;
		stream.Position = startPosition;
		writer.Write((int)length);
		stream.Position = endPosition;
	}

	#endregion

	#region General Complexity

	public void Save()
	{
		Save(true);
	}
	
	public void Save(bool checkForOverwrite)
	{
		if(checkForOverwrite)
		{
			if(stream.MayRequireOverwrite())
				Delete();
		}
		stream.Store();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Save(byte[] bytes)
	{
		Delete(bytes);
		stream.Store();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool Rename(string oldTag, string newTag)
	{
		using(ES2Reader reader = ES2Reader.Create(settings))
		{
			return reader.RenameTag(oldTag, newTag, this);
		}
	}

	/* 
	 * Marks a tag for deletion. 
	 * This is different to the Delete() method which is used to delete the tags.
	 */
	public void Delete(string tag)
	{
		// Marks a tag for deletion.
		tagsToDelete.Add(tag);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool Delete()
	{
		using(ES2Reader reader = ES2Reader.Create(settings))
		{
			return reader.DeleteTags(tagsToDelete, this);
		}
	}

	/*
	 * 	Reads tags from the bytes provided, and writes them to the Writer if they're not marked for deletion.
	 */
	private bool Delete(byte[] bytes)
	{
		using(ES2Reader reader = ES2Reader.Create(bytes, settings))
		{
			return reader.DeleteTags(tagsToDelete, this);
		}
	}

	public static ES2Writer Create(string identifier)
	{
		return Create(new ES2Settings(identifier));
	}

	public static ES2Writer Create(string identifier, ES2Settings settings)
	{
		return Create(settings.Clone(identifier));
	}

	public static ES2Writer Create(ES2Settings settings)
	{
		return new ES2Writer(settings);
	}

	public ES2Writer(ES2Settings settings)
	{
		this.settings = settings;
		stream = ES2Stream.Create(settings, ES2Stream.Operation.Write);
		writer = new BinaryWriter(stream.stream);
	}
	#endregion
}
