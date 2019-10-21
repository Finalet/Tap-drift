using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Vector3 : ES2Type
{
	public ES2_Vector3() : base(typeof(Vector3))
	{
		key = (byte)11;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Vector3 param = (Vector3)data;
		writer.writer.Write(param.x);
		writer.writer.Write(param.y);
		writer.writer.Write(param.z);
	}

	public override object Read(ES2Reader reader)
	{
		return new Vector3(reader.reader.ReadSingle(),reader.reader.ReadSingle(),reader.reader.ReadSingle());
	}
}

