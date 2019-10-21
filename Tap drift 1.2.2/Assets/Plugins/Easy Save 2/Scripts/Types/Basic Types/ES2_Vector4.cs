using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Vector4 : ES2Type
{
	public ES2_Vector4() : base(typeof(Vector4))
	{
		key = (byte)12;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Vector4 param = (Vector4)data;
		writer.writer.Write(param.x);
		writer.writer.Write(param.y);
		writer.writer.Write(param.z);
		writer.writer.Write(param.w);
	}

	public override object Read(ES2Reader reader)
	{
		return new Vector4(reader.reader.ReadSingle(),reader.reader.ReadSingle(),reader.reader.ReadSingle(),reader.reader.ReadSingle());
	}
}

