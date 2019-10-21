using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Matrix4x4 : ES2Type
{
	public ES2_Matrix4x4() : base(typeof(Matrix4x4))
	{
		key = (byte)32;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Matrix4x4 param = (Matrix4x4)data;
		writer.Write(param.GetColumn(0));
		writer.Write(param.GetColumn(1));
		writer.Write(param.GetColumn(2));
		writer.Write(param.GetColumn(3));
	}

	public override object Read(ES2Reader reader)
	{
		Matrix4x4 matrix = new Matrix4x4();
		matrix.SetColumn(0, reader.Read<Vector4>());
		matrix.SetColumn(1, reader.Read<Vector4>());
		matrix.SetColumn(2, reader.Read<Vector4>());
		matrix.SetColumn(3, reader.Read<Vector4>());
		return matrix;
	}
}
