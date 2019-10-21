using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_BoxCollider : ES2Type
{
	public ES2_BoxCollider()  : base(typeof(BoxCollider))
	{
		key = (byte)22;
	}

	public override void Write(object data, ES2Writer writer)
	{
		BoxCollider param = (BoxCollider)data;
		writer.writer.Write((byte)3);
		writer.Write(param.center);
		writer.Write(param.size);
		writer.writer.Write (param.isTrigger);
	}

	public override object Read(ES2Reader reader)
	{
		BoxCollider param = GetOrCreate<BoxCollider>();
		Read(reader, param);
		return param;
	}

	public override void Read(ES2Reader reader, object c)
	{
		BoxCollider param = (BoxCollider)c;
		int settingCount = (int)reader.reader.ReadByte();
		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
			case 0:
				param.center = reader.Read<Vector3>();
				break;
			case 1:
				param.size = reader.Read<Vector3>();
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