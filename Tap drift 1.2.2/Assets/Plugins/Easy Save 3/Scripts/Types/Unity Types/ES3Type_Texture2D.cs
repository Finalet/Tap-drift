using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("filterMode", "anisoLevel", "wrapMode", "mipMapBias", "rawTextureData")]
	public class ES3Type_Texture2D : ES3UnityObjectType
	{
		public static ES3Type Instance = null;

		public ES3Type_Texture2D() : base(typeof(UnityEngine.Texture2D)){ Instance = this; }

		protected override void WriteUnityObject(object obj, ES3Writer writer)
		{
			var instance = (UnityEngine.Texture2D)obj;

			writer.WriteProperty("width", instance.width, ES3Type_int.Instance);
			writer.WriteProperty("height", instance.height, ES3Type_int.Instance);
			writer.WriteProperty("format", instance.format);
			writer.WriteProperty("mipmapCount", instance.mipmapCount, ES3Type_int.Instance);
			writer.WriteProperty("filterMode", instance.filterMode);
			writer.WriteProperty("anisoLevel", instance.anisoLevel, ES3Type_int.Instance);
			writer.WriteProperty("wrapMode", instance.wrapMode);
			writer.WriteProperty("mipMapBias", instance.mipMapBias, ES3Type_float.Instance);
			writer.WriteProperty("rawTextureData", instance.GetRawTextureData(), ES3Type_byteArray.Instance);
		}

		protected override void ReadUnityObject<T>(ES3Reader reader, object obj)
		{
			var instance = (UnityEngine.Texture2D)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					case "filterMode":
						instance.filterMode = reader.Read<UnityEngine.FilterMode>();
						break;
					case "anisoLevel":
						instance.anisoLevel = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "wrapMode":
						instance.wrapMode = reader.Read<UnityEngine.TextureWrapMode>();
						break;
					case "mipMapBias":
						instance.mipMapBias = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "rawTextureData":
						// LoadRawTextureData requires that the correct width, height, TextureFormat and mipMaps are set before being called.
						// If an error occurs here, it's likely that we're using LoadInto to load into a Texture which differs in these values.
						// In this case, LoadInto should be avoided and Load should be used instead.
						instance.LoadRawTextureData(reader.Read<byte[]>(ES3Type_byteArray.Instance));
						instance.Apply();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadUnityObject<T>(ES3Reader reader)
		{
			var instance = new Texture2D(	reader.Read<int>(ES3Type_int.Instance), // Property name has already been read in ES3UnityObjectType, so we only need to read the value.
											reader.ReadProperty<int>(ES3Type_int.Instance),
											reader.ReadProperty<TextureFormat>(), 
											(reader.ReadProperty<int>(ES3Type_int.Instance) > 1));
			ReadObject<T>(reader, instance);
			return instance;
		}
	}

	public class ES3Type_Texture2DArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3Type_Texture2DArray() : base(typeof(UnityEngine.Texture2D[]), ES3Type_Texture2D.Instance)
		{
			Instance = this;
		}
	}
}