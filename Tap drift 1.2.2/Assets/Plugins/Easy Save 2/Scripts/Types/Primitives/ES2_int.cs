using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_int : ES2Type
{
	public ES2_int() : base(typeof(int))
	{
		key = (byte)2;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((int)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadInt32();
	}
}
