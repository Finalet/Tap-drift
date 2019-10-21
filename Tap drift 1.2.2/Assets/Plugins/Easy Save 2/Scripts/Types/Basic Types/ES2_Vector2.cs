using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Vector2 : ES2Type
{
	public ES2_Vector2() : base(typeof(Vector2))
	{
		key = (byte)10;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Vector2 param = (Vector2)data;
		writer.writer.Write(param.x);
		writer.writer.Write(param.y);
	}

	public override object Read(ES2Reader reader)
	{
		return new Vector2(reader.reader.ReadSingle(),reader.reader.ReadSingle());
	}
}

