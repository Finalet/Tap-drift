using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Enum : ES2EditorType
{
	public ES2Editor_Enum() : base(typeof(System.Enum))
	{
		key = (byte)32;
	}
	
	public override object DisplayGUI(object data)
	{
		EditorGUILayout.LabelField("Note: this is an integer representation of the Enum.");
		return EditorGUILayout.IntField((int)data);
	}
	
	public override object CreateInstance()
	{
		return 0;
	}
}

