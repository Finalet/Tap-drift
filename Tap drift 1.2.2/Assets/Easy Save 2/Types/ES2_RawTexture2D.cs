using UnityEngine;
using System.Collections;

public sealed class ES2_RawTexture2D : ES2Type
{
	public ES2_RawTexture2D() : base(typeof(Texture2D)){key = (byte)17;}

	public override void Write(object data, ES2Writer writer)
	{
		Texture2D param = (Texture2D)data;
		writer.writer.Write((byte)6);
		byte[] png = param.GetRawTextureData(); 
		writer.writer.Write(png.Length);
		writer.writer.Write(png);
		writer.writer.Write((int)param.filterMode);
		writer.writer.Write(param.anisoLevel);
		writer.writer.Write((int)param.wrapMode);
		writer.writer.Write(param.mipMapBias);
	}

	public override object Read(ES2Reader reader)
	{
		Texture2D result = new Texture2D(0,0);
		Read(reader, result);
		return result;
	}
	
	public override void Read(ES2Reader reader, object obj)
	{
		Texture2D result = (Texture2D)obj;
		int settingCount = (int)reader.reader.ReadByte();
		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
			case 0:
				result.LoadRawTextureData(reader.reader.ReadBytes(reader.reader.ReadInt32()));
				result.Apply();
				break;
			case 1:
				result.filterMode = (FilterMode)reader.reader.ReadInt32();
				break;
			case 2:
				result.anisoLevel = reader.reader.ReadInt32();
				break;
			case 3:
				result.wrapMode = (TextureWrapMode)reader.reader.ReadInt32();
				break;
			case 4:
				result.mipMapBias = reader.reader.ReadSingle();
				break;
			default:
				return;
			}
		}
	}
}