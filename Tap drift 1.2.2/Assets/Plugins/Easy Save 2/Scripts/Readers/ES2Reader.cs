using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MoodkieSecurity;

public partial class ES2Reader : System.IDisposable
{
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2Stream stream;
	public ES2Settings settings;

	/* START ES2Reader functionality */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	ES2CachedFile cachedFile = null;
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2Tag currentTag = new ES2Tag("",0,0,0);

	public BinaryReader reader;

	#region Internal Read Methods (with ES2Type)
	
	public T Read<T>(ES2Type es2type)
	{
		if(settings.encrypt)
			return ReadEncrypted<T>(es2type);

		if(es2type != null)
			return (T)es2type.Read(this);
		else
			Debug.LogError ("Easy Save does not support loading of type "+typeof(T).ToString()+", or if you are trying to load a collection such as a Dictionary or Array, use the collection methods (for example, LoadDictionary<>() or LoadArray<>()).");
		
		return default(T);
	}
	
	public void Read<T>(ES2Type es2type, T c) where T : class
	{
		if(settings.encrypt)
		{
			ReadEncrypted<T>(es2type, c);
			return;
		}
		
		if(es2type != null)
			es2type.Read(this, c);
		else
			Debug.LogError("Component of type "+typeof(T).ToString()+"is not supported by Easy Save. If you would like to request that we support this component, please contact us on www.moodkie.com/contact.");
	}
	
	public T[] ReadArray<T>(ES2Type type)
	{
		if(settings.encrypt)
			return ReadEncryptedArray<T>(type);

		int count = reader.ReadInt32();

		T[] result = new T[count];
		
		for(int i=0;i<count;i++)
		{
			result[i] = Read<T>(type);
		}
		return result;
	}
	
	public void ReadArray<T>(ES2Type type, T[] c) where T : class
	{
		if(settings.encrypt)
		{
			ReadEncryptedArray<T>(type, c);
			return;
		}
		
		int count = reader.ReadInt32();
		
		for(int i=0;i<count;i++)
			Read<T>(type, c[i]);
	}
	
	public T[,] Read2DArray<T>(ES2Type type)
	{
		if(settings.encrypt)
			return ReadEncrypted2DArray<T>(type);
		
		int dimensionCount = reader.ReadInt32();
		
		// If array we are loading does not have 2 dimensions, throw error.
		if(dimensionCount != 2)
			Debug.LogError("Easy Save 2 Error: The array you are loading with ES2.Load2DArray must have 2 dimensions, not "+dimensionCount+" dimensions.");
		
		int[] dimensions = new int[]{reader.ReadInt32()+1, reader.ReadInt32()+1};
		
		T[,] result = new T[dimensions[0],dimensions[1]];
		
		for(int i=0; i<dimensions[0]; i++)
		{
			for(int j=0; j<dimensions[1]; j++)
			{
				result[i, j] = (T)Read<T>(type);
			}
		}
		return result;
	}
	
	public T[,,] Read3DArray<T>(ES2Type type)
	{
		if(settings.encrypt)
			return ReadEncrypted3DArray<T>(type);
		
		int dimensionCount = reader.ReadInt32();
		
		// If array we are loading does not have 2 dimensions, throw error.
		if(dimensionCount != 3)
			Debug.LogError("Easy Save 2 Error: The array you are loading with ES2.Load3DArray must have 3 dimensions, not "+dimensionCount+" dimensions.");
		
		int[] dimensions = new int[]{reader.ReadInt32()+1, reader.ReadInt32()+1, reader.ReadInt32()+1};
		
		T[,,] result = new T[dimensions[0],dimensions[1], dimensions[2]];
		
		for(int i=0; i<dimensions[0]; i++)
		{
			for(int j=0; j<dimensions[1]; j++)
			{
				for(int k=0; k<dimensions[2]; k++)
				{
					result[i, j, k] = (T)Read<T>(type);
				}
			}
		}
		return result;
	}
	
	public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(ES2Type keyType, ES2Type valueType)
	{
		if(settings.encrypt)
			return ReadEncryptedDictionary<TKey, TValue>(keyType, valueType);
		Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
		
		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.
		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.

		int count = reader.ReadInt32();
		
		for(int i=0;i<count;i++)
		{
			result.Add((TKey)Read<TKey>(keyType), (TValue)Read<TValue>(valueType));
		}
		return result;
	}
	
	public List<T> ReadList<T>(ES2Type type)
	{
		if(settings.encrypt)
			return ReadEncryptedList<T>(type);
		
		List<T> result = new List<T>();
		
		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.
		int count = reader.ReadInt32();

		for(int i=0;i<count;i++)
		{
			result.Add((T)Read<T>(type));
		}
		return result;
	}
	
