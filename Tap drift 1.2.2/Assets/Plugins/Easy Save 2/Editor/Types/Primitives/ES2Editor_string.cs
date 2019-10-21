using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_string : ES2EditorType
{
	public ES2Editor_string() : base(typeof(string))
	{
		key = (byte)1;
	}

	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.TextField((string)data);
	}
}