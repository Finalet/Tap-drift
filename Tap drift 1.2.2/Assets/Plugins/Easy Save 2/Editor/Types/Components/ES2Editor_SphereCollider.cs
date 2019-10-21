using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_SphereCollider : ES2EditorType
{
	public ES2Editor_SphereCollider() : base(typeof(SphereCollider))
	{
		key = (byte)21;
	}
	
	public override object DisplayGUI(object data)
	{
		SphereCollider c = (SphereCollider)data;
		c.center = EditorGUILayout.Vector3Field("Center", c.center);
		c.radius = EditorGUILayout.FloatField("Radius", c.radius);
		c.isTrigger = EditorGUILayout.Toggle("Is Trigger?", c.isTrigger);
		return c;
	}
}