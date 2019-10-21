using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_decimal : ES2EditorType
{
	public ES2Editor_decimal()  : base(typeof(decimal))
	{
		key = (byte)34;
	}

	public override object DisplayGUI(object data)
	{
		return (decimal)EditorGUILayout.FloatField((float)((decimal)data));
	}
}