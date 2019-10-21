using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Rect : ES2Type
{
	public ES2_Rect() : base(typeof(Rect))
	{
		key = (byte)29;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Rect param = (Rect)data;
		writer.Write(param.x);
		writer.Write(param.y);
		writer.Write(param.width);
		writer.Write(param.height);
	}

	public override object Read(ES2Reader reader)
	{
		return new Rect(reader.reader.ReadSingle(), reader.reader.ReadSingle(), reader.reader.ReadSingle(), reader.reader.ReadSingle());
	}
}

