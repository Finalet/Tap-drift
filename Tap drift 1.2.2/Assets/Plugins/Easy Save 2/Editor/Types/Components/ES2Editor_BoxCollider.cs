using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_BoxCollider : ES2EditorType
{
	public ES2Editor_BoxCollider()  : base(typeof(BoxCollider))
	{
		key = (byte)22;
	}
	
	public override object DisplayGUI(object data)
	{
		BoxCollider c = (BoxCollider)data;
		c.center = EditorGUILayout.Vector3Field("Center", c.center);
		c.size = EditorGUILayout.Vector3Field("Size", c.size);
		c.isTrigger = EditorGUILayout.Toggle("Is Trigger?", c.isTrigger);
		return c;
	}
}