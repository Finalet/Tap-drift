using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Rect : ES2EditorType
{
	public ES2Editor_Rect() : base(typeof(Rect))
	{
		key = (byte)29;
	}
	
	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.RectField((Rect)data);
	}
}