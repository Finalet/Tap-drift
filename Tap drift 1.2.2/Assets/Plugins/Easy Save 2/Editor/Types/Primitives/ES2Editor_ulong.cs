using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_ulong : ES2EditorType
{
	public ES2Editor_ulong() : base(typeof(ulong))
	{
		key = (byte)20;
	}

	public override object DisplayGUI(object data)
	{
		return (ulong)EditorGUILayout.IntField((int)((ulong)data));
	}
}