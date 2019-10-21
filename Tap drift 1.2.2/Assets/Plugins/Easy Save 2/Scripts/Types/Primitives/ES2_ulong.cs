using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_ulong : ES2Type
{
	public ES2_ulong() : base(typeof(ulong))
	{
		key = (byte)20;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((ulong)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadUInt64();
	}
}