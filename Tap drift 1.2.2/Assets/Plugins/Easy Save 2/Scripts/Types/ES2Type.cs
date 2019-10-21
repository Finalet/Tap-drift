using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class ES2Type
{
	public byte key = (byte)255; // The unique identifier for the type.
	public int hash;
	public System.Type type; // The System.Type represented by this ES2Type.
	
	public ES2Type(System.Type type)
	{
		this.type = type;
		hash = GetHash(this.type);
	}

	public abstract void Write(object data, ES2Writer writer);

	public virtual object Read(ES2Reader reader)
	{
		Debug.LogError("This Load method is not supported on Types of "+type.ToString()+". Try a self-assigning Load method instead");
		return null;
	}

	/*
	 * 	This method remains for backwards compatibility, but ReadRef is now preferred as it works with value types.
	 */
	public virtual void Read(ES2Reader reader, System.Object c)
	{
		Debug.LogError("Self-Assigning Load is not supported on Types of "+type.ToString()+".\nType must not be a value type, and must have a parameterless constructor.");
	}
	
	/* Included for backwards compatibility, when self assigning load only worked with Components */
	public virtual void Read(ES2Reader reader, Component c)
	{
		Read(reader, (System.Object)c);
	}

	public static T GetOrCreate<T>() where T : Component
	{
		T param;
		GameObject go = new GameObject();
		go.name = "Easy Save 2 Loaded Component";
		
		param = go.GetComponent<T>();
		if(param == null)
			param = go.AddComponent<T>();

		return param;
	}

	/* Generates an (almost) unique and persistent hashcode for a particular type */
	public static int GetHash(System.Type type)
	{
		return GetHash(type.ToString());
	}
	
	public static int GetHash(string value)
	{
		unchecked
		{
			// check for degenerate input
			if (string.IsNullOrEmpty(value))
				return 0;
			
			int length = value.Length;
			uint hash = (uint) length;
			
			int remainder = length & 1;
			length >>= 1;
			
			// main loop
			int index = 0;
			for (; length > 0; length--)
			{
				hash += value[index];
				uint temp = (uint) (value[index + 1] << 11) ^ hash;
				hash = (hash << 16) ^ temp;
				index += 2;
				hash += hash >> 11;
			}
			
			// handle odd string length
			if (remainder == 1)
			{
				hash += value[index];
				hash ^= hash << 11;
				hash += hash >> 17;
			}
			
			// force "avalanching" of final 127 bits
			hash ^= hash << 3;
			hash += hash >> 5;
			hash ^= hash << 4;
			hash += hash >> 17;
			hash ^= hash << 25;
			hash += hash >> 6;
			
			return (int) hash;
		}
	}
}

