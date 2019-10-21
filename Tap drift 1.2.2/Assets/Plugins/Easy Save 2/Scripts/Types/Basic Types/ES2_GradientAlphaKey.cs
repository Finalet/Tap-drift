using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_GradientAlphaKey : ES2Type
{
	public ES2_GradientAlphaKey() : base(typeof(GradientAlphaKey)){}

	public override void Write(object data, ES2Writer writer)
	{
		GradientAlphaKey param = (GradientAlphaKey)data;
		writer.Write(param.alpha);
		writer.Write(param.time);
	}

	public override object Read(ES2Reader reader)
	{
		return new GradientAlphaKey(reader.reader.ReadSingle(), reader.reader.ReadSingle());
	}
}
