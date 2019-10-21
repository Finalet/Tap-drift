#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.IO;

public class ES2EditorTypeUtility
{
	public static string[] es2TypeAssemblies = new string[]{"ES2","Assembly-CSharp", "Assembly-UnityScript"};

	private const string file_prefix 		= @"ES2UserType_";
	private const string directory_path		= "/Easy Save 2/Types/";
	private const string es2InitDirectory 	= "/Easy Save 2/";
	private const string es2InitFilename 	= "ES2Init.cs";
	public static string pathToES2Init
	{
		get{ return Application.dataPath + es2InitDirectory + es2InitFilename; }
	}

	private static Dictionary<Type, ES2Type> _supportedTypes = null;
	public static Dictionary<Type, ES2Type> supportedTypes
	{
		get
		{
			if(_supportedTypes == null)
				_supportedTypes = GetSupportedTypes();
			return _supportedTypes;
		}
		set{ _supportedTypes = value; }
	}

	/*
	 * 	Adds support for a Type, and all of it's supportable variables.
	 */
	public static void AddType(Type type)
	{
		string writes = "";
		string reads = "";

		FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach(FieldInfo field in fields)
		{
			if(!field.FieldType.IsValueType || !TypeIsSupported(field.FieldType) || !FieldIsSupportable(field))
				continue;

			Type fieldType = field.FieldType;

			reads 	+= CreateReadCall(fieldType, "data."+field.Name) + "\n";
			writes 	+= CreateWriteCall(fieldType, "data."+field.Name);
		}

		foreach(PropertyInfo property in properties)
		{
			if(!property.PropertyType.IsValueType || !TypeIsSupported(property.PropertyType) || !PropertyIsSupportable(property))
				continue;

			Type propertyType = property.PropertyType;

			reads 	+= CreateReadCall(propertyType, "data."+property.Name)+"\n";
			writes 	+= CreateWriteCall(propertyType, "data."+property.Name)+"\n";
		}
		
		string namespace_string = "";
		if(!String.IsNullOrEmpty(type.Namespace) && type.Namespace != "UnityEngine")
			namespace_string = "using "+type.Namespace+";";
		
		string create_instance = ""; // Used when creating a new instance of this object.
		if((typeof(Component).IsAssignableFrom(type)))
			create_instance = "GetOrCreate<"+type.ToString()+">()";
		else if((typeof(ScriptableObject).IsAssignableFrom(type)))
			create_instance = "ScriptableObject.CreateInstance<"+type.ToString()+">()";
		else
			create_instance = "new "+type.ToString()+"()";
		
		
		string file;
		/*if(typeof(Component).IsAssignableFrom(type))
			file = String.Format(ES2EditorTemplates.es2ComponentTypeTemplate, file_prefix, GetES2TypeName(type), type.ToString(), writes, reads, namespace_string);
		else*/
		file = String.Format(ES2EditorTemplates.GetTemplate("ES2TypeTemplate"), file_prefix, GetES2TypeName(type), type.ToString(), writes, reads, namespace_string, create_instance);
		
		// Write the file.
		File.WriteAllText(GetPathToES2Type(type), file);
		AddTypeToES2Init(type);
		AssetDatabase.Refresh();
	}

	public static string CreateWriteCall(Type type, string propertyName, string writeMethod = "writer.Write")
	{
		return writeMethod+"("+propertyName+");";
	}

	public static string CreateReadCall(Type type, string propertyName, string readMethod = "reader.Read")
	{
		string readCall = propertyName + " = " + readMethod;
		string genericParams = "";

		if(IsCollectionType(type))
		{
			readCall += GetCollectionTypeName(type);
			foreach(Type argumentType in type.GetGenericArguments())
			{
				if(genericParams != "")
					genericParams += ",";
				if(type.IsNested || type.IsEnum)
					genericParams += argumentType.ToString().Replace('+','.');
				else
					genericParams += argumentType.ToString();
			}
		}
		else
		{
			if(type.IsNested || type.IsEnum)
				genericParams = type.ToString().Replace('+','.');
			else
				genericParams = type.ToString();
		}


		readCall += "<"+genericParams+">();";
		return readCall;
	}