	public void ReadList<T>(ES2Type type, List<T> c) where T : class
	{
		if(settings.encrypt)
		{
			ReadEncryptedList<T>(type, c);
			return;
		}

		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.
		int count = reader.ReadInt32();
		
		for(int i=0;i<count;i++)
			Read<T>(type, c[i]);
	}
	
	public HashSet<T> ReadHashSet<T>(ES2Type type)
	{	
		if(settings.encrypt)
			return ReadEncryptedHashSet<T>(type);
		
		HashSet<T> result = new HashSet<T>();
		
		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.
		int count = reader.ReadInt32();
		
		for(int i=0;i<count;i++)
		{
			result.Add((T)Read<T>(type));
		}
		return result;
	}
	
	public Queue<T> ReadQueue<T>(ES2Type type)
	{
		if(settings.encrypt)
			return ReadEncryptedQueue<T>(type);
		
		Queue<T> result = new Queue<T>();
		
		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.
		int count = reader.ReadInt32();
		
		for(int i=0;i<count;i++)
		{
			result.Enqueue((T)Read<T>(type));
		}
		return result;
	}
	
	public Stack<T> ReadStack<T>(ES2Type type)
	{
		if(settings.encrypt)
			return ReadEncryptedStack<T>(type);
		
		Stack<T> result = new Stack<T>();
		
		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.
		int count = reader.ReadInt32();
		
		for(int i=0;i<count;i++)
		{
			result.Push((T)Read<T>(type));
		}
		return result;
	}

	public System.Array ReadSystemArray(ES2Type type)
	{
		if(settings.encrypt)
			return ReadEncryptedSystemArray(type);

		int rank = reader.ReadInt32();
		var lengths = new int[rank];
		for(int i=0; i<rank; i++)
		{
			lengths[i] = reader.ReadInt32()+1;
		}

		System.Array result = System.Array.CreateInstance(type.type, lengths);

		for(int i=0; i<result.Length; i++)
			result.SetValue(Read<System.Object>(type), GetMultidimensionalIndices(result, i));
		return result;
	}

	/*
	 * 	Reads a generic ICollection.
	 * 	CollectionType must be a generic type definition (i.e. typeof(List<>) );
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ICollection ReadICollection(System.Type collectionType, ES2Type type)
	{
		if(settings.encrypt)
			return ReadEncryptedICollection(collectionType, type);

		var result = (IList)ES2Reflection.CreateGenericInstance(collectionType, type.type);

		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.
		int count = reader.ReadInt32();

		for(int i=0;i<count;i++)
		{
			result.Add(Read<object>(type));
		}
		return result;
	}

	public IDictionary ReadIDictionary(System.Type dictionaryType, ES2Type keyType, ES2Type valueType)
	{
		if(settings.encrypt)
			return ReadEncryptedIDictionary(dictionaryType, keyType, valueType);
		var result = (IDictionary)ES2Reflection.CreateGenericInstance(dictionaryType, keyType.type, valueType.type);

		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.
		reader.ReadByte(); // TODO: Remove this line on the next change to ES2 format.

		int count = reader.ReadInt32();

		for(int i=0;i<count;i++)
		{
			result.Add(Read<System.Object>(keyType), Read<System.Object>(valueType));
		}
		return result;
	}
	
	#endregion

	#region Read Methods (Encrypted)
	
	
	private T ReadEncrypted<T>(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return (T)encryptedReader.Read<T>(type);
	}
	
	
	private void ReadEncrypted<T>(ES2Type type, T c) where T : class
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			encryptedReader.Read<T>(type, c);
	}
	
	
	private T[] ReadEncryptedArray<T>(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadArray<T>(type);
	}
	
	
	private void ReadEncryptedArray<T>(ES2Type type, T[] c) where T : class
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			encryptedReader.ReadArray<T>(type, c);
	}
	
	
	private T[,] ReadEncrypted2DArray<T>(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.Read2DArray<T>(type);
	}
	
	private T[,,] ReadEncrypted3DArray<T>(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.Read3DArray<T>(type);
	}
	
	private Dictionary<TKey, TValue> ReadEncryptedDictionary<TKey, TValue>(ES2Type keyType, ES2Type valueType)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadDictionary<TKey, TValue>(keyType, valueType);
	}
	
	
	private List<T> ReadEncryptedList<T>(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadList<T>(type);
	}
	
	private void ReadEncryptedList<T>(ES2Type type, List<T> c) where T : class
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			encryptedReader.ReadList<T>(type, c);
	}
	
	
	private HashSet<T> ReadEncryptedHashSet<T>(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadHashSet<T>(type);
	}
	
	
	private Queue<T> ReadEncryptedQueue<T>(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadQueue<T>(type);
	}
	
	
	private Stack<T> ReadEncryptedStack<T>(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadStack<T>(type);
	}

	private System.Array ReadEncryptedSystemArray(ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadSystemArray(type);
	}

	private ICollection ReadEncryptedICollection(System.Type collectionType, ES2Type type)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadICollection(collectionType, type);
	}

	private IDictionary ReadEncryptedIDictionary(System.Type dictionaryType, ES2Type keyType, ES2Type valueType)
	{
		using(ES2Reader encryptedReader = GetEncryptedReader())
			return encryptedReader.ReadIDictionary(dictionaryType, keyType, valueType);
	}

	#endregion

	#region Header Complexity
	
	protected void ProcessHeader(ES2Keys.Key expectedCollectionType, ES2Type expectedValue, ES2Type expectedKey, string tag)
	{
		if(ScanToTag(tag) == false)
			Debug.LogError("Easy Save 2 Error: The data, tag, file or folder you are looking for does not exist. Please ensure that ES2.Exists(string identifier) returns true before calling this method.");

		ES2Header header = ReadHeader();
		
		
		/* Check that we're loading the correct types */
		if(expectedValue == null)
			Debug.LogError ("This type is not supported by Easy Save, but you may be able to add support by going to 'Manage Types' in the 'Assets/Easy Save 2' menu.");
			
