using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_SkinnedMeshRenderer : ES2EditorType
{
	public ES2Editor_SkinnedMeshRenderer() : base(typeof(SkinnedMeshRenderer))
	{
		key = (byte)34;
	}
}