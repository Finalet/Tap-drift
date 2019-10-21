using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_byte : ES2Type
{
	public ES2_byte()  : base(typeof(byte))
	{
		key = (byte)0;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((byte)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadByte();
	}
}