using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Vector2 : ES2EditorType
{
	public ES2Editor_Vector2() : base(typeof(Vector2))
	{
		key = (byte)10;
	}

	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.Vector2Field("", (Vector2)data);
	}
}
