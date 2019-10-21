using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Color : ES2Type
{
	public ES2_Color() : base(typeof(Color))
	{
		key = (byte)13;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Color param = (Color)data;
		writer.Write(param.r);
		writer.Write(param.g);
		writer.Write(param.b);
		writer.Write(param.a);
	}

	public override object Read(ES2Reader reader)
	{
		return new Color(reader.reader.ReadSingle(),reader.reader.ReadSingle(),reader.reader.ReadSingle(),reader.reader.ReadSingle());
	}
}

