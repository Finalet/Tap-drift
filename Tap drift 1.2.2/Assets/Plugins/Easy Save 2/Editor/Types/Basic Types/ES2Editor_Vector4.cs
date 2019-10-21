using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Vector4 : ES2EditorType
{
	public ES2Editor_Vector4() : base(typeof(Vector4))
	{
		key = (byte)12;
	}
	
	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.Vector4Field("", (Vector4)data);
	}
}
