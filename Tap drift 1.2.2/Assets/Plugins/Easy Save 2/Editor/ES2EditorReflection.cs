using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

public class ES2EditorReflection
{
	// Returns all non-editor assemblies.
	public static string[] assemblies
	{
		get
		{
			//= new string[]{"Assembly-CSharp", "Assembly-UnityScript", "Assembly-CSharp-firstpass", "Assembly-UnityScript-firstpass", "UnityEngine", "System"};
			if(_assemblies != null)
				return _assemblies;

			Assembly[] assemblyObjects = AppDomain.CurrentDomain.GetAssemblies();
			var assemblyList = new List<string>();
				
			foreach(var assemblyObject in assemblyObjects)
			{
				string name = assemblyObject.GetName().Name;
				if(!name.Contains("Editor"))
					assemblyList.Add(name);
			}
			return (_assemblies = assemblyList.ToArray());
		}
	}
	public static string[] es2TypeAssemblies = new string[]{"Assembly-CSharp", "Assembly-UnityScript", "Assembly-CSharp-firstpass"};
	public static string[] editorAssemblies = new string[]{"Assembly-CSharp-Editor", "Assembly-CSharp-Editor-firstpass", "Assembly-UnityScript-Editor", "Assembly-UnityScript-Editor-firstpass"};

	public static string[] _assemblies = null;

	public static Assembly GetAssembly(string assemblyName)
	{
		try
		{
			return Assembly.Load(assemblyName);
		}
		catch(Exception e)
		{
			e.ToString();
			return null;
		}
	}
	
	/* 
		Gets all of the supportable types of the assemblies in the assemblies array, and orders alphabetically.
		If a searchTerm is specified, it only returns types with names containing that search term.
	*/
	public static List<Type> GetSupportableTypes(string searchTerm="")
	{
		List<Type> types = new List<Type>();

		foreach(string assemblyName in assemblies)
		{	
			Assembly assembly = GetAssembly(assemblyName);
			if(assembly == null)
				continue;

			try
			{
				IEnumerable<System.Type> tempTypes = assembly.GetTypes().Where (( t => TypeIsSupportable(t) && t.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0));
				types.AddRange(tempTypes);
			}
			catch(Exception e)
			{
				Debug.LogWarning("Easy Save could not load types from assembly "+ assemblyName + "as doing so threw the following exception:" + e.ToString());
			}
		}
		types = types.OrderBy(o=>o.Name).ToList();
		
		return types;
	}
	
	public static bool TypeIsSupportable(System.Type t)
	{
		// IMMEDIATELY SUPPORTABLE TYPES.
		if(typeof(ScriptableObject).IsAssignableFrom(t) || t.IsEnum)
			return true;
		
		if(typeof(Component).IsAssignableFrom(t))
			return true;
		
		// AMBIGUOUSLY SUPPORTABLE TYPES.
		if( typeof(IEnumerable).IsAssignableFrom(t) ||
		   typeof(ES2Type).IsAssignableFrom(t) ||
			t.IsGenericType ||
			!t.IsVisible ||
			!t.IsPublic || 
			t.IsAbstract ||
			t.Name.Contains("c__"))
			return false;
		
		return true;
	}
	
	public static Dictionary<Type, ES2EditorType> GetEditorTypes()
	{
		Dictionary<Type, ES2EditorType> es2Types = new Dictionary<Type, ES2EditorType>();
		
		foreach(string assemblyName in editorAssemblies)
		{
			Assembly assembly = GetAssembly(assemblyName);
			if(assembly == null)
				continue;
				
			IEnumerable<Type> tempTypes = assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(ES2EditorType)));
			foreach(Type type in tempTypes)
			{
				ES2EditorType es2EditorType = Activator.CreateInstance(type) as ES2EditorType;
				es2Types[es2EditorType.type] = es2EditorType;
			}
		}
		
		return es2Types;
	}
	
	/* 
		Gets all ES2Type objects as a Dictionary containing it's appropriate type as the key 
	*/
	public static Dictionary<Type, ES2Type> GetSupportedTypes()
	{
		Dictionary<Type, ES2Type> es2Types = new Dictionary<Type, ES2Type>();
		
		foreach(string assemblyName in es2TypeAssemblies)
		{
			Assembly assembly = GetAssembly(assemblyName);
			if(assembly == null)
				continue;
			
			IEnumerable<Type> tempTypes = assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(ES2Type)));
			foreach(Type type in tempTypes)
			{
				ES2Type es2Type = Activator.CreateInstance(type) as ES2Type;
				es2Types[es2Type.type] = es2Type;
			}
		}
		
		return es2Types;
	}
	
	public static Dictionary<string, ES2Header> GetHeadersFromFile(string path)
	{
		Dictionary<string, ES2Header> headers = new Dictionary<string, ES2Header>();
		ES2Settings settings = new ES2Settings(path);
		
		using(ES2Reader reader = new ES2Reader(settings))
		{
			while(reader.Next())
				headers[reader.currentTag.tag] = reader.ReadHeader();
		}
		
		return headers;
	}
	
	/* 
		Gets the type name of the collection (i.e. Dictionary, Array, 2DArray).
		Name will always begin with an uppercase letter or number.
		Returns a blank string if not a collection. 
	*/
	public static string GetSupportedCollectionType(Type type)
	{
		if(!IsCollectionType(type))
			return "";
		
		if(type.IsArray)
		{
			int arrayRank = type.GetArrayRank();
			// If array has more than 2 dimensions, we prepend the rank to the beginning.
			if(arrayRank > 1)
				return arrayRank+"DArray";
			return "Array";	
		}
		else if(type.IsGenericType)
			return type.Name.Substring(0, type.Name.IndexOf("`"));
		else
			return type.Name;
	}
	
	/* 
		Gets the types inside this collection type.
	*/
	public static Type[] GetSupportedCollectionTypes(Type type)
	{
		if(!IsCollectionType(type))
			return new Type[0];
		else if(type.IsGenericType)
			return type.GetGenericArguments();
		return new Type[]{type.GetElementType()};
	}
	
	
	/* Determines if this is a collection type */
	public static bool IsCollectionType(Type type)
	{
		if(!typeof(IEnumerable).IsAssignableFrom(type))
			return false;
		
		if(type.IsArray)
			return true;
		
		// If it's not an array and doesn't have generic arguments, we don't consider it to be a collection.
		if(type.GetGenericArguments().Length == 0)
			return false;
		
		return true;
	}
	
	/*
		Returns the names of the generic types for this type.
		Returns empty array if type is not generic.
	*/ 
	public static string[] GetGenericArgumentTypeNames(Type type)
	{
		Type[] args = type.GetGenericArguments();
		string[] names = new string[args.Length];
		
		for(int i=0; i<args.Length; i++)
			names[i] = args[i].Name;
		
		return names;
	}
}

