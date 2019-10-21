using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_MeshCollider : ES2EditorType
{
	public ES2Editor_MeshCollider() : base(typeof(MeshCollider))
	{
		key = (byte)24;
	}
	
	public override object DisplayGUI(object data)
	{
		MeshCollider c = (MeshCollider)data;
		c.sharedMesh = (Mesh)EditorGUILayout.ObjectField("Shared Mesh", c.sharedMesh, typeof(Mesh), true);
		c.convex = EditorGUILayout.Toggle ("Convex", c.convex);
		c.isTrigger = EditorGUILayout.Toggle ("Is Trigger?", c.isTrigger);
		return c;
	}
}