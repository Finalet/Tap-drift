using UnityEngine;
using System.Collections;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_AudioClip : ES2Type
{
	public ES2_AudioClip() : base(typeof(AudioClip))
	{
		key = (byte)25;
	}
	
	public override void Write(object data, ES2Writer writer)
	{
		AudioClip param = (AudioClip)data;
		writer.writer.Write((byte)5);
		float[] samples = new float[param.samples * param.channels];
		param.GetData(samples, 0);
		writer.writer.Write(param.name);
		writer.writer.Write(param.samples);
		writer.writer.Write(param.channels);
		writer.writer.Write(param.frequency);
		writer.Write(samples);
	}
	
	public override object Read(ES2Reader reader)
	{
		AudioClip result = null;
		string name = "";
		int samples = 0;
		int channels = 0;
		int frequency = 0;
		
		
		int settingCount = (int)reader.reader.ReadByte();
		for(int i=0;i<settingCount;i++)
		{
			switch(i)
			{
			case 0:
				name = reader.reader.ReadString();
				break;
			case 1:
				samples = reader.reader.ReadInt32();
				break;
			case 2:
				channels = reader.reader.ReadInt32();
				break;
			case 3:
				frequency = reader.reader.ReadInt32();
				break;
			case 4:
				#if UNITY_5_3_OR_NEWER
				result = AudioClip.Create(name, samples, channels, frequency, false);
				#else
				result = AudioClip.Create(name, samples, channels, frequency, false);
				#endif
				result.SetData(reader.ReadArray<float>(new ES2_float()),0);
				break;
			default:
				return result;
			}
		}
		return result;
	}
}