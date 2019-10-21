using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_MeshCollider : ES2Type
{
	public ES2_MeshCollider() : base(typeof(MeshCollider))
	{
		key = (byte)24;
	}

	public override void Write(object data, ES2Writer writer)
	{
		MeshCollider param = (MeshCollider)data;
		writer.settings.saveTangents = false;
		writer.writer.Write((byte)4);
		writer.Write(param.sharedMesh);
		writer.writer.Write(param.convex);
		writer.writer.Write(false); // Smooth sphere collisions is no longer supported in Unity 5.
		writer.writer.Write (param.isTrigger);
	}

	public override object Read(ES2Reader reader)
	{
		MeshCollider param = GetOrCreate<MeshCollider>();
		Read(reader, param);
		return param;
	}

	public override void Read(ES2Reader reader, object c)
	{
		MeshCollider param = (MeshCollider)c;
		int settingCount = (int)reader.reader.ReadByte();
		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
			case 0:
				param.sharedMesh = null;
				param.sharedMesh = reader.Read<Mesh>();
				break;
			case 1:
				param.convex = reader.reader.ReadBoolean();
				break;
			case 2:
				reader.reader.ReadBoolean(); // Smooth sphere collisions is no longer supported in Unity 5.
				break;
			case 3:
				param.isTrigger = reader.reader.ReadBoolean();
				break;
			default:
				return;
			}
		}
	}
}

