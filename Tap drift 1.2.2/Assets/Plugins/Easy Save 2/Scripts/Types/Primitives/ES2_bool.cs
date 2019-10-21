using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_bool : ES2Type
{
	public ES2_bool() : base(typeof(bool))
	{
		key = (byte)9;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((bool)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadBoolean();
	}
}