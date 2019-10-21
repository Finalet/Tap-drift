#if !UNITY_4
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2_ES2AutoSaveManager : ES2Type
{
	public ES2_ES2AutoSaveManager() : base(typeof(ES2AutoSaveManager)){}
	
	public override void Write(object data, ES2Writer writer)
	{
		ES2AutoSaveManager mgr = (ES2AutoSaveManager)data;
		mgr.WriteAutoSaves(writer);
	}
	
	public override object Read(ES2Reader reader)
	{
		// If we're trying to use this method in ES2.LoadAll() or otherwise,
		// skip the data as it cannot be read with ES2.LoadAll().
		reader.Skip();
		return null;
	}

	public override void Read(ES2Reader reader, object obj)
	{
		ES2AutoSaveManager mgr = (ES2AutoSaveManager)obj;
		mgr.ReadAutoSaves(reader);
	}
}
#endif