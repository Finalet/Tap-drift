using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_double : ES2EditorType
{
	public ES2Editor_double()  : base(typeof(double))
	{
		key = (byte)8;
	}

	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.DoubleField((double)data);
	}
}
