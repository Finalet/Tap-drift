using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Material : ES2Type
{
	public ES2_Material() : base(typeof(Material))
	{
		key = (byte)27;
	}

	public override void Write(object data, ES2Writer writer)
	{
		Material param = (Material)data;
		writer.Write (param.shader.name);
		writer.Write((byte)3);

		bool hasProperty = param.HasProperty("_Color");
		writer.Write (hasProperty);
		if(hasProperty)
			writer.Write(param.color);

		// Find out what textures this material has.
		string[] potentialTextureNames = {"_BackTex","_BumpMap","_BumpSpecMap","_Control","_DecalTex","_Detail", "_DownTex", "_FrontTex", "_GlossMap", "_Illum", "_LeftTex","_LightMap", "_LightTextureB0", "_MainTex", "_ParallaxMap", "_RightTex","_ShadowOffset", "_Splat0", "_Splat1", "_Splat2", "_Splat3","_TranslucencyMap", "_UpTex", "_Tex", "_Cube", "_Albedo", "_MetallicGlossMap" };
		List<string> textureNames = new List<string>();
		List<Texture2D> textures = new List<Texture2D>();
		foreach(string name in potentialTextureNames)
		{
			if(param.HasProperty(name))
			{
				Texture2D tex = param.GetTexture(name) as Texture2D;
				if(tex != null)
				{
					textureNames.Add(name);
					textures.Add(tex);
				}
			}
		}
		// Write number of textures this Material has.
		writer.Write((int)textures.Count);

		for(int i=0; i<textures.Count; i++)
		{
			writer.Write (textureNames[i]);
			writer.Write(textures[i]);
			writer.Write (param.GetTextureOffset(textureNames[i]));
			writer.Write (param.GetTextureScale(textureNames[i]));
		}
			
		// Write Specular Color
		hasProperty = param.HasProperty("_SpecColor");
		writer.Write (hasProperty);
		if(hasProperty)
			writer.Write(param.GetColor("_SpecColor"));

		// Write Emission Color
		hasProperty = param.HasProperty("_EmissionColor");
		writer.Write (hasProperty);
		if(hasProperty)
			writer.Write(param.GetColor("_EmissionColor"));

		// Write Reflect Color
		hasProperty = param.HasProperty("_ReflectColor");
		writer.Write (hasProperty);
		if(hasProperty)
			writer.Write(param.GetColor("_ReflectColor"));

		// Write Albedo Color
		hasProperty = param.HasProperty("_Albedo");
		writer.Write (hasProperty);
		if(hasProperty)
			writer.Write(param.GetColor("_Albedo"));

		// Write Glossiness/Smoothness
		hasProperty = param.HasProperty("_Glossiness");
		writer.Write (hasProperty);
		if(hasProperty)
			writer.Write(param.GetFloat("_Glossiness"));

		// Write Glossiness/Smoothness
		hasProperty = param.HasProperty("_Metallic");
		writer.Write (hasProperty);
		if(hasProperty)
			writer.Write(param.GetFloat("_Metallic"));
	}

	public override object Read(ES2Reader reader)
	{
		Material result = new Material(Shader.Find("Diffuse"));
		Read(reader, result);
		return result;
	}
	
	public override void Read(ES2Reader reader, object obj)
	{
		Material result = (Material)obj;
		result.shader = Shader.Find(reader.reader.ReadString());
		int settingCount = (int)reader.reader.ReadByte();

		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
				case 0:
					if(reader.reader.ReadBoolean())
						result.color = reader.Read<Color>();
					break;
				case 1:
					int textureCount = reader.reader.ReadInt32();
					for(int j=0; j<textureCount; j++)
					{
						string textureName = reader.reader.ReadString();
						result.SetTexture(textureName, reader.Read<Texture2D>());
						result.SetTextureOffset(textureName, reader.Read<Vector2>());
						result.SetTextureScale(textureName, reader.Read<Vector2>());
					}
					break;
				case 2:
					if(reader.reader.ReadBoolean())
						result.SetColor("_SpecColor", reader.Read<Color>());
					if(reader.reader.ReadBoolean())
						result.SetColor("_EmissionColor", reader.Read<Color>());
					if(reader.reader.ReadBoolean())
						result.SetColor("_ReflectColor", reader.Read<Color>());
					if(reader.reader.ReadBoolean())
						result.SetColor("_Albedo", reader.Read<Color>());
					if(reader.reader.ReadBoolean())
						result.SetFloat("_Glossiness", reader.Read<float>());
					if(reader.reader.ReadBoolean())
						result.SetFloat("_Metallic", reader.Read<float>());
					break;
				default:
					return;
			}
		}
		return;
	}
}