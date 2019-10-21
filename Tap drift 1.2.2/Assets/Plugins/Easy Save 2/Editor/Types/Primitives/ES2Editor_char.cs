using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_char : ES2EditorType
{
	public ES2Editor_char() : base(typeof(char))
	{
		key = (byte)19;
	}

	public override object DisplayGUI(object data)
	{
		string str = EditorGUILayout.TextField(((char)data).ToString());
		if(str.Length>0)
			return str[0];
		return ' ';
	}
}