using UnityEngine;
using System.IO;

internal sealed class ES2MemoryStream : ES2Stream
{
	public byte[] storedBytes = null;
	
	public ES2MemoryStream(ES2Settings settings)
	{
		this.settings = settings;
		stream = new MemoryStream();
	}

	public ES2MemoryStream(byte[] bytes, ES2Settings settings)
	{
		stream = new MemoryStream(bytes);
	}
	
	public ES2MemoryStream()
	{
		stream = new MemoryStream();
		settings = new ES2Settings();
	}
	
	public override void Store()
	{
		using(MemoryStream storageStream = new MemoryStream())
		{
			if(settings.optimizeMode == ES2Settings.OptimizeMode.Fast)
				WriteToStreamFast(storageStream);
			else if(settings.optimizeMode == ES2Settings.OptimizeMode.LowMemory)
				WriteToStreamLowMemory(storageStream);

			storedBytes = storageStream.ToArray();
		}
	}

	public override bool MayRequireOverwrite()
	{
		return false;
	}

	public override byte[] ReadAllBytes()
	{
		if(storedBytes != null)
			return storedBytes;
		else
			return base.ReadAllBytes();
	}
}
