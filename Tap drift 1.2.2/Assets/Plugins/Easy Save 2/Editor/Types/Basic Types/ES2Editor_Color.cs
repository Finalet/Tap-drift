using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Color : ES2EditorType
{
	public ES2Editor_Color() : base(typeof(Color))
	{
		key = (byte)13;
	}
	
	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.ColorField((Color)data);
	}
}