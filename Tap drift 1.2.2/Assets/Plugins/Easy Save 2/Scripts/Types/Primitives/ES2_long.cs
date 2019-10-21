using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_long : ES2Type
{
	public ES2_long() : base(typeof(long))
	{
		key = (byte)7;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((long)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadInt64();
	}
}