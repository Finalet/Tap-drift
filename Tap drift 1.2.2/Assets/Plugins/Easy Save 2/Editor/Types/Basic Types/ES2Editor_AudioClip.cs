using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_AudioClip : ES2EditorType
{
	public ES2Editor_AudioClip() : base(typeof(AudioClip))
	{
		key = (byte)33;
	}
	
	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.ObjectField((AudioClip)data, typeof(AudioClip),true);
	}
	
	public override object CreateInstance()
	{
		return AudioClip.Create("",0,0,0,false);
	}
}