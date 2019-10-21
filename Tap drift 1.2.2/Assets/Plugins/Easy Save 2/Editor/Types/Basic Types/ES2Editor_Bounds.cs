using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Bounds : ES2EditorType
{
	public ES2Editor_Bounds() : base(typeof(Bounds))
	{
		key = (byte)30;
	}
	
	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.BoundsField((Bounds)data);
	}
}
