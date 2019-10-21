using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_uint : ES2EditorType
{
	public ES2Editor_uint() : base(typeof(uint))
	{
		key = (byte)3;
	}

	public override object DisplayGUI(object data)
	{
		return (uint)EditorGUILayout.IntField((int)((uint)data));
	}
}