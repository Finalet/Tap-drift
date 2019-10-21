using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_object : ES2Type
{
	public ES2_object()  : base(typeof(object)){}

	public override void Write(object data, ES2Writer writer)
	{
		var dataType = data.GetType();
		ES2Type es2Type = ES2TypeManager.GetES2Type(dataType);
		if(es2Type == null)
		{
			Debug.LogError("Cannot save Object of type "+dataType+" as it is not a supported type");
			return;
		}
		writer.Write(es2Type.hash);
		writer.Write(data, es2Type);
	}

	public override object Read(ES2Reader reader)
	{
		int hash = reader.Read<int>();
		ES2Type es2Type = ES2TypeManager.GetES2Type(hash);
		if(es2Type == null)
		{
			Debug.LogError("Cannot load Object of this type as it is not a supported type");
			return null;
		}

		return reader.Read<object>(es2Type);
	}
}