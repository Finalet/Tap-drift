using UnityEngine;
using UnityEditor;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Mesh : ES2EditorType
{
	public ES2Editor_Mesh() : base(typeof(Mesh))
	{
		key = (byte)15;
	}
	
	public override object DisplayGUI(object data)
	{
		return EditorGUILayout.ObjectField((Mesh)data, typeof(Mesh), true);
	}
	
	public override object CreateInstance()
	{
		// Create a Plane as a generic instance.
		Vector3[] verts = new Vector3[4];
		Vector3[] normals = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		int[] tri = new int[6];
		verts[0] = new Vector3(0, 0, 0);
		verts[1] = new Vector3(1, 0, 0);
		verts[2] = new Vector3(0, 0, 1);
		verts[3] = new Vector3(1, 0, 1);
		
		for (int i = 0; i < normals.Length; i++)
		{
			normals[i] = Vector3.up;
		}
		
		uv[0] = new Vector2(0, 0);
		uv[1] = new Vector2(1, 0);
		uv[2] = new Vector2(0, 1);
		uv[3] = new Vector2(1, 1);
		
		tri[0] = 0;
		tri[1] = 2;
		tri[2] = 3;
		
		tri[3] = 0;
		tri[4] = 3;
		tri[5] = 1;
		
		Mesh mesh = new Mesh();
		mesh.vertices = verts;
		mesh.triangles = tri;
		mesh.uv = uv;
		mesh.normals = normals;
		
		return mesh;
	}
}