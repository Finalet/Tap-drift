using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_short : ES2Type
{
	public ES2_short() : base(typeof(short))
	{
		key = (byte)4;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((short)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadInt16();
	}
}
