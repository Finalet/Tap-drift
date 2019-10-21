using UnityEngine;
using System.IO;

internal sealed class ES2PlayerPrefsStream : ES2Stream
{
	public ES2PlayerPrefsStream(ES2Settings settings, Operation operation)
	{
		this.settings = settings;

		if(operation == Operation.Read)
			stream = CreateReadStream();
		else
			stream = CreateWriteStream();
	}

	public override void Store()
	{
		using(Stream storageStream = CreateStorageStream())
		{
			if(settings.optimizeMode == ES2Settings.OptimizeMode.Fast)
				WriteToStreamFast(storageStream);
			else if(settings.optimizeMode == ES2Settings.OptimizeMode.LowMemory)
				WriteToStreamLowMemory(storageStream);

			if(settings.fileMode == ES2Settings.ES2FileMode.Append)
				AppendRaw((stream as MemoryStream).ToArray());
			else
				StoreRaw ((stream as MemoryStream).ToArray());
		}
	}
	
	private void StoreRaw()
	{
		StoreRaw((stream as MemoryStream).ToArray());
	}
	
	private void StoreRaw(byte[] bytes)
	{
		PlayerPrefs.SetString(settings.filenameData.playerPrefsPath, System.Convert.ToBase64String(bytes));
	}
	
	private void StoreRaw(string bytes)
	{
		PlayerPrefs.SetString(settings.filenameData.playerPrefsPath, bytes);
	}
	
	private void AppendRaw(byte[] bytes)
	{
		if(PlayerPrefs.HasKey(settings.filenameData.playerPrefsPath))
		{
			StoreRaw(PlayerPrefs.GetString(settings.filenameData.playerPrefsPath) + 
					System.Convert.ToBase64String(bytes));
		}
		else
			StoreRaw(bytes);
	}

	private Stream CreateStorageStream ()
	{
		return new MemoryStream();
	}

	private Stream CreateReadStream()
	{
		// If PlayerPrefs doesn't have this data, throw exception.
		if(!PlayerPrefs.HasKey(settings.filenameData.playerPrefsPath))
			return new MemoryStream();
		
		MemoryStream stream = new MemoryStream(System.Convert.FromBase64String(PlayerPrefs.GetString(settings.filenameData.playerPrefsPath)));
		
		// Trim null values from end of stream.
		/*if(stream.Length > 0)
		{
			stream.Position = stream.Length-1;
			while(stream.ReadByte() == (byte)0)
			{
				stream.SetLength(stream.Length-1);
				stream.Position = stream.Length-1;
			}
		}*/
		
		stream.Position = 0;
		
		return stream;
	}

	private Stream CreateWriteStream()
	{
		return new MemoryStream();
	}

	public override bool MayRequireOverwrite()
	{
		return PlayerPrefs.HasKey(settings.filenameData.playerPrefsPath);
	}
}
