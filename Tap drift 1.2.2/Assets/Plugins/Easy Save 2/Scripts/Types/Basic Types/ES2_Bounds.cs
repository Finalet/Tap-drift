using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Bounds : ES2Type
{
	public ES2_Bounds() : base(typeof(Bounds))
	{
		key = (byte)30;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Bounds param = (Bounds)data;
		writer.Write(param.center);
		writer.Write(param.size);
	}

	public override object Read(ES2Reader reader)
	{
		return new Bounds(reader.Read<Vector3>(), reader.Read<Vector3>());
	}
}

