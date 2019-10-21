using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_float : ES2EditorType
{
	public ES2Editor_float()  : base(typeof(float))
	{
		key = (byte)6;
	}

	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.FloatField((float)data);
	}
}

