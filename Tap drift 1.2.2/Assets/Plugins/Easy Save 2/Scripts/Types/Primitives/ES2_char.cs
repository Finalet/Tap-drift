using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_char : ES2Type
{
	public ES2_char() : base(typeof(char))
	{
		key = (byte)19;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((char)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadChar();
	}
}