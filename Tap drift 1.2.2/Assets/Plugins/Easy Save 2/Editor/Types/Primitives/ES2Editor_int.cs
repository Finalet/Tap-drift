using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_int : ES2EditorType
{
	public ES2Editor_int() : base(typeof(int))
	{
		key = (byte)2;
	}

	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.IntField((int)data);
	}
}