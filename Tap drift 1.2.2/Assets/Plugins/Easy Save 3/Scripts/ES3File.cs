﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using ES3Types;
using UnityEngine;
using ES3Internal;

/// <summary>Represents a cached file which can be saved to and loaded from, and commited to storage when necessary.</summary>
public class ES3File
{
	public ES3Settings settings;
	private Dictionary<string, ES3Data> cache = new Dictionary<string, ES3Data>();
	private bool syncWithFile = false;
	
	/// <summary>Creates a new ES3File and loads the default file into the ES3File if there is data to load.</summary>
	public ES3File() : this(new ES3Settings(), true){}
	
	/// <summary>Creates a new ES3File and loads the specified file into the ES3File if there is data to load.</summary>
	/// <param name="filepath">The relative or absolute path of the file in storage our ES3File is associated with.</param>
	public ES3File(string filePath) : this(new ES3Settings(filePath), true){}
	
	/// <summary>Creates a new ES3File and loads the specified file into the ES3File if there is data to load.</summary>
	/// <param name="filepath">The relative or absolute path of the file in storage our ES3File is associated with.</param>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	public ES3File(string filePath, ES3Settings settings) : this(new ES3Settings(filePath, settings), true){}
	
	/// <summary>Creates a new ES3File and loads the specified file into the ES3File if there is data to load.</summary>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	public ES3File(ES3Settings settings) : this(settings, true){}
	
	/// <summary>Creates a new ES3File and only loads the default file into it if syncWithFile is set to true.</summary>
	/// <param name="syncWithFile">Whether we should sync this ES3File with the one in storage immediately after creating it.</param>
	public ES3File(bool syncWithFile) : this(new ES3Settings(), syncWithFile){}
	/// <summary>Creates a new ES3File and only loads the specified file into it if syncWithFile is set to true.</summary>
	/// <param name="filepath">The relative or absolute path of the file in storage our ES3File is associated with.</param>
	/// <param name="syncWithFile">Whether we should sync this ES3File with the one in storage immediately after creating it.</param>
	public ES3File(string filePath, bool syncWithFile) : this(new ES3Settings(filePath), syncWithFile){}
	/// <summary>Creates a new ES3File and only loads the specified file into it if syncWithFile is set to true.</summary>
	/// <param name="filepath">The relative or absolute path of the file in storage our ES3File is associated with.</param>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	/// <param name="syncWithFile">Whether we should sync this ES3File with the one in storage immediately after creating it.</param>
	public ES3File(string filePath, ES3Settings settings, bool syncWithFile) : this(new ES3Settings(filePath, settings), syncWithFile){}
	
	/// <summary>Creates a new ES3File and loads the specified file into the ES3File if there is data to load.</summary>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	/// <param name="syncWithFile">Whether we should sync this ES3File with the one in storage immediately after creating it.</param>
	public ES3File(ES3Settings settings, bool syncWithFile)
	{
		this.settings = settings;
		this.syncWithFile = syncWithFile;
		if(syncWithFile)
		{
			// Type checking must be enabled when syncing.
			var settingsWithTypeChecking = (ES3Settings)settings.Clone();
			settingsWithTypeChecking.typeChecking = true;
			
			using(var reader = ES3Reader.Create(settingsWithTypeChecking))
			{
				if(reader == null)
					return;
				foreach (KeyValuePair<string,ES3Data> kvp in reader.RawEnumerator)
					cache [kvp.Key] = kvp.Value;
			}
		}
	}
		
	/// <summary>Creates a new ES3File and loads the specified file into the ES3File if there is data to load.</summary>
	/// <param name="bytes">The bytes representing our file.</param>
	public ES3File(byte[] bytes) : this(bytes, new ES3Settings()){}

	/// <summary>Creates a new ES3File and loads the bytes into the ES3File. Note the bytes must represent that of a file.</summary>
	/// <param name="bytes">The bytes representing our file.</param>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	/// <param name="syncWithFile">Whether we should sync this ES3File with the one in storage immediately after creating it.</param>
	public ES3File(byte[] bytes, ES3Settings settings)
	{
		this.settings = settings;
		SaveRaw(bytes, settings);
	}
	
	/// <summary>Synchronises this ES3File with a file in storage.</summary>
	public void Sync()
	{
		Sync(this.settings);
	}
	
