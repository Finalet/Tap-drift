using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_long : ES2EditorType
{
	public ES2Editor_long() : base(typeof(long))
	{
		key = (byte)7;
	}
	
	public override object DisplayGUI(object data)
	{
		return (long)EditorGUILayout.IntField((int)((long)data));
	}
}
