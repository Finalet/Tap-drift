using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ES2Data
{
	public Dictionary<string, object> loadedData = new Dictionary<string,object>();

	// Adds a piece of data to the Dictionary
	public void Add(string tag, object value)
	{
		loadedData[tag] = value;
	}

	public bool TagExists(string tag)
	{
		return loadedData.ContainsKey(tag);
	}

	public string[] GetTags()
	{
		string[] tags = new string[loadedData.Count];
		int i=0;
		foreach(KeyValuePair<string,object> entry in loadedData)
		{
			tags[i] = entry.Key;
			i++;
		}
		return tags;
	}
	
	public T Load<T>(string tag)
	{
		object loaded;
		if(loadedData.TryGetValue(tag, out loaded))
			return (T)loaded;
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return default(T);
	}
	
	public T[] LoadArray<T>(string tag)
	{	
		object loadedObject;
		if(loadedData.TryGetValue(tag, out loadedObject))
		{
			// Cast the object to the correct type and return.
			object[] objectArray = loadedObject as object[];
			T[] casted = new T[objectArray.Length];
			for(int i=0; i<objectArray.Length; i++)
				casted[i] = (T)objectArray[i];
			return casted;
		}
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return null;
	}
	
	public T[,] Load2DArray<T>(string tag)
	{	
		object loadedObject;
		if(loadedData.TryGetValue(tag, out loadedObject))
		{
			// Cast the object to the correct type and return.
			object[,] objectArray = loadedObject as object[,];
			T[,] casted = new T[objectArray.GetLength(0),objectArray.GetLength(1)];
			for(int i=0; i<objectArray.GetLength(0); i++)
				for(int j=0; j<objectArray.GetLength(0); j++)
					casted[i,j] = (T)objectArray[i,j];
			return casted;
		}
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return null;
	}
	
	public T[,,] Load3DArray<T>(string tag)
	{	
		object loadedObject;
		if(loadedData.TryGetValue(tag, out loadedObject))
		{
			// Cast the object to the correct type and return.
			object[,,] objectArray = loadedObject as object[,,];
			T[,,] casted = new T[objectArray.GetLength(0),objectArray.GetLength(1),objectArray.GetLength(2)];
			for(int i=0; i<objectArray.GetLength(0); i++)
				for(int j=0; j<objectArray.GetLength(1); j++)
					for(int k=0; k<objectArray.GetLength(2); k++)
						casted[i,j,k] = (T)objectArray[i,j,k];
			return casted;
		}
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return null;
	}
	
	public Dictionary<TKey, TValue> LoadDictionary<TKey, TValue>(string tag)
	{	
		object loadedObject;
		if(loadedData.TryGetValue(tag, out loadedObject))
		{
			// Cast the object to the correct type and return.
			Dictionary<object,object> objectDict = loadedObject as Dictionary<object,object>;
			Dictionary<TKey, TValue> casted = new Dictionary<TKey, TValue>();
			foreach(KeyValuePair<object,object> entry in objectDict)
				casted.Add((TKey)entry.Key, (TValue)entry.Value);
			return casted;
		}
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return null;
	}
	
	public List<T> LoadList<T>(string tag)
	{	
		object loadedObject;
		if(loadedData.TryGetValue(tag, out loadedObject))
		{
			// Cast the object to the correct type and return.
			List<object> objectArray = loadedObject as List<object>;
			List<T> casted = new List<T>();
			foreach(object obj in objectArray)
				casted.Add((T)obj);
			return casted;
		}
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return null;
	}
	
	public HashSet<T> LoadHashSet<T>(string tag)
	{	
		object loadedObject;
		if(loadedData.TryGetValue(tag, out loadedObject))
		{
			// Cast the object to the correct type and return.
			HashSet<object> objectArray = loadedObject as HashSet<object>;
			HashSet<T> casted = new HashSet<T>();
			foreach(object obj in objectArray)
				casted.Add((T)obj);
			return casted;
		}
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return null;
	}
	
	public Queue<T> LoadQueue<T>(string tag)
	{	
		object loadedObject;
		if(loadedData.TryGetValue(tag, out loadedObject))
		{
			// Cast the object to the correct type and return.
			Queue<object> objectArray = loadedObject as Queue<object>;
			Queue<T> casted = new Queue<T>();
			foreach(object obj in objectArray)
				casted.Enqueue((T)obj);
			return casted;
		}
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return null;
	}
	
	public Stack<T> LoadStack<T>(string tag)
	{
		object loadedObject;
		if(loadedData.TryGetValue(tag, out loadedObject))
		{
			// Cast the object to the correct type and return.
			Stack<object> objectArray = loadedObject as Stack<object>;
			Stack<T> casted = new Stack<T>();
			foreach(object obj in objectArray)
				casted.Push((T)obj);
			return casted;
		}
		Debug.LogError("This tag does not exist in this data. Please use ES2Data.Exists beforehand if you are uncertain that a tag exists.");
		return null;
	}
}

