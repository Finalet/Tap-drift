using System.IO;

internal sealed class ES2FileStream : ES2Stream
{
	public ES2FileStream(ES2Settings settings, Operation operation)
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
		}
        stream.Dispose();
        ES2FileUtility.Delete(settings.filenameData.filePath + "tmp");

    }

    private Stream CreateStorageStream()
	{
		if(!ES2DirectoryUtility.Exists(settings.filenameData.directoryPath))
			ES2DirectoryUtility.CreateDirectory(settings.filenameData.directoryPath);
		return ES2FileUtility.CreateFileStream(settings.filenameData.filePath, settings.fileMode, settings.bufferSize);
	}
	
	private Stream CreateWriteStream()
	{
		if(settings.optimizeMode == ES2Settings.OptimizeMode.Fast)
			return new MemoryStream();
		else
		{
			if(!ES2DirectoryUtility.Exists(settings.filenameData.directoryPath))
				ES2DirectoryUtility.CreateDirectory(settings.filenameData.directoryPath);
			
			return ES2FileUtility.CreateFileStream(settings.filenameData.filePath+"tmp", ES2Settings.ES2FileMode.Create, settings.bufferSize);
		}
	}

	private Stream CreateReadStream()
	{
		if(settings.optimizeMode == ES2Settings.OptimizeMode.Fast)
			return new MemoryStream(ES2FileUtility.ReadAllBytes(settings.filenameData.filePath));
		else
			return ES2FileUtility.CreateFileStream(settings.filenameData.filePath, ES2Settings.ES2FileMode.Open, settings.bufferSize);
	}

	public override bool MayRequireOverwrite()
	{
		return ES2FileUtility.Exists(settings.filenameData.filePath);
	}
}
