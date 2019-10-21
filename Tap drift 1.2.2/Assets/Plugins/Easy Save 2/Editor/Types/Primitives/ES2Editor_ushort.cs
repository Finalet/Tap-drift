using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_ushort : ES2EditorType
{
	public ES2Editor_ushort() : base(typeof(ushort))
	{
		key = (byte)5;
	}
	
	public override object DisplayGUI(object data)
	{
		return (ushort)EditorGUILayout.IntField((int)((ushort)data));
	}
}