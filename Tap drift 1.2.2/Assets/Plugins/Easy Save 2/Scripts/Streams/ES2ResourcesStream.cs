using UnityEngine;
using System.IO;

internal sealed class ES2ResourcesStream : ES2Stream
{
	public ES2ResourcesStream(ES2Settings settings)
	{
		if(settings.filenameData.extension != ".bytes")
			Debug.LogError("Easy Save 2 Error: Can only load files from Resources with the extension '.bytes'.");
		this.settings = settings;
		
		TextAsset data = Resources.Load(settings.filenameData.resourcesPath) as TextAsset;

		if(data == null)
			Debug.LogError("Easy Save 2 Error: The data, tag, file or folder you are looking for does not exist. Please ensure that ES2.Exists(string identifier) returns true before calling this method.");

		stream = new MemoryStream(data.bytes);
	}

	public override void Store()
	{
		Debug.LogError("Easy Save 2 Error: You cannot save to Resources at runtime.");
	}
	
	public override bool MayRequireOverwrite()
	{
		Debug.LogError("Easy Save 2 Error: You cannot save to Resources at runtime.");
		return false;
	}
}
