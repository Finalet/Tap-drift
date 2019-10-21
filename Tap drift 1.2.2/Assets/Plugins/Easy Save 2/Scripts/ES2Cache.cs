using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ES2Cache
{
	private static Dictionary<string, ES2CachedFile> fileCaches = new Dictionary<string, ES2CachedFile>();

	public static ES2CachedFile AddNewCachedFile(string absolutePath)
	{
		return (fileCaches[absolutePath] = new ES2CachedFile());
	}

	public static bool DeleteCachedFile(string absolutePath)
	{
		return fileCaches.Remove(absolutePath);
	}

	public static ES2CachedFile GetOrCreateCachedFile(string absolutePath)
	{
		ES2CachedFile fileCache;
		if(!fileCaches.TryGetValue(absolutePath, out fileCache))
			return AddNewCachedFile(absolutePath);
		return fileCache;
	}

	/*
	 * Tries to get cached file, or returns null if one doesn't exist.
	 */
	public static ES2CachedFile GetCachedFile(string absolutePath)
	{
		ES2CachedFile fileCache;
		if(!fileCaches.TryGetValue(absolutePath, out fileCache))
			return null;
		return fileCache;
	}

	public static new string ToString()
	{
		string logString = "";
		foreach(KeyValuePair<string, ES2CachedFile> entry in fileCaches)
		{
			logString += "{"+entry.Key+":\n" + entry.Value.ToString()+"\n}\n";
		}
		return logString;
	}
}

public class ES2CachedFile
{
	private Dictionary<string, ES2Tag> tagCache = new Dictionary<string, ES2Tag>();
	
	public void AddTag(string tag, long position, long settingsPosition, long nextTagPosition)
	{
		AddTag( new ES2Tag(tag, position, settingsPosition, nextTagPosition) );
	}

	public void AddTag(ES2Tag tag)
	{
		tagCache[tag.tag] = tag; 
	}

	public ES2Tag AddOrUpdateTag(string tag, long position, long settingsPosition, long nextTagPosition)
	{
		ES2Tag cachedTag;

		if(!tagCache.TryGetValue(tag, out cachedTag))
			return (tagCache[tag] = new ES2Tag(tag, position, settingsPosition, nextTagPosition));

		cachedTag.position = position;
		cachedTag.settingsPosition = settingsPosition;
		cachedTag.nextTagPosition = nextTagPosition;
		return cachedTag;
	}

	/*
	 * 	Gets the position of a tag in a file, or returns -1 if tag isn't cached.
	 */
	public ES2Tag GetTag(string tag)
	{
		ES2Tag cachedTag;
		if(!tagCache.TryGetValue(tag, out cachedTag))
			return new ES2Tag(true);
		return cachedTag;
	}

	public bool RemoveTag(string tag)
	{
		return tagCache.Remove(tag);
	}

	public void RenameTag(string oldTag, string newTag)
	{
		tagCache[newTag] = tagCache[oldTag];
		RemoveTag(oldTag);
	}

	public override string ToString()
	{
		string logString = "";
		foreach(KeyValuePair<string, ES2Tag> item in tagCache)
		{
			ES2Tag tag = item.Value;
			logString += "{tag:"+tag.tag+", position:"+tag.position+", settingsPosition:"+tag.settingsPosition+", nextTagPosition:"+tag.nextTagPosition+"}\n";
		}
		return logString;
	}
}

public struct ES2Tag
{
	public string tag; 
	public long position; // The absolute position in the stream, before the _tag byte.
	public long settingsPosition; // The position where you can begin to read the settings bytes.
	public long nextTagPosition; // Position of next tag = Before next ES2Keys._tag, on terminator.
	public bool isNull;


	public ES2Tag(string tag, long position, long settingsPosition, long nextTagPosition)
	{
		this.tag = tag;
		this.position = position;
		this.settingsPosition = settingsPosition;
		this.nextTagPosition = nextTagPosition;
		this.isNull = false;
	}

	public ES2Tag(bool isNull)
	{
		this.isNull = isNull;
		this.tag = "";
		this.position = 0;
		this.settingsPosition = 0;
		this.nextTagPosition = 0;
	}
}