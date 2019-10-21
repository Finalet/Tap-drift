using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_string : ES2Type
{
	public ES2_string() : base(typeof(string))
	{
		key = (byte)1;
	}
	
	public override void Write(object data, ES2Writer writer)
	{
		writer.writer.Write((string)data);
	}
	
	public override object Read(ES2Reader reader)
	{
		return reader.reader.ReadString();
	}
}