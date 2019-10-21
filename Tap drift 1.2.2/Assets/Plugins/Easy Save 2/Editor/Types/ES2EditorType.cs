using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class ES2EditorType
{
	public byte key = (byte)255; // The unique identifier for the type.
	public int hash;
	public System.Type type; // The System.Type represented by this ES2Type.
	
	public static Dictionary<Type, ES2EditorType> editorTypes = null;
	
	public ES2EditorType(System.Type type)
	{
		this.type = type;
		hash = GetHash(this.type);
	}
	
	public static void Init()
	{
		if(editorTypes == null)
			editorTypes = ES2EditorReflection.GetEditorTypes();
	}
	
	public static ES2EditorType Get(System.Type type)
	{
		Init();
		ES2EditorType temp;
		if(editorTypes.TryGetValue(type, out temp))
			return temp;
		return null;
	}
	
	public static ES2EditorType Get(byte key)
	{
		Init();
		foreach(KeyValuePair<System.Type, ES2EditorType> type in editorTypes)
			if(type.Value.key == key)
				return type.Value;
		return null;
	}
	
	public static ES2EditorType Get(int hash)
	{
		Init();
		foreach(KeyValuePair<System.Type, ES2EditorType> type in editorTypes)
			if(type.Value.hash == hash)
				return type.Value;
		return null;
	}
	
	// Gets all of the supported Editor types as an array of System.Types.
	public static Type[] GetTypes()
	{
		Init();
		Type[] types = new Type[editorTypes.Count];
		
		int index = 0;
		foreach(KeyValuePair<Type, ES2EditorType> entry in editorTypes)
		{
			types[index] = entry.Key;
			index++;
		}
		return types;
	}
	
	/* Return true if a GUI is implemented for this type */
	public virtual object DisplayGUI(object data)
	{
		return null;
	}
	
	public virtual object CreateInstance() 
	{
		if(typeof(Component).IsAssignableFrom(type))
		{
			GameObject blankObject = Resources.Load<GameObject>("ES2/ES2BlankObject");
			Component component = null;
			if((component = blankObject.GetComponent(type)) == null)
				component = blankObject.AddComponent(type);
			return component;
		}
		else if(typeof(string).IsAssignableFrom(type))
		{
			return "";
		}
		return Activator.CreateInstance(type);
	}
	
	/* Generates an (almost) unique and persistent hashcode for a particular type */
	public static int GetHash(System.Type type)
	{
		return ES2Type.GetHash(type);
	}
}
