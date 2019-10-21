#if NETFX_CORE
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
using System;

public class ES2FileUtility
{
public static void Delete(string path)
{
if (!Exists(path))
return;

StorageFile file = OpenStorageFile(path);
if (file == null)
return;
Task deleteFileTask = Task.Run(async () => { await file.DeleteAsync(); });
deleteFileTask.Wait();
}

public static bool Exists(string path)
{
string[] files = ES2DirectoryUtility.GetFiles(path, "*");

foreach (string file in files)
if (file == path)
return true;
return false;
/*StorageFolder folder = ES2DirectoryUtility.GetStorageFolder(path);
if(OpenStorageFile(path, folder) == null)
return false;
return true;*/
}

public static void Move(string from, string to)
{
StorageFolder folder = ES2DirectoryUtility.GetStorageFolder(from);
StorageFile file = OpenStorageFile(from, folder);
Task moveFileTask = Task.Run(async () => { await file.MoveAsync(folder, to, NameCollisionOption.ReplaceExisting); });
moveFileTask.Wait();
}

public static byte[] ReadAllBytes(string path)
{
if (!Exists(path))
return new byte[0];

StorageFile file = OpenStorageFile(path);
if(file == null)
return new byte[0];
Task<byte[]> getFileTask = Task<byte[]>.Run<byte[]>(async () => { return await ReadAllBytesAsync(file); });
getFileTask.Wait();
return getFileTask.Result;
}

public static System.IO.Stream CreateFileStream(string path, ES2Settings.ES2FileMode filemode, int bufferSize)
{
if (filemode == ES2Settings.ES2FileMode.Create)
return WindowsRuntimeStreamExtensions.AsStream(GetWriteStream(CreateStorageFile(path)));
else if (filemode == ES2Settings.ES2FileMode.Append)
return WindowsRuntimeStreamExtensions.AsStream(GetAppendStream(OpenOrCreateStorageFile(path)));
else // ES2FileMode.Open
{
if (!Exists(path))
throw new FileNotFoundException();
StorageFile file = OpenStorageFile(path);
if(file == null)
throw new FileNotFoundException();
return WindowsRuntimeStreamExtensions.AsStream(GetReadStream(OpenStorageFile(path)));
}
}

// Assorted StorageFile/StorageFolder methods

public static StorageFile CreateStorageFile(string path)
{
return CreateStorageFile(path, ES2DirectoryUtility.GetStorageFolder(path));
}

public static StorageFile CreateStorageFile(string path, StorageFolder folder)
{
Task<StorageFile> getFileTask = Task<StorageFile>.Run<StorageFile>(async () => { return await folder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting); });
getFileTask.Wait();
return getFileTask.Result;
}

public static StorageFile OpenOrCreateStorageFile(string path)
{
return OpenOrCreateStorageFile(path, ES2DirectoryUtility.GetStorageFolder(path));
}

public static StorageFile OpenOrCreateStorageFile(string path, StorageFolder folder)
{
Task<StorageFile> getFileTask = Task<StorageFile>.Run<StorageFile>(async () => { return await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists); });
getFileTask.Wait();
return getFileTask.Result;
}

public static StorageFile OpenStorageFile(string path)
{
return OpenStorageFile(path, ES2DirectoryUtility.GetStorageFolder(path));
}

/* Returns null if file doesn't exist */
public static StorageFile OpenStorageFile(string path, StorageFolder folder)
{
if (!Exists(path))
return null;

StorageFile result = null;

try
{
Task<StorageFile> getFileTask = Task<StorageFile>.Run<StorageFile>(async () => { return await folder.GetFileAsync(path); });
getFileTask.Wait();
result = getFileTask.Result;
}
catch(Exception)
{
result = null;
}
return result;
}

public static IRandomAccessStream GetReadStream(StorageFile file)
{
Task<IRandomAccessStream> getStreamTask = Task<IRandomAccessStream>.Run<IRandomAccessStream>(async () => { return await file.OpenReadAsync(); });
getStreamTask.Wait();
return getStreamTask.Result;
}

public static IRandomAccessStream GetWriteStream(StorageFile file)
{
Task<IRandomAccessStream> getStreamTask = Task<IRandomAccessStream>.Run<IRandomAccessStream>(async () => { return await file.OpenAsync(FileAccessMode.ReadWrite); });
getStreamTask.Wait();
return getStreamTask.Result;
}

public static IRandomAccessStream GetAppendStream(StorageFile file)
{
IRandomAccessStream stream = GetWriteStream(file);
stream.Seek(stream.Size);
return stream;
}

public static async Task<byte[]> ReadAllBytesAsync(StorageFile file)
{
using (IRandomAccessStream stream = await file.OpenReadAsync())
{
using (DataReader reader = new DataReader(stream.GetInputStreamAt(0)))
{
await reader.LoadAsync((uint)stream.Size);
byte[] Bytes = new byte[stream.Size];
reader.ReadBytes(Bytes);
return Bytes;
}
}
}
}

#else

using System.IO;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2FileUtility
{
	public static void Delete(string path)
	{
		File.Delete(path);
	}

	public static bool Exists(string path)
	{
		return File.Exists(path);
	}

	public static void Move(string from, string to)
	{
		File.Move(from, to);
	}

	public static byte[] ReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}

	public static Stream CreateFileStream(string path, ES2Settings.ES2FileMode filemode, int bufferSize)
	{
		if(filemode == ES2Settings.ES2FileMode.Create)
			return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);
		else if(filemode == ES2Settings.ES2FileMode.Append)
			return new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize);
		else // ES2FileMode.Open
			return new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None, bufferSize);
	}
}

#endif

