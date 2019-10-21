using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Color32 : ES2Type
{
	public ES2_Color32() : base(typeof(Color32))
	{
		key = (byte)26;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Color32 param = (Color32)data;
		writer.Write(param.r);
		writer.Write(param.g);
		writer.Write(param.b);
		writer.Write(param.a);
	}

	public override object Read(ES2Reader reader)
	{
		return new Color32(reader.reader.ReadByte(), reader.reader.ReadByte(), reader.reader.ReadByte(), reader.reader.ReadByte());
	}
}