	/// <summary>Synchronises this ES3File with a file in storage.</summary>
	/// <param name="filepath">The relative or absolute path of the file in storage we want to synchronise with.</param>
	public void Sync(string filePath)
	{
		Sync(new ES3Settings(filePath));
	}
	
	/// <summary>Synchronises this ES3File with a file in storage.</summary>
	/// <param name="filepath">The relative or absolute path of the file in storage we want to synchronise with.</param>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	public void Sync(string filePath, ES3Settings settings)
	{
		Sync(new ES3Settings(filePath, settings));
	}
	
	/// <summary>Synchronises this ES3File with a file in storage.</summary>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	public void Sync(ES3Settings settings)
	{
		if(cache.Count == 0)
			return;
		
		using(var baseWriter = ES3Writer.Create(settings, true, !syncWithFile, false))
		{
			foreach (var kvp in cache) 
			{
				// If we change the name of a type, the type may be null.
				// In this case, use System.Object as the type.
				Type type;
				if (kvp.Value.type == null)
					type = typeof(System.Object);
				else
					type = kvp.Value.type.type;
				baseWriter.Write (kvp.Key, type, kvp.Value.bytes);
			}
			baseWriter.Save(!syncWithFile);
		}
	}
	
	/// <summary>Removes the data stored in this ES3File. The ES3File will be empty after calling this method.</summary>
	public void Clear()
	{
		cache.Clear();
	}
	
	/// <summary>Returns an array of all of the key names in this ES3File.</summary>
	public string[] GetKeys()
	{
		var keyCollection = cache.Keys;
		var keys = new string[keyCollection.Count];
		keyCollection.CopyTo(keys, 0);
		return keys;
	}
	
	#region Save Methods
	
	/// <summary>Saves a value to a key in this ES3File.</summary>
	/// <param name="key">The key we want to use to identify our value in the file.</param>
	/// <param name="value">The value we want to save.</param>
	public void Save<T>(string key, object value)
	{
		using(var stream = new MemoryStream(settings.bufferSize))
		{
			var unencryptedSettings = (ES3Settings)settings.Clone();
			unencryptedSettings.encryptionType = ES3.EncryptionType.None;
			var es3Type = ES3TypeMgr.GetOrCreateES3Type(typeof(T));
			
			using (var baseWriter = ES3Writer.Create(stream, unencryptedSettings, false, false))
				baseWriter.Write(value, es3Type, settings.referenceMode);
			
			cache[key] = new ES3Data(es3Type, stream.ToArray());
		}
	}

	/// <summary>Merges the data specified by the bytes parameter into this ES3File.</summary>
	/// <param name="bytes">The bytes we want to merge with this ES3File.</param>
	public void SaveRaw(byte[] bytes)
	{
		SaveRaw(bytes, settings);
	}

	/// <summary>Merges the data specified by the bytes parameter into this ES3File.</summary>
	/// <param name="bytes">The bytes we want to merge with this ES3File.</param>
	/// <param name="settings">The settings we want to use to override the default settings.</param>
	public void SaveRaw(byte[] bytes, ES3Settings settings)
	{
		// Type checking must be enabled when syncing.
		var settingsWithTypeChecking = (ES3Settings)settings.Clone();
		settingsWithTypeChecking.typeChecking = true;

		using(var reader = ES3Reader.Create(bytes, settingsWithTypeChecking))
		{
			if(reader == null)
				return;
			foreach (KeyValuePair<string,ES3Data> kvp in reader.RawEnumerator)
				cache [kvp.Key] = kvp.Value;
		}
	}
	
	#endregion
	
	#region Load Methods
	
	/* Standard load methods */
	
	/// <summary>Loads the value from this ES3File with the given key.</summary>
	/// <param name="key">The key which identifies the value we want to load.</param>
	public T Load<T>(string key)
	{
		ES3Data es3Data;
		
		if(!cache.TryGetValue(key, out es3Data))
			throw new KeyNotFoundException("Key \"" + key + "\" was not found in this ES3File. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");
		
		var settings = (ES3Settings)this.settings.Clone();
		settings.encryptionType = ES3.EncryptionType.None;
		
		using(var ms = new System.IO.MemoryStream(es3Data.bytes, false))
			using(var reader = ES3Reader.Create(ms, settings, false))
				return reader.Read<T>(ES3TypeMgr.GetOrCreateES3Type(typeof(T)));
	}
	
