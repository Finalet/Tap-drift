using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_float : ES2Type
{
	public ES2_float()  : base(typeof(float))
	{
		key = (byte)6;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((float)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadSingle();
	}
}