using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_GradientColorKey : ES2Type
{
	public ES2_GradientColorKey() : base(typeof(GradientColorKey)){}

	public override void Write(object data, ES2Writer writer)
	{
		GradientColorKey param = (GradientColorKey)data;
		writer.Write(param.color);
		writer.Write(param.time);
		writer.Write((GradientColorKey)data);
	}

	public override object Read(ES2Reader reader)
	{
		return new GradientColorKey(reader.Read<Color>(), reader.reader.ReadSingle());
	}
}