		if(expectedCollectionType != header.collectionType)
		{
			// If the user is trying to load a non-collection with a collection method ...
			if(expectedCollectionType == ES2Keys.Key._Null)
				Debug.LogError("Easy Save 2 Error: The data you are trying to load is a Collection. Please use the Load method for that type of collection (for example, ES2.LoadArray or ES2.LoadDictionary)");
			else
				Debug.LogError("Easy Save 2 Error: The data you are trying to load is not a Collection, but you are using a Collection method to use it. Use ES2.Load instead.");
		}
		
		if(expectedKey != null)
			if(header.keyType != expectedKey.hash)
				Debug.LogError("Easy Save 2 Error: The type of key in the Dictionary you are loading does not match the key you are trying to load with.");
		
		if(expectedValue != null)
			if(header.valueType != expectedValue.hash)
				Debug.LogError("Easy Save 2 Error: The data you are trying to load does not match the Load method you are using to load it. ");

		if(header.settings.encrypt)
			this.settings.encrypt = true;
	}

	protected void ProcessHeader(ES2Keys.Key expectedCollectionType, ES2Type expectedValue, ES2Type expectedKey)
	{
		ProcessHeader(expectedCollectionType, expectedValue, expectedKey, settings.filenameData.tag);
	}

	/*
	 * 	Gets the header as an ES2Header struct.
	 * 	The stream must point at the position after the tag for this to work.
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2Header ReadHeader()
	{
		ES2Keys.Key collectionType = ES2Keys.Key._Null;
		int keyType = 0;
		int valueType = 0;
		ES2Settings settings = new ES2Settings();

		while(true)
		{
			byte currentByte = reader.ReadByte();
			
			if(currentByte == (byte)ES2Keys.Key._Encrypt) // If encrypt, set encrypted in settings.
			{
				settings.encrypt = true;
				settings.encryptionType = ES2Settings.EncryptionType.AES128;
			}
			else if(currentByte == (byte)ES2Keys.Key._Obfuscate) // If encrypt, set encrypted in settings.
			{
				settings.encrypt = true;
				settings.encryptionType = ES2Settings.EncryptionType.Obfuscate;
			}
			else if(currentByte == (byte)ES2Keys.Key._Terminator)
			{
				continue;
			}
			else if((int)currentByte == 255) // We're loading data after v2.54 where types are identified by integer hashes.
			{
				if(collectionType == ES2Keys.Key._Dictionary) // keyType comes second if we're loading a Dictionary.
					keyType = reader.ReadInt32();
				else
					valueType = reader.ReadInt32();
				return new ES2Header(collectionType, keyType, valueType, settings);
			}
			else if((int)currentByte < 81) // LEGACY SAVE DATA before v2.54: If data type, set data type in settings. End of header.
			{
				if(collectionType == ES2Keys.Key._Dictionary) // keyType comes second if we're loading a Dictionary.
					keyType = ES2TypeManager.GetES2Type(currentByte).hash;
				else
					valueType = ES2TypeManager.GetES2Type(currentByte).hash;
				return new ES2Header(collectionType, keyType, valueType, settings);
			}
			else if((int)currentByte < 101) //Array Type
			{
				collectionType = (ES2Keys.Key)currentByte;

				if(currentByte == (byte)ES2Keys.Key._Dictionary) //If it's a Dictionary, the value byte comes first.
				{
					byte valueByte = reader.ReadByte();
					
					if((int)valueByte == 255) // Handle value bytes after v2.54.
					{
						valueType = reader.ReadInt32();
						keyType = reader.ReadInt32();
						return new ES2Header(collectionType, keyType, valueType, settings);
					}
					else // Handle value bytes before v2.54.
						valueType = ES2TypeManager.GetES2Type(valueByte).hash;
				}
			}
			else
				throw new ES2InvalidDataException();
		}
	}
	
	#endregion
	
	#region Seeking Complexity
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool Next()
	{	
		if(currentTag.nextTagPosition >= stream.Length)
			return false;
		
		stream.Position = currentTag.nextTagPosition;
		
		// Current position = Before tag, on terminator.
		currentTag.position = stream.Position;
		
		byte currentByte = reader.ReadByte();
		
		if(currentByte == (byte)0)
			return false;

		if(currentByte != (byte)ES2Keys.Key._Tag)
			throw new ES2InvalidDataException();
		
		currentTag.tag = reader.ReadString();
		currentTag.nextTagPosition = reader.ReadInt32()+stream.Position;
		currentTag.settingsPosition = stream.Position;

		return true;
	}

	/*
	 *	Skips to the beginning of the next tag.
	 *	This can be called regardless of where you are in the stream.
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Skip()
	{
		stream.Position = currentTag.nextTagPosition;
	}

	#endregion
	
	#region General Complexity
	
	protected bool WriteBytesBeforeTag(string tag, ES2Writer writer)
	{
		// If tag doesn't exist, write nothing.
		if(ScanToTag(tag) == false)
			return false;
		
		int position = (int)stream.Position;
		stream.Position = 0;
		writer.WriteRaw(reader.ReadBytes((int)currentTag.position));
		stream.Position = position;
		return true;
	}
	
	
	protected bool WriteRemainingBytes(ES2Writer writer)
	{
		int noOfBytes = (int)(stream.Length-stream.Position);
		
		if(noOfBytes > 0)
		{
			writer.WriteRaw(stream.ReadBytes(noOfBytes));
			return true;
		}
		return false;
	}
	
	
	protected bool WriteBytesBeforeTagLowMemory(string tag, ES2Writer writer)
	{
		// If tag doesn't exist, write nothing.
		if(ScanToTag(tag) == false)
			return false;
		long position = stream.Position;

		long streamLength = currentTag.position;
		stream.Position = 0;
		while(stream.Position < streamLength)
		{
			long thisBuffer;
			
			if(stream.Position+ES2GlobalSettings.defaultBufferSize > streamLength)
				thisBuffer =  streamLength-stream.Position;
			else
				thisBuffer = ES2GlobalSettings.defaultBufferSize;

			writer.WriteRaw(stream.ReadBytes((int)thisBuffer));
		}
		stream.Position = position;
		return true;
	}
	
	
	protected bool WriteRemainingBytesLowMemory(ES2Writer writer)
	{	
		int noOfBytes = (int)(stream.Length-stream.Position);
		if(noOfBytes > 0)
		{
			int streamLength = noOfBytes;
			int offset = (int)stream.Position;
			while(stream.Position-offset < streamLength)
			{
				int thisBuffer;
				
				if((stream.Position-offset)+ES2GlobalSettings.defaultBufferSize > streamLength)
					thisBuffer = streamLength-((int)stream.Position-offset);
				else
					thisBuffer = ES2GlobalSettings.defaultBufferSize;

				writer.WriteRaw(stream.ReadBytes(thisBuffer));
			}
			return true;
		}
		return false;
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool RenameTag(string oldTag, string newTag, ES2Writer writer)
	{
		// Remove tag from cache.
		if(cachedFile != null)
			cachedFile.RenameTag(oldTag, newTag);

		if(settings.optimizeMode == ES2Settings.OptimizeMode.Fast)
		{
			if(WriteBytesBeforeTag(oldTag, writer) == false)
				return false;
			
			stream.Position = currentTag.position;
			
			// Read past tag byte and tag string;
			reader.ReadByte();
			reader.ReadString();
			
			// Write tag byte and new tag string.
			writer.Write ((byte)ES2Keys.Key._Tag);
			writer.Write (newTag);
			
			// Write new length based on difference between new tag and old tag.
			int oldLength = reader.ReadInt32();
			int difference = newTag.Length - currentTag.tag.Length;
			writer.Write(oldLength + difference);
			
			WriteRemainingBytes(writer);
			return true;
		}
		else
		{
			if(WriteBytesBeforeTagLowMemory(oldTag, writer) == false)
				return false;
			
			stream.Position = currentTag.position;
			
			// Read past tag byte and tag string;
			reader.ReadByte();
			reader.ReadString();
			
			// Write tag byte and new tag string.
			writer.Write ((byte)ES2Keys.Key._Tag);
			writer.Write (newTag);
			
			// Write new length based on difference between new tag and old tag.
			int oldLength = reader.ReadInt32();
			int difference = newTag.Length - currentTag.tag.Length;
			writer.Write(oldLength + difference);
			
			WriteRemainingBytesLowMemory(writer);
			return true;
		}
	}
	
	/* Return true if any other data exists in this file */
	protected bool DeleteTag(string tag, ES2Writer writer)
	{	
		if(Length <= 0)
			return false;
		
		Reset(); // Reset so that we start from the beginning.
		
		bool tagExists = false;

		// Remove tag from cache.
		if(cachedFile != null)
			cachedFile.RemoveTag(tag);

		while(Next())
		{
			if(cachedFile != null)
				cachedFile.AddTag(currentTag);
			if(currentTag.tag != tag)
				WriteCurrentTag(writer);
			else
				tagExists = true;
		}
		return tagExists;
	}
	
	protected void WriteCurrentTag(ES2Writer writer)
	{
		long count = currentTag.nextTagPosition-currentTag.position;
		stream.Position = currentTag.position;
		writer.WriteRaw(reader.ReadBytes((int)count));
	}
	
	/* 
	 * Writes all tags not contained in the tags collection to the specified FormatWriter.
	 * No error is thrown if any tags do not exist.
	 * Returns true if any of the tags were found.
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool DeleteTags(ICollection<string> tags, ES2Writer writer)
	{
		if(Length <= 0)
			return false;
		
		Reset(); // Reset so that we start from the beginning.

		// Remove tags from cache.
		if(cachedFile != null)
			foreach(string tag in tags)
				cachedFile.RemoveTag(tag);

		bool hasTag = false;

		while(Next())
		{
			if(cachedFile != null)
				cachedFile.AddTag(currentTag);

			if(!tags.Contains(currentTag.tag))
			{
				// Remove tag from cache.
				if(cachedFile != null)
					cachedFile.RemoveTag(currentTag.tag);
				WriteCurrentTag(writer);
			}
			else
				hasTag = true;
		}
		return hasTag;
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public string[] GetTags()
	{
		List<string> tags = new List<string>();
		
		if(Length <= 0)
			return tags.ToArray();
		
		while(Next())	
			tags.Add(currentTag.tag);
		
		return tags.ToArray();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public byte[] ReadMeshSettings()
	{
		return reader.ReadBytes((int)reader.ReadByte());
	}
	
	
	private byte[] GetDecryptedBytes()
	{
		if(settings.encryptionType == ES2Settings.EncryptionType.AES128) // AES 128-bit encryption
		{
			AESEncryptor aesEncryptor = new AESEncryptor(settings.encryptionPassword, MoodkieSecurity.AESBits.BITS128);
			return aesEncryptor.Decrypt(reader.ReadBytes(reader.ReadInt32()));
		}
		else // XOR Obfuscation
			return Obfuscator.Obfuscate(reader.ReadBytes(reader.ReadInt32()), settings.encryptionPassword);
	}
	
	
	private ES2Reader GetEncryptedReader()
	{
		ES2Settings encryptedSettings = new ES2Settings();
		encryptedSettings.saveLocation = ES2Settings.SaveLocation.Memory;
		// Make sure encrypt=false so we don't enter an infinite loop/Stackoverflow situation.
		encryptedSettings.encrypt = false;
		return ES2Reader.Create(GetDecryptedBytes(), encryptedSettings);
	}
	
	public int Length
	{
		get{return (int)stream.Length;}
	}
	
	/*
	 *  Seeks back to the beginning of the Stream and resets any
	 * 	variables associated with seeking.
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void Reset()
	{
		stream.Position = 0;
		currentTag = new ES2Tag(true);
	}
	
	public void Dispose()
	{
		ES2Dispose.Dispose(reader);
	}
	#endregion

	// ES2FormatReader methods

	#region Seeking Complexity
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public void CacheFile()
	{
		cachedFile = ES2Cache.AddNewCachedFile(settings.filenameData.filePath);

		if(Length <= 0)
			return;
		
		Reset(); // Reset so that we start from the beginning.
		
		while(Next())
			cachedFile.AddTag(currentTag.tag, currentTag.position, currentTag.settingsPosition, currentTag.nextTagPosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool ScanToTag(string tag)
	{
		if(Length <= 0)
			return false;

		if(cachedFile != null)
		{
			ES2Tag thisTag;
			if(!(thisTag = cachedFile.GetTag(tag)).isNull)
			{
				currentTag = thisTag;
				stream.Position = thisTag.settingsPosition;
				return true;
			}
		}

		Reset(); // Reset so that we start from the beginning.
		
		while(Next())
		{
			if(currentTag.tag == tag)
				return true;
		}
		return false;
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool ScanToTag()
	{
		return ScanToTag(settings.filenameData.tag);
	}
	
	public bool TagExists(string tag)
	{
		bool exists = ScanToTag(tag);
		Reset();
		return exists;
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public bool TagExists()
	{
		return TagExists(settings.filenameData.tag);
	}
	
	#endregion

	/* END ES2Reader functionality */

	#region User-Exposed Read Methods (Without Tag)

	public byte[] ReadRaw()
	{
		return stream.ReadBytes((int)stream.Length);
	}

	public T Read<T>()
	{
		return Read<T>(ES2TypeManager.GetES2Type(typeof(T)));
	}

	public void Read<T>(T c) where T : class
	{
		Read<T>(ES2TypeManager.GetES2Type(typeof(T)), c);
	}

	public T[] ReadArray<T>()
	{
		return ReadArray<T>(ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public void ReadArray<T>(T[] c) where T : class
	{
		ReadArray<T>(ES2TypeManager.GetES2Type(typeof(T)), c);
	}
	
	public T[,] Read2DArray<T>()
	{
		return Read2DArray<T>(ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public T[,,] Read3DArray<T>()
	{
		return Read3DArray<T>(ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
	{
		return ReadDictionary<TKey,TValue>(ES2TypeManager.GetES2Type(typeof(TKey)),ES2TypeManager.GetES2Type(typeof(TValue)));
	}
	
	public List<T> ReadList<T>()
	{
		return ReadList<T>(ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public void ReadList<T>(List<T> c) where T : class
	{
		ReadList<T>(ES2TypeManager.GetES2Type(typeof(T)), c);
	}
	
	public HashSet<T> ReadHashSet<T>()
	{	
		return ReadHashSet<T>(ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public Queue<T> ReadQueue<T>()
	{
		return ReadQueue<T>(ES2TypeManager.GetES2Type(typeof(T)));
	}
	
	public Stack<T> ReadStack<T>()
	{
		return ReadStack<T>(ES2TypeManager.GetES2Type(typeof(T)));
	}

	#endregion

	#region User-Exposed Read Methods (With Tag)
	
	public T Read<T>(string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._Null, type, null, tag);
		return (T)Read<T>(type);
	}
	
	public void Read<T>(string tag, T c) where T : class
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._Null, type, null, tag);
		Read<T>(type, c);
	}
	
	public T[] ReadArray<T>(string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._NativeArray, type, null, tag);
		return ReadArray<T>(type);
	}
	
	public void ReadArray<T>(string tag, T[] c) where T : class
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._NativeArray, type, null, tag);
		ReadArray<T>(type, c);
	}
	
	public T[,] Read2DArray<T>(string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._NativeArray, type, null, tag);
		return Read2DArray<T>(type);
	}
	
	public T[,,] Read3DArray<T>(string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._NativeArray, type, null, tag);
		return Read3DArray<T>(type);
	}
	
	public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(string tag)
	{
		ES2Type keyType = ES2TypeManager.GetES2Type(typeof(TKey));
		ES2Type valueType = ES2TypeManager.GetES2Type(typeof(TValue));
		ProcessHeader(ES2Keys.Key._Dictionary, valueType, keyType, tag);
		return ReadDictionary<TKey,TValue>(keyType, valueType);
	}
	
	public List<T> ReadList<T>(string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._List, type, null, tag);
		return ReadList<T>(type);
	}
	
	public void ReadList<T>(string tag, List<T> c) where T : class
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._List, type, null, tag);
		ReadList<T>(type, c);
	}
	
	public HashSet<T> ReadHashSet<T>(string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._HashSet, type, null, tag);
		return ReadHashSet<T>(type);
	}
	
	public Queue<T> ReadQueue<T>(string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._Queue, type, null, tag);
		return ReadQueue<T>(type);
	}
	
	public Stack<T> ReadStack<T>(string tag)
	{
		ES2Type type = ES2TypeManager.GetES2Type(typeof(T));
		ProcessHeader(ES2Keys.Key._Stack, type, null, tag);
		return ReadStack<T>(type);
	}

	/*
	 * 	Reads all objects from the file as Dictionary where the Key is the tag
	 * 	and the value is the loaded data as an object.
	 * 	It will skip arrays/collections and reference types.
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public ES2Data ReadAll()
	{
		ES2Data data = new ES2Data();

		if(Length <= 0)
			return data;
		
		Reset(); // Reset so that we start from the beginning.
		
		while(Next())
			data.loadedData[currentTag.tag] = ReadObject();

		return data;
	}
	
	/*
	 * 	Reads an object from the file, using the header data to determine type
	 *	and load appropriately. Must be positioned before a tag before using.
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public System.Object ReadObject()
	{
		ES2Header header = ReadHeader();
		
		ES2Type es2Type = ES2TypeManager.GetES2Type(header.valueType);
		
		// Handle encrypted data
		if(header.settings.encrypt)
		{
			// Handle non-encrypted data
			if(header.collectionType != ES2Keys.Key._Null)
			{
				if(header.collectionType == ES2Keys.Key._NativeArray)
					return ReadEncryptedArray<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._List)
					return ReadEncryptedList<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._Queue)
					return ReadEncryptedQueue<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._Stack)
					return ReadEncryptedStack<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._HashSet)
					return ReadEncryptedHashSet<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._Dictionary)
				{
					ES2Type keyType = ES2TypeManager.GetES2Type(header.keyType);
					return ReadEncryptedDictionary<System.Object, System.Object>(keyType, es2Type);
				}
			}
			else
				return ReadEncrypted<System.Object>(es2Type);
		}
		else
		{
			// Handle non-encrypted data
			if(header.collectionType != ES2Keys.Key._Null)
			{
				if(header.collectionType == ES2Keys.Key._NativeArray)
					return ReadArray<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._List)
					return ReadList<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._Queue)
					return ReadQueue<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._Stack)
					return ReadStack<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._HashSet)
					return ReadHashSet<System.Object>(es2Type);
				else if(header.collectionType == ES2Keys.Key._Dictionary)
				{
					ES2Type keyType = ES2TypeManager.GetES2Type(header.keyType);
					return ReadDictionary<System.Object, System.Object>(keyType, es2Type);
				}
			}
			else
				return Read<System.Object>(es2Type);
		}
		
		return null;
	}
	
	/*
	 * 	Reads an object from the file, using the header data to determine type
	 *	and load appropriately using self-assigning method. Must be positioned before a tag before using.
	 *	Should not be used to load collections or non-value types.
	 */
	public void ReadObject(object obj)
	{
		ES2Header header = ReadHeader();
		
		ES2Type es2Type = ES2TypeManager.GetES2Type(header.valueType);
		
		// Handle encrypted data
		if(header.settings.encrypt)
			ReadEncrypted<System.Object>(es2Type, obj);
		else
			Read<System.Object>(es2Type, obj);
	}
	
	/*
	 * 	Reads an object from the file with the given tag, using the header data to determine type
	 *	and load appropriately.
	 */
	public System.Object ReadObject(string tag)
	{
		if(Length <= 0)
			return null;
		
		Reset();
		
		if(!ScanToTag(tag))
			return null;
		return ReadObject(); 
	}
	
	/*
	 * 	Reads an object from the file with the given tag, using the header data to determine type
	 *	and load appropriately using self-assigning method. Must be positioned before a tag before using.
	 *	Should not be used to load collections or non-value types.
	 */
	public void ReadObject(string tag, object obj)
	{
		if(Length <= 0)
			return;
		
		Reset();
		
		if(!ScanToTag(tag))
			return;
		ReadObject(obj);
	}
	
	/*
	 * 	Reads a Component from the file into Component on provided GameObject, 
	 *	using the header data to determine exact type.
	 * 	If GameObject doesn't contain Component, it will be added.
	 */
	public void ReadComponent(GameObject go)
	{
		ES2Header header = ReadHeader();
		
		ES2Type es2Type = ES2TypeManager.GetES2Type(header.valueType);
		
		// Get Component from GameObject, or add it if it doesn't have one.
		Component c = go.GetComponent (es2Type.type);
		if(c == null)
			c = go.AddComponent(es2Type.type);
		
		// Handle encrypted data
		if(header.settings.encrypt)
			ReadEncrypted<System.Object>(es2Type, c);
		else
			Read<System.Object>(es2Type, c);
	}
	
	/*
	 * 	Reads a Component from the file into Component on provided GameObject, 
	 *	using the header data to determine exact type, and using the given tag.
	 * 	If GameObject doesn't contain Component, it will be added.
	 */
	public void ReadComponent(string tag, GameObject go)
	{
		if(Length <= 0)
			return;
		
		Reset();
		
		if(!ScanToTag(tag))
			return;
		ReadComponent(go);
	}
	
	/*
	 * 	Reads all of the headers for each tag from a file and returns them.
	 *	This is useful when you want type information for each tag.
	 */
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public Dictionary<string, ES2Header> ReadAllHeaders()
	{
		Dictionary<string, ES2Header> headers = new Dictionary<string, ES2Header>();
		
		if(Length <= 0)
			return headers;
		
		Reset(); // Reset so that we start from the beginning.
		
		while(Next())
		{
			headers[currentTag.tag] = ReadHeader();
		}
		
		return headers;
	}
	
	#endregion

	#region Read Property Methods

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public T ReadProperty<T>()
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();

		try
		{
			return Read<T>();
		}
		catch
		{
			stream.Position = startPosition + length;
			return default(T);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public T ReadProperty<T>(T obj) where T : class
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			Read<T>(obj);
			return obj;
		}
		catch
		{
			stream.Position = startPosition + length;
			return default(T);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public T[] ReadPropertyArray<T>()
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			return ReadArray<T>();
		}
		catch
		{
			stream.Position = startPosition + length;
			return null;
		}	
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public T[] ReadPropertyArray<T>(T[] obj) where T : class
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			ReadArray<T>(obj);
			return obj;
		}
		catch
		{
			stream.Position = startPosition + length;
			return null;
		}	
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public T[,] ReadProperty2DArray<T>()
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			return Read2DArray<T>();
		}
		catch
		{
			stream.Position = startPosition + length;
			return null;
		}	
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public T[,,] ReadProperty3DArray<T>()
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			return Read3DArray<T>();
		}
		catch
		{
			stream.Position = startPosition + length;
			return null;
		}	
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public List<T> ReadPropertyList<T>()
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			return ReadList<T>();
		}
		catch
		{
			stream.Position = startPosition + length;
			return null;
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public Queue<T> ReadPropertyQueue<T>()
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			return ReadQueue<T>();
		}
		catch
		{
			stream.Position = startPosition + length;
			return null;
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public Stack<T> ReadPropertyStack<T>()
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			return ReadStack<T>();
		}
		catch
		{
			stream.Position = startPosition + length;
			return null;
		}	
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public Dictionary<TKey,TValue> ReadPropertyDictionary<TKey, TValue>()
	{
		int startPosition = (int)stream.Position;
		int length = reader.ReadInt32();
		
		try
		{
			return ReadDictionary<TKey,TValue>();
		}
		catch
		{
			stream.Position = startPosition + length;
			return null;
		}
	}

	#endregion
	
	#region General Complexity

	private static int[] GetMultidimensionalIndices(System.Array a, int idx)
	{
		var indices = new int[a.Rank];

		for (var i = 0; i < a.Rank; i++)
		{
			var div = 1;

			for (var j = i + 1; j < a.Rank; j++)
			{
				div *= a.GetLength(j);
			}

			indices[i] = a.GetLowerBound(i) + idx / div % a.GetLength(i);
		}

		return indices;
	}

	public static ES2Reader Create(string identifier)
	{
		return Create(new ES2Settings(identifier));
	}
	
	public static ES2Reader Create(string identifier, ES2Settings settings)
	{
		return Create(settings.Clone(identifier));
	}

	public static ES2Reader Create(ES2Settings settings)
	{
		return new ES2Reader(settings);
	}

	public static ES2Reader Create(byte[] bytes, ES2Settings settings)
	{
		return new ES2Reader(bytes, settings);
	}

	#endregion

	public ES2Reader(ES2Settings settings)
	{
		this.settings = settings;
		this.stream = ES2Stream.Create(settings, ES2Stream.Operation.Read);
		this.reader = new BinaryReader(stream.stream);
		cachedFile = ES2Cache.GetCachedFile(settings.filenameData.filePath);
	}

	public ES2Reader(byte[] bytes, ES2Settings settings)
	{
		this.settings = settings;
		this.stream = ES2Stream.Create(bytes, settings);
		this.reader = new BinaryReader(stream.stream);
	}
}

public class ES2InvalidDataException : System.Exception
{
	public ES2InvalidDataException() : base("Easy Save 2 Error: The file provided does not contain data that is readable by Easy Save. Please make sure that file was created by Easy Save.")
	{
	}
}
