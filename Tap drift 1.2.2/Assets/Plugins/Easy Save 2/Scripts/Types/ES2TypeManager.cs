using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2TypeManager
{
	public static Dictionary<System.Type, ES2Type> types = null;
	
	private static ES2Type cachedType = null;
	
	public static void AddES2Type(ES2Type es2Type)
	{
		types[es2Type.type] = es2Type;
	}
	
	/*
	 * 	Gets the ES2Type for a specific System.Type.
	 * 	Returns null if one is not found.
	 */
	public static ES2Type GetES2Type(System.Type type)
	{
		if(types == null)
			ES2.Init();

		// We use caching of the last type as it is often likely that repeated calls to this method will be made for the same type.
		if(cachedType != null)
			if(cachedType.type.Equals(type))
				return cachedType;

		if(types.TryGetValue(type, out cachedType))
			return cachedType;
		else if(ES2TypeUtility.IsEnum(type))
			return new ES2_Enum();

		return null;
	}

	/*
	 * 	Gets the ES2Type with a specific 'key' byte.
	 * 	Returns null if one is not found.
	 */
	public static ES2Type GetES2Type(byte key)
	{
		if(types == null)
			ES2.Init();
		
		// We use caching of the last type as it is often likely that repeated calls to this method will be made for the same type.
		if(cachedType != null)
			if(cachedType.key == key)
				return cachedType;

		// Iterate over array.
		foreach(KeyValuePair<System.Type, ES2Type> type in types)
			if(type.Value.key == key)
				return cachedType = type.Value;
		return null;
	}
	
	/*
	 * 	Gets the ES2Type with a specific ES2Key.
	 * 	Returns null if one is not found.
	 */
	public static ES2Type GetES2Type(ES2Keys.Key key)
	{
		return GetES2Type((byte)key);
	}
	
	/*
	 * 	Gets the ES2Type with a specific 'key' byte.
	 * 	Returns null if one is not found.
	 */
	public static ES2Type GetES2Type(int hash)
	{
		if(types == null)
			ES2.Init();
		
		// We use caching of the last type as it is often likely that repeated calls to this method will be made for the same type.
		if(cachedType != null)
			if(cachedType.hash == hash)
				return cachedType;
		
		// Iterate over array.
		foreach(KeyValuePair<System.Type, ES2Type> type in types)
			if(type.Value.hash == hash)
				return cachedType = type.Value;
		return null;
	}
}

