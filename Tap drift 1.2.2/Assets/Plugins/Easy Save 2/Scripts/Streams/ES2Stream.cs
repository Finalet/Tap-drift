using UnityEngine;
using System.Collections;
using System.IO;

public abstract class ES2Stream
{
	public abstract void Store();
	public abstract bool MayRequireOverwrite();

	public ES2Settings settings;
	public Stream stream;

	public enum Operation{Read, Write};

	public long Length
	{
		get{return stream.Length;}
		set{stream.SetLength(value);}
	}

	public long Position
	{
		get{return stream.Position;}
		set{stream.Position = value;}
	}

	public byte[] ReadBytes(int count)
	{
		byte[] bytes = new byte[count];
		stream.Read(bytes, 0, count);
		return bytes;
	}

	public virtual byte[] ReadAllBytes()
	{
		stream.Position = 0;
		byte[] bytes = new byte[stream.Length];
		stream.Read(bytes, 0, bytes.Length);
		return bytes;
	}

	protected void WriteToStreamFast(Stream writableStream)
	{
		byte[] bytes = ReadAllBytes();
		writableStream.Write(bytes, 0, (int)stream.Position);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	protected void WriteToStreamLowMemory(Stream writableStream)
	{
		stream.Position = 0;
		byte[] buffer = new byte[settings.bufferSize];
		int bytesRead;

		while((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
		{
			writableStream.Write (buffer, 0, bytesRead);
		}
	}

	public static ES2Stream Create(byte[] bytes, ES2Settings settings)
	{
		return new ES2MemoryStream(bytes, settings);
	}

	public static ES2Stream Create(ES2Settings settings, Operation operation)
	{
		// Web
		if(settings.filenameData.IsURL())
			return new ES2MemoryStream(settings);
		// File
		else if (settings.saveLocation == ES2Settings.SaveLocation.File)
		{
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				Debug.LogError("Easy Save 2 Error: You cannot load from file on this platform. Loading from PlayerPrefs instead.");
				return new ES2PlayerPrefsStream(settings, operation);
			}
			return new ES2FileStream(settings, operation);
		}
		// Memory
		else if(settings.saveLocation == ES2Settings.SaveLocation.Memory)
			return new ES2MemoryStream(settings);
		// PlayerPrefs
		else if(settings.saveLocation == ES2Settings.SaveLocation.PlayerPrefs)
			return new ES2PlayerPrefsStream(settings, operation);
		// Resources
		else if(settings.saveLocation == ES2Settings.SaveLocation.Resources)
			return new ES2ResourcesStream(settings);
		return null;
	}
}

