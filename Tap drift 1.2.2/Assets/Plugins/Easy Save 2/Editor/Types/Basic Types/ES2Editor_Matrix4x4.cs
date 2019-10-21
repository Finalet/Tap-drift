using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Matrix4x4 : ES2EditorType
{
	public ES2Editor_Matrix4x4() : base(typeof(Matrix4x4))
	{
		key = (byte)32;
	}
}
