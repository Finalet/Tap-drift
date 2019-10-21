using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_CapsuleCollider : ES2EditorType
{
	public ES2Editor_CapsuleCollider() : base(typeof(CapsuleCollider))
	{
		key = (byte)23;
	}

	public override object DisplayGUI(object data)
	{
		CapsuleCollider c = (CapsuleCollider)data;
		c.center = EditorGUILayout.Vector3Field("Center", c.center);
		c.radius = EditorGUILayout.FloatField("Radius", c.radius);
		c.height = EditorGUILayout.FloatField("Height", c.height);
		c.direction = EditorGUILayout.IntField ("Direction", c.direction);
		c.isTrigger = EditorGUILayout.Toggle ("Is Trigger?", c.isTrigger);
		return c;
	}
}
