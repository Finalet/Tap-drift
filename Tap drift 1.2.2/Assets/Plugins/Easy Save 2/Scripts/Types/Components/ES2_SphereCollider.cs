using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_SphereCollider : ES2Type
{
	public ES2_SphereCollider() : base(typeof(SphereCollider))
	{
		key = (byte)21;
	}

	public override void Write(object data, ES2Writer writer)
	{
		SphereCollider param = (SphereCollider)data;
		writer.writer.Write((byte)3);
		writer.Write(param.center);
		writer.writer.Write(param.radius);
		writer.writer.Write (param.isTrigger);
	}

	public override object Read(ES2Reader reader)
	{
		SphereCollider param = GetOrCreate<SphereCollider>();
		Read(reader, param);
		return param;
	}

	public override void Read(ES2Reader reader, object c)
	{
		SphereCollider param = (SphereCollider)c;
		int settingCount = (int)reader.reader.ReadByte();
		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
			case 0:
				param.center = reader.Read<Vector3>();
				break;
			case 1:
				param.radius = reader.reader.ReadSingle();
				break;
			case 2:
				param.isTrigger = reader.reader.ReadBoolean();
				break;
			default:
				return;
			}
		}
	}
}