	/// <summary>Loads the value from this ES3File with the given key.</summary>
	/// <param name="key">The key which identifies the value we want to load.</param>
	/// <param name="defaultValue">The value we want to return if the key does not exist in this ES3File.</param>
	public T Load<T>(string key, T defaultValue)
	{
		ES3Data es3Data;
		
		if(!cache.TryGetValue(key, out es3Data))
			return defaultValue;
		var settings = (ES3Settings)this.settings.Clone();
		settings.encryptionType = ES3.EncryptionType.None;
		
		using(var ms = new System.IO.MemoryStream(es3Data.bytes, false))
			using(var reader = ES3Reader.Create(ms, settings, false))
				return reader.Read<T>(ES3TypeMgr.GetOrCreateES3Type(typeof(T)));
	}
	
	/// <summary>Loads the value from this ES3File with the given key into an existing object.</summary>
	/// <param name="key">The key which identifies the value we want to load.</param>
	/// <param name="obj">The object we want to load the value into.</param>
	public void LoadInto<T>(string key, T obj) where T : class
	{
		ES3Data es3Data;
		
		if(!cache.TryGetValue(key, out es3Data))
			throw new KeyNotFoundException("Key \"" + key + "\" was not found in this ES3File. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");
		
		var settings = (ES3Settings)this.settings.Clone();
		settings.encryptionType = ES3.EncryptionType.None;
		
		using(var ms = new System.IO.MemoryStream(es3Data.bytes, false))
			using(var reader = ES3Reader.Create(ms, settings, false))
				reader.ReadInto<T>(obj, ES3TypeMgr.GetOrCreateES3Type(typeof(T)));
	}
	
	#endregion
	
	#region Load Raw Methods
	
	/// <summary>Loads the ES3File as a raw byte array.</summary>
	public byte[] LoadRawBytes()
	{
		if(cache.Count == 0)
			return new byte[0];

		using (var ms = new System.IO.MemoryStream ())
		{
			var memorySettings = (ES3Settings)settings.Clone();
			memorySettings.location = ES3.Location.Memory;
			using (var baseWriter = ES3Writer.Create(ES3Stream.CreateStream(ms, memorySettings, ES3FileMode.Write), memorySettings, true, false))
			{
				foreach (var kvp in cache)
					baseWriter.Write(kvp.Key, kvp.Value.type.type, kvp.Value.bytes);
				baseWriter.Save(false);
			}

			return ms.ToArray();
		}
	}
	
	/// <summary>Loads the ES3File as a string, using the encoding defined in the ES3File's settings variable.</summary>
	public string LoadRawString()
	{
		if(cache.Count == 0)
			return "";
		return settings.encoding.GetString(LoadRawBytes());
	}
	
	#endregion
	
	#region Other ES3 Methods

	/// <summary>Deletes a key from this ES3File.</summary>
	/// <param name="key">The key we want to delete.</param>
	public void DeleteKey(string key)
	{
		cache.Remove(key);
	}
	
	/// <summary>Checks whether a key exists in this ES3File.</summary>
	/// <param name="key">The key we want to check the existence of.</param>
	/// <returns>True if the key exists, otherwise False.</returns>
	public bool KeyExists(string key)
	{
		return cache.ContainsKey(key);
	}
	
	/// <summary>Gets the size of the cached data in bytes.</summary>
	public int Size()
	{
		int size = 0;
		foreach(var kvp in cache)
			size += kvp.Value.bytes.Length;
		return size;
	}

	public Type GetKeyType(string key)
	{
		ES3Data es3data;
		if(!cache.TryGetValue(key, out es3data))
			throw new KeyNotFoundException("Key \"" + key + "\" was not found in this ES3File. Use Load<T>(key, defaultValue) if you want to return a default value if the key does not exist.");
		
		return es3data.type.type;
	}
	
	#endregion
}

namespace ES3Internal
{
	public struct ES3Data
	{
		public ES3Type type;
		public byte[] bytes;
		
		public ES3Data(Type type, byte[] bytes)
		{
			this.type = type == null ? null : ES3TypeMgr.GetOrCreateES3Type(type);
			this.bytes = bytes;
		}
		
		public ES3Data(ES3Type type, byte[] bytes)
		{
			this.type = type;
			this.bytes = bytes;
		}
	}
}
