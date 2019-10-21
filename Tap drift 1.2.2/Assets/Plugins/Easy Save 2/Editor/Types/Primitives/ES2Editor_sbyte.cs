using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_sbyte : ES2EditorType
{
	public ES2Editor_sbyte() : base(typeof(sbyte))
	{
		key = (byte)32;
	}
	
	public override object DisplayGUI(object data)
	{
		return (sbyte)EditorGUILayout.IntSlider((byte)((sbyte)data),-128,127);
	}
}
