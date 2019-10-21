using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_SkinnedMeshRenderer : ES2Type
{
	public ES2_SkinnedMeshRenderer() : base(typeof(SkinnedMeshRenderer))
	{
		key = (byte)34;
	}

	public override void Write(object data, ES2Writer writer)
	{
		SkinnedMeshRenderer param = (SkinnedMeshRenderer)data;
		writer.writer.Write((byte)0);
		writer.Write(param.bones);
		writer.Write(param.localBounds);
		writer.Write((int)param.quality);
		writer.Write(param.sharedMesh);
		for(int idx=0; idx<param.sharedMesh.blendShapeCount; idx++)
			writer.Write(param.GetBlendShapeWeight(idx));
	}

	public override object Read(ES2Reader reader)
	{
		SkinnedMeshRenderer param = GetOrCreate<SkinnedMeshRenderer>();
		Read(reader, param);
		return param;
	}

	public override void Read(ES2Reader reader, object c)
	{
		SkinnedMeshRenderer param = (SkinnedMeshRenderer)c;
		int settingCount = (int)reader.reader.ReadByte();
		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
			case 0:
				param.bones = reader.ReadArray<Transform>();

				foreach(Transform bone in param.bones)
					bone.parent = param.transform;

				param.localBounds = reader.Read<Bounds>();
				param.quality = (SkinQuality)reader.reader.ReadInt32();
				reader.Read<Mesh>(param.sharedMesh);
				for(int idx=0; idx<param.sharedMesh.blendShapeCount; idx++)
					param.SetBlendShapeWeight(idx, reader.reader.ReadSingle());
				break;
			default:
				return;
			}
		}
	}
}
