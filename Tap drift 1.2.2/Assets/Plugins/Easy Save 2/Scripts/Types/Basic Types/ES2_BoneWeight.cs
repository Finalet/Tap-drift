using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_BoneWeight : ES2Type
{
	public ES2_BoneWeight() : base(typeof(BoneWeight))
	{
		key = (byte)33;
	}

	public override void Write(object data, ES2Writer writer)
	{
		BoneWeight param = (BoneWeight)data;
		writer.Write(param.boneIndex0);
		writer.Write(param.boneIndex1);
		writer.Write(param.boneIndex2);
		writer.Write(param.boneIndex3);
		writer.Write(param.weight0);
		writer.Write(param.weight1);
		writer.Write(param.weight2);
		writer.Write(param.weight3);
	}

	public override object Read(ES2Reader reader)
	{
		BoneWeight boneWeight = new BoneWeight();
		boneWeight.boneIndex0 = reader.reader.ReadInt32();
		boneWeight.boneIndex1 = reader.reader.ReadInt32();
		boneWeight.boneIndex2 = reader.reader.ReadInt32();
		boneWeight.boneIndex3 = reader.reader.ReadInt32();
		boneWeight.weight0 = reader.reader.ReadSingle();
		boneWeight.weight1 = reader.reader.ReadSingle();
		boneWeight.weight2 = reader.reader.ReadSingle();
		boneWeight.weight3 = reader.reader.ReadSingle();
		return boneWeight;
	}
}

