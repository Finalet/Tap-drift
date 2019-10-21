using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_double : ES2Type
{
	public ES2_double()  : base(typeof(double))
	{
		key = (byte)8;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((double)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadDouble();
	}
}