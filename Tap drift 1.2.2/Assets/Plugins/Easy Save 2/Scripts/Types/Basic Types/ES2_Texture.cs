using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Texture : ES2Type
{
	public ES2_Texture() : base(typeof(Texture)){}

	public override void Write(object data, ES2Writer writer)
	{
		System.Type type = data.GetType();
		ES2Type es2Type = ES2TypeManager.GetES2Type(type);

		if(es2Type == null)
			Debug.LogError("Textures of type "+type.ToString()+" are not currently supported.");
		else
		{
			writer.Write(es2Type.hash);
			writer.Write(data, es2Type);
		}
	}

	public override object Read(ES2Reader reader)
	{
		int hash = reader.Read<int>();
		ES2Type es2Type = ES2TypeManager.GetES2Type(hash);
		if(es2Type == null)
		{
			Debug.LogError("Textures of type "+type.ToString()+" are not currently supported.");
			return null;
		}
		else
			return reader.Read<object>(es2Type);
	}
	
	public override void Read(ES2Reader reader, object obj)
	{
		int hash = reader.Read<int>();
		ES2Type es2Type = ES2TypeManager.GetES2Type(hash);
		if(es2Type == null)
			Debug.LogError("Textures of type "+type.ToString()+" are not currently supported.");
		else
			reader.Read<object>(es2Type, obj);
	}
}