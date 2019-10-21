using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Enum : ES2Type
{
	public ES2_Enum() : base(typeof(System.Enum))
	{
		key = (byte)32;
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

