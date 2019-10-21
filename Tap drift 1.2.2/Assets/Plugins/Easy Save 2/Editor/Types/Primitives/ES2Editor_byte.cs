using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_byte : ES2EditorType
{
	public ES2Editor_byte()  : base(typeof(byte))
	{
		key = (byte)0;
	}

	public override object DisplayGUI(object data)
	{
		return (byte)EditorGUILayout.IntSlider((byte)data,0,255);
	}
}