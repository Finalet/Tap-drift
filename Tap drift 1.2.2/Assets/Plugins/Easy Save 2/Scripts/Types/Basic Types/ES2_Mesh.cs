using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_Mesh : ES2Type
{
	public ES2_Mesh() : base(typeof(Mesh))
	{
		key = (byte)15;
	}
	
	public override void Write(object data, ES2Writer writer)
	{
		Mesh param = (Mesh)data;
		writer.writer.Write(writer.settings.MeshSettingsToByteArray());

		#if UNITY_2017_3
		writer.writer.Write((int)param.indexFormat);
		#endif
		writer.Write(param.vertices);
		writer.Write(param.triangles);

		// Save Submeshes
		if(writer.settings.saveSubmeshes)
		{			
			writer.Write(param.subMeshCount);

			for(int i=0; i<param.subMeshCount; i++)
				writer.Write(param.GetTriangles(i));
		}

		// Save skinning
		if(writer.settings.saveSkinning)
		{
			writer.Write(param.bindposes);
			writer.Write(param.boneWeights);
		}

		if(writer.settings.saveNormals)
			writer.Write(param.normals);
		if(writer.settings.saveUV)
			writer.Write(param.uv);
		if(writer.settings.saveUV2)
			writer.Write(param.uv2);
		if(writer.settings.saveTangents)
			writer.Write(param.tangents);
		if(writer.settings.saveColors)
			writer.Write(param.colors32);
	}
	
	public override object Read(ES2Reader reader)
	{
		Mesh mesh = new Mesh();
		Read(reader, mesh);
		return mesh;
	}
	
	public override void Read(ES2Reader reader, object obj)
	{
		Mesh mesh = (Mesh)obj;
		reader.settings.MeshSettingsFromByteArray(reader.ReadMeshSettings());
		#if UNITY_2017_3
		mesh.indexFormat = (UnityEngine.Rendering.IndexFormat)reader.reader.ReadInt32();
		#endif
		mesh.vertices = reader.ReadArray<Vector3>(new ES2_Vector3());
		mesh.triangles = reader.ReadArray<int>(new ES2_int());

		// Save submeshes
		if(reader.settings.saveSubmeshes)
		{
			mesh.subMeshCount = reader.reader.ReadInt32();

			for(int i=0; i<mesh.subMeshCount; i++)
				mesh.SetTriangles(reader.ReadArray<int>(new ES2_int()), i);
		}

		// Save skinning
		if(reader.settings.saveSkinning)
		{
			mesh.bindposes = reader.ReadArray<Matrix4x4>();
			mesh.boneWeights = reader.ReadArray<BoneWeight>();
		}

		if(reader.settings.saveNormals)
			mesh.normals = reader.ReadArray<Vector3>(new ES2_Vector3());
		else
			mesh.RecalculateNormals();

		if(reader.settings.saveUV)
			mesh.uv = reader.ReadArray<Vector2>(new ES2_Vector2());
		if(reader.settings.saveUV2)
			mesh.uv2 = reader.ReadArray<Vector2>(new ES2_Vector2());
		if(reader.settings.saveTangents)
			mesh.tangents = reader.ReadArray<Vector4>(new ES2_Vector4());
		if(reader.settings.saveColors)
			mesh.colors32 = reader.ReadArray<Color32>(new ES2_Color32());
	}
}

/*public partial class ES2Reader : System.IDisposable
{
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public Mesh Read_Mesh(Mesh mesh)
	{
		settings.MeshSettingsFromByteArray(ReadMeshSettings());
		mesh.vertices = ReadArray<Vector3>(new ES2_Vector3());
		mesh.triangles = ReadArray<int>(new ES2_int());

		// Save submeshes
		if(settings.saveSubmeshes)
		{
			mesh.subMeshCount = reader.ReadInt32();
			
			for(int i=0; i<mesh.subMeshCount; i++)
				mesh.SetTriangles(ReadArray<int>(new ES2_int()), i);
		}

		// Save skinning
		if(settings.saveSkinning)
		{
			mesh.bindposes = ReadArray<Matrix4x4>();
			mesh.boneWeights = ReadArray<BoneWeight>();
		}
		
		if(settings.saveNormals)
			mesh.normals = ReadArray<Vector3>(new ES2_Vector3());
		else
			mesh.RecalculateNormals();
		
		if(settings.saveUV)
			mesh.uv = ReadArray<Vector2>(new ES2_Vector2());
		if(settings.saveUV2)
			mesh.uv2 = ReadArray<Vector2>(new ES2_Vector2());
		if(settings.saveTangents)
			mesh.tangents = ReadArray<Vector4>(new ES2_Vector4());
		if(settings.saveColors)
			mesh.colors32 = ReadArray<Color32>(new ES2_Color32());
		return mesh;
	}
}

public partial class ES2Writer : System.IDisposable
{
	public void Write_Mesh(Mesh param)
	{
		writer.Write(settings.MeshSettingsToByteArray());

		Write(param.vertices);
		Write(param.triangles);

		// Save Submeshes
		if(settings.saveSubmeshes)
		{			
			Write(param.subMeshCount);
			
			for(int i=0; i<param.subMeshCount; i++)
				Write(param.GetTriangles(i));
		}

		// Save skinning
		if(settings.saveSkinning)
		{
			Write(param.bindposes);
			Write(param.boneWeights);
		}

		if(settings.saveNormals)
			Write(param.normals);
		if(settings.saveUV)
			Write(param.uv);
		if(settings.saveUV2)
			Write(param.uv2);
		if(settings.saveTangents)
			Write(param.tangents);
		if(settings.saveColors)
			Write(param.colors32);
	}
}*/