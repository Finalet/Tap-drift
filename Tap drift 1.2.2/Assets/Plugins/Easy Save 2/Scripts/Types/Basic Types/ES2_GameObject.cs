/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class ES2_GameObject : ES2Type
{
	public ES2_GameObject() : base(typeof(GameObject)){}

	public override void Write(object data, ES2Writer writer)
	{
		GameObject go = (GameObject)data;
		// Get the Components of the GameObject that you want to save and save them.
		var components = go.GetComponents(typeof(Component));
		var supportedComponents = new List<Component>();

		// Get the supported Components and put them in a List.
		foreach(var component in components)
			if(ES2TypeManager.GetES2Type(component.GetType() != null))
				supportedComponents.Add(Component);

		// Write how many Components we're saving so we know when we're loading.
		writer.Write(supportedComponents.Count);

		// Save each Component individually.
		foreach(var component in supportedComponents)
			writer.Write(component);
	}
	
	public override void Read(ES2Reader reader, object c)
	{
		GameObject go = (GameObject)c;
		// How many components do we need to load?
		int componentCount = reader.Read<int>();

		for(int i=0; i<componentCount; i++)
			reader.ReadComponent(go);
	}

	public override object Read(ES2Reader reader)
	{
		GameObject go = new GameObject();
		Read(reader, go);
		return go;
	}
}*/