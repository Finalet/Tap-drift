using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_decimal : ES2Type
{
	public ES2_decimal()  : base(typeof(decimal))
	{
		key = (byte)34;
	}

	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((decimal)data);
	}

	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadDecimal();
	}
}