	/* Recreates the ES2Init, adding to it the given type */
	public static void AddTypeToES2Init(Type type)
	{			
		string es2TypeList = "";
		string userTypeList = "";

		foreach(KeyValuePair<Type, ES2Type> es2Type in supportedTypes)
		{
			// Don't add the same type twice.
			if(es2Type.Key == type)
				continue;
			
			// Add ES2Types built in to ES2 to the list first, and then the user ones later.
			if(es2Type.Value.GetType().Assembly.GetName().Name == "ES2")
				es2TypeList += @"		ES2TypeManager.types[typeof("+es2Type.Key.ToString()+")] = new "+es2Type.Value.GetType ().Name+"();\n";
			else
				userTypeList += @"		ES2TypeManager.types[typeof("+es2Type.Key.ToString()+")] = new "+es2Type.Value.GetType ().Name+"();\n";
		}
		// Add the type specified as a parameter.
		if(type != null)
			userTypeList += @"		ES2TypeManager.types[typeof("+type.ToString()+")] = new "+file_prefix+GetES2TypeName(type)+"();\n";
		
		File.WriteAllText(pathToES2Init, String.Format (ES2EditorTemplates.GetTemplate("ES2InitTemplate"), es2TypeList+userTypeList) );
	}

	/*
	 * 	Whether there is currently an ES2Type for this Type.
	 */
	public static bool TypeIsSupported(Type type)
	{
		if (type == typeof(string))
			return true;

		if(IsCollectionType(type))
		{
			if(!CollectionIsSupported(type))
				return false;

			if(type.IsGenericType)
			{
				foreach(Type arg in type.GetGenericArguments())
					if(!TypeIsSupported(arg))
						return false;
				return true;
			}
			else if(type.IsArray)
			{
				// Check that it's a type of array that we support ...
				if(type.GetArrayRank() > 3)
					return false;
				else if(!TypeIsSupported(type.GetElementType()))
					return false;
				return true;
			}
			return false;
		}

		if(type.IsEnum)
			return true;

		return supportedTypes.ContainsKey(type);
	}

	/*
	 * 	Whether this Property is supportable based on it's PropertyInfo.
	 */
	public static bool PropertyIsSupportable(PropertyInfo property)
	{
		if(property.GetGetMethod(false) == null || property.GetSetMethod(false) == null)
			return false;

		// Check that this property hasn't been deprecated.
		object[] attributes = property.GetCustomAttributes(false);
		foreach(object obj in attributes)
			if(obj is ObsoleteAttribute)
				return false;
		if(property.Name == "runInEditMode")
			return false;

		return TypeIsSupportable(property.PropertyType);
	}

	/*
	 * 	Whether this Field is supportable based on it's FieldInfo.
	 */
	public static bool FieldIsSupportable(FieldInfo field)
	{
		if(!field.IsPublic)
			return false;

		// Check that this property hasn't been deprecated.
		object[] attributes = field.GetCustomAttributes(false);
		foreach(object obj in attributes)
			if(obj is ObsoleteAttribute)
				return false;

		if(field.Name == "runInEditMode")
			return false;

		return TypeIsSupportable(field.FieldType);
	}

	public static ES2Type GetES2Type(Type type)
	{
		ES2Type es2Type;
		if(supportedTypes.TryGetValue(type, out es2Type))
			return es2Type;
		return null;
	}

	public static bool TypeIsSupportable(Type type)
	{
		return string.IsNullOrEmpty(TypeIsSupportableWithDescription(type));
	}

