using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Texture2D : ES2EditorType
{
	public ES2Editor_Texture2D() : base(typeof(Texture2D))
	{
		key = (byte)17;
	}
	
	public override object DisplayGUI(object data)
	{
		Texture2D tex = (Texture2D)data;
		tex = (Texture2D)EditorGUILayout.ObjectField(tex, typeof(Texture2D), true, GUILayout.Width(128), GUILayout.Height(128));
		return tex;
	}
	
	public override object CreateInstance()
	{
		return new Texture2D(0,0);
	}
}