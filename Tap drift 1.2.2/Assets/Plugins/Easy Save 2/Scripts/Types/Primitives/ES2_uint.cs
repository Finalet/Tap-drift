using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_uint : ES2Type
{
	public ES2_uint() : base(typeof(uint))
	{
		key = (byte)3;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((uint)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadUInt32();
	}
}