	/*
	 * 	Whether an ES2Type can be created for this Type.
	 * 	Returns: blank string if it can be supported.
	 * 	Returns: description of why it can't be supported if it can't be supported.
	 */
	public static string TypeIsSupportableWithDescription(Type type)
	{
		if(IsCollectionType(type))
		{
			if(!CollectionIsSupported(type))
				return "This type of collection is not currently supported.";
			// If it's a generic collection ...
			if(type.IsGenericType)
			{
				foreach(Type arg in type.GetGenericArguments())
					if(!TypeIsSupportable(arg))
						return "A type inside this collection is not supportable.";
				return "";
			}
			// Else if it's a normal array ...
			else if(type.IsArray)
			{
				// Check that it's a type of array that we support ...
				if(type.GetArrayRank() > 3)
					return "Only 1D, 2D and 3D arrays are supported.";
				else if(!TypeIsSupportable(type.GetElementType()))
					return "The type of data inside this array is not supportable.";
				return "";
			}
			return "This collection type is not supported.";
		}

		// ES2AutoSave can not be supported.
		if(type == typeof(ES2AutoSave) || typeof(ES2AutoSave).IsAssignableFrom(type))
			return "ES2AutoSave cannot be supported.";

		if(type.IsEnum)
			return "";

		// IMMEDIATELY SUPPORTABLE TYPES.
		if(typeof(ScriptableObject).IsAssignableFrom(type) || typeof(Component).IsAssignableFrom(type))
			return "";

		if(TypeIsSupported(type))
			return "";
		
		// AMBIGUOUSLY SUPPORTABLE TYPES.
		if(typeof(IEnumerable).IsAssignableFrom(type))
			return "Types of IEnumerable cannot be supported.";

		if(typeof(ES2Type).IsAssignableFrom(type))
			return "ES2Types cannot be supported.";

		if(type.IsGenericType)
			return "Generic types cannot be supported.";

		if(!type.IsVisible ||
		   !type.IsPublic || 
		   type.IsAbstract ||
		   type.Name.Contains("c__"))
			return "Type must be visible, public, non-abstract and non-internal to be supported.";

		return "";
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
	 * 	Whether a Collection is supported. Note that this does not check whether the
	 * 	inner types of the collection are supported.
	 */
	private static bool CollectionIsSupported(Type type)
	{
		if(type.IsGenericType)
		{
			Type genericTypeDef = type.GetGenericTypeDefinition();
			if(	genericTypeDef.IsAssignableFrom(typeof(List<>)) ||
			   genericTypeDef.IsAssignableFrom(typeof(Dictionary<,>)) ||
			   genericTypeDef.IsAssignableFrom(typeof(Queue<>)) ||
			   genericTypeDef.IsAssignableFrom(typeof(Stack<>)) ||
			   genericTypeDef.IsAssignableFrom(typeof(HashSet<>)))
				return true;
			return false;
		}
		else if(type.IsArray)
		{
			// Check that it's a type of array that we support ...
			if(type.GetArrayRank() > 3)
				return false;
			return true;
		}
		return false;
	}
	
	/* 
		Gets all ES2Type objects as a Dictionary containing it's appropriate type as the key 
	*/
	private static Dictionary<Type, ES2Type> GetSupportedTypes()
	{
		Dictionary<Type, ES2Type> es2Types = new Dictionary<Type, ES2Type>();
		
		foreach(string assemblyName in es2TypeAssemblies)
		{
			Assembly assembly = ES2EditorReflection.GetAssembly(assemblyName);
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
	
	/* Gets the path to an ES2Type file, whether it exists or not */
	private static string GetPathToES2Type(Type type)
	{
		return Application.dataPath + directory_path + file_prefix + GetES2TypeName(type) +".cs";
	}

	/* Gets the name of an ES2Type based on it's Type */
	private static string GetES2TypeName(Type type)
	{
		return type.ToString().Replace(".", string.Empty);
	}

	/* 
		Gets the type name of the collection (i.e. Dictionary, Array, 2DArray).
		Name will always begin with an uppercase letter or number.
	*/
	private static string GetCollectionTypeName(Type type)
	{
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
}
#endif