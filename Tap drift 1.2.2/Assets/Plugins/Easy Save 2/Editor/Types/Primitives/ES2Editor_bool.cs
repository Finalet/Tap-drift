using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_bool : ES2EditorType
{
	public ES2Editor_bool() : base(typeof(bool))
	{
		key = (byte)9;
	}
	
	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.Toggle((bool)data);
	}
}