/*public class ES2EditorTypeInfo
{
	public Type type = null;
	public ES2Type es2Type = null;
	
	public bool isComponent = false;

	public bool isSupportable = false;
	public string reasonNotSupportable = "";
	
	public bool isSupported = false;
	
	public ES2EditorTypeInfo(Type type)
	{
		this.type = type;
		
		isComponent = typeof(Component).IsAssignableFrom(type);
		
		reasonNotSupportable = IsSupportable(type);
		isSupportable = (reasonNotSupportable == "");
	}
	
	public static bool IsSupported(Type type)
	{
		// TODO : Implement IsSupported to show whether ES2EditorType has an ES2Type associated with it.
		return false;
	}
	
	//	Determines whether a type is supportable.
	//	Returns empty string if it is supportable.
	//	Returns reason why it's not supportable if not.
	public static string IsSupportable(Type type)
	{
		if(typeof(IEnumerable).IsAssignableFrom(type))
			return "Types which use the IEnumerable interface are not currently supported by Easy Save.";
		
		if(type.IsGenericType)
			return "Generic types are not currently supported by Easy Save.";
		
		if(!type.IsVisible)
			return "This type is not supported as it is private or not visible to other classes in Unity.";
		
		if(type.IsAbstract)
			return "Abstract classes are not supported as they can't be instantiated.";
		
		if(type.IsInterface)
			return "Interfaces are not supported as they can't be instantiated.";
		
		return "";
	}
}*/

public class ES2EditorProperty
{
	public string name;
	public Type type;
	public bool isSupported;
	public string unsupportedDescription;
	public bool isSelected;
	public bool isComponent;
	
	public bool isCollection;
	public string collectionType;
	// The types of the data in the array.
	public Type[] collectionContentTypes;
	
	public ES2EditorProperty(PropertyInfo info)
	{
		this.name = info.Name;
		this.isSelected = false;
		this.type = info.PropertyType;
		this.isComponent = typeof(Component).IsAssignableFrom(this.type);
		
		this.collectionType = ES2EditorReflection.GetSupportedCollectionType(this.type);
		this.isCollection = (collectionType != "");
		this.collectionContentTypes = ES2EditorReflection.GetSupportedCollectionTypes(type);
		
		bool hasGetter = (info.GetGetMethod() != null);
		bool hasSetter = (info.GetSetMethod() != null);
		
		// Determine if property can be saved.
		if(info.GetGetMethod().IsStatic)
		{
			isSupported = false;
			unsupportedDescription = "Static properties should not be saved from an ES2Type.";
		}
		else if(!hasSetter || !hasGetter)
		{
			isSupported = false;
			unsupportedDescription = "This property needs a getter and a setter to be able to save it.";
		}
		else
		{
			this.unsupportedDescription = ES2EditorManageTypes.TypeIsSupported(this.type);
			isSupported = (unsupportedDescription == "");
		}
	}
	
	public ES2EditorProperty(FieldInfo info)
	{
		this.name = info.Name;
		this.isSelected = false;
		this.type = info.FieldType;
		this.isComponent = typeof(Component).IsAssignableFrom(this.type);
		
		this.collectionType = ES2EditorReflection.GetSupportedCollectionType(this.type);
		this.isCollection = (collectionType != "");
		this.collectionContentTypes = ES2EditorReflection.GetSupportedCollectionTypes(type);
		
		// Determine if property can be saved.
		if(info.IsStatic)
		{
			isSupported = false;
			unsupportedDescription = "Static properties should not be saved from an ES2Type.";
		}
		else if(info.IsInitOnly)
		{
			isSupported = false;
			unsupportedDescription = "This field needs to be writable and readable to be able to save it.";
		}
		else
		{
			this.unsupportedDescription = ES2EditorManageTypes.TypeIsSupported(this.type);
			isSupported = (unsupportedDescription == "");
		}
	}
	
	public void DisplayToggle()
	{
		EditorGUILayout.BeginHorizontal();
		
		if(!isSupported)
		{
			EditorGUILayout.PrefixLabel(new GUIContent(name, type.ToString()));
			EditorGUILayout.LabelField(new GUIContent("Property not supported", unsupportedDescription), EditorStyles.miniLabel);
		}
		else
			isSelected = EditorGUILayout.Toggle(new GUIContent(name, type.ToString()), isSelected);
		
		EditorGUILayout.EndHorizontal();
	}
}

