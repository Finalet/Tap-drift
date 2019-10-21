using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_CapsuleCollider : ES2Type
{
	public ES2_CapsuleCollider() : base(typeof(CapsuleCollider))
	{
		key = (byte)23;
	}

	public override void Write(object data, ES2Writer writer)
	{
		CapsuleCollider param = (CapsuleCollider)data;
		writer.writer.Write((byte)5);
		writer.Write(param.center);
		writer.writer.Write(param.radius);
		writer.writer.Write(param.height);
		writer.writer.Write(param.direction);
		writer.writer.Write (param.isTrigger);
	}

	public override object Read(ES2Reader reader)
	{
		CapsuleCollider param = GetOrCreate<CapsuleCollider>();
		Read(reader, param);
		return param;
	}

	public override void Read(ES2Reader reader, object c)
	{
		CapsuleCollider param = (CapsuleCollider)c;
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
				param.height = reader.reader.ReadSingle();
				break;
			case 3:
				param.direction = reader.reader.ReadInt32();
				break;
			case 4:
				param.isTrigger = reader.reader.ReadBoolean();
				break;
			default:
				return;
			}
		}
	}
}