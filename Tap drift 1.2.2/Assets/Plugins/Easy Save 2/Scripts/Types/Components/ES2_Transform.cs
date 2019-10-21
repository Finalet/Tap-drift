using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Transform : ES2Type
{
	public ES2_Transform() : base(typeof(Transform))
	{
		key = (byte)16;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Transform param = (Transform)data;
		writer.writer.Write((byte)4);
		writer.Write(param.localPosition);
		writer.Write(param.localRotation);
		writer.Write(param.localScale);
		writer.writer.Write(param.tag);
	}

	public override object Read(ES2Reader reader)
	{
		Transform param = GetOrCreate<Transform>();
		Read(reader, param);
		return param;
	}
	
	public override void Read(ES2Reader reader, object c)
	{
		Transform param = (Transform)c;
		int settingCount = (int)reader.reader.ReadByte();
		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
			case 0:
				param.localPosition = reader.Read<Vector3>();
				break;
			case 1:
				param.localRotation = reader.Read<Quaternion>();
				break;
			case 2:
				param.localScale = reader.Read<Vector3>();
				break;
			case 3:
				param.tag = reader.reader.ReadString();
				break;
			default:
				return;
			}
		}
	}
}