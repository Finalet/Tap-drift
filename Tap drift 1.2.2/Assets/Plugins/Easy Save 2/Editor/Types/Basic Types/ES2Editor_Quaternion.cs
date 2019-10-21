using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Quaternion : ES2EditorType
{
	public ES2Editor_Quaternion() : base(typeof(Quaternion))
	{
		key = (byte)14;
	}

	public override object DisplayGUI(object data)
	{
		Quaternion q = (Quaternion)data;
		Vector4 v = EditorGUILayout.Vector4Field("", new Vector4(q.x,q.y,q.z,q.w));
		return new Quaternion(v.x,v.y,v.z,v.w);
	}
}