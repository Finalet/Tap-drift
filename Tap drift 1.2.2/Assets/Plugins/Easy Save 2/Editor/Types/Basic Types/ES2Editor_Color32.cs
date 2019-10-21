using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Color32 : ES2EditorType
{
	public ES2Editor_Color32() : base(typeof(Color32))
	{
		key = (byte)26;
	}
	
	public override object DisplayGUI(object data)
	{
		return (Color32)EditorGUILayout.ColorField((Color32)data);
	}
}