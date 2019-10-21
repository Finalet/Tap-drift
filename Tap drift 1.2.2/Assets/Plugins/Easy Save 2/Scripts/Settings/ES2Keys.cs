using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* ES2Keys Class */
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ES2Keys
{
	#region Keys
	public enum Key : byte	{	// Types : 0 to 80
								/*_byte 	= 0,
								_string = 1,
								_int 	= 2,
								_uint 	= 3,
								_short 	= 4,
								_ushort = 5,
								_float 	= 6,
								_long 	= 7,
								_double = 8,
								_bool	= 9,
								_Vector2 = 10,
								_Vector3 = 11,
								_Vector4 = 12,
								_Color 	= 13,
								_Quaternion = 14,
								_Mesh 	= 15,
								_Transform = 16,
								_Texture2D = 17,
								_Object	= 18,
								_char = 19,
								_ulong = 20,
								_SphereCollider = 21,
								_BoxCollider = 22,
								_CapsuleCollider = 23,
								_MeshCollider = 24,
								_AudioClip = 25,
								_Color32 = 26,
								_Material = 27,
								//_TreeInstance = 28,
								_Rect = 29,
								_Bounds = 30,
								//_Sprite = 31,*/
								
								
								// Arrays : 81 to 100
								_NativeArray = 81,
								_Dictionary = 82,
								_List 		= 83,
								_HashSet 	= 84,
								_Queue 		= 85,
								_Stack		= 86,
								
								// Identifiers : 101 to 127
								_Obfuscate 	= 121,
								_Size		= 122,
								_Terminator = 123,
								_Null 		= 124,
								_Settings	= 125,
								_Tag		= 126,
								_Encrypt	= 127,
						};
	#endregion

	#region Type from Keys
	public static string TypeFromKeys(Key collection, Key value, Key key)
	{
		string valueType = "";
		string keyType = "";

		// Iterate through Dictionary and find the type with the matching ID.
		// Performance here doesn't matter as this is only called when there's an error, so should
		// never be called at runtime.
		foreach(KeyValuePair<System.Type, ES2Type> entry in ES2TypeManager.types)
		{
			if(entry.Value.key == (byte)value)
				valueType = entry.Key.ToString();

			if(entry.Value.key == (byte)key)
				keyType = entry.Key.ToString();
		}

		// If it's a single type.
		if(collection == Key._Null)
			return valueType.ToString();
		// If it has TKey, TValue (i.e. Dictionary).
		if(keyType != "")
			return "Dictionary<"+keyType+","+valueType+">";
		// If it's a native array.
		if(collection == Key._NativeArray)
			return valueType+"[ ]";
		// Else, it's a generic collection.
		return collection+"<"+valueType+">";
	}
	#endregion

	#region Key(s) from ES2Type
	public static Key[] KeysFromType(Key collectionType, Key valueType, Key keyType)
	{
		return new Key[]{collectionType, valueType, keyType};
	}

	public static Key KeyFromES2Type(ES2Type type)
	{
		if(type != null)
			return (Key)type.key;
		return Key._Null;
	}

	public static Key[] KeysFromES2Type(ES2Type type)
	{
		return KeysFromType(Key._Null, KeyFromES2Type(type), Key._Null);
	}

	public static Key[] KeysFromES2Type(ES2Type type, Key collectionType)
	{
		return KeysFromType(collectionType, KeyFromES2Type(type), Key._Null);
	}

	public static Key[] KeysFromES2Type(ES2Type keyType, ES2Type valueType, Key collectionType)
	{
		return KeysFromType(collectionType, KeyFromES2Type(valueType), KeyFromES2Type(keyType));
	}
	
	#endregion

	#region Key(s) from System.Type

	public static Key GetCollectionType(System.Type type)
	{
		if(!ES2Reflection.IsAssignableFrom(typeof(IEnumerable), type))
			return Key._Null;

		if(type.IsArray)
			return Key._NativeArray;

		// If it's not an array and doesn't have generic arguments, we don't consider it to be a collection.
		if(!ES2Reflection.IsGenericType(type))
			return Key._Null;

		System.Type genericType = type.GetGenericTypeDefinition();

		if(genericType == typeof(Dictionary<,>))
			return Key._Dictionary;
		else if(genericType == typeof(List<>))
			return Key._List;
		else if(genericType == typeof(Queue<>))
			return Key._Queue;
		else if(genericType == typeof(Stack<>))
			return Key._Stack;
		if(genericType == typeof(HashSet<>))
			return Key._HashSet;

		return Key._Null;
	}

	#endregion
}