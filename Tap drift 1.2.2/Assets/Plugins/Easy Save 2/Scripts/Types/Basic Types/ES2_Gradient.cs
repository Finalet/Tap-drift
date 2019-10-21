using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Gradient : ES2Type
{
	public ES2_Gradient() : base(typeof(Gradient)){}

	public override void Write(object data, ES2Writer writer)
	{
		Gradient param = (Gradient)data;
		writer.Write<GradientColorKey>(param.colorKeys);
		writer.Write<GradientAlphaKey>(param.alphaKeys);
	}

	public override object Read(ES2Reader reader)
	{
		Gradient result = new Gradient();
		result.SetKeys(reader.ReadArray<GradientColorKey>(new ES2_GradientColorKey()), reader.ReadArray<GradientAlphaKey>(new ES2_GradientAlphaKey()));
		return result;
	}
}
