#if !UNITY_4
using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_PolygonCollider2D : ES2Type
{
	public ES2_PolygonCollider2D() : base(typeof(PolygonCollider2D))
	{
		key = (byte)21;
	}

	public override void Write(object data, ES2Writer writer)
	{
		PolygonCollider2D param = (PolygonCollider2D)data;
		writer.writer.Write((byte)1); // Version number.

		writer.Write(param.isTrigger);
		writer.Write(param.offset);
		writer.Write(param.usedByEffector);

		writer.Write(param.pathCount);
		for(int i=0; i<param.pathCount; i++)
			writer.Write(param.GetPath(i));
	}

	public override object Read(ES2Reader reader)
	{
		PolygonCollider2D param = GetOrCreate<PolygonCollider2D>();
		Read(reader, param);
		return param;
	}

	public override void Read(ES2Reader reader, object c)
	{
		PolygonCollider2D param = (PolygonCollider2D)c;
		int settingCount = (int)reader.reader.ReadByte();
		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
				case 0:
					param.isTrigger = reader.Read<bool>();
					param.offset = reader.Read<Vector2>();
					param.usedByEffector = reader.Read<bool>();
					int pathCount = reader.Read<int>();
					for(int pathIndex=0; pathIndex<pathCount; pathIndex++)
						param.SetPath(pathIndex, reader.ReadArray<Vector2>(new ES2_Vector2()));
					break;
				default:
					return;
			}
		}
	}
}
#endif