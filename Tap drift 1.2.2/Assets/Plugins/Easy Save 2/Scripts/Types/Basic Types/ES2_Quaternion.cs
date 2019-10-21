using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Quaternion : ES2Type
{
	public ES2_Quaternion() : base(typeof(Quaternion))
	{
		key = (byte)14;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Quaternion param = (Quaternion)data;
		writer.writer.Write(param.x);
		writer.writer.Write(param.y);
		writer.writer.Write(param.z);
		writer.writer.Write(param.w);
	}

	public override object Read(ES2Reader reader)
	{
		return new Quaternion(reader.reader.ReadSingle(), reader.reader.ReadSingle(), reader.reader.ReadSingle(), reader.reader.ReadSingle());
	}
}

