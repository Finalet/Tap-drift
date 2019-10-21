using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_short : ES2EditorType
{
	public ES2Editor_short() : base(typeof(short))
	{
		key = (byte)4;
	}

	public override object DisplayGUI(object data)
	{
		return (short)EditorGUILayout.IntField((int)((short)data));
	}
}