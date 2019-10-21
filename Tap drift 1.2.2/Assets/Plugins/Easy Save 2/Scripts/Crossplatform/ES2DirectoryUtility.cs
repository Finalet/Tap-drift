#if NETFX_CORE
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage;
using System.Collections.Generic;
using System;
using System.IO;

public class ES2DirectoryUtility
{
public static void Delete(string path, bool recursive)
{
}

public static bool Exists(string path)
{
return true;
}

public static void Move(string from, string to)
{
}

public static void CreateDirectory(string path)
{
}

public static string[] GetDirectories(string path)
{
return new string[0];
}

public static string[] GetFiles(string path, string searchPattern)
{
StorageFolder folder = GetStorageFolder(path);
Task<IReadOnlyList<StorageFile>> getFilesTask = Task<IReadOnlyList<StorageFile>>.Run<IReadOnlyList<StorageFile>>(async () => { return await folder.GetFilesAsync(); });
getFilesTask.Wait();

List<string> filenames = new List<string>();

foreach (StorageFile file in getFilesTask.Result)
{
if (searchPattern != "" && searchPattern != "*")
if (Path.GetExtension(file.Name) != searchPattern.Replace("*", ""))
continue;
filenames.Add(file.Name);
}

return filenames.ToArray();
}

public static StorageFolder GetStorageFolder(string path)
{
return ApplicationData.Current.LocalFolder;
}
}

#else
using System.IO;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2DirectoryUtility
{
	public static void Delete(string path, bool recursive)
	{
		Directory.Delete(path, recursive);
	}

	public static bool Exists(string path)
	{
		return Directory.Exists(path);
	}

	public static void Move(string from, string to)
	{
		Directory.Move(from, to);
	}

	public static void CreateDirectory(string path)
	{
		Directory.CreateDirectory(path);
	}

	public static string[] GetDirectories(string path)
	{
		return Directory.GetDirectories(path);
	}

	public static string[] GetFiles(string path, string searchPattern)
	{
		return Directory.GetFiles(path, searchPattern);
	}
}
#endif

