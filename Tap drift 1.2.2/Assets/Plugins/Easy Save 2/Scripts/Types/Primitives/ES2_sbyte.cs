using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_sbyte : ES2Type
{
	public ES2_sbyte() : base(typeof(sbyte))
	{
		key = (byte)32;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((sbyte)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadSByte();
	}
}