using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_BoneWeight : ES2EditorType
{
	public ES2Editor_BoneWeight() : base(typeof(BoneWeight))
	{
		key = (byte)25;
	}
}