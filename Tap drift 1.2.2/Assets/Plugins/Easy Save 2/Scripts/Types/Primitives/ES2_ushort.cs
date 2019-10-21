using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_ushort : ES2Type
{
	public ES2_ushort() : base(typeof(ushort))
	{
		key = (byte)5;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((ushort)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadUInt16();
	}